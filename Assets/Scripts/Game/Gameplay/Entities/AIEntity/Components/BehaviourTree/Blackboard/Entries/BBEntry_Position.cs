using System.Collections;
using UnityEngine;

namespace Entities.AI
{
	[System.Serializable]
	public class BBEntry_Position : BlackboardEntryKeyValue<Vector3>
	{
#if UNITY_EDITOR
		protected override void OnGUIlayout()
		{
			UnityEditor.EditorGUILayout.Vector3Field(string.Empty, Value);
		}
#endif
	}
}

