
namespace AI.Behaviours {
	
	public abstract class Behaviour_Base {
		
		protected			IBrain				m_Brain				= null;
		protected			IEntity				m_ThisEntity		= null;

		public		virtual		void			Setup( Brain brain, IEntity ThisEntity )
		{
			m_ThisEntity = ThisEntity;
			m_Brain = brain;
		}

		public		abstract	void			Enable();

		public		abstract	void			Disable();

		public		abstract	void			OnThink();

		public		abstract	void			OnFrame( float DeltaTime );

		protected	virtual		void			print( string msg )
		{
			UnityEngine.Debug.Log( msg );
		}

	}

}