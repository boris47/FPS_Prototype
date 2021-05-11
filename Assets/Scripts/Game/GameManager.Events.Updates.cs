
using UnityEngine;




//	DELEGATES FOR EVENTS
public partial struct GameEvents
{
	// UPDATES
	public	delegate	void		OnThinkEvent();								// UpdateEvents.OnThink
	public	delegate	void		OnPhysicFrameEvent(float FixedDeltaTime);	// UpdateEvents.OnPhysicFrame
	public	delegate	void		OnFrameEvent(float DeltaTime);				// UpdateEvents.OnFrame
}




//////////////////////////////////////////////////////////////////
//							UPDATES								//
//////////////////////////////////////////////////////////////////

public interface IUpdateEvents
{
	/// <summary> TODO </summary>
		event		GameEvents.OnThinkEvent			OnThink;

	/// <summary> TODO </summary>
		event		GameEvents.OnPhysicFrameEvent	OnPhysicFrame;

	/// <summary> TODO </summary>
		event		GameEvents.OnFrameEvent			OnFrame;

		event		GameEvents.OnFrameEvent			OnLateFrame;
}

// UPDATES IMPLEMENTATION
public partial class GameManager : IUpdateEvents
{
	private static event	GameEvents.OnThinkEvent			m_OnThink				= delegate { };
	private static event	GameEvents.OnPhysicFrameEvent	m_OnPhysicFrame			= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnFrame				= delegate { };
	private static event	GameEvents.OnFrameEvent			m_OnLateFrame			= delegate { };

	public	static			IUpdateEvents					UpdateEvents			=> m_Instance;

	event		GameEvents.OnThinkEvent			IUpdateEvents.OnThink
	{
		add		{	if (value.IsNotNull())	m_OnThink += value;	}
		remove	{	if (value.IsNotNull())	m_OnThink -= value;	}
	}

	event		GameEvents.OnPhysicFrameEvent	IUpdateEvents.OnPhysicFrame
	{
		add		{	if (value.IsNotNull())	m_OnPhysicFrame += value; }
		remove	{	if (value.IsNotNull())	m_OnPhysicFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnFrame
	{
		add		{	if (value.IsNotNull())	m_OnFrame += value;	}
		remove	{	if (value.IsNotNull())	m_OnFrame -= value; }
	}

	event		GameEvents.OnFrameEvent			IUpdateEvents.OnLateFrame
	{
		add		{	if (value.IsNotNull())	m_OnLateFrame += value;	}
		remove	{	if (value.IsNotNull())	m_OnLateFrame -= value; }
	}

	private void ResetUpdateEvents()
	{
		m_OnThink				= delegate { };
		m_OnPhysicFrame			= delegate { };
		m_OnFrame				= delegate { };
		m_OnLateFrame			= delegate { };
	}
}

