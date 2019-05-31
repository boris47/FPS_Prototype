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

	private	bool			m_bIsInitialized				= true;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
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



			Vector3 previousPanelPosition = m_MainPanel.position;
			gameObject.SetActive(true);
			m_MainPanel.position = Vector3.down * 2000f;

			StartCoroutine( Procedur2( previousPanelPosition ) );

		}

		if ( m_bIsInitialized )
		{

		}
		else
		{
			Debug.LogError( "UI_Inventory: Bad initialization!!!" );
		}
		return m_bIsInitialized;
	}
	

	public	IEnumerator Procedur2( Vector3 previousPanelPosition )
	{
		yield return new WaitForSecondsRealtime(1f);

		// LOAD PREFAB
		ResourceManager.LoadData<UI_InventorySlot> loadData = new ResourceManager.LoadData<UI_InventorySlot>();
		if ( m_bIsInitialized &= ResourceManager.LoadResourceSync( "Prefabs/UI/UI_InventorySlot", loadData ) )
		{
			m_UI_MatrixSlots = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

			Canvas canvas = GetComponent<Canvas>();
//			print( "canvas.scaleFactor: " + canvas.scaleFactor );

			float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
//			print( "My scale factor: " + scaleFactor );			

			float ratio = (float)Screen.width / (float)Screen.height;
			print(ratio);
			m_GridLayoutGroup.padding			= new RectOffset( left: m_HorizzontalPadding/2, right: m_HorizzontalPadding/2, top: m_VerticalPadding/2, bottom: m_VerticalPadding/2 );
			m_GridLayoutGroup.cellSize			= new Vector2( m_CellSizeX, m_CellSizeY / ratio ) * scaleFactor;
			m_GridLayoutGroup.spacing			= new Vector2( m_HSpaceBetweenSlots, m_VSpaceBetweenSlots / ratio ) * scaleFactor;
			m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
			m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
			m_GridLayoutGroup.constraintCount	= m_CellCountHorizontal;

			UI_InventorySlot inventorySlotPrefab = loadData.Asset;

			Vector2 instancedInventorySlotAnchoredPosition = Vector2.zero;
			for ( int i = 0; i < m_CellCountVertical; i++ )
			{
				for ( int j = 0; j < m_CellCountHorizontal; j++ )
				{
					UI_InventorySlot instancedInventorySlot = Instantiate<UI_InventorySlot>( inventorySlotPrefab );
					instancedInventorySlot.transform.SetParent( m_InventorySlots, worldPositionStays: false );

					IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlot;
					slot.Initialize();
				}
			}
		}

		gameObject.SetActive(false);
		m_MainPanel.position = previousPanelPosition;
	}

	public	IEnumerator Procedur()
	{
		yield return null;
		ResourceManager.LoadData<GameObject> loadData = new ResourceManager.LoadData<GameObject>();
		if ( m_bIsInitialized &= ResourceManager.LoadResourceSync( "Prefabs/UI/UI_InventorySlot", loadData ) )
		{
			m_UI_MatrixSlots = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

			GameObject inventorySlotGO = loadData.Asset;
			RectTransform inventorySlotRectTransform = ( inventorySlotGO.transform as RectTransform );
			Canvas canvas = GetComponent<Canvas>();
			print( "canvas.scaleFactor: " + canvas.scaleFactor );

//			Vector2 leftTopCorner		= Vector2.zero,	rightTopCorner		= Vector2.zero,
//					leftBottomCorner	= Vector2.zero,	rightBottomCorner	= Vector2.zero;

			// Define he starting point
			Vector2 startPoint = Vector2.zero;
			{
				RectTransform m_HelperRectTransform = new GameObject("RectTransformHelper").AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );
				m_HelperRectTransform.anchorMin = Vector2.zero;
				m_HelperRectTransform.anchorMax = Vector2.zero;
				startPoint = m_HelperRectTransform.anchoredPosition;
				Destroy( m_HelperRectTransform.gameObject );
			}

			/*
			// Define he starting point
			{
				RectTransform m_HelperRectTransform = new GameObject( "RectTransformHelper" ).AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );
				yield return null;
				// LEFT BOTTOM
				m_HelperRectTransform.anchorMin = Vector2.zero;				m_HelperRectTransform.anchorMax = Vector2.zero;
				leftBottomCorner = m_HelperRectTransform.position;
				new GameObject( "leftBottomCorner" ).transform.position		= leftBottomCorner;
				yield return null;
				// LEFT TOP
				m_HelperRectTransform.anchorMin = Vector2.up;				m_HelperRectTransform.anchorMax = Vector2.up;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
				leftTopCorner = m_HelperRectTransform.position;
				new GameObject( "leftTopCorner" ).transform.position		= leftTopCorner;
				yield return null;
				// RIGHT TOP
				m_HelperRectTransform.anchorMin = Vector2.one;				m_HelperRectTransform.anchorMax = Vector2.one;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
				rightTopCorner = m_HelperRectTransform.position;
				new GameObject( "rightTopCorner" ).transform.position		= rightTopCorner;
				yield return null;
				// RIGHT BOTTOM
				m_HelperRectTransform.anchorMin = Vector2.right;			m_HelperRectTransform.anchorMax = Vector2.right;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
				rightBottomCorner = m_HelperRectTransform.position;
				new GameObject( "rightBottomCorner" ).transform.position	= rightBottomCorner;
				yield return null;
				Destroy( m_HelperRectTransform.gameObject );
			}
			*/

			float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
			print( "My scale factor: " + scaleFactor );
			
			float step_X = ( inventorySlotRectTransform.rect.width  + m_HSpaceBetweenSlots ) * scaleFactor;
			float step_Y = ( inventorySlotRectTransform.rect.height + m_VSpaceBetweenSlots ) * scaleFactor;

			float currentPositionX = startPoint.x;
			float currentPositionY = startPoint.y;

			Vector2 instancedInventorySlotAnchoredPosition = Vector2.zero;
			for ( int i = 0; i < m_CellCountVertical; i++ )
			{
				for ( int j = 0; j < m_CellCountHorizontal; j++ )
				{
					RectTransform instancedInventorySlotRectTransform = Instantiate<RectTransform>( inventorySlotRectTransform );
					instancedInventorySlotRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );

					// Size of the intanced inventory slot
					
					Vector3 instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;
						instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, instancedInventorySlotSize.x * scaleFactor );
						instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   instancedInventorySlotSize.y * scaleFactor );
					instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;

					instancedInventorySlotRectTransform.anchorMin = Vector2.zero;
					instancedInventorySlotRectTransform.anchorMax = Vector2.zero;
					instancedInventorySlotAnchoredPosition.Set
					(
						currentPositionX + instancedInventorySlotSize.x * 0.5f,
						currentPositionY + instancedInventorySlotSize.y * 0.5f
					);
					instancedInventorySlotRectTransform.anchoredPosition = instancedInventorySlotAnchoredPosition;

					IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlotRectTransform.GetComponent<UI_InventorySlot>();
					slot.Initialize();
					
					currentPositionX += step_X;
				}

				currentPositionX = startPoint.x;
				currentPositionY += step_Y;
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
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

		CameraControl.Instance.CanParseInput	= false;
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
