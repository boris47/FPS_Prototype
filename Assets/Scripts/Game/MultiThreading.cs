using System.Threading;

public	static	class	MultiThreading
{
	public	static	Thread	CreateThread( System.Action body, bool bCanStart = true, System.Action onCompletion = null )
	{
		if ( body == null )
			return null;

		ThreadStart starter = delegate
		{
			body();
		};

		if ( onCompletion != null )
		{
			starter += delegate
			{
				onCompletion();
			};
		}

		Thread thread = new Thread( starter );

		if ( bCanStart )
		{
			thread.Start();
		}

		return thread;
	}
	
}
