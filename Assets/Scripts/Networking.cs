using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;

public class Networking : MonoBehaviour {


	// Use this for initialization
	void Start()
	{
		NetworkServer.Listen( 7070 );

		Debug.Log("Registering server callbacks");
		NetworkServer.RegisterHandler( MsgType.Connect,			OnConnect	);
		NetworkServer.RegisterHandler( MsgType.ObjectSpawn,		OnSpawn		);
	}


	private void OnConnect( NetworkMessage netMsg )
	{
		
	}


	private void OnSpawn( NetworkMessage netMsg )
	{
		GameObject obj = netMsg.reader.ReadGameObject();
	}

	/*
	// Update is called once per frame
	void Update()
	{

	}
	*/

}
