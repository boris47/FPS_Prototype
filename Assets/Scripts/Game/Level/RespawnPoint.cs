
using UnityEngine;
using System.Collections;

public	interface	IRespawn
{
	void	OnRespawn();
}

public class RespawnPoint : MonoBehaviour {

	private	float	m_Counter	= 0f;


	//////////////////////////////////////////////////////////////////////////
	// Respawn
	public	void	Respawn( IRespawn entity, float time )
	{
		CoroutinesManager.Start( RespawnCO( entity, time ), "RespawnPoint::Respawn" );
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
