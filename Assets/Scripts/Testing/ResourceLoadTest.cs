using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeatherSystem;

public class ResourceLoadTest : MonoBehaviour {

	[SerializeField]
	ResourceManager.LoadData<Weathers> weathers = new ResourceManager.LoadData<Weathers>();

	private IEnumerator Start()
	{
		yield return new WaitForSeconds( 2f );

		print( "Start()" );
		


		
		ResourceManager.LoadResourceSync(
			WeatherManager.WEATHERS_COLLECTION,
			weathers			
	//		,delegate( Weathers w ) { print( "loaded" ); }
		);
		print( "Start() End" );
	}

	private void OnDestroy()
	{
		
	}

}
