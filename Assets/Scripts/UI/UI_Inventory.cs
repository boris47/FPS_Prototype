﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UI_Inventory : MonoBehaviour, IStateDefiner {

	private		RectTransform		m_MainPanel				= null;
	private		RectTransform		m_InventorySlots		= null;

	private		UI_InventorySlot[,]	m_UI_MatrixSlots		= null;

	private		GridLayoutGroup		m_GridLayoutGroup		= null;

	private	bool			m_bIsInitialized				= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
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
			m_bIsInitialized &= GlobalManager.Configs.bGetSection( "UI_Inventory", m_InventorySectionData );

			// Search grid component
			m_bIsInitialized &= m_InventorySlots.SearchComponent( ref m_GridLayoutGroup, SearchContext.LOCAL );
			
			// LOAD PREFAB
			ResourceManager.LoadData<GameObject> loadData = new ResourceManager.LoadData<GameObject>();
			yield return ResourceManager.LoadResourceAsyncCoroutine( "Prefabs/UI/UI_InventorySlot", loadData, null );

			if ( m_bIsInitialized &= loadData.Asset != null )
			{
				m_UI_MatrixSlots = new UI_InventorySlot[ m_InventorySectionData.CellCountHorizontal, m_InventorySectionData.CellCountVertical ];

				Canvas canvas = transform.parent.GetComponent<Canvas>();

				float scaleFactor = ( canvas.scaleFactor < 1.0f ) ? canvas.scaleFactor : 1f / canvas.scaleFactor;
//				print( "My scale factor: " + scaleFactor );			

				float ratio = (float)Screen.width / (float)Screen.height;
//				print(ratio);
				
				int halfHorizontalPadding = m_InventorySectionData.HorizzontalPadding / 2;
				int halfVerticalPadding   = m_InventorySectionData.VerticalPadding    / 2;
				RectOffset padding = new RectOffset
				( 
					left: halfHorizontalPadding,	right: halfHorizontalPadding, 
					top: halfVerticalPadding,		bottom: halfVerticalPadding 
				);
				m_GridLayoutGroup.padding			= padding;
				m_GridLayoutGroup.cellSize			= new Vector2( m_InventorySectionData.CellSizeX, m_InventorySectionData.CellSizeY ) * scaleFactor;
				m_GridLayoutGroup.spacing			= new Vector2( m_InventorySectionData.HSpaceBetweenSlots, m_InventorySectionData.VSpaceBetweenSlots ) * scaleFactor / ratio;
				m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
				m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
				m_GridLayoutGroup.constraintCount	= m_InventorySectionData.CellCountHorizontal;

				GameObject inventorySlotPrefab = loadData.Asset;
				
				for ( int i = 0; i < m_InventorySectionData.CellCountVertical; i++ )
				{
					for ( int j = 0; j < m_InventorySectionData.CellCountHorizontal; j++ )
					{
						GameObject instancedInventorySlot = Instantiate( inventorySlotPrefab );
						instancedInventorySlot.transform.SetParent( m_InventorySlots, worldPositionStays: false );
						( instancedInventorySlot.transform as RectTransform ).anchorMin = Vector2.zero;
						( instancedInventorySlot.transform as RectTransform ).anchorMax = Vector2.one;

						IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlot.GetComponent<UI_InventorySlot>();
						CoroutinesManager.Start( slot.Initialize() );
					}
				}
			}
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
		bool bHasBeenFound = m_UI_MatrixSlots.FindByPredicate( ref matrixSlot, ref position, ( UI_InventorySlot i ) => i.Section.GetName() == itemName );
		if ( bHasBeenFound )
		{
			matrixSlot.Reset();
		}
		return bHasBeenFound;
	}

	
	//////////////////////////////////////////////////////////////////////////
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

		InputManager.IsEnabled						= false;

		GlobalManager.SetCursorVisibility( true );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		if ( CameraControl.Instance.IsNotNull() )
		{
			CameraControl.Instance.CanParseInput	= true;
		}

		InputManager.IsEnabled						= true;

		GlobalManager.SetCursorVisibility( false );
	}

}