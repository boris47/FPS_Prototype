using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class PersistentObject : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
