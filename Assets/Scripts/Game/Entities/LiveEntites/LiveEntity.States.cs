using System.Collections;
using System.Collections.Generic;




public abstract partial class LiveEntity {

	[System.Serializable]
	protected class _States {

		public	bool	IsCrouched		= false;

		public	bool	IsMoving		= false;
		public	bool	IsWalking		= false;
		public	bool	IsRunning		= false;

		public	bool	IsLeaning		= false;

		public	bool	IsJumping		= false;
		public	bool	IsHanging		= false;
		public	bool	IsFalling		= false;

		public void Reset()
		{
			IsMoving = IsWalking = IsRunning = IsJumping = IsHanging = IsFalling = false;
		}
	};

	protected   _States			m_States			= new _States();
	[System.NonSerialized]
	protected	_States			m_PreviousStates	= new _States();


	public		bool	IsMoving {		get { return  m_States.IsMoving; }		set { m_States.IsMoving = value; }	}
	public		bool	IsIdle {		get { return  !m_States.IsMoving; }		set { m_States.IsMoving = !value; }	}
	public		bool	IsLeaning {		get { return  m_States.IsLeaning; }		set { m_States.IsLeaning = value; }	}
	public		bool	IsWalking {		get { return  m_States.IsWalking; }		set { m_States.IsWalking = value; }	}

	public		bool	IsRunning {		get { return  m_States.IsRunning; }		set { m_States.IsRunning = value; }	}

	public		bool	IsJumping {		get { return  m_States.IsJumping; }		set { m_States.IsJumping = value; }	}
	public		bool	IsHanging {		get { return  m_States.IsHanging; }		set { m_States.IsHanging = value; }	}
	public		bool	IsFalling {		get { return  m_States.IsFalling; }		set { m_States.IsFalling = value; }	}

	public		bool	IsCrouched {	get { return  m_States.IsCrouched; }	set { m_States.IsCrouched = value; }}

	public		void	ResetStates()	{ m_States.Reset(); }

}