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
	private	Resolution[]			m_AvailableResolutions	= null;
	public	delegate	void	OnResolutionChangedDelegate( float newWidth, float newHeight );

	private static	event	OnResolutionChangedDelegate	m_OnResolutionChanged = delegate { };
	public static event OnResolutionChangedDelegate OnResolutionChanged
	{
		add		{ if ( value != null )	m_OnResolutionChanged += value; }
		remove	{ if ( value != null )	m_OnResolutionChanged -= value; }
	}

	// Quality
	private	string[]				m_QualityLevelNames		= null;


	// CHANGES STRUCTURES
	// ---------------------------
	private struct ScreenData {
		public	Resolution resolution;
		public	bool bIsFullScreen;
		public	int	iResolutionIndex;
		public	bool isDirty;
	}
	private	ScreenData m_ScreenData;

	// ---------------------------
	private struct FiltersData {
		public	bool bHasAnisotropicFilter;
		public	int iAntialiasing;
		public	bool isDirty;
	}
	private	FiltersData m_FilterData;

	// ---------------------------
	private struct QualityData {
		public	int	iQualityLevel;
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
	IEnumerator IStateDefiner.Initialize()
	{
		if ( m_bIsInitialized == true )
			yield break;

		m_bIsInitialized = true;
		{
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

			// Sort Resolutions
			System.Comparison<Resolution> comparer = delegate( Resolution a, Resolution b )
			{
				int mulA = a.width*a.height;
				int mulB = b.width*b.height;
				return  mulA < mulB ? -1 : mulA > mulB ? 1 : 0; // a.width < b.width ? -1 : a.width > b.width ? 1 : 0;
			};

			List<Resolution> sortedResolutions = new List<Resolution>( Screen.resolutions );
			sortedResolutions.Sort ( comparer );
			
			m_AvailableResolutions = sortedResolutions.ToArray();

			if ( m_bIsInitialized &= transform.SearchComponentInChild( "ResolutionsDropDown", ref m_ResolutionDropDown ) )
			{
				m_ResolutionDropDown.onValueChanged.AddListener( OnResolutionChosen );
				m_ResolutionDropDown.AddOptions( 
					new List<Resolution>( m_AvailableResolutions ).ConvertAll( 
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
						UIManager.Confirmation.Show( "Apply Changes?", OnApplyChanges, delegate { ReadFromRegistry(); UpdateUI(); } );
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
						UIManager.Confirmation.Show( "Reset?", ApplyDefaults );
					}	
				);
			}

			// disable navigation for everything
			foreach( Selectable s in GetComponentsInChildren<Selectable>() )
			{
				s.navigation = noNavigationMode;
			}

			if ( m_bIsInitialized )
			{
				ReadFromRegistry();

				UpdateUI();

				m_ScreenData.isDirty		= true;
				m_FilterData.isDirty		= true;
				m_QualityData.isDirty		= true;

				OnApplyChanges();
			}
			else
			{
				Debug.LogError( "UI_Graphics: Bad initialization!!!" );
			}
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
	// GetResolutionIndex
	/// <summary> Search into available resolution the closer to the given one </summary>
	private	int GetResolutionIndex( Resolution res )
	{
		int bestWidthtDelta = int.MaxValue;
		int bestHeightDelta = int.MaxValue;
		int currentIndex = 0;
		for ( int i = 0; i < m_AvailableResolutions.Length; i++ )
		{
			Resolution r = m_AvailableResolutions[ i ];
			int deltaWidth = Mathf.Abs( res.width - r.width );
			if ( deltaWidth < bestWidthtDelta )
			{
				int deltaHeight = Mathf.Abs( res.height - r.height );
				if ( deltaHeight < bestHeightDelta )
				{
					currentIndex = i;
					bestHeightDelta = deltaHeight;
				}
				bestWidthtDelta = deltaWidth;
			}
		}

		return currentIndex;
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
		Resolution chosen = m_AvailableResolutions[ index ];
		m_ScreenData.resolution			= chosen;
		m_ScreenData.isDirty			= true;
		m_ScreenData.iResolutionIndex	= index;
		m_ApplyButton.interactable		= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnFullScreenSet
	private	void	OnFullScreenSet( bool newValue )
	{
		m_ScreenData.bIsFullScreen			= newValue;
		m_ScreenData.isDirty			= true;
		m_ApplyButton.interactable		= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnAnisotropicFilterSet
	private	void	OnAnisotropicFilterSet( bool newValue )
	{
		m_FilterData.bHasAnisotropicFilter	= newValue;
		m_FilterData.isDirty			= true;
		m_ApplyButton.interactable		= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnAntialiasingSet
	private	void	OnAntialiasingSet( int newiIndex )
	{
		m_FilterData.iAntialiasing		= newiIndex;
		m_FilterData.isDirty			= true;
		m_ApplyButton.interactable		= true;
	}


	//////////////////////////////////////////////////////////////////////////
	// OnQualityLevelSet
	private	void	OnQualityLevelSet( int newiIndex )
	{
		m_QualityData.iQualityLevel		= newiIndex;
		m_QualityData.isDirty			= true;
		m_ApplyButton.interactable		= true;
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
			m_ScreenData.resolution			= new Resolution() { width = 800, height = 600, refreshRate = 60 };
			m_ScreenData.iResolutionIndex	= GetResolutionIndex( m_ScreenData.resolution );
			m_ScreenData.bIsFullScreen		= true;
			m_ScreenData.isDirty			= true;

			// Filters
			m_FilterData.bHasAnisotropicFilter	= false;
			m_FilterData.iAntialiasing		= 0;
			m_FilterData.isDirty			= true;
		
			// Quality
			m_QualityData.iQualityLevel		= 0;
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
		m_ScreenData.iResolutionIndex		= PlayerPrefs.GetInt( VAR_RESOLUTION_INDEX );
		if ( m_ScreenData.iResolutionIndex > -1 )
		{
			m_ScreenData.resolution			= m_AvailableResolutions[m_ScreenData.iResolutionIndex];
		}

		m_ScreenData.bIsFullScreen			= PlayerPrefs.GetInt( VAR_IS_FULLSCREEN ) != 0;

		// Filters
		m_FilterData.bHasAnisotropicFilter	= PlayerPrefs.GetInt( VAR_ANISOTROPIC_FILTERING ) != 0;
		m_FilterData.iAntialiasing			= PlayerPrefs.GetInt( VAR_ANTIALIASING_LEVEL );
		
		// Quality
		m_QualityData.iQualityLevel			= PlayerPrefs.GetInt( VAR_QUALITY_LEVEL );
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

		m_ResolutionDropDown.value		= m_ScreenData.iResolutionIndex;
		m_FullScreenToogle.isOn			= m_ScreenData.bIsFullScreen;
		m_AnisotropicFilterToogle.isOn	= m_FilterData.bHasAnisotropicFilter;
		m_AntialiasingDropDown.value	= m_FilterData.iAntialiasing;
		m_QualityLevelDropDown.value	= m_QualityData.iQualityLevel;
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
			PlayerPrefs.SetInt( VAR_RESOLUTION_INDEX,		m_ScreenData.iResolutionIndex );
			PlayerPrefs.SetInt( VAR_IS_FULLSCREEN,			m_ScreenData.bIsFullScreen ? 1 : 0 );

			PlayerPrefs.SetInt( VAR_ANISOTROPIC_FILTERING,	m_FilterData.bHasAnisotropicFilter ? 1 : 0 );
			PlayerPrefs.SetInt( VAR_ANTIALIASING_LEVEL,		m_FilterData.iAntialiasing );

			PlayerPrefs.SetInt( VAR_QUALITY_LEVEL,			m_QualityData.iQualityLevel );
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

			Screen.SetResolution
			(
				width:			m_ScreenData.resolution.width,
				height:			m_ScreenData.resolution.height,
				fullscreen:		m_ScreenData.bIsFullScreen
			);

			m_OnResolutionChanged( m_ScreenData.resolution.width, m_ScreenData.resolution.height );
//			print( "Applying SCREEN settings" );
		}

		// Filter
		if ( m_FilterData.isDirty )
		{ 
			m_FilterData.isDirty = false;

			QualitySettings.anisotropicFiltering	= m_FilterData.bHasAnisotropicFilter ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
			QualitySettings.antiAliasing			= m_FilterData.iAntialiasing * 2;

//			print( "Applying FILTER settings" );
		}

		// Quality
		if ( m_QualityData.isDirty )
		{
			m_QualityData.isDirty = false;
			
			QualitySettings.SetQualityLevel( m_QualityData.iQualityLevel, applyExpensiveChanges: true );

//			print( "Applying QUALITY settings" );
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
