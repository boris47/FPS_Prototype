
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

		void StartPoolCreation(GameObject prefab)
		{
			var options = new GameObjectsPoolConstructorData<Decal>(model: prefab, size: 200)
			{
				ContainerName = "DecalsContainer",
				IsAsyncBuild = true,
			};
			m_DecalsPool = new GameObjectsPool<Decal>(options);
		}
		ResourceManager.LoadResourceAsync<GameObject>
		(
			resourcePath:			"Prefabs/UI/UI_CommandRow",
			onResourceLoaded:		StartPoolCreation,
			onFailure:				resPath => Debug.LogError($"DecalsManager::OnInitialize: Cannot load {resPath}")
		);
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
