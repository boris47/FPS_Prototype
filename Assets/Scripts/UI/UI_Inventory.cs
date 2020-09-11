using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public sealed class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixSlots		= null;

	private		GridLayoutGroup		m_GridLayoutGroup		= null;


	private		Button				m_ReturnToGame			= null;
	private		Button				m_SwitchToWeaponCustomization		= null;

	private		bool				m_IsInitialized		= false;


	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}

	[System.Serializable]
	private class InventorySectionData {
		public		float				CellSizeX			= 0;
		public		float				CellSizeY			= 0;
		public		int					CellCountHorizontal	= 0;
		public		int					CellCountVertical	= 0;
		public		int					HorizzontalPadding	= 0;
		public		int					VerticalPadding		= 0;
		public		float				HSpaceBetweenSlots	= 0;
		public		float				VSpaceBetweenSlots	= 0;
	}
	[SerializeField, ReadOnly]
	private InventorySectionData	m_InventorySectionData	= new InventorySectionData();

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		this.m_IsInitialized = true;
		{
			this.m_MainPanel = (this.transform.Find( "MainPanel" ) as RectTransform );
			this.m_IsInitialized &= this.m_MainPanel != null;

			if (this.m_IsInitialized == true )
			{
				this.m_InventorySlots = (this.m_MainPanel.Find("InventorySlots") as RectTransform );
				this.m_IsInitialized &= this.m_InventorySlots != null;
			}

			yield return null;


			// LOAD SECTION
			Database.Section inventorySection = null;
			this.m_IsInitialized &= GlobalManager.Configs.GetSection("UI_Inventory", ref inventorySection ) &&  GlobalManager.Configs.bSectionToOuter(inventorySection, this.m_InventorySectionData );

			// Search grid component
			this.m_IsInitialized &= this.m_InventorySlots.SearchComponent( ref this.m_GridLayoutGroup, ESearchContext.LOCAL );
			
			// LOAD PREFAB
			ResourceManager.LoadedData<GameObject> loadedResource = new ResourceManager.LoadedData<GameObject>();
			yield return ResourceManager.LoadResourceAsyncCoroutine( "Prefabs/UI/UI_InventorySlot", loadedResource, null );

			if (this.m_IsInitialized &= loadedResource.Asset != null )
			{
				this.m_UI_MatrixSlots = new UI_InventorySlot[this.m_InventorySectionData.CellCountHorizontal, this.m_InventorySectionData.CellCountVertical ];

				Canvas canvas = this.transform.parent.GetComponent<Canvas>();

				float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
//				print( "My scale factor: " + scaleFactor );			

				float ratio = (float)Screen.width / (float)Screen.height;
//				print(ratio);
				
				int halfHorizontalPadding = this.m_InventorySectionData.HorizzontalPadding / 2;
				int halfVerticalPadding   = this.m_InventorySectionData.VerticalPadding    / 2;
				RectOffset padding = new RectOffset
				( 
					left: halfHorizontalPadding,	right: halfHorizontalPadding, 
					top: halfVerticalPadding,		bottom: halfVerticalPadding 
				);
				this.m_GridLayoutGroup.padding			= padding;

				//	m_GridLayoutGroup.cellSize			= new Vector2( m_InventorySectionData.CellSizeX, m_InventorySectionData.CellSizeY ) * scaleFactor;
				this.m_GridLayoutGroup.spacing			= new Vector2(this.m_InventorySectionData.HSpaceBetweenSlots, this.m_InventorySectionData.VSpaceBetweenSlots ) * scaleFactor / ratio;
				this.m_GridLayoutGroup.cellSize = new Vector2
				(
					(this.m_InventorySlots.rect.width - this.m_GridLayoutGroup.spacing.x ) / this.m_InventorySectionData.CellCountVertical, 
					(this.m_InventorySlots.rect.height - this.m_GridLayoutGroup.spacing.y ) / this.m_InventorySectionData.CellCountHorizontal
				);
				this.m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
				this.m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
				this.m_GridLayoutGroup.constraintCount	= this.m_InventorySectionData.CellCountHorizontal;

				yield return null;

				UserSettings.VideoSettings.OnResolutionChanged += this.OnResolutionChange;

				GameObject inventorySlotPrefab = loadedResource.Asset;
				
				for ( int i = 0; i < this.m_InventorySectionData.CellCountVertical; i++ )
				{
					for ( int j = 0; j < this.m_InventorySectionData.CellCountHorizontal; j++ )
					{
						GameObject instancedInventorySlot = Instantiate( inventorySlotPrefab );
						instancedInventorySlot.transform.SetParent(this.m_InventorySlots, worldPositionStays: false );
						( instancedInventorySlot.transform as RectTransform ).anchorMin = Vector2.zero;
						( instancedInventorySlot.transform as RectTransform ).anchorMax = Vector2.one;

						IStateDefiner slot = this.m_UI_MatrixSlots[i, j] = instancedInventorySlot.GetComponent<UI_InventorySlot>();
						CoroutinesManager.Start( slot.Initialize() );
					}

					yield return null;
				}
			}

			// SWITCH TO INVENTORY
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "SwitchToWeaponCustomization", ref this.m_SwitchToWeaponCustomization ) )
			{
				this.m_SwitchToWeaponCustomization.onClick.AddListener
				(
					this.OnSwitchToWeaponCustomization
				);
			}

			yield return null;

			// RETURN TO GAME
			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "ReturnToGame", ref this.m_ReturnToGame ) )
			{
				this.m_ReturnToGame.onClick.AddListener
				(
					this.OnReturnToGame
				);
			}

			yield return null;
		}

		if (this.m_IsInitialized )
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
		return this.m_IsInitialized;
	}
	

	//////////////////////////////////////////////////////////////////////////
	public	bool	AddItem( Database.Section itemSection, Texture2D itemIcon )
	{
		Vector2 position = Vector2.zero;
		UI_InventorySlot matrixSlot = null;
		bool bAllAttempDone = this.m_UI_MatrixSlots.FindByPredicate( ref matrixSlot, ref position, ( UI_InventorySlot i ) => i.IsSet == false );
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
		bool bHasBeenFound = this.m_UI_MatrixSlots.FindByPredicate( ref matrixSlot, ref position, ( UI_InventorySlot i ) => i.Section.GetSectionName() == itemName );
		if ( bHasBeenFound )
		{
			matrixSlot.Reset();
		}
		return bHasBeenFound;
	}

	
	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if (this.m_IsInitialized == false )
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
		if (this.m_IsInitialized == false )
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
		this.m_GridLayoutGroup.cellSize = new Vector2
		(
			(this.m_InventorySlots.rect.width - this.m_GridLayoutGroup.spacing.x ) / this.m_InventorySectionData.CellCountVertical, 
			(this.m_InventorySlots.rect.height - this.m_GridLayoutGroup.spacing.y ) / this.m_InventorySectionData.CellCountHorizontal
		);
	}
	
}
