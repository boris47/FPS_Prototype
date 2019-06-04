using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleInputEvent {

	private	InputDelegateHandler	m_InputEvent	= null;

	private	System.Func<bool>		m_Condition		= delegate() { return true; };

	private	string					m_InputEventID	= "NONE";

	//////////////////////////////////////////////////////////////////////////
	public	string					InputEventID
	{
		get { return m_InputEventID; }
	}

	//////////////////////////////////////////////////////////////////////////
	public SingleInputEvent( string Id, InputDelegateHandler eventToCall, System.Func<bool> condition )
	{
		m_InputEventID = Id;
		m_InputEvent = eventToCall;
		m_Condition = condition;
	}

	//////////////////////////////////////////////////////////////////////////
	public	void	Rebind( InputDelegateHandler eventToCall, System.Func<bool> condition )
	{
		m_InputEvent = eventToCall;
	}

	//////////////////////////////////////////////////////////////////////////
	public void Call()
	{
		if ( m_Condition() )
		{
			m_InputEvent();
		}
	}

}

public class InputEventCollection {	

	private static System.Func<bool>		m_AlwaysTrueCondition	= delegate() { return true; };
	private	List<SingleInputEvent>			m_Events				= new List<SingleInputEvent>();

	//////////////////////////////////////////////////////////////////////////
	public	InputEventCollection	Bind( string inputEventID, InputDelegateHandler method, System.Func<bool> condition )
	{
		System.Func<bool> internalCondition = ( condition != null ) ? condition : m_AlwaysTrueCondition;

		int index = m_Events.FindIndex( s => s.InputEventID == inputEventID );
		if ( index == -1 )
		{
			m_Events.Add( new SingleInputEvent( inputEventID, method, internalCondition ) );
		}
		else
		{
			m_Events[ index ].Rebind( method, internalCondition );
		}
		return this;
	}

	//////////////////////////////////////////////////////////////////////////
	public	InputEventCollection	Unbind( string inputEventID )
	{
		int index = m_Events.FindIndex( s => s.InputEventID == inputEventID );
		if ( index > -1 ) // Only if found
		{
			m_Events.RemoveAt( index );
		}
		return this;
	}

	//////////////////////////////////////////////////////////////////////////
	public void Call()
	{
		m_Events.ForEach( s => s.Call() );
	}
}