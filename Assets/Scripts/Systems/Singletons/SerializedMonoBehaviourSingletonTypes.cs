
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ScriptableObjectResourcePath(nameof(SerializedMonoBehaviourSingletonTypes))]
public class SerializedMonoBehaviourSingletonTypes : GlobalScriptableObjectSingleton<SerializedMonoBehaviourSingletonTypes>, ISerializationCallbackReceiver
{
	[System.NonSerialized]
	private readonly Dictionary<string, List<System.Type>> m_MappedTypes = new Dictionary<string, List<System.Type>>();


	/////////////////////////////////////////////////////////////////////////////
	public void Add(string InIdentifier, System.Type InType)
	{
		m_MappedTypes.FindOrAdd(InIdentifier, () => new List<System.Type>()).AddUnique(InType);
	}

	/////////////////////////////////////////////////////////////////////////////
	public void AddRange(string InIdentifier, System.Type[] InTypes)
	{
		var list = m_MappedTypes.FindOrAdd(InIdentifier, () => new List<System.Type>());
		foreach (System.Type item in InTypes)
		{
			list.AddUnique(item);
		}
	}

	/////////////////////////////////////////////////////////////////////////////
	public System.Type[] Get(string InIdentifier)
	{
		System.Type[] OutValue = new System.Type[0];
		if (m_MappedTypes.TryGetValue(InIdentifier, out var list))
		{
			OutValue = list.ToArray();
		}
		return OutValue;
	}

	/////////////////////////////////////////////////////////////////////////////
	public void Remove(string InIdentifier, System.Type InType)
	{
		if (m_MappedTypes.TryGetValue(InIdentifier, out var list))
		{
			list.Remove(InType);
			if (list.Count == 0)
			{
				m_MappedTypes.Remove(InIdentifier);
			}
		}
	}


	#region SERIALIZATION
	[System.Serializable]
	private class TypeIdentifier
	{
		[ReadOnly]
		public string TypeFullName = string.Empty;
		[ReadOnly]
		public string AssemblyName = string.Empty;

		public TypeIdentifier(System.Type InType)
		{
			TypeFullName = InType.FullName;
			AssemblyName = InType.Assembly.FullName;
		}
	}

	[System.Serializable]
	private class DataHolder
	{
		[ReadOnly]
		public string Identifier = string.Empty;
		[ReadOnly]
		public TypeIdentifier[] Types = new TypeIdentifier[0];
	}

	// A list that can be serialized
	[SerializeField]
	private List<DataHolder> m_Serializable = new List<DataHolder>();


	// Save
	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_Serializable.Clear();
		foreach (KeyValuePair<string, List<System.Type>> pair in m_MappedTypes)
		{
			m_Serializable.Add(new DataHolder()
			{
				Identifier = pair.Key,
				Types = pair.Value.Select(t => new TypeIdentifier(t)).ToArray()
			});
		}
	}

	// Load
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_MappedTypes.Clear();
		foreach (DataHolder dataHolder in m_Serializable)
		{
			m_MappedTypes.FindOrAdd(dataHolder.Identifier, () => new List<System.Type>()).AddRange
			(
				dataHolder.Types.Select(typeIdentifier => System.Type.GetType(($"{typeIdentifier.TypeFullName}, {typeIdentifier.AssemblyName}"))).Where(t => t.IsNotNull())
			);
		}
	}

	#endregion
}