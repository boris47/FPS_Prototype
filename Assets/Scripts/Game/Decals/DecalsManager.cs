
using UnityEngine;


public class DecalsManager : OnDemandSingleton<DecalsManager>
{
	[SerializeField]
	private			float							m_DecalLifeTime			= 10f;
	private			GameObjectsPool<Decal>			m_DecalsPool			= null;


	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		base.Awake();

		void ActionOnDecal(Decal decal)
		{
			decal.gameObject.SetActive(false);
		}

		void StartPoolCreation(GameObject prefab)
		{
			var options = new GameObjectsPoolConstructorData<Decal>(model: prefab, size: 200)
			{
				ContainerName = "DecalsContainer",
				IsAsyncBuild = true,
				ActionOnObject = ActionOnDecal
			};
			m_DecalsPool = new GameObjectsPool<Decal>(options);
		}
		ResourceManager.LoadResourceAsync<GameObject>
		(
			resourcePath:			"Prefabs/Decal",
			onResourceLoaded:		StartPoolCreation,
			onFailure:				resPath => Debug.LogError($"DecalsManager::OnInitialize: Cannot load {resPath}")
		);
	}


	//////////////////////////////////////////////////////////////////////////
	protected override void OnDestroy()
	{
		base.OnDestroy();

		m_DecalsPool.Destroy();
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
