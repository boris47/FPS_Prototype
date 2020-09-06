
using UnityEngine;
using System.Collections;

public	interface	IRespawn
{
	void	OnRespawn();
}

public class RespawnPoint : MonoBehaviour {

	//////////////////////////////////////////////////////////////////////////
	// Respawn
	public	void	Respawn( IRespawn entity, float time )
	{
		CoroutinesManager.Start(this.RespawnCO( entity, time ), "RespawnPoint::Respawn" );
	}


	//////////////////////////////////////////////////////////////////////////
	// RespawnCO ( Coroutine )
	private	IEnumerator	RespawnCO( IRespawn entity, float time )
	{
		yield return new WaitForSeconds(time * 1000f);
		entity.OnRespawn();
	}


}
