using UnityEngine;
using CFG_Reader;

#if UNITY_EDITOR

using UnityEditor;

public class WindowValueStep : EditorWindow {

	public	static	WindowValueStep		m_Window		= null;
	public	static	cValue		Value		= null;

	private	System.Action		OnOK		= null;
	private	System.Action		OnCancel	= null;

	public static void Init<T>( System.Action callbackOK , System.Action callbackCancel = null )
	{
		if ( m_Window != null )
		{
			m_Window.Close();
			m_Window = null;
		}

		System.Type requestedType = typeof( T );
		if ( requestedType != typeof( bool )  &&
			 requestedType != typeof( int )   && 
			 requestedType != typeof( float ) && 
			 requestedType != typeof( string )
		)
		return;
		m_Window = EditorWindow.GetWindow<WindowValueStep>( true, "Weather Manager" );

		m_Window.OnOK		= callbackOK;
		m_Window.OnCancel	= callbackCancel;

		Value = new cValue( requestedType );
	}

	private	bool	HasValidValue()
	{
		if ( Value.Value == null )
			return false;

		if ( Value.Value.GetType() == typeof( string ) )
			if ( Value.As<string>() == "" )
				return false;

		return true;
	}

	private void OnGUI()
	{
		GUILayout.BeginVertical();
		{
			GUILayout.Label( "Value" );

			if ( Value.Value != null )
			{

				System.Type valueType = Value.Value.GetType();

				if ( valueType == typeof( bool ) )
					Value = EditorGUILayout.Toggle( Value );

				if ( valueType == typeof( int ) )
					Value = EditorGUILayout.IntField( Value );

				if ( valueType == typeof( float ) )
					Value = EditorGUILayout.FloatField( Value );

				if ( valueType == typeof( string ) )
					Value = EditorGUILayout.TextField( Value );

			}

			GUILayout.BeginHorizontal();
			{
				if ( GUILayout.Button( "OK" ) && HasValidValue() )
				{
					if ( OnOK != null )
						OnOK.Invoke();
					m_Window.Close();
					return;
				}
				if ( GUILayout.Button( "Cancel" ) )
				{
					if ( OnCancel != null )
						OnCancel.Invoke();
					m_Window.Close();
					return;
				}

			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	/*
	private void OnLostFocus()
	{
		m_Window.Close();
	}
	*/

	private void OnDestroy()
	{
		Value = string.Empty;
	}
}

#endif