using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class TimersManager : SingletonMonoBehaviour<TimersManager>
{
	private	int id = 0;
	private	int NextId() => id++;


	[System.Serializable]
	private	class InternalTimer {

		[SerializeField] public		readonly	int				id			= -1;
		[SerializeField] private				float			interval	= 0;
		[SerializeField] private				System.Action	action		= () => { };
		[SerializeField] private				bool			repeat		= false;
		[SerializeField] private				bool			scaled		= false;
		[SerializeField] private				float			currentTime	= 0.0f;

		// CONSTRUCTOR
		public InternalTimer( int id, float startDuration, float interval, System.Action action, bool repeat, bool scaled )
		{
			this.id				= id;
			currentTime	= startDuration;
			this.interval		= interval;
			this.action			= action ?? this.action;
			this.repeat			= repeat;
			this.scaled			= scaled;
		}


		/// <summary> Must be kept alive </summary>
		public bool Eval()
		{
			currentTime -= scaled ? Time.deltaTime : Time.unscaledDeltaTime;
			if (currentTime <= 0 )
			{
				action();
				currentTime = interval;
				return repeat;
			}
			return true;
		}
	}

	[SerializeField]
	private List<InternalTimer> m_Timers = new List<InternalTimer>();


	//////////////////////////////////////////////////////////////////////////
	/// <summary> INTERNAL USE ONLY: Construct and add a new timer to list of timers </summary>
	private	int	AddInternal( float startDuration, System.Action action, bool bMustRepeat, float interval, bool bMustBeScaled )
	{
		// Sanity check
		if ( startDuration < 0.0f || ( bMustRepeat && interval <= 0 ) )
		{
			return -1;
		}

		InternalTimer newTimer = new InternalTimer(NextId(), startDuration, interval, action, bMustRepeat, bMustBeScaled );
		m_Timers.Add( newTimer );

		return newTimer.id;
	}


	/// <summary> Add an unscaled interval </summary>
	public	int	AddIntervalUnscaled( float interval, System.Action action ) => AddInternal( startDuration: interval, action: action, bMustRepeat: true, interval: interval, bMustBeScaled: false );
	

	/// <summary> Add an unscaled timer </summary>
	public	int	AddTimerUnscaled( float duration, System.Action action ) => AddInternal( startDuration: duration, action: action, bMustRepeat: false, interval: 0, bMustBeScaled: false );


	/// <summary> Add an scaled interval </summary>
	public	int	AddIntervalScaled( float interval, System.Action action ) => AddInternal( startDuration: interval, action: action, bMustRepeat: true, interval: interval, bMustBeScaled: true );

	
	/// <summary> Add an scaled timer </summary>
	public	int	AddTimerScaled( float duration, System.Action action ) => AddInternal( startDuration: duration, action: action, bMustRepeat: false, interval: 0, bMustBeScaled: true );


	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Search a timer with given id and remove it if found </summary>
	public	void	RemoveTimer( int id )
	{
		if ( id >= 0 )
		{
			int index = m_Timers.FindIndex( t => t.id == id );
			if ( index > -1 )
			{
				m_Timers.RemoveAt( index );
			}
		}
	}



	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		for ( int i = m_Timers.Count - 1; i >= 0; i-- )
		{
			InternalTimer timer = m_Timers[i];

			if ( timer.Eval() == false )
			{
				m_Timers.RemoveAt(i);
			}
		}
	}



}
