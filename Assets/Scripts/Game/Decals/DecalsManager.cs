
using UnityEngine;


public class DecalsManager : OnDemandSingleton<DecalsManager>
{
	[SerializeField]
	private			float							m_DecalLifeTime			= 10f;
	private			GameObjectsPool<Decal>			m_DecalsPool			= null;

	private			GameObject						m_DecalPrefab			= null;


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();
		ResourceManager.AsyncLoadedData<GameObject> loadedResource = new ResourceManager.AsyncLoadedData<GameObject>();
		ResourceManager.LoadResourceAsyncCoroutine
		(
			ResourcePath:			"Prefabs/UI/UI_CommandRow",
			loadedResource:			loadedResource,
			OnResourceLoaded:		(a) => m_DecalPrefab = a,
			OnFailure:				(resPath) => Debug.LogError($"DecalsManager::OnInitialize: Cannot load {resPath}")
		);

		var options = new GameObjectsPoolConstructorData<Decal>(model: m_DecalPrefab, size: 200)
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
				decal.SetDecalMaterial(decalMaterial);
				decal.Show(WorldPosition, Quaternion.Euler(normal), m_DecalLifeTime);
			}
		}
		else
		{
			Debug.LogWarning( $"DecalsManager::PlaceDecal: Cannot find decal for collider {collider.name}" );
		}
	}
}
