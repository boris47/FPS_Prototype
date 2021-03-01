using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SpawnPoint : MonoBehaviour
{
	public GameObject m_objectToSpawn = null;

	public void Spawn()
	{
		if (m_objectToSpawn)
		{
			Object.Instantiate(m_objectToSpawn, transform.position, transform.rotation);
		}
	}
}
