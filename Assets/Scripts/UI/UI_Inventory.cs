using UnityEngine;
using UnityEngine.UI;


public sealed class UI_Inventory : UI_Base, IStateDefiner
{
	[System.Serializable]
	private class UI_InventorySectionData
	{
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
	private UI_InventorySectionData	m_InventorySectionData	= new UI_InventorySectionData();

	private				RectTransform		m_MainPanel							= null;
	private				RectTransform		m_InventorySlots					= null;
	private				UI_InventorySlot[,]	m_UI_MatrixSlots					= null;
	private				GridLayoutGroup		m_GridLayoutGroup					= null;
	private				Button				m_ReturnToGame						= null;
	private				Button				m_SwitchToWeaponCustomization		= null;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("MainPanel", out m_MainPanel)))
			{
				if (CustomAssertions.IsTrue(m_MainPanel.TrySearchComponentByChildName("InventorySlots", out m_InventorySlots)))
				{
					if (CustomAssertions.IsTrue(m_InventorySlots.TrySearchComponent(ESearchContext.LOCAL, out m_GridLayoutGroup)))
					{
						if (transform.parent.IsNotNull() && transform.parent.TryGetComponent(out Canvas parentCanvas))
						{
							float scaleFactor = (parentCanvas.scaleFactor < 1.0f) ? parentCanvas.scaleFactor : 1f / parentCanvas.scaleFactor;

							float ratio = (float)Screen.width / (float)Screen.height;

							// Section Data
							if (CustomAssertions.IsTrue
							(
								GlobalManager.Configs.TryGetSection("UI_Inventory", out Database.Section inventorySection) && GlobalManager.Configs.TrySectionToOuter(inventorySection, m_InventorySectionData),
								"Cannot load UI_InventorySectionData"
							))
							{
								int halfHorizontalPadding = m_InventorySectionData.HorizontalPadding  / 2;
								int halfVerticalPadding   = m_InventorySectionData.VerticalPadding    / 2;
								RectOffset padding = new RectOffset
								( 
									left: halfHorizontalPadding,	right: halfHorizontalPadding, 
									top: halfVerticalPadding,		bottom: halfVerticalPadding 
								);
								m_GridLayoutGroup.padding			= padding;
								m_GridLayoutGroup.spacing			= new Vector2(m_InventorySectionData.HSpaceBetweenSlots, m_InventorySectionData.VSpaceBetweenSlots) * scaleFactor / ratio;
								m_GridLayoutGroup.cellSize			= new Vector2
								(
									(m_InventorySlots.rect.width - m_GridLayoutGroup.spacing.x) / m_InventorySectionData.CellCountVertical,
									(m_InventorySlots.rect.height - m_GridLayoutGroup.spacing.y) / m_InventorySectionData.CellCountHorizontal
								);
								m_GridLayoutGroup.childAlignment	= TextAnchor.MiddleCenter;
								m_GridLayoutGroup.constraint		= GridLayoutGroup.Constraint.FixedColumnCount;
								m_GridLayoutGroup.constraintCount	= m_InventorySectionData.CellCountHorizontal;


								m_UI_MatrixSlots = new UI_InventorySlot[m_InventorySectionData.CellCountHorizontal, m_InventorySectionData.CellCountVertical];

								if (CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Prefabs/UI/UI_InventorySlot", out GameObject inventorySlotPrefab)))
								{
									CustomAssertions.IsTrue(inventorySlotPrefab.transform.HasComponent<UI_InventorySlot>());

									for (int i = 0; i < m_InventorySectionData.CellCountVertical; i++)
									{
										for (int j = 0; j < m_InventorySectionData.CellCountHorizontal; j++)
										{
											GameObject instancedInventorySlot = Instantiate(inventorySlotPrefab);
											instancedInventorySlot.transform.SetParent(m_InventorySlots, worldPositionStays: false);
											(instancedInventorySlot.transform as RectTransform).anchorMin = Vector2.zero;
											(instancedInventorySlot.transform as RectTransform).anchorMax = Vector2.one;

											IStateDefiner slot = m_UI_MatrixSlots[i, j] = instancedInventorySlot.GetComponent<UI_InventorySlot>();
											slot.Initialize();
										}
									}
								}
							}
						}
					}
				}
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("SwitchToWeaponCustomization", out m_SwitchToWeaponCustomization)))
			{
				m_SwitchToWeaponCustomization.onClick.AddListener(OnSwitchToWeaponCustomization);
			}

			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("ReturnToGame", out m_ReturnToGame)))
			{
				m_ReturnToGame.onClick.AddListener(OnReturnToGame);
			}

			UserSettings.VideoSettings.OnResolutionChanged += OnResolutionChange;

			m_IsInitialized = true;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////
	void	IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		// TODO evaluate cleaner solution
		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, false);

		InputManager.IsEnabled = false;

		GlobalManager.SetCursorVisibility(true);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		// TODO evaluate cleaner solution
		GlobalManager.InputMgr.SetCategory(EInputCategory.CAMERA, true);

		InputManager.IsEnabled = true;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool AddItem(Database.Section itemSection, Texture2D itemIcon)
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		if(m_UI_MatrixSlots.FindByPredicate(out UI_InventorySlot matrixSlot, out Vector2 position, i => !i.IsSet))
		{
			matrixSlot.TrySet(itemIcon, itemSection);
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	public bool RemoveItem(string itemName)
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		if(m_UI_MatrixSlots.FindByPredicate(out UI_InventorySlot matrixSlot, out Vector2 position, i => i.Section?.GetSectionName() == itemName))
		{
			matrixSlot.Reset();
			return true;
		}
		return false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void	OnSwitchToWeaponCustomization()
	{
		GlobalManager.Instance.RequireFrameSkip();

		UIManager.Instance.GoToMenu(UIManager.WeaponCustomization);
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnReturnToGame()
	{
		GlobalManager.Instance.RequireFrameSkip();

		UIManager.Instance.GoToMenu(UIManager.InGame);

		UIManager.InGame.UpdateUI();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnResolutionChange(float newWidth, float newHeight)
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		m_GridLayoutGroup.cellSize = new Vector2
		(
			(m_InventorySlots.rect.width  - m_GridLayoutGroup.spacing.x ) / m_InventorySectionData.CellCountVertical, 
			(m_InventorySlots.rect.height - m_GridLayoutGroup.spacing.y ) / m_InventorySectionData.CellCountHorizontal
		);
	}
}
