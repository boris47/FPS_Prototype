
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
				this.OnEditorAttached( value );
				Debug.Log( "m_INTERNAL_EditorLinked: " + m_IsEditorLinked );
			}
		}
		bool				IWeatherManager_Editor.EDITOR_EditorCycleLinked						{ get; set; }
		bool				IWeatherManager_Editor.EDITOR_EditorDescriptorLinked				{ get; set; }
		float				IWeatherManager_Editor.EDITOR_DayTimeNow
		{
			get { return this.m_DayTimeNow; }
			set { this.m_DayTimeNow = value; }
		}
		EnvDescriptor		IWeatherManager_Editor.EDITOR_CurrentDescriptor						{ get { return this.m_EnvDescriptorCurrent; } }
		EnvDescriptor		IWeatherManager_Editor.EDITOR_NextDescriptor						{ get { return this.m_EnvDescriptorNext; } }

		/////////////////////////////////////////////////////////////////////////////
		Weathers			IWeatherManager_Editor.EDITOR_Cycles
		{
			get { return this.m_Cycles; }
			set { this.m_Cycles = value; }
		}

		/////////////////////////////////////////////////////////////////////////////
		bool IWeatherManager_Editor.EDITOR_EditModeEnabled
		{
			get { return this.runInEditMode; }
			set { this.runInEditMode = value; }
		}

		/////////////////////////////////////////////////////////////////////////////
		void	IWeatherManager_Editor.INTERNAL_Start( WeatherCycle cycle, float choiceFactor )
		{
			this.m_CurrentCycle			= cycle;
			this.m_CurrentCycleName		= this.m_CurrentCycle.name;
			this.m_WeatherChoiceFactor	= choiceFactor;
		}

		/////////////////////////////////////////////////////////////////////////////
		void	IWeatherManager_Editor.INTERNAL_StartSelectDescriptors( float DayTime, WeatherCycle cycle )
		{
			this.m_CurrentCycle					= cycle;
			this.m_CurrentCycleName				= cycle.name;
			this.m_WeatherChoiceFactor			= 2f;
			this.m_EnvDescriptorCurrent			= null;
			this.m_EnvDescriptorNext				= null;
			this.StartSelectDescriptors( DayTime, cycle );
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
					this.Setup_Cycles();
					m_Instance_Editor = this;
					Editor.EDITOR_EditorCycleLinked = false;
					UnityEditor.EditorApplication.update -= this.EditorUpdate;
					UnityEditor.EditorApplication.update += this.EditorUpdate;
				}
			}
			else
			{
				if ( UnityEditor.EditorApplication.isPlaying == false )
				{
					m_Instance_Editor = null;
					UnityEditor.EditorApplication.update -= this.EditorUpdate;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		// EditorUpdate
		private	void	EditorUpdate()
		{
			if (this.m_EnvDescriptorCurrent.set == false || this.m_EnvDescriptorNext.set == false )
				return;

			if ( Editor.EDITOR_EditorCycleLinked == false )
			{
				this.m_DayTimeNow += Time.deltaTime * this.m_TimeFactor;
			}
			if (this.m_DayTimeNow > DAY_LENGTH ) this.m_DayTimeNow = 0.0f;

			this.SelectDescriptors(this.m_DayTimeNow );

			this.EnvironmentLerp();

			this.AmbientEffectUpdate();

			// Sun rotation by data
			if ( Editor.EDITOR_EditorDescriptorLinked == false )
			{
				this.m_Sun.transform.rotation = this.m_RotationOffset * Quaternion.LookRotation(this.m_EnvDescriptorMixer.SunRotation );
			}

			TransformTime(this.m_DayTimeNow, ref this.m_CurrentDayTime );
		}
	}

}
#endif