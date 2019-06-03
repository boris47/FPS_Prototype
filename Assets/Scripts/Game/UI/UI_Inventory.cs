using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixSlots		= null;

	private		GridLayoutGroup		m_GridLayoutGroup		= null;

	private		float				m_CellSizeX				= 75f;
	private		float				m_CellSizeY				= 75f;
	private		int					m_CellCountHorizontal	= 3;
	private		int					m_CellCountVertical		= 3;
	private		int					m_HorizzontalPadding	= 100;
	private		int					m_VerticalPadding		= 100;

	private		float				m_HSpaceBetweenSlots	= 20f;
	private		float				m_VSpaceBetweenSlots	= 20f;

	private	bool			m_bIsInitialized				= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{
			m_MainPanel = ( transform.Find( "MainPanel" ) as RectTransform );
			m_bIsInitialized &= m_MainPanel != null;

			if ( m_bIsInitialized == true )
			{
				m_InventorySlots = ( m_MainPanel.Find("InventorySlots") as RectTransform );
				m_bIsInitialized &= m_InventorySlots != null;
			}
			
			// LOAD SECTION
			Database.Section uiSection = null;
			if ( m_bIsInitialized &= GameManager.Configs.bGetSection( "UI_Inventory", ref uiSection ) )
			{
				m_CellSizeX				= Mathf.Max( uiSection.AsFloat( "CellSizeX",			m_CellSizeX				), m_CellSizeX				);
				m_CellSizeY				= Mathf.Max( uiSection.AsFloat( "CellSizeY",			m_CellSizeY				), m_CellSizeY				);
				m_CellCountHorizontal	= Mathf.Max( uiSection.AsInt(   "CellCountHorizontal",	m_CellCountHorizontal	), m_CellCountHorizontal	);
				m_CellCountVertical		= Mathf.Max( uiSection.AsInt(   "CellCountVertical",	m_CellCountVertical		), m_CellCountVertical		);
				m_HorizzontalPadding	= Mathf.Max( uiSection.AsInt(	"HorizzontalPadding",	m_HorizzontalPadding	), m_HorizzontalPadding		);
				m_VerticalPadding		= Mathf.Max( uiSection.AsInt(	"VerticalPadding",		m_VerticalPadding		), m_VerticalPadding		);
				m_HSpaceBetweenSlots	= Mathf.Max( uiSection.AsFloat( "HSpaceBetweenSlots",	m_HSpaceBetweenSlots	), m_HSpaceBetweenSlots		);
				m_VSpaceBetweenSlots	= Mathf.Max( uiSection.AsFloat( "VSpaceBetweenSlots",	m_VSpaceBetweenSlots	), m_VSpaceBetweenSlots		);
			}
			m_bIsInitialized &= m_InventorySlots.SearchComponent( ref m_GridLayoutGroup, SearchContext.LOCAL );
			
			// Trick where panel is set very on the bottom of ui and allowed to do onFrame stuf in coroutine, then gameobject is deactivated
			Vector3 previousCanvasPosition = transform.position;
			gameObject.SetActive(true);
			transform.position = Vector3.down * 2000f;

			
			// LOAD PREFAB
			ResourceManager.LoadData<GameObject> loadData = new ResourceManager.LoadData<GameObject>();
			yield return ResourceManager.LoadResourceAsyncCoroutine( "Prefabs/UI/UI_InventorySlot", loadData, null );

			if ( m_bIsInitialized &= loadData.Asset != null )
			{
				m_UI_MatrixSlots = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

				Canvas canvas = GetComponent<Canvas>();
//				print( "canvas.scaleFactor: " + canvas.scaleFactor );

				float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
//				print( "My scale factor: " + scaleFactor );			

				float ratio = (float)Screen.width / (float)Screen.height;
//				print(ratio);
				m_GridLayoutGroup.padding			= new RectOffset( left: m_HorizzontalPadding/2, right: m_HorizzontalPadding/2, top: m_VerticalPadding/2, bottom: m_VerticalPadding/2 );
				m_GridLayoutGroup.cellSize			= new Vector2( m_CellSizeX, m_CellSizeY ) * scaleFactor;
				m_GridLayoutGroup.spacing			= new Vector2( m_HSpaceBetweenSlots, m_VSpaceBetweenSlots ) * scaleFactor / ratio;
				m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
				m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
				m_GridLayoutGroup.constraintCount	= m_CellCountHorizontal;

				GameObject inventorySlotPrefab = loadData.Asset;
				
				Vector2 instancedInventorySlotAnchoredPosition = Vector2.zero;
				for ( int i = 0; i < m_CellCountVertical; i++ )
				{
					for ( int j = 0; j < m_CellCountHorizontal; j++ )
					{
						GameObject instancedInventorySlot = Instantiate( inventorySlotPrefab );
						instancedInventorySlot.transform.SetParent( m_InventorySlots, worldPositionStays: false );
						( instancedInventorySlot.transform as RectTransform ).anchorMin = Vector2.zero;
						( instancedInventorySlot.transform as RectTransform ).anchorMax = Vector2.one;

						IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlot.GetComponent<UI_InventorySlot>();
						yield return StartCoroutine( slot.Initialize() );
					}
				}
			}

			gameObject.SetActive(false);
			transform.position = previousCanvasPosition;
		}

		if ( m_bIsInitialized )
		{

		}
		else
		{
			Debug.LogError( "UI_Inventory: Bad initialization!!!" );
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator	IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public	bool	AddItem( Database.Section itemSection, Texture2D itemIcon )
	{
		Vector2 position = Vector2.zero;
		UI_InventorySlot matrixSlot = null;
		bool bAllAttempDone = m_UI_MatrixSlots.FindByPredicate( ( UI_InventorySlot i ) => i.IsSet == false, ref matrixSlot, ref position );
		if ( bAllAttempDone )
		{
			bAllAttempDone &= matrixSlot.TrySet( itemIcon, itemSection );
		}
		
		return bAllAttempDone;
	}

	


	//////////////////////////////////////////////////////////////////////////
	public	bool	RemoveItem( string itemName )
	{
		Vector2 position = Vector2.zero;
		UI_InventorySlot matrixSlot = null;
		bool bHasBeenFound = m_UI_MatrixSlots.FindByPredicate( ( UI_InventorySlot i ) => i.Section.GetName() == itemName, ref matrixSlot, ref position );
		if ( bHasBeenFound )
		{
			matrixSlot.Reset();
		}
		return bHasBeenFound;
	}

	
	private void OnEnable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		if ( CameraControl.Instance.IsNotNull() )
		{
			CameraControl.Instance.CanParseInput	= false;
		}

		InputManager.IsEnabled					= false;

		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void OnDisable()
	{
		if ( CameraControl.Instance != null )
		{
			CameraControl.Instance.CanParseInput	= true;
		}
		InputManager.IsEnabled					= true;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

}
