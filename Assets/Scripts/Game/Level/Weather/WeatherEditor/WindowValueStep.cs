using UnityEngine;
using Database;

#if UNITY_EDITOR

using UnityEditor;

public class WindowValueStep : EditorWindow {

	public	static	WindowValueStep		m_Window		{ get; private set; }
	public	static	cValue				Value			{ get; private set; }
	public	static	cValue				Arg				{ get; private set; }

	private	System.Action		OnOK					= null;
	private System.Action		OnCancel				= null;
	private System.Func<cValue>	OptionalArg				= null;
	private	System.Type			RequestedType			= null;

	//
	public static void Init<T>( System.Action callbackOK, System.Action callbackCancel = null, System.Func<cValue> optionalArg = null )
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

		m_Window.OnOK				= callbackOK;
		m_Window.OnCancel			= callbackCancel;
		m_Window.OptionalArg		= optionalArg;
		m_Window.RequestedType		= requestedType;

		if ( requestedType == typeof( bool ) )		Value = new cValue( false );
		if ( requestedType == typeof( int ) )		Value = new cValue( 0 );
		if ( requestedType == typeof( float ) )		Value = new cValue( 0.0f );		
		if ( requestedType == typeof( string ) )	Value = new cValue( "None" );

		Arg = "";
	}


	//
	private static	bool	HasValidValue( cValue Value )
	{
		if ( Value.ToSystemObject() == null )
			return false;

		if ( Value.GetType() == typeof( string ) )
			if ( Value.As<string>() == "" )
				return false;

		return true;
	}

	//
	private void OnGUI()
	{
		EditorGUIUtility.editingTextField = true;
		GUILayout.BeginVertical();
		{
			GUILayout.Label( "Value" );

	//		if ( Value.ToSystemObject() != null )
			{
				if ( RequestedType == typeof( bool ) )					Value = EditorGUILayout.Toggle( Value );
				if ( RequestedType == typeof( int ) )					Value = EditorGUILayout.IntField( Value );
				if ( RequestedType == typeof( float ) )					Value = EditorGUILayout.FloatField( Value );
				if ( RequestedType == typeof( string ) )				Value = EditorGUILayout.TextField( Value );
			}

			if ( OptionalArg != null )
			{
				GUILayout.BeginHorizontal();	
				{
					if ( GUILayout.Button( "Arg", GUILayout.MaxWidth(32f) ) )
					{
						Arg = OptionalArg.Invoke();

					}

					EditorGUIUtility.editingTextField = false;
					System.Type currentArgType = Arg.GetType();
					if ( currentArgType == typeof( bool ) )						Arg = EditorGUILayout.Toggle( Arg );
					if ( currentArgType == typeof( int ) )						Arg = EditorGUILayout.IntField( Arg );
					if ( currentArgType == typeof( float ) )					Arg = EditorGUILayout.FloatField( Arg );
					if ( currentArgType == typeof( string ) )					Arg = EditorGUILayout.TextField( Arg );
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			{
				if ( GUILayout.Button( "OK" ) && HasValidValue(Value) )
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
		EditorGUIUtility.editingTextField = true;
	}


}

#endif