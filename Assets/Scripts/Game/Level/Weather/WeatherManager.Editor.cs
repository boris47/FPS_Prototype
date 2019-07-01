
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Editor {
		bool						INTERNAL_EditorLinked				{ get; set; }
		bool						INTERNAL_EditorCycleLinked			{ get; set; }
		bool						INTERNAL_EditorDescriptorLinked		{ get; set; }
		float						INTERNAL_DayTimeNow					{ get; set; }
		EnvDescriptor				INTERNAL_CurrentDescriptor			{ get; }
		EnvDescriptor				INTERNAL_NextDescriptor				{ get; }
		Weathers					INTERNAL_Cycles						{ get; set; }
		List<EnvDescriptor>			INTERNAL_CurrentDescriptors			{ get; set; }
#if UNITY_EDITOR
		bool						INTERNAL_EditModeEnabled			{ get; set; }
#endif
		Light						INTERNAL_Sun						{ get; }

		void						INTERNAL_StartSelectDescriptors( float DayTime, WeatherCycle cycle );
		void						INTERNAL_Start( WeatherCycle cycle, float choiseFactor );
		void						INTERNAL_ForceEnable();
		void						INTERNAL_ForceDisable();
		void						INTERNAL_ForceLoadResources();
		void						INTERNAL_ForceUpdate();
	}


	public partial class WeatherManager : IWeatherManager_Editor {

		private	static	IWeatherManager_Editor	m_EditorInstance	= null;
		public	static	IWeatherManager_Editor	Editor
		{
			get { return m_EditorInstance; }
		}

		private	static	bool m_INTERNAL_EditorLinked = false;

#region INTERFACE
		bool				IWeatherManager_Editor.INTERNAL_EditorLinked
		{
			get { return m_INTERNAL_EditorLinked; }
			set {m_INTERNAL_EditorLinked = value; Debug.Log( "m_INTERNAL_EditorLinked: " + m_INTERNAL_EditorLinked ); }
		}
		bool				IWeatherManager_Editor.INTERNAL_EditorCycleLinked					{ get; set; }
		bool				IWeatherManager_Editor.INTERNAL_EditorDescriptorLinked				{ get; set; }
		float				IWeatherManager_Editor.INTERNAL_DayTimeNow
		{
			get { return m_DayTimeNow; }
			set { m_DayTimeNow = value; }
		}
		EnvDescriptor		IWeatherManager_Editor.INTERNAL_CurrentDescriptor					{ get { return m_EnvDescriptorCurrent; } }
		EnvDescriptor		IWeatherManager_Editor.INTERNAL_NextDescriptor						{ get { return m_EnvDescriptorNext; } }
		Weathers			IWeatherManager_Editor.INTERNAL_Cycles
		{
			get { return m_Cycles; }
			set { m_Cycles = value; }
		}
		List<EnvDescriptor>	IWeatherManager_Editor.INTERNAL_CurrentDescriptors
		{
			get { return m_Descriptors; }
			set { m_Descriptors = value; }
		}
#if UNITY_EDITOR
		bool IWeatherManager_Editor.INTERNAL_EditModeEnabled
		{
			get { return this.runInEditMode; }
			set { this.runInEditMode = value; }
		}
#endif
		Light	IWeatherManager_Editor.INTERNAL_Sun
		{
			get { return m_Sun; }
		}

		void	IWeatherManager_Editor.INTERNAL_Start( WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle; 
			m_Descriptors			= m_CurrentCycle.LoadedDescriptors;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_WeatherChoiceFactor	= choiceFactor;
		}

		void	IWeatherManager_Editor.INTERNAL_StartSelectDescriptors( float DayTime, WeatherCycle cycle )
		{
			m_CurrentCycle					= cycle;
			m_CurrentCycleName				= cycle.name;
			m_WeatherChoiceFactor			= 2f;
			m_EnvDescriptorCurrent			= null;
			m_EnvDescriptorNext				= null;
			StartSelectDescriptors( DayTime, cycle );
		}

		void	IWeatherManager_Editor.INTERNAL_ForceEnable()
		{
			OnEnable();
		}

		void	IWeatherManager_Editor.INTERNAL_ForceDisable()
		{
			OnDisable();
		}

		void	IWeatherManager_Editor.INTERNAL_ForceLoadResources()
		{
			ResourceLoad_Editor();
		}

		void	IWeatherManager_Editor.INTERNAL_ForceUpdate()
		{
			Update();
		}
#endregion

		/////////////////////////////////////////////////////////////////////////////
		private	void		Awake_Editor()
		{
			m_EditorInstance	= this as IWeatherManager_Editor;
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void		OnEnable_Editor()
		{
			m_EditorInstance	= this as IWeatherManager_Editor;
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void		OnDisable_Editor()
		{
			if ( m_INTERNAL_EditorLinked == false && m_Cycles != null )
			{
				m_Cycles.LoadedCycles.ForEach( w => w.LoadedDescriptors = new List<EnvDescriptor>(24) );
				m_Cycles.LoadedCycles = new List<WeatherCycle>();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		private IEnumerator	Start_Editor()
		{
			yield return null;
		}


		/////////////////////////////////////////////////////////////////////////////
		private	void		Update_Editor()
		{

		}


		/////////////////////////////////////////////////////////////////////////////
		private	void		Reset_Editor()
		{

		}


		/////////////////////////////////////////////////////////////////////////////
		private	void		ResourceLoad_Editor()
		{
			m_AreResLoaded_Cylces = false;

			ResourceManager.LoadedData<Weathers> weathersData = new ResourceManager.LoadedData<Weathers>();
			if ( ResourceManager.LoadResourceSync( WEATHERS_COLLECTION, weathersData ) )
			{
				m_Cycles = weathersData.Asset;
				m_AreResLoaded_Cylces = true;
				Setup_Cycles();
			}
		}
	}

}
