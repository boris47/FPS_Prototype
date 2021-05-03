using System.Collections.Generic;


public class SingleInputEvent
{
	private static	readonly System.Func<bool>				m_AlwaysTrueCondition	= () => true;

	public		string					InputEventID	{ get; private set; } = "NONE";

	private		System.Action			m_InputEvent	= null;
	private		System.Func<bool>		m_Condition		= m_AlwaysTrueCondition;


	//////////////////////////////////////////////////////////////////////////
	public SingleInputEvent( string Id, System.Action eventToCall, System.Func<bool> condition )
	{
		InputEventID = Id;
		m_InputEvent = eventToCall;
		m_Condition = condition ?? m_AlwaysTrueCondition;
	}

	//////////////////////////////////////////////////////////////////////////
	public	void	Rebind( System.Action eventToCall, System.Func<bool> condition = null )
	{
		m_InputEvent = eventToCall;
		m_Condition = condition ?? m_AlwaysTrueCondition;
	}

	//////////////////////////////////////////////////////////////////////////
	public void Call()
	{
		if (m_Condition())
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
	public	InputEventCollection	Bind( string inputEventID, System.Action method, System.Func<bool> predicate )
	{
		CustomAssertions.IsTrue(!string.IsNullOrEmpty(inputEventID));

		System.Func<bool> internalCondition = predicate ?? m_AlwaysTrueCondition;
		int index = m_Events.FindIndex(s => s.InputEventID == inputEventID);
		if (index == -1)
		{
			m_Events.Add(new SingleInputEvent(inputEventID, method, internalCondition));
		}
		else
		{
			m_Events[index].Rebind(method, internalCondition);
		}
		return this;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Removed an event if id is Specified otherwise all the events </summary>
	/// <param name="inputEventID">If null all events are removed</param>
	public	InputEventCollection	Unbind( string inputEventID = null)
	{
		if (inputEventID.IsNotNull())
		{
			int index = m_Events.FindIndex(s => s.InputEventID == inputEventID);
			if (index > -1) // Only if found
			{
				m_Events.RemoveAt(index);
			}
		}
		else
		{
			m_Events.Clear();
		}
		return this;
	}

	//////////////////////////////////////////////////////////////////////////
	public void Call()
	{
		m_Events.ForEach( s => s.Call() );
	}
}