using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct inputs_t {
	public bool Forward, Backward, StrafeLeft, StrafeRight;
	public bool LeanLeft, LeanRight;
	public bool Crouch, Jump, Run;
	public bool Use;
	public bool Item1, Item2, Item3, Item4;

	public void Reset()
	{
		Forward = Backward = StrafeLeft = StrafeRight =
		LeanLeft = LeanRight =
		Crouch = Jump = Run =
		Use =
		Item1 = Item2 = Item3 = Item4 = false;
	}
};


public class Inputmanager {

	public	static bool HoldCrouch		{ get; set; }
	public	static bool HoldJump		{ get; set; }
	public	static bool HoldRun			{ get; set; }

	private static inputs_t m_Inputs;
	public  static inputs_t	Inputs
	{
		get { return m_Inputs; }
	}
	
	// Update is called once per frame
	public void	Update ()
	{
		
		Inputs.Reset();

		m_Inputs.Forward		= Input.GetKey ( KeyCode.W ) || Input.GetKey ( KeyCode.UpArrow );
		m_Inputs.Backward		= Input.GetKey ( KeyCode.S ) || Input.GetKey ( KeyCode.DownArrow );
		m_Inputs.StrafeLeft		= Input.GetKey ( KeyCode.A ) || Input.GetKey ( KeyCode.LeftArrow );
		m_Inputs.StrafeRight	= Input.GetKey ( KeyCode.D ) || Input.GetKey ( KeyCode.RightArrow );

		m_Inputs.LeanLeft		= Input.GetKey ( KeyCode.Q ) || Input.GetKey ( KeyCode.Keypad7 );
		m_Inputs.LeanRight		= Input.GetKey ( KeyCode.E ) || Input.GetKey ( KeyCode.Keypad9 );

		m_Inputs.Crouch			= HoldCrouch ?
								( Input.GetKey ( KeyCode.LeftControl ) || Input.GetKey ( KeyCode.RightControl ) )
								:
								( Input.GetKeyDown ( KeyCode.LeftControl ) || Input.GetKeyDown ( KeyCode.RightControl ) );

		m_Inputs.Jump			= HoldJump ?
								( Input.GetKey ( KeyCode.Space ) || Input.GetKey ( KeyCode.Keypad0 ) )
								:
								( Input.GetKeyDown ( KeyCode.Space ) || Input.GetKeyDown ( KeyCode.Keypad0 ) );

		m_Inputs.Run			= HoldRun ?
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
