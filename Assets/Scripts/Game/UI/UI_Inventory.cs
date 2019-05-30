using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixItems		= null;

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
			ResourceManager.LoadData<GameObject> loadData = new ResourceManager.LoadData<GameObject>();
			if ( m_bIsInitialized &= ResourceManager.LoadResourceSync( "Prefabs/UI/UI_InventorySlot", loadData ) )
			{

				RectTransform m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
				m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: true );
				m_HelperRectTransform.anchorMin = Vector2.zero;
				m_HelperRectTransform.anchorMax = Vector2.zero;
//				m_HelperRectTransform.hideFlags = HideFlags.HideAndDontSave;


				m_UI_MatrixItems = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

				GameObject inventorySlotGO = loadData.Asset;
				RectTransform inventorySlotRectTransform = ( inventorySlotGO.transform as RectTransform );

				float currentPositionX = 0;
				float currentPositionY = 0;

				var canvas = GetComponent<Canvas>();

				for ( int i = 0; i < m_CellCountVertical; i++ )
				{
				//	currentPositionY -= m_InventorySlots.rect.size.y * 0.5f;

					for ( int j = 0; j < m_CellCountHorizontal; j++ )
					{
				//		currentPositionX -= m_InventorySlots.rect.size.x * 0.5f;

						GameObject instancedInventoryItem = Instantiate( inventorySlotGO, m_InventorySlots );
						Vector2 localPosition = new Vector2 ( currentPositionX, currentPositionY ) ;
						m_HelperRectTransform.anchoredPosition = localPosition;
						instancedInventoryItem.transform.position = m_HelperRectTransform.position;

						currentPositionX += ( inventorySlotRectTransform.rect.width + m_HSpaceBetweenSlots );
						IStateDefiner item = m_UI_MatrixItems[i, j] = inventorySlotGO.GetComponent<UI_InventorySlot>();
						item.Initialize();
					}

					currentPositionY += ( inventorySlotRectTransform.rect.height + m_VSpaceBetweenSlots );
					currentPositionX = 0;
				}

//				Destroy( m_HelperRectTransform.gameObject );
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

			var canvas = GetComponent<Canvas>();

			RectTransform m_HelperRectTransform = new GameObject("MinimapHelper").AddComponent<RectTransform>();
			m_HelperRectTransform.SetParent( m_InventorySlots, worldPositionStays: true );
			m_HelperRectTransform.anchorMin = Vector2.one * 0.5f;
			m_HelperRectTransform.anchorMax = Vector2.one * 0.5f;
//				m_HelperRectTransform.hideFlags = HideFlags.HideAndDontSave;


			m_UI_MatrixItems = new UI_InventorySlot[ m_CellCountHorizontal, m_CellCountVertical ];

			GameObject inventorySlotGO = loadData.Asset;
			RectTransform inventorySlotRectTransform = ( inventorySlotGO.transform as RectTransform );

	//		float ratio = Screen.width / Screen.height;
	//		print( ratio );
	//		print( canvas.scaleFactor );
			
			float currentPositionX = 0;
			float currentPositionY = 0;
			float step_X = ( inventorySlotRectTransform.rect.width / canvas.scaleFactor + m_HSpaceBetweenSlots / canvas.scaleFactor );
			float step_Y = ( inventorySlotRectTransform.rect.height / canvas.scaleFactor + m_VSpaceBetweenSlots / canvas.scaleFactor );

			for ( int i = 0; i < m_CellCountVertical; i++, currentPositionX = 0, currentPositionY +=step_Y )
			{
				for ( int j = 0; j < m_CellCountHorizontal; j++, currentPositionX += step_X )
				{
					GameObject instancedInventoryItem = Instantiate( inventorySlotGO, m_InventorySlots );
					RectTransform trasnf = instancedInventoryItem.transform as RectTransform;
					trasnf.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, trasnf.rect.width / canvas.scaleFactor );
					trasnf.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, trasnf.rect.height / canvas.scaleFactor );

					Vector2 localPosition = new Vector2 ( currentPositionX, currentPositionY ) ;
					instancedInventoryItem.transform.localPosition = localPosition;
					/*
					Vector2 localPosition = new Vector2 ( currentPositionX, currentPositionY ) ;

					m_HelperRectTransform.anchoredPosition = Vector2.zero;

					Vector2 delta = m_InventorySlots.position - m_HelperRectTransform.position;
					localPosition.x -= delta.x;
					localPosition.y -= delta.y;
					yield return null;
					m_HelperRectTransform.anchoredPosition = localPosition;
					instancedInventoryItem.transform.position = m_HelperRectTransform.position;

					IStateDefiner item = m_UI_MatrixItems[i, j] = inventorySlotGO.GetComponent<UI_InventorySlot>();
					item.Initialize();
					*/
					yield return null;
				}
			}
			yield return null;

//				Destroy( m_HelperRectTransform.gameObject );
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
		UI_InventorySlot matrixItem = null;
		bool bAllAttempDone = m_UI_MatrixItems.FindByPredicate( ( UI_InventorySlot i ) => i.IsSet == false, ref matrixItem, ref position );
		if ( bAllAttempDone )
		{
			bAllAttempDone &= matrixItem.TrySet( itemIcon, itemSection );
		}
		
		return bAllAttempDone;
	}

	


	//////////////////////////////////////////////////////////////////////////
	public	bool	RemoveItem( string itemName )
	{
		Vector2 position = Vector2.zero;
		UI_InventorySlot matrixItem = null;
		bool bHasBeenFound = m_UI_MatrixItems.FindByPredicate( ( UI_InventorySlot i ) => i.Section.GetName() == itemName, ref matrixItem, ref position );
		if ( bHasBeenFound )
		{
			matrixItem.Reset();
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
