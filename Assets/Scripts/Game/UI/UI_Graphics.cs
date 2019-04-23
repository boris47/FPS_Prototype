using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Graphics : MonoBehaviour, IUIOptions, IStateDefiner {

	// Registry Keys
	private	const	string	FLAG_SAVED_GRAPHIC_SETTINGS	= "bSavedVideoSettings";
	private	const	string	VAR_RESOLUTION_INDEX		= "iResolutionIndex";
	private	const	string	VAR_IS_FULLSCREEN			= "bFullScreen";
	private	const	string	VAR_ANISOTROPIC_FILTERING	= "bAnisotropicFiltering";
	private	const	string	VAR_ANTIALIASING_LEVEL		= "iAntialiasingLevel";
	private	const	string	VAR_QUALITY_LEVEL			= "iQualityLevel";


	// Resolution
	private	Resolution[]			m_AviableResolutions	= null;

	// Quality
	private	string[]				m_QualityLevelNames		= null;


	// CHANGES STRUCTURES
	// ---------------------------
	private struct ScreenData {
		public	Resolution resolution;
		public	bool fullScreen;
		public	int	resolutionIndex;
		public	bool isDirty;
	}
	private	ScreenData m_ScreenData;

	// ---------------------------
	private struct FiltersData {
		public	bool anisotropicFilter;
		public	int antialiasing;
		public	bool isDirty;
	}
	private	FiltersData m_FilterData;

	// ---------------------------
	private struct QualityData {
		public	int	qualityLevel;
		public	bool isDirty;
	}
	private	QualityData m_QualityData;


	// UI Components
	private	Dropdown		m_ResolutionDropDown		= null;
	private	Toggle			m_FullScreenToogle			= null;
	private	Toggle			m_AnisotropicFilterToogle	= null;
	private	Dropdown		m_AntialiasingDropDown		= null;
	private	Dropdown		m_QualityLevelDropDown		= null;
	private	Button			m_ApplyButton				= null;
	private	Button			m_ResetButton				= null;

	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	} 


	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
		{
			return true;
		}

		m_bIsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			// Get Components
			m_AviableResolutions = Screen.resolutions;
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ResolutionsDropDown", ref m_ResolutionDropDown ) )
			{
				m_ResolutionDropDown.onValueChanged.AddListener( OnResolutionChosen );
				m_ResolutionDropDown.AddOptions( 
					new List<Resolution>( m_AviableResolutions ).ConvertAll( 
						new System.Converter<Resolution, string>( ( Resolution res ) => { return res.ToString(); } )
					)
				);
			}

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "FullScreenToggle", ref m_FullScreenToogle ) )
			{
				m_FullScreenToogle.onValueChanged.AddListener( OnFullScreenSet );
			}

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "AnisotropicFilterToogle", ref m_AnisotropicFilterToogle ) )
			{
				m_AnisotropicFilterToogle.onValueChanged.AddListener( OnAnisotropicFilterSet );
			}

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "AntialiasingDropDown", ref m_AntialiasingDropDown ) ) 
			{
				m_AntialiasingDropDown.onValueChanged.AddListener( OnAntialiasingSet );
				m_AntialiasingDropDown.AddOptions(
					new List<string>( new string[4] { "None", "2x","4x", "8x" } )
				);
			}

			m_QualityLevelNames = QualitySettings.names;
			if ( m_bIsInitialized &= transform.SearchComponentInChild( "QualityLevelDropDown", ref m_QualityLevelDropDown ) )
			{
				m_QualityLevelDropDown.onValueChanged.AddListener( OnQualityLevelSet );
				m_QualityLevelDropDown.AddOptions( new List<string>( m_QualityLevelNames ) );
			}

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ApplyButton", ref m_ApplyButton ) )
			{
				m_ApplyButton.onClick.AddListener
				(	
					delegate()
					{
						UI.Instance.Confirmation.Show( "Apply Changes?", OnApplyChanges, delegate { ReadFromRegistry(); UpdateUI(); } );
					}
				);
				m_ApplyButton.interactable = false;
			}

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ResetButton", ref m_ResetButton ) )
			{
				m_ResetButton.onClick.AddListener
				(
					delegate()
					{
						UI.Instance.Confirmation.Show( "Reset?", ApplyDefaults );
					}	
				);
			}

			// disable navigation for everything
			foreach( Selectable s in GetComponentsInChildren<Selectable>() )
			{
				s.navigation = noNavigationMode;
			}
		}
		return m_bIsInitialized;
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
	// GetResolutionIndex
	private	int GetResolutionIndex( Resolution res )
	{
		System.Predicate<Resolution> pred = delegate( Resolution r )
		{
			return r.height == res.height && res.width == r.width && res.refreshRate == r.refreshRate;
		};
		return System.Array.FindIndex( m_AviableResolutions, pred );
	}


	//////////////////////////////////////////////////////////////////////////
	// OnEnable
	public void OnEnable()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		if ( PlayerPrefs.HasKey( FLAG_SAVED_GRAPHIC_SETTINGS ) == true )
		{
			ReadFromRegistry();

			UpdateUI();
		}
		else
		{
			ApplyDefaults();

			PlayerPrefs.SetString( FLAG_SAVED_GRAPHIC_SETTINGS, "1" );
		}

		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnResolutionChosen
	private	void	OnResolutionChosen( int index )
	{
		Resolution chosen = m_AviableResolutions[ index ];
		m_ScreenData.resolution = chosen;
		m_ScreenData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFullScreenSet
	private	void	OnFullScreenSet( bool newValue )
	{
		m_ScreenData.fullScreen = newValue;
		m_ScreenData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnAnisotropicFilterSet
	private	void	OnAnisotropicFilterSet( bool newValue )
	{
		m_FilterData.anisotropicFilter = newValue;
		m_FilterData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnAntialiasingSet
	private	void	OnAntialiasingSet( int newiIndex )
	{
		m_FilterData.antialiasing = newiIndex;
		m_FilterData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnQualityLevelSet
	private	void	OnQualityLevelSet( int newiIndex )
	{
		m_QualityData.qualityLevel = newiIndex;
		m_QualityData.isDirty = true;
		m_ApplyButton.interactable = true;
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDefaults
	public	void	ApplyDefaults()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		// Remove keys from registry
		Reset();
		{
			// Screen
			m_ScreenData.resolution			= new Resolution() { width = 640, height = 480, refreshRate = 60 };
			m_ScreenData.resolutionIndex	= GetResolutionIndex( m_ScreenData.resolution );
			m_ScreenData.fullScreen			= true;
			m_ScreenData.isDirty			= true;

			// Filters
			m_FilterData.anisotropicFilter	= false;
			m_FilterData.antialiasing		= 0;
			m_FilterData.isDirty			= true;
		
			// Quality
			m_QualityData.qualityLevel		= 0;
			m_QualityData.isDirty			= true;
		}
		// Save new keys into registry
		SaveToRegistry();

		//Update UI components
		UpdateUI();

		// Apply the default settings
		OnApplyChanges();

		// Reset buttons state
		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReadFromRegistry
	/// <summary> Read value from Registry </summary>
	public	void	ReadFromRegistry()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		// Screen
		m_ScreenData.resolutionIndex	= PlayerPrefs.GetInt( VAR_RESOLUTION_INDEX );
		if ( m_ScreenData.resolutionIndex > -1 )
		{
			m_ScreenData.resolution			= m_AviableResolutions[m_ScreenData.resolutionIndex];
		}

//		m_ScreenData.resolution			= m_AviableResolutions[m_ScreenData.resolutionIndex];
		m_ScreenData.fullScreen			= PlayerPrefs.GetInt( VAR_IS_FULLSCREEN ) != 0;

		// Filters
		m_FilterData.anisotropicFilter	= PlayerPrefs.GetInt( VAR_ANISOTROPIC_FILTERING ) != 0;
		m_FilterData.antialiasing		= PlayerPrefs.GetInt( VAR_ANTIALIASING_LEVEL );
		
		// Quality
		m_QualityData.qualityLevel		= PlayerPrefs.GetInt( VAR_QUALITY_LEVEL );
	}


	//////////////////////////////////////////////////////////////////////////
	// ApplyDefaults
	/// <summary> Updates UI Components </summary>
	public	void	UpdateUI()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		m_ResolutionDropDown.value		= m_ScreenData.resolutionIndex;
		m_FullScreenToogle.isOn			= m_ScreenData.fullScreen;;
		m_AnisotropicFilterToogle.isOn	= m_FilterData.anisotropicFilter;
		m_AntialiasingDropDown.value	= m_FilterData.antialiasing;
		m_QualityLevelDropDown.value	= m_QualityData.qualityLevel;
	}


	//////////////////////////////////////////////////////////////////////////
	// SaveToRegistry
	/// <summary> Save settings </summary>
	public	void	SaveToRegistry()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		// Save settings
		{
			PlayerPrefs.SetInt( VAR_RESOLUTION_INDEX, m_ScreenData.resolutionIndex );
			PlayerPrefs.SetInt( VAR_IS_FULLSCREEN, m_ScreenData.fullScreen ? 1 : 0 );

			PlayerPrefs.SetInt( VAR_ANISOTROPIC_FILTERING,m_FilterData.anisotropicFilter == true ? 1 : 0 );
			PlayerPrefs.SetInt( VAR_ANTIALIASING_LEVEL, m_FilterData.antialiasing );

			PlayerPrefs.SetInt( VAR_QUALITY_LEVEL, m_QualityData.qualityLevel );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnApplyChanges
	/// <summary> Apply changes </summary>
	public	void	OnApplyChanges()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		// Screen
		if ( m_ScreenData.isDirty )
		{
			m_ScreenData.isDirty = false;

			Screen.SetResolution(
				width:			m_ScreenData.resolution.width,
				height:			m_ScreenData.resolution.height,
				fullscreen:		m_ScreenData.fullScreen
			);

			print( "Applying screen settings" );
		}

		// Filter
		if ( m_FilterData.isDirty )
		{
			m_FilterData.isDirty = false;

			QualitySettings.anisotropicFiltering	= m_FilterData.anisotropicFilter ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
			QualitySettings.antiAliasing			= m_FilterData.antialiasing;

			print( "Applying filter settings" );
		}

		// Quality
		if ( m_QualityData.isDirty )
		{
			m_QualityData.isDirty = false;
			
			QualitySettings.SetQualityLevel( m_QualityData.qualityLevel, applyExpensiveChanges: true );

			print( "Applying qaulity settings" );
		}

		// Save settings
		SaveToRegistry();

		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// Reset
	/// <summary> Remove key from registry </summary>
	public	void	Reset()
	{
		if ( m_bIsInitialized == false )
		{
			return;
		}

		PlayerPrefs.DeleteKey( FLAG_SAVED_GRAPHIC_SETTINGS );
		{
			PlayerPrefs.DeleteKey( VAR_RESOLUTION_INDEX );
			PlayerPrefs.DeleteKey( VAR_IS_FULLSCREEN );
			PlayerPrefs.DeleteKey( VAR_ANISOTROPIC_FILTERING );
			PlayerPrefs.DeleteKey( VAR_ANTIALIASING_LEVEL );
			PlayerPrefs.DeleteKey( VAR_QUALITY_LEVEL );
		}
	}

}
