using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public sealed class UI_Inventory : UI_Base, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixSlots		= null;

	private		GridLayoutGroup		m_GridLayoutGroup		= null;


	private		Button				m_ReturnToGame			= null;
	private		Button				m_SwitchToWeaponCustomization		= null;

	private		bool				m_IsInitialized		= false;


	bool IStateDefiner.IsInitialized
	{
		get { return m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return name; }
	}

	[System.Serializable]
	private class InventorySectionData {
		public		float				CellSizeX			= 0;
		public		float				CellSizeY			= 0;
		public		int					CellCountHorizontal	= 0;
		public		int					CellCountVertical	= 0;
		public		int					HorizontalPadding	= 0;
		public		int					VerticalPadding		= 0;
		public		float				HSpaceBetweenSlots	= 0;
		public		float				VSpaceBetweenSlots	= 0;
	}
	[SerializeField, ReadOnly]
	private InventorySectionData	m_InventorySectionData	= new InventorySectionData();

	//////////////////////////////////////////////////////////////////////////
	public void PreInit() { }

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		m_IsInitialized = true;
		{
			m_MainPanel = (transform.Find( "MainPanel" ) as RectTransform );
			m_IsInitialized &= m_MainPanel != null;

			if (m_IsInitialized == true )
			{
				m_InventorySlots = (m_MainPanel.Find("InventorySlots") as RectTransform );
				m_IsInitialized &= m_InventorySlots != null;
			}

			yield return null;


			// LOAD SECTION
			m_IsInitialized &= GlobalManager.Configs.TryGetSection("UI_Inventory", out Database.Section inventorySection ) &&  GlobalManager.Configs.TrySectionToOuter(inventorySection, m_InventorySectionData );

			// Search grid component
			m_IsInitialized &= m_InventorySlots.TrySearchComponent(ESearchContext.LOCAL, out m_GridLayoutGroup );
			
			// LOAD PREFAB
			ResourceManager.AsyncLoadedData<GameObject> loadedResource = new ResourceManager.AsyncLoadedData<GameObject>();
			yield return ResourceManager.LoadResourceAsyncCoroutine( "Prefabs/UI/UI_InventorySlot", loadedResource, null );

			if (m_IsInitialized &= loadedResource.Asset != null )
			{
				m_UI_MatrixSlots = new UI_InventorySlot[m_InventorySectionData.CellCountHorizontal, m_InventorySectionData.CellCountVertical ];

				Canvas canvas = transform.parent.GetComponent<Canvas>();

				float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
//				print( "My scale factor: " + scaleFactor );			

				float ratio = (float)Screen.width / (float)Screen.height;
//				print(ratio);
				
				int halfHorizontalPadding = m_InventorySectionData.HorizontalPadding  / 2;
				int halfVerticalPadding   = m_InventorySectionData.VerticalPadding    / 2;
				RectOffset padding = new RectOffset
				( 
					left: halfHorizontalPadding,	right: halfHorizontalPadding, 
					top: halfVerticalPadding,		bottom: halfVerticalPadding 
				);
				m_GridLayoutGroup.padding			= padding;

				//	m_GridLayoutGroup.cellSize			= new Vector2( m_InventorySectionData.CellSizeX, m_InventorySectionData.CellSizeY ) * scaleFactor;
				m_GridLayoutGroup.spacing			= new Vector2(m_InventorySectionData.HSpaceBetweenSlots, m_InventorySectionData.VSpaceBetweenSlots ) * scaleFactor / ratio;
				m_GridLayoutGroup.cellSize = new Vector2
				(
					(m_InventorySlots.rect.width - m_GridLayoutGroup.spacing.x ) / m_InventorySectionData.CellCountVertical, 
					(m_InventorySlots.rect.height - m_GridLayoutGroup.spacing.y ) / m_InventorySectionData.CellCountHorizontal
				);
				m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
				m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
				m_GridLayoutGroup.constraintCount	= m_InventorySectionData.CellCountHorizontal;

				yield return null;

				UserSettings.VideoSettings.OnResolutionChanged += OnResolutionChange;

				GameObject inventorySlotPrefab = loadedResource.Asset;
				
				for ( int i = 0; i < m_InventorySectionData.CellCountVertical; i++ )
				{
					for ( int j = 0; j < m_InventorySectionData.CellCountHorizontal; j++ )
					{
						GameObject instancedInventorySlot = Instantiate( inventorySlotPrefab );
						instancedInventorySlot.transform.SetParent(m_InventorySlots, worldPositionStays: false );
						( instancedInventorySlot.transform as RectTransform ).anchorMin = Vector2.zero;
						( instancedInventorySlot.transform as RectTransform ).anchorMax = Vector2.one;

						IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlot.GetComponent<UI_InventorySlot>();
						CoroutinesManager.Start( slot.Initialize() );
					}

					yield return null;
				}
			}

			// SWITCH TO INVENTORY
			if (m_IsInitialized &= transform.TrySearchComponentByChildName( "SwitchToWeaponCustomization", out m_SwitchToWeaponCustomization ) )
			{
				m_SwitchToWeaponCustomization.onClick.AddListener
				(
					OnSwitchToWeaponCustomization
				);
			}

			yield return null;

			// RETURN TO GAME
			if (m_IsInitialized &= transform.TrySearchComponentByChildName( "ReturnToGame", out m_ReturnToGame ) )
			{
				m_ReturnToGame.onClick.AddListener
				(
					OnReturnToGame
				);
			}

			yield return null;
		}

		if (m_IsInitialized )
		{
			CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
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
		return m_IsInitialized;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public	bool	AddItem( Database.Section itemSection, Texture2D itemIcon )
	{
		Vector2 position = Vector2.zero;
		UI_InventorySlot matrixSlot = null;
		bool bAllAttempDone = m_UI_MatrixSlots.FindByPredicate( ref matrixSlot, ref position, ( UI_InventorySlot i ) => i.IsSet == false );
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
		bool bHasBeenFound = m_UI_MatrixSlots.FindByPredicate( ref matrixSlot, ref position, ( UI_InventorySlot i ) => i.Section.GetSectionName() == itemName );
		if ( bHasBeenFound )
		{
			matrixSlot.Reset();
		}
		return bHasBeenFound;
	}

	
	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if (m_IsInitialized == false )
		{
			return;
		}

		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);

		InputManager.IsEnabled						= false;

		GlobalManager.SetCursorVisibility( true );
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnSwitchToWeaponCustomization()
	{
		UIManager.Instance.GoToMenu( UIManager.WeaponCustomization );
		GameManager.Instance.RequireFrameSkip();
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnReturnToGame()
	{
		GameManager.Instance.RequireFrameSkip();
		UIManager.Instance.GoToMenu( UIManager.InGame );
		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if (m_IsInitialized == false )
		{
			return;
		}

		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

		InputManager.IsEnabled						= true;

		GlobalManager.SetCursorVisibility( false );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnResolutionChange( float newWidth, float newHeight )
	{
		m_GridLayoutGroup.cellSize = new Vector2
		(
			(m_InventorySlots.rect.width - m_GridLayoutGroup.spacing.x ) / m_InventorySectionData.CellCountVertical, 
			(m_InventorySlots.rect.height - m_GridLayoutGroup.spacing.y ) / m_InventorySectionData.CellCountHorizontal
		);
	}
	
}
