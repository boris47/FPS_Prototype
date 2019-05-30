using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixSlots		= null;

	private		int					m_CellCountHorizontal	= 3;
	private		int					m_CellCountVertical		= 3;
	private		float				m_HorizzontalPadding	= 100f;
	private		float				m_VerticalPadding		= 100f;

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
				m_CellCountHorizontal	= Mathf.Max( uiSection.AsInt(   "CellCountHorizontal",	m_CellCountHorizontal	), m_CellCountHorizontal	);
				m_CellCountVertical		= Mathf.Max( uiSection.AsInt(   "CellCountVertical",	m_CellCountVertical		), m_CellCountVertical		);
				m_HorizzontalPadding	= Mathf.Max( uiSection.AsFloat( "HorizzontalPadding",	m_HorizzontalPadding	), m_HorizzontalPadding		);
				m_VerticalPadding		= Mathf.Max( uiSection.AsFloat( "VerticalPadding",		m_VerticalPadding		), m_VerticalPadding		);
				m_HSpaceBetweenSlots	= Mathf.Max( uiSection.AsFloat( "HSpaceBetweenSlots",	m_HSpaceBetweenSlots	), m_HSpaceBetweenSlots		);
				m_VSpaceBetweenSlots	= Mathf.Max( uiSection.AsFloat( "VSpaceBetweenSlots",	m_VSpaceBetweenSlots	), m_VSpaceBetweenSlots		);
			}
			/*
			// LOAD PREFAB
			ResourceManager.LoadData<GameObject> loadData = new ResourceManager.LoadData<GameObject>();
			if ( m_bIsInitialized &= ResourceManager.LoadResourceSync( "Prefabs/UI/UI_InventorySlot", loadData ) )
			{
				m_UI_MatrixSlots = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

				GameObject inventorySlotGO = loadData.Asset;
				RectTransform inventorySlotRectTransform = ( inventorySlotGO.transform as RectTransform );
				Canvas canvas = GetComponent<Canvas>();
		//		float ratio = Screen.width / Screen.height;
		//		print( ratio );
				print( canvas.scaleFactor );

				Vector2 leftTopCorner		= Vector2.zero,	rightTopCorner		= Vector2.zero,
						leftBottomCorner	= Vector2.zero,	rightBottomCorner	= Vector2.zero;


				// Define he starting point
				Vector2 startPoint = Vector2.zero;
				{
					RectTransform m_HelperRectTransform = new GameObject( "RectTransformHelper" ).AddComponent<RectTransform>();
					m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );

					// LEFT BOTTOM
					m_HelperRectTransform.anchorMin = Vector2.zero;
					m_HelperRectTransform.anchorMax = Vector2.zero;
					leftBottomCorner = m_HelperRectTransform.position;
					new GameObject( "leftBottomCorner" ).transform.position = leftBottomCorner;

					// LEFT TOP
					m_HelperRectTransform.anchorMin = Vector2.zero;
					m_HelperRectTransform.anchorMax = Vector2.up;
					m_HelperRectTransform.anchoredPosition = Vector2.zero;
					leftTopCorner = m_HelperRectTransform.position;
					new GameObject( "leftTopCorner" ).transform.position = leftTopCorner;

					// RIGHT TOP
					m_HelperRectTransform.anchorMin = Vector2.one;
					m_HelperRectTransform.anchorMax = Vector2.one;
					m_HelperRectTransform.anchoredPosition = Vector2.zero;
					rightTopCorner = m_HelperRectTransform.position;
					new GameObject( "rightTopCorner" ).transform.position = rightTopCorner;

					// RIGHT BOTTOM
					m_HelperRectTransform.anchorMin = Vector2.one;
					m_HelperRectTransform.anchorMax = Vector2.right;
					m_HelperRectTransform.anchoredPosition = Vector2.zero;
					rightBottomCorner = m_HelperRectTransform.position;
					new GameObject( "rightBottomCorner" ).transform.position = rightBottomCorner;

					Destroy( m_HelperRectTransform.gameObject );
				}
			
				float step_X = ( inventorySlotRectTransform.rect.width  + m_HSpaceBetweenSlots ) / canvas.scaleFactor;
				float step_Y = ( inventorySlotRectTransform.rect.height + m_VSpaceBetweenSlots ) / canvas.scaleFactor;

				float currentPositionX = leftBottomCorner.x;
				float currentPositionY = leftBottomCorner.y;

				Vector2 instancedInventorySlotAnchoredPosition = Vector2.zero;
				for ( int i = 0; i < m_CellCountVertical; i++ )
				{
					for ( int j = 0; j < m_CellCountHorizontal; j++ )
					{
						RectTransform instancedInventorySlotRectTransform = Instantiate<RectTransform>( inventorySlotRectTransform );
						instancedInventorySlotRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );
						instancedInventorySlotRectTransform.anchorMin = Vector2.zero;;
						instancedInventorySlotRectTransform.anchorMax = Vector2.zero;

						// Size of the intanced inventory slot
						Vector3 instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;
							instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, instancedInventorySlotSize.x / canvas.scaleFactor );
							instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   instancedInventorySlotSize.y / canvas.scaleFactor );
						instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;

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
			*/
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

	IEnumerator current;

	private void Update()
	{
		if ( Input.GetKeyDown( KeyCode.RightArrow ) )
		{
			if ( current == null )
			{
				current = Procedur();
			}
			else
			{
				current.MoveNext();
			}
		}
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
	//		float ratio = Screen.width / Screen.height;
	//		print( ratio );
			print( canvas.scaleFactor );

			Vector2 leftTopCorner		= Vector2.zero,	rightTopCorner		= Vector2.zero,
					leftBottomCorner	= Vector2.zero,	rightBottomCorner	= Vector2.zero;

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
			
			// Define he starting point
			{
				RectTransform m_HelperRectTransform = new GameObject( "RectTransformHelper" ).AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );
				yield return null;
				// LEFT BOTTOM
				m_HelperRectTransform.anchorMin = Vector2.zero;				m_HelperRectTransform.anchorMax = Vector2.zero;
										leftBottomCorner = m_HelperRectTransform.position;
				new GameObject( "leftBottomCorner" ).transform.position = leftBottomCorner;
				yield return null;
				// LEFT TOP
				m_HelperRectTransform.anchorMin = Vector2.up;				m_HelperRectTransform.anchorMax = Vector2.up;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
										leftTopCorner = m_HelperRectTransform.position;
				new GameObject( "leftTopCorner" ).transform.position = leftTopCorner;
				yield return null;
				// RIGHT TOP
				m_HelperRectTransform.anchorMin = Vector2.one;				m_HelperRectTransform.anchorMax = Vector2.one;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
										rightTopCorner = m_HelperRectTransform.position;
				new GameObject( "rightTopCorner" ).transform.position = rightTopCorner;
				yield return null;
				// RIGHT BOTTOM
				m_HelperRectTransform.anchorMin = Vector2.right;			m_HelperRectTransform.anchorMax = Vector2.right;
				m_HelperRectTransform.anchoredPosition = Vector2.zero;
										rightBottomCorner = m_HelperRectTransform.position;
				new GameObject( "rightBottomCorner" ).transform.position = rightBottomCorner;
				yield return null;
				Destroy( m_HelperRectTransform.gameObject );
			}
			
			float step_X = ( inventorySlotRectTransform.rect.width  + m_HSpaceBetweenSlots ) / canvas.scaleFactor;
			float step_Y = ( inventorySlotRectTransform.rect.height + m_VSpaceBetweenSlots ) / canvas.scaleFactor;

			float currentPositionX = startPoint.x;
			float currentPositionY = startPoint.y;

			Vector2 instancedInventorySlotAnchoredPosition = Vector2.zero;
			for ( int i = 0; i < m_CellCountVertical; i++ )
			{
				for ( int j = 0; j < m_CellCountHorizontal; j++ )
				{
					RectTransform instancedInventorySlotRectTransform = Instantiate<RectTransform>( inventorySlotRectTransform );
					instancedInventorySlotRectTransform.SetParent( m_InventorySlots, worldPositionStays: false );
					instancedInventorySlotRectTransform.anchorMin = Vector2.zero;;
					instancedInventorySlotRectTransform.anchorMax = Vector2.zero;

					// Size of the intanced inventory slot
					Vector3 instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;
						instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, instancedInventorySlotSize.x / canvas.scaleFactor );
						instancedInventorySlotRectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical,   instancedInventorySlotSize.y / canvas.scaleFactor );
					instancedInventorySlotSize = instancedInventorySlotRectTransform.rect.size;

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
