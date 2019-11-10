using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class TimersManager : SingletonMonoBehaviour<TimersManager>
{
	private	int id = 0;
	private	int NextId() => id++;

	public	enum TimerType {
		SINGLE_CALL,
		REPEAT
	};

	private	enum TimerContext {
		SCALED, UNSCALED
	}

	[System.Serializable]
	private	class InternalTimer {
		public	float currentTime;
		public	float interval;
		public	TimerType type;
		public	TimerContext context;
		public	System.Action action;
		public	int id;
	}

	[SerializeField]
	private List<InternalTimer> m_Timers = new List<InternalTimer>();


	//
	private	int	AddInternal( float duration, System.Action action, bool bMustRepeat, float interval, bool bMustBeScaled )
	{
		if ( duration < 0.0f )
		{
			return -1;
		}

		if ( bMustRepeat && interval <= 0 )
		{
			return -1;
		}

		if ( action == null )
		{
			action = () => { };
		}

		InternalTimer newTimer = new InternalTimer()
		{
			currentTime = duration,
			interval = interval,
			type = bMustRepeat ? TimerType.REPEAT : TimerType.SINGLE_CALL,
			context = bMustBeScaled ? TimerContext.SCALED : TimerContext.UNSCALED,
			action = action,
			id = NextId()
		};

		m_Timers.Add( newTimer );

		return newTimer.id;
	}



	//
	public	int	AddIntervalUnscaled( float interval, System.Action action )
	{
		return this.AddInternal( duration: interval, action: action, bMustRepeat: true, interval: interval, bMustBeScaled: false );
	}

	
	//
	public	int	AddTimerUnscaled( float duration, System.Action action )
	{
		return this.AddInternal( duration: duration, action: action, bMustRepeat: false, interval: 0, bMustBeScaled: false );
	}



	//
	public	int	AddIntervalScaled( float interval, System.Action action )
	{
		return this.AddInternal( duration: interval, action: action, bMustRepeat: true, interval: interval, bMustBeScaled: true );
	}

	
	//
	public	int	AddTimerScaled( float duration, System.Action action )
	{
		return this.AddInternal( duration: duration, action: action, bMustRepeat: false, interval: 0, bMustBeScaled: true );
	}


	//
	public	void	RemoveTimer( int id )
	{
		if ( id == -1 )
		{
			return;
		}

		int index = m_Timers.FindIndex( t => t.id == id );
		if ( index > -1 )
		{
			m_Timers.RemoveAt( index );
		}
	}


	//
	private void Update()
	{
		for ( int i = m_Timers.Count - 1; i >= 0; i-- )
		{
			InternalTimer timer = m_Timers[i];

			float timeToRemove = timer.context == TimerContext.SCALED ? Time.deltaTime : Time.unscaledDeltaTime;
			timer.currentTime-= timeToRemove;

			if ( timer.currentTime <= 0 )
			{
				timer.action();
				timer.currentTime = timer.interval;
				if ( timer.type == TimerType.SINGLE_CALL )
				{
					m_Timers.RemoveAt(i);
				}
			}
		}
	}



}
