
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
		Weathers					Cycles								{ get; set; }
#if UNITY_EDITOR
		bool						INTERNAL_EditModeEnabled			{ get; set; }
#endif

		void						INTERNAL_StartSelectDescriptors( float DayTime, WeatherCycle cycle );
		void						INTERNAL_Start( WeatherCycle cycle, float choiseFactor );
	}


	public partial class WeatherManager : IWeatherManager_Editor {

		private	static	IWeatherManager_Editor	m_Instance_Editor	= null;
		public	static	IWeatherManager_Editor	Editor => m_Instance_Editor;

		private	static	bool m_INTERNAL_EditorLinked = false;

#region INTERFACE
		bool				IWeatherManager_Editor.INTERNAL_EditorLinked
		{
			get { return m_INTERNAL_EditorLinked; }
			set {
				m_INTERNAL_EditorLinked = value;
				OnEditorAttached( value );
				Debug.Log( "m_INTERNAL_EditorLinked: " + m_INTERNAL_EditorLinked );
			}
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
		Weathers			IWeatherManager_Editor.Cycles
		{
			get { return m_Cycles; }
			set { m_Cycles = value; }
		}
#if UNITY_EDITOR
		bool IWeatherManager_Editor.INTERNAL_EditModeEnabled
		{
			get { return this.runInEditMode; }
			set { this.runInEditMode = value; }
		}
#endif

		void	IWeatherManager_Editor.INTERNAL_Start( WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle; 
//			m_Descriptors			= m_CurrentCycle.LoadedDescriptors;
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

#endregion


		private	void		OnEditorAttached(bool bIsAttaching )
		{
#if UNITY_EDITOR
			if ( bIsAttaching )
			{
				if ( UnityEditor.EditorApplication.isPlaying == false )
				{
					UnityEditor.EditorApplication.update += Update;
			//		UnityEditor.EditorApplication.update += Update;
				}
			}
			else
			{
				if ( UnityEditor.EditorApplication.isPlaying == false )
				{
			//		UnityEditor.EditorApplication.update -= Update;
			//		UnityEditor.EditorApplication.update -= Update;
					UnityEditor.EditorApplication.update -= Update;
				}
			}
#endif
		}
	}

}
