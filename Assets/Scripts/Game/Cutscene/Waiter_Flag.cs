public class Waiter_Flag : Waiter_Base {

	public		void	CanContinue()
	{
		HasToWait = false;
	}

	public		override	void		Wait()
	{

	}

}

