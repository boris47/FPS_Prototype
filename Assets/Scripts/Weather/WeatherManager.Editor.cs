
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
namespace WeatherSystem {

	// Public interface
	public partial interface IWeatherManager_Editor {

		bool						EDITOR_EditorLinked					{ get; set; }
		bool						EDITOR_EditorCycleLinked			{ get; set; }
		bool						EDITOR_EditorDescriptorLinked		{ get; set; }
		float						EDITOR_DayTimeNow					{ get; set; }
		EnvDescriptor				EDITOR_CurrentDescriptor			{ get; }
		EnvDescriptor				EDITOR_NextDescriptor				{ get; }
		Weathers					EDITOR_Cycles						{ get; set; }
		bool						EDITOR_EditModeEnabled				{ get; set; }

		void						INTERNAL_StartSelectDescriptors		( float DayTime, WeatherCycle cycle );
		void						INTERNAL_Start						( WeatherCycle cycle, float choiseFactor );

	}


	public sealed partial class WeatherManager : IWeatherManager_Editor {

		private	static	IWeatherManager_Editor	m_Instance_Editor	= null;
		public	static	IWeatherManager_Editor	Editor => m_Instance_Editor;

		private	static	bool m_IsEditorLinked = false;

#region INTERFACE
		bool				IWeatherManager_Editor.EDITOR_EditorLinked
		{
			get { return m_IsEditorLinked; }
			set {
				m_IsEditorLinked = value;
				OnEditorAttached( value );
				Debug.Log( "m_INTERNAL_EditorLinked: " + m_IsEditorLinked );
			}
		}
		bool				IWeatherManager_Editor.EDITOR_EditorCycleLinked						{ get; set; }
		bool				IWeatherManager_Editor.EDITOR_EditorDescriptorLinked				{ get; set; }
		float				IWeatherManager_Editor.EDITOR_DayTimeNow
		{
			get { return m_DayTimeNow; }
			set { m_DayTimeNow = value; }
		}
		EnvDescriptor		IWeatherManager_Editor.EDITOR_CurrentDescriptor						{ get { return m_EnvDescriptorCurrent; } }
		EnvDescriptor		IWeatherManager_Editor.EDITOR_NextDescriptor						{ get { return m_EnvDescriptorNext; } }

		/////////////////////////////////////////////////////////////////////////////
		Weathers			IWeatherManager_Editor.EDITOR_Cycles
		{
			get { return m_Cycles; }
			set { m_Cycles = value; }
		}

		/////////////////////////////////////////////////////////////////////////////
		bool IWeatherManager_Editor.EDITOR_EditModeEnabled
		{
			get { return runInEditMode; }
			set { runInEditMode = value; }
		}

		/////////////////////////////////////////////////////////////////////////////
		void	IWeatherManager_Editor.INTERNAL_Start( WeatherCycle cycle, float choiceFactor )
		{
			m_CurrentCycle			= cycle;
			m_CurrentCycleName		= m_CurrentCycle.name;
			m_WeatherChoiceFactor	= choiceFactor;
		}

		/////////////////////////////////////////////////////////////////////////////
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

		/////////////////////////////////////////////////////////////////////////////
		// OnEditorAttached
		private	void		OnEditorAttached(bool bIsAttaching )
		{
			if ( bIsAttaching )
			{

				if ( UnityEditor.EditorApplication.isPlaying == false )
				{
					Setup_Cycles();
					m_Instance_Editor = this;
					Editor.EDITOR_EditorCycleLinked = false;
					UnityEditor.EditorApplication.update -= EditorUpdate;
					UnityEditor.EditorApplication.update += EditorUpdate;
				}
			}
			else
			{
				if ( UnityEditor.EditorApplication.isPlaying == false )
				{
					m_Instance_Editor = null;
					UnityEditor.EditorApplication.update -= EditorUpdate;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// EditorUpdate
		private	void	EditorUpdate()
		{
			if (m_EnvDescriptorCurrent.IsSet == false || m_EnvDescriptorNext.IsSet == false )
				return;

			if ( Editor.EDITOR_EditorCycleLinked == false )
			{
				m_DayTimeNow += Time.deltaTime * m_TimeFactor;
			}
			if (m_DayTimeNow > DAY_LENGTH ) m_DayTimeNow = 0.0f;

			SelectDescriptors(m_DayTimeNow );

			EnvironmentLerp();

			AmbientEffectUpdate();

			// Sun rotation by data
			if ( Editor.EDITOR_EditorDescriptorLinked == false )
			{
				m_Sun.transform.rotation = m_RotationOffset * Quaternion.LookRotation(m_EnvDescriptorMixer.SunRotation );
			}

			TransformTime(m_DayTimeNow, ref m_CurrentDayTime );
		}
	}

}
#endif