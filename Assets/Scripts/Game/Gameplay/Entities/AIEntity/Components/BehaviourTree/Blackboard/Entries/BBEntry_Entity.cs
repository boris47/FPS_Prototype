using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public class BBEntry_Entity : BlackboardEntryKeyValue<Entity>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.ObjectField(Value, StoredType, false);
		}
#endif
	}
}

