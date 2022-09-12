
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

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
			// Save inof into micro containers
			TypeIdentifier[] types = pair.Value.Select(t => new TypeIdentifier(t)).ToArray();

			// Put them in macro container and serializable structure
			m_Serializable.Add(new DataHolder()
			{
				Identifier = pair.Key,
				Types = types
			});
		}
	//	m_MappedTypes.Clear();
	}

	// Load
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_MappedTypes.Clear();
		foreach (DataHolder dataHolder in m_Serializable)
		{
			// Emplace keyValue and create list
			List<System.Type> list = m_MappedTypes.FindOrAdd(dataHolder.Identifier, () => new List<System.Type>());

			// Restore valid types
			foreach (TypeIdentifier typeIdentifier in dataHolder.Types)
			{
				if (typeIdentifier.TryGetType(out System.Type outType))
				{
					list.Add(outType);
				}
			}
		}
		m_Serializable.Clear();
	}
	#endregion
}