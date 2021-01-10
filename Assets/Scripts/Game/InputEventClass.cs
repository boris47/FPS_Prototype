using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SingleInputEvent
{
	private static	readonly System.Func<bool>				m_AlwaysTrueCondition	= () => true;

	public		string					InputEventID	{ get; private set; } = "NONE";

	private		InputDelegateHandler	m_InputEvent	= null;
	private		System.Func<bool>		m_Condition		= m_AlwaysTrueCondition;


	//////////////////////////////////////////////////////////////////////////
	public SingleInputEvent( string Id, InputDelegateHandler eventToCall, System.Func<bool> condition )
	{
		InputEventID = Id;
		m_InputEvent = eventToCall;
		m_Condition = condition ?? m_AlwaysTrueCondition;
	}

	//////////////////////////////////////////////////////////////////////////
	public	void	Rebind( InputDelegateHandler eventToCall, System.Func<bool> condition = null )
	{
		m_InputEvent = eventToCall;
		m_Condition = condition ?? m_AlwaysTrueCondition;
	}

	//////////////////////////////////////////////////////////////////////////
	public void Call()
	{
		if (m_Condition() )
		{
			m_InputEvent();
		}
	}

}

public class InputEventCollection
{
	private static	readonly System.Func<bool>				m_AlwaysTrueCondition	= () => true;
	private			readonly List<SingleInputEvent>			m_Events				= new List<SingleInputEvent>();

	//////////////////////////////////////////////////////////////////////////
	public	InputEventCollection	Bind( string inputEventID, InputDelegateHandler method, System.Func<bool> predicate )
	{
		System.Func<bool> internalCondition = predicate ?? m_AlwaysTrueCondition;

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