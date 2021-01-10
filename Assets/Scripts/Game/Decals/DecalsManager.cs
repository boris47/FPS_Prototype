using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DecalsManager : InGameSingleton<DecalsManager>
{
	[SerializeField]
	private float DecalLifeTime = 10f;

	private GameObjectsPool<Decal> m_DecalsPool = null;

	private GameObject decalPrefab = null;

	//////////////////////////////////////////////////////////////////////////
	protected override void OnInitialize()
	{
		base.OnInitialize();

		ResourceManager.AsyncLoadedData<GameObject> loadedResource = new ResourceManager.AsyncLoadedData<GameObject>();
		ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Prefabs/UI/UI_CommandRow",
			loadedResource:			loadedResource,
			OnResourceLoaded:		(a) => decalPrefab = a,
			OnFailure:				(resPath) => Debug.LogError($"DecalsManager::OnInitialize: Cannot load {resPath}")
		);

		GameObjectsPoolConstructorData<Decal> options = new GameObjectsPoolConstructorData<Decal>(model: decalPrefab, size: 200)
		{
			ContainerName = "DecalsContainer",
			IsAsyncBuild = true,
		};
		m_DecalsPool = new GameObjectsPool<Decal>(options);

	}

	//////////////////////////////////////////////////////////////////////////
	public void PlaceDecal(Collider collider, Vector3 WorldPosition, Vector3 normal)
	{
		if (SurfaceManager.Instance.TryGetDecal(out Material decalMaterial, collider, new Ray(WorldPosition, -normal)))
		{
			Decal decal = m_DecalsPool.GetNextComponent();
			{
				decal.SetDecal(decalMaterial);
				decal.Show(WorldPosition, Quaternion.Euler(normal), DecalLifeTime);
			}
		}
		else
		{
			Debug.LogWarning( $"DecalsManager::PlaceDecal: Cannot find decal for collider {collider.name}" );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
