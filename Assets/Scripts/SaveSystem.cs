using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

public static class SaveSystem
{
	[System.Serializable]
	private class ComponentInfo
	{
		public string assemblyQualifiedName = string.Empty;
		public string serializedProps = string.Empty;
		public string serializedFields = string.Empty;

		public ComponentInfo(string assemblyQualifiedName, string serializedProps, string serializedFields)
		{
			this.assemblyQualifiedName = assemblyQualifiedName;
			this.serializedProps = serializedProps;
			this.serializedFields = serializedFields;
		}
	}


	[System.Serializable]
	private class GOToJSON
	{
		public string name = string.Empty;
		public string tag = string.Empty;
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
		public Vector3 scale = Vector3.zero;
		public int layer = 0;
		public bool activeSelf = false;

		public string serializedComponents = string.Empty;
		public string serializedChildren = string.Empty;

		public GOToJSON(GameObject go)
		{
			this.name = go.name;
			this.tag = go.tag;
			this.layer = go.layer;
			this.activeSelf = go.activeSelf;
			this.position = go.transform.localPosition;
			this.rotation = go.transform.localRotation;
			this.scale = go.transform.localScale;

			// Components
			{
				List<ComponentInfo> components = new List<ComponentInfo>();
				// Components
				foreach (Component component in go.GetComponents<Component>())
				{
					string assemblyQualifiedName = component.GetType().AssemblyQualifiedName;

					Dictionary<string, object> propertiesInfo = new Dictionary<string, object>();
					Dictionary<string, object> fieldsInfo = new Dictionary<string, object>();
					Utils.Base.GetComponentFieldsAndPropertiesInfo(component, propertiesInfo, fieldsInfo);
					if (Utils.Base.SerializeObject(propertiesInfo, out string serializedProps) && Utils.Base.SerializeObject(fieldsInfo, out string serializedFields))
					{
						components.Add(new ComponentInfo(assemblyQualifiedName, serializedProps, serializedFields));
					}
					else
					{
						System.Diagnostics.Debugger.Break();
					}
				}

				serializedComponents = JsonUtility.ToJson(new ToJsonWrapper<ComponentInfo[]>(components.ToArray()));
			}

			// Children
			{
				GOToJSON[] children = go.transform.GetComponentsOnlyInChildren<Transform>(false, true).Select(el => new GOToJSON(el.gameObject)).ToArray();
				serializedChildren = JsonUtility.ToJson(new ToJsonWrapper<GOToJSON[]>(children));
			}
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="go"></param>
	/// <returns></returns>
	public static string GameObjectToJSON(UnityEngine.GameObject go)
	{
		return JsonUtility.ToJson(new GOToJSON(go));
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static GameObject JSONToGameObject(string source)
	{
		GOToJSON rootData = JsonUtility.FromJson<GOToJSON>(source);

		GameObject root = new GameObject(rootData.name)
		{
			tag = rootData.tag,
			layer = rootData.layer
		};
		root.SetActive(false);
		root.transform.localPosition = rootData.position;
		root.transform.localRotation = rootData.rotation;
		root.transform.localScale = rootData.scale;
		{
			void SetupGameObject(GameObject parent, ComponentInfo[] components, GOToJSON[] children)
			{
				foreach (ComponentInfo componentData in components)
				{
					string type = componentData.assemblyQualifiedName;
					System.Type objectType = System.Type.GetType(type);
					if (objectType.IsNotNull())
					{
						if (Utils.Base.DeserializeObject(componentData.serializedProps, out Dictionary<string, object> propertiesInfo) &&
							Utils.Base.DeserializeObject(componentData.serializedFields, out Dictionary<string, object> fieldsInfo))
						{
							Component newComponent = objectType == typeof(Transform) ? parent.transform : parent.AddComponent(objectType);
							Utils.Base.SetComponentFieldsAndPropertiesInfo(newComponent, propertiesInfo, fieldsInfo);
						}
					}
					else
					{
						Debug.LogError($"Cannot add component {type} at gameObject {parent.name}");
					}
				}

				foreach (GOToJSON childData in children)
				{
					GameObject childGO = new GameObject(childData.name)
					{
						tag = childData.tag,
						layer = childData.layer
					};
					childGO.SetActive(childData.activeSelf);
					childGO.transform.SetParent(parent.transform);
					childGO.transform.localPosition = childData.position;
					childGO.transform.localRotation = childData.rotation;
					childGO.transform.localScale = childData.scale;

					SetupGameObject(
						childGO,
						JsonUtility.FromJson<ToJsonWrapper<ComponentInfo[]>>(childData.serializedComponents).content,
						JsonUtility.FromJson<ToJsonWrapper<GOToJSON[]>>(childData.serializedChildren).content
					);
				}
			}
			SetupGameObject(
				root,
				JsonUtility.FromJson<ToJsonWrapper<ComponentInfo[]>>(rootData.serializedComponents).content,
				JsonUtility.FromJson<ToJsonWrapper<GOToJSON[]>>(rootData.serializedChildren).content
			);
		}
		// Let awake and all callbacks to be called
		root.SetActive(rootData.activeSelf);
		return root;
	}
}
