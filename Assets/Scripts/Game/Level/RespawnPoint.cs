
using UnityEngine;
using System.Collections;

public class RespawnPoint : MonoBehaviour {

	private	float	m_Counter	= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Respawn
	public	void	Respawn( IRespawn entity, float time )
	{
		StartCoroutine( RespawnCO( entity, time ) );
	}


	//////////////////////////////////////////////////////////////////////////
	// RespawnCO ( Coroutine )
	private	IEnumerator	RespawnCO( IRespawn entity, float time )
	{
		while ( m_Counter < time )
		{
			m_Counter += Time.deltaTime;
			yield return null;
		}

		m_Counter = 0f;
		entity.OnRespawn();
	}


}
