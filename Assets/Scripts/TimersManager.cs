using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class TimersManager
{

	private class ScheduledTask
	{
		internal readonly System.Action Action;
		internal System.Timers.Timer Timer;
		internal System.EventHandler TaskComplete;

		public ScheduledTask(System.Action action, float timeoutMS)
		{
			Action = action;
			Timer = new System.Timers.Timer() { Interval = timeoutMS };
			Timer.Elapsed += TimerElapsed;            
		}

		private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Timer.Stop();
			Timer.Elapsed -= TimerElapsed;
			Timer = null;

			Action();
			TaskComplete(this, System.EventArgs.Empty);
		}
	}

	private static readonly Dictionary<System.Action, ScheduledTask> m_ScheduledTasks = new Dictionary<System.Action, ScheduledTask>();


	//////////////////////////////////////////////////////////////////////////
    public static void AddTimer(System.Action action, float timeoutMS )
    {
        ScheduledTask task = new ScheduledTask( action, timeoutMS );
        task.TaskComplete += RemoveTask;
        m_ScheduledTasks.Add( action, task );
        task.Timer.Start();
    }


	//////////////////////////////////////////////////////////////////////////
    private static void RemoveTask(object sender, System.EventArgs e)
    {
        ScheduledTask task = (ScheduledTask) sender;
        task.TaskComplete -= RemoveTask;
        m_ScheduledTasks.Remove(task.Action);
    }

}
