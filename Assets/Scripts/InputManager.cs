using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct inputs_t {
	public	bool	Forward, Backward, StrafeLeft, StrafeRight;
	public	bool	LeanLeft, LeanRight;
	public	bool	Crouch, Jump, Run;
	public	bool	Use;
	public	bool	SwitchPrev, SwitchNext;
	public	bool	Item1, Item2, Item3, Item4;
	public	bool	ItemAction1, ItemAction2, ItemAction3;
	public	bool	Fire1, Fire2, Fire1Loop, Fire2Loop, Fire1Released, Fire2Released;
	public	bool	Reload;


	//////////////////////////////////////////////////////////////////////////
	// Reset
	public void Reset()
	{
		Forward = Backward = StrafeLeft = StrafeRight =
		LeanLeft = LeanRight =
		Crouch = Jump = Run =
		Use =
		SwitchPrev = SwitchNext =
		Item1 = Item2 = Item3 = Item4 =
		ItemAction1 = ItemAction2 = ItemAction3 =
		Fire1 = Fire2 = Fire1Loop = Fire2Loop = Fire1Released = Fire2Released =
		Reload = false;
	}
};


public class InputManager {

	public	static	bool			HoldCrouch		{ get; set; }
	public	static	bool			HoldJump		{ get; set; }
	public	static	bool			HoldRun			{ get; set; }

	private static	inputs_t		m_Inputs;
	public  static	inputs_t		Inputs
	{
		get { return m_Inputs; }
	}

	public	static	bool			IsEnabled		= true;
	
	//////////////////////////////////////////////////////////////////////////
	// Update
	public void	Update()
	{
		if ( IsEnabled == false )
			return;
		
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

		m_Inputs.SwitchPrev		= Input.mouseScrollDelta.y > 0;
		m_Inputs.SwitchNext		= Input.mouseScrollDelta.y < 0;

		m_Inputs.Item1			= Input.GetKeyDown ( KeyCode.F1 ) || Input.GetKeyDown ( KeyCode.Keypad1 );
		m_Inputs.Item2			= Input.GetKeyDown ( KeyCode.F2 ) || Input.GetKeyDown ( KeyCode.Keypad2 );
		m_Inputs.Item3			= Input.GetKeyDown ( KeyCode.F3 ) || Input.GetKeyDown ( KeyCode.Keypad3 );
		m_Inputs.Item4			= Input.GetKeyDown ( KeyCode.F4 ) || Input.GetKeyDown ( KeyCode.Keypad4 );

		m_Inputs.ItemAction1	= Input.GetKeyDown( KeyCode.Mouse1 );
		m_Inputs.ItemAction2	= Input.GetKeyDown( KeyCode.H );
		m_Inputs.ItemAction3	= Input.GetKeyDown( KeyCode.J);

		m_Inputs.Fire1			= Input.GetKeyDown( KeyCode.Mouse0 );
		m_Inputs.Fire2			= Input.GetKeyDown( KeyCode.Mouse2 );
		m_Inputs.Fire1Loop		= Input.GetKey( KeyCode.Mouse0 );
		m_Inputs.Fire2Loop		= Input.GetKey( KeyCode.Mouse2 );
		m_Inputs.Fire1Released	= Input.GetKeyUp( KeyCode.Mouse0 );
		m_Inputs.Fire2Released	= Input.GetKeyUp( KeyCode.Mouse2 );

		m_Inputs.Fire2Released	= Input.GetKeyUp( KeyCode.Mouse2 );

		m_Inputs.Reload			= Input.GetKeyDown( KeyCode.R );
	}
}
