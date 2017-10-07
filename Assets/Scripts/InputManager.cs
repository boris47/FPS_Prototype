using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct inputs_t {
		public bool Forward, Backward, StrafeLeft, StrafeRight;
		public bool LeanLeft, LeanRight;
		public bool Crouch, Jump, Run;
		public bool Use;
		public bool Item1, Item2, Item3, Item4;

		public void Reset() {
			Forward = Backward = StrafeLeft = StrafeRight =
			LeanLeft = LeanRight =
			Crouch = Jump = Run =
			Use =
			Item1 = Item2 = Item3 = Item4 = false;
		}

};


public class Inputmanager {

	static bool m_HoldCrouch			= false;
	static public bool HoldCrouch {
		set { m_HoldCrouch = value; }
		get { return m_HoldCrouch; }
	}

	static bool m_HoldJump				= false;
	static public bool HoldJump {
		set { m_HoldJump = value; }
		get { return m_HoldJump; }
	}

	static bool m_HoldRun				= true;
	static public bool HoldRun {
		set { m_HoldRun = value; }
		get { return m_HoldCrouch; }
	}

	private static inputs_t m_Inputs;
	public  static inputs_t	Inputs {
		get { return m_Inputs; }
	}
	
	// Update is called once per frame
	public void	Update () {
		
		Inputs.Reset();

		m_Inputs.Forward		= Input.GetKey ( KeyCode.W ) || Input.GetKey ( KeyCode.UpArrow );
		m_Inputs.Backward		= Input.GetKey ( KeyCode.S ) || Input.GetKey ( KeyCode.DownArrow );
		m_Inputs.StrafeLeft		= Input.GetKey ( KeyCode.A ) || Input.GetKey ( KeyCode.LeftArrow );
		m_Inputs.StrafeRight	= Input.GetKey ( KeyCode.D ) || Input.GetKey ( KeyCode.RightArrow );

		m_Inputs.LeanLeft		= Input.GetKey ( KeyCode.Q ) || Input.GetKey ( KeyCode.Keypad7 );
		m_Inputs.LeanRight		= Input.GetKey ( KeyCode.E ) || Input.GetKey ( KeyCode.Keypad9 );

		m_Inputs.Crouch			= m_HoldCrouch ?
			( Input.GetKey ( KeyCode.LeftControl ) || Input.GetKey ( KeyCode.RightControl ) )
			:
			( Input.GetKeyDown ( KeyCode.LeftControl ) || Input.GetKeyDown ( KeyCode.RightControl ) );

		m_Inputs.Jump			= m_HoldJump ?
			( Input.GetKey ( KeyCode.Space ) || Input.GetKey ( KeyCode.Keypad0 ) )
			:
			( Input.GetKeyDown ( KeyCode.Space ) || Input.GetKeyDown ( KeyCode.Keypad0 ) );

		m_Inputs.Run			= m_HoldRun ?
			( Input.GetKey ( KeyCode.LeftShift ) || Input.GetKey ( KeyCode.RightShift ) )
			:
			( Input.GetKeyDown ( KeyCode.LeftShift ) || Input.GetKeyDown ( KeyCode.RightShift ) );

		m_Inputs.Use			= Input.GetKeyDown ( KeyCode.F ) || Input.GetKeyDown ( KeyCode.Return );

		m_Inputs.Item1			= Input.GetKeyDown ( KeyCode.F1 ) || Input.GetKeyDown ( KeyCode.Keypad1 );
		m_Inputs.Item2			= Input.GetKeyDown ( KeyCode.F2 ) || Input.GetKeyDown ( KeyCode.Keypad2 );
		m_Inputs.Item3			= Input.GetKeyDown ( KeyCode.F3 ) || Input.GetKeyDown ( KeyCode.Keypad3 );
		m_Inputs.Item4			= Input.GetKeyDown ( KeyCode.F4 ) || Input.GetKeyDown ( KeyCode.Keypad4 );

	}
}
