
using System.Collections.Generic;
using UnityEngine;

[System.FlagsAttribute] 
public enum InputFlags {
	/// <summary> Controls movements ( default: WASD ) </summary>
	MOVE		= 1 << 01,
	/// <summary> Cotnrols State ( default: CTRL, SFIFT ) </summary>
	STATE		= 1 << 02,
	/// <summary> Ability input ( default: Q ) </summary>
	ABILITY		= 1 << 03,
	/// <summary> Usage input ( default: E and F ) </summary>
	USE			= 1 << 04,
	/// <summary> Weapons switch ( default: Mouse wheel ) </summary>
	SWITCH		= 1 << 05,
	/// <summary> Selection input ( default: 1 ... 9 ) </summary>
	SELECTION	= 1 << 06,
	/// <summary> Item usage ( default: F1 .. F4 ) </summary>
	ITEM		= 1 << 07,
	/// <summary> Accessory usage ( default: G, H, J ) </summary>
	GADGET		= 1 << 08,
	/// <summary> Primary fire ( default: Mouse Left Button ) </summary>
	FIRE1		= 1 << 09,
	/// <summary> Secondary fire ( default: Mouse Right Button ) </summary>
	FIRE2		= 1 << 10,
	/// <summary> Reload ( default: R ) </summary>
	RELOAD		= 1 << 11,
	/// <summary> All </summary>
	ALL			= MOVE | STATE | ABILITY | USE | SWITCH | SELECTION | ITEM | GADGET | FIRE1 | FIRE2 | RELOAD
}




public struct inputs_t {

	public	bool	Forward, Backward, StrafeLeft, StrafeRight;
	public	bool	Crouch, Jump, Run;
	public	bool	Ability1, Ability1Loop, Ability1Released;
	public	bool	Use;
	public	bool	SwitchPrev, SwitchNext;
	public	bool	Selection1, Selection2, Selection3, Selection4, Selection5, Selection6, Selection7, Selection8, Selection9;
	public	bool	Item1, Item2, Item3, Item4;
	public	bool	Gadget1, Gadget2, Gadget3;
	public	bool	Fire1, Fire1Loop, Fire1Released;
	public	bool	Fire2, Fire2Loop, Fire2Released;
	public	bool	Reload;


	//////////////////////////////////////////////////////////////////////////
	// Reset
	public void Reset()
	{
		Forward = Backward = StrafeLeft = StrafeRight =
		Crouch = Jump = Run =
		Ability1Loop = Ability1 = Ability1Released = 
		Use =
		SwitchPrev = SwitchNext =
		Selection1 = Selection2 = Selection3 = Selection4 = Selection5 = Selection6 = Selection7 = Selection8 = Selection9 =
		Item1 = Item2 = Item3 = Item4 =
		Gadget1 = Gadget2 = Gadget3 =
		Fire1 = Fire1Loop = Fire1Released =
		Fire2 = Fire2Loop = Fire2Released =
		Reload = false;
	}
};

public	delegate	void	InputDelegateHandler();

public class InputEventClass {

	static int id = 0;
	private InputEventClass thisaa;

	private	event InputDelegateHandler m_InputEvent = null;
	private int iid;

	public	InputEventClass()
	{
		iid = id;
		id ++;
//		m_InputEvent = () => { };	// Ensure at last one call, this avoids null check every call
	}

	public	void	Bind( InputDelegateHandler method )
	{
		m_InputEvent += method;
		thisaa = this;
	}

	public	void	Unbind( InputDelegateHandler method )
	{
		m_InputEvent -= method;
	}

	public void Call()
	{
		if ( thisaa == this )

		m_InputEvent();
	}
}

public class InputManager {

	public	static	bool			HoldCrouch		{ get; set; }
	public	static	bool			HoldJump		{ get; set; }
	public	static	bool			HoldRun			{ get; set; }

	public  static	inputs_t		Inputs;

	public	static	bool			IsEnabled		= true;

	private	InputFlags				m_Flags			= InputFlags.ALL;


	private	KeyBindings				m_Bindings		= null;

	private	Dictionary<InputCommands, InputEventClass> m_ActionMap = new Dictionary<InputCommands, InputEventClass>();
	

	//////////////////////////////////////////////////////////////////////////
	// ( Constructor )
	public InputManager()
	{
		foreach ( InputCommands command in System.Enum.GetValues(typeof(InputCommands)))
		{
			m_ActionMap.Add( command, new InputEventClass() );
		}

		GenerateDefaultBindings();
	}


	//////////////////////////////////////////////////////////////////////////
	// BindCall
	public	void	BindCall( InputCommands command, InputDelegateHandler method )
	{
		m_ActionMap[command].Bind( method );
	}

	//////////////////////////////////////////////////////////////////////////
	// UnbindCall
	public	void	UnbindCall( InputCommands command, InputDelegateHandler method )
	{
		m_ActionMap[command].Unbind( method );
	}


	//////////////////////////////////////////////////////////////////////////
	// SetFlags
	public	void	SetFlags( InputFlags flags )
	{
		if ( ( m_Flags & InputFlags.MOVE )			== 0 && ( flags & InputFlags.MOVE )			!= 0 ) m_Flags |= InputFlags.MOVE;
		if ( ( m_Flags & InputFlags.STATE )			== 0 && ( flags & InputFlags.STATE )		!= 0 ) m_Flags |= InputFlags.STATE;
		if ( ( m_Flags & InputFlags.ABILITY )		== 0 && ( flags & InputFlags.ABILITY )		!= 0 ) m_Flags |= InputFlags.ABILITY;
		if ( ( m_Flags & InputFlags.USE )			== 0 && ( flags & InputFlags.USE )			!= 0 ) m_Flags |= InputFlags.USE;
		if ( ( m_Flags & InputFlags.SWITCH )		== 0 && ( flags & InputFlags.SWITCH )		!= 0 ) m_Flags |= InputFlags.SWITCH;
		if ( ( m_Flags & InputFlags.SELECTION )		== 0 && ( flags & InputFlags.SELECTION )	!= 0 ) m_Flags |= InputFlags.SELECTION;
		if ( ( m_Flags & InputFlags.ITEM )			== 0 && ( flags & InputFlags.ITEM )			!= 0 ) m_Flags |= InputFlags.ITEM;
		if ( ( m_Flags & InputFlags.GADGET )		== 0 && ( flags & InputFlags.GADGET )		!= 0 ) m_Flags |= InputFlags.GADGET;
		if ( ( m_Flags & InputFlags.FIRE1 )			== 0 && ( flags & InputFlags.FIRE1 )		!= 0 ) m_Flags |= InputFlags.FIRE1;
		if ( ( m_Flags & InputFlags.FIRE2 )			== 0 && ( flags & InputFlags.FIRE2 )		!= 0 ) m_Flags |= InputFlags.FIRE2;
		if ( ( m_Flags & InputFlags.RELOAD )		== 0 && ( flags & InputFlags.RELOAD )		!= 0 ) m_Flags |= InputFlags.RELOAD;
	}


	//////////////////////////////////////////////////////////////////////////
	// RemoveFlags
	public	void	RemoveFlags( InputFlags flags )
	{
		if ( ( m_Flags & InputFlags.MOVE )			!= 0 && ( flags & InputFlags.MOVE )			== 0 ) m_Flags &= ~InputFlags.MOVE;
		if ( ( m_Flags & InputFlags.STATE )			!= 0 && ( flags & InputFlags.STATE )		== 0 ) m_Flags &= ~InputFlags.STATE;
		if ( ( m_Flags & InputFlags.ABILITY )		!= 0 && ( flags & InputFlags.ABILITY )		== 0 ) m_Flags &= ~InputFlags.ABILITY;
		if ( ( m_Flags & InputFlags.USE )			!= 0 && ( flags & InputFlags.USE )			== 0 ) m_Flags &= ~InputFlags.USE;
		if ( ( m_Flags & InputFlags.SWITCH )		!= 0 && ( flags & InputFlags.SWITCH )		== 0 ) m_Flags &= ~InputFlags.SWITCH;
		if ( ( m_Flags & InputFlags.SELECTION )		!= 0 && ( flags & InputFlags.SELECTION )	== 0 ) m_Flags &= ~InputFlags.SELECTION;
		if ( ( m_Flags & InputFlags.ITEM )			!= 0 && ( flags & InputFlags.ITEM )			== 0 ) m_Flags &= ~InputFlags.ITEM;
		if ( ( m_Flags & InputFlags.GADGET )		!= 0 && ( flags & InputFlags.GADGET )		== 0 ) m_Flags &= ~InputFlags.GADGET;
		if ( ( m_Flags & InputFlags.FIRE1 )			!= 0 && ( flags & InputFlags.FIRE1 )		== 0 ) m_Flags &= ~InputFlags.FIRE1;
		if ( ( m_Flags & InputFlags.FIRE2 )			!= 0 && ( flags & InputFlags.FIRE2 )		== 0 ) m_Flags &= ~InputFlags.FIRE2;
		if ( ( m_Flags & InputFlags.RELOAD )		!= 0 && ( flags & InputFlags.RELOAD )		== 0 ) m_Flags &= ~InputFlags.RELOAD;
	}


	//////////////////////////////////////////////////////////////////////////
	// ScrollUpCheck
	private	bool	ScrollUpCheck( KeyCode k )
	{
		return Input.mouseScrollDelta.y > 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// ScrollDownCheck
	private	bool	ScrollDownCheck( KeyCode k )
	{
		return Input.mouseScrollDelta.y < 0f;
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	public	void	Update()
	{
		if ( IsEnabled == false )
			return;
		
		m_Bindings.Pairs.ForEach
		(
			( KeyCommandPair commandPair ) =>
			{
				// Choose the check function based on the requested key state 
				System.Func<KeyCode, bool> keyCheck = null;
				switch ( commandPair.KeyState )
				{
					case KeyState.PRESS:		keyCheck	= Input.GetKeyDown;		break;
					case KeyState.HOLD:			keyCheck	= Input.GetKey;			break;
					case KeyState.RELEASE:		keyCheck	= Input.GetKeyUp;		break;
					case KeyState.SCROLL_UP:	keyCheck	= ScrollUpCheck;		break;
					case KeyState.SCROLL_DOWN:	keyCheck	= ScrollDownCheck;		break;
				}

				// Check Primary and secodnary button
				if ( keyCheck( commandPair.Keys[0] ) || keyCheck( commandPair.Keys[1] ) )
				{
					// call binded delegate
					m_ActionMap[commandPair.Command].Call();
				}
			}
		);


		#region old
		/*
		Inputs.Reset();

		if ( ( m_Flags & InputFlags.MOVE ) != 0 )
		{
			Inputs.Forward				= Input.GetKey ( KeyCode.W ) || Input.GetKey ( KeyCode.UpArrow );
			Inputs.Backward				= Input.GetKey ( KeyCode.S ) || Input.GetKey ( KeyCode.DownArrow );
			Inputs.StrafeLeft			= Input.GetKey ( KeyCode.A ) || Input.GetKey ( KeyCode.LeftArrow );
			Inputs.StrafeRight			= Input.GetKey ( KeyCode.D ) || Input.GetKey ( KeyCode.RightArrow );
		}

		if ( ( m_Flags & InputFlags.STATE ) != 0 )
		{
			Inputs.Crouch				= HoldCrouch ?
										( Input.GetKey ( KeyCode.LeftControl ) || Input.GetKey ( KeyCode.RightControl ) )
										:
										( Input.GetKeyDown ( KeyCode.LeftControl ) || Input.GetKeyDown ( KeyCode.RightControl ) );

			Inputs.Jump					= HoldJump ?
										( Input.GetKey ( KeyCode.Space ) || Input.GetKey ( KeyCode.Keypad0 ) )
										:
										( Input.GetKeyDown ( KeyCode.Space ) || Input.GetKeyDown ( KeyCode.Keypad0 ) );

			Inputs.Run					= HoldRun ?
										( Input.GetKey ( KeyCode.LeftShift ) || Input.GetKey ( KeyCode.RightShift ) )
										:
										( Input.GetKeyDown ( KeyCode.LeftShift ) || Input.GetKeyDown ( KeyCode.RightShift ) );
		}

		if ( ( m_Flags & InputFlags.ABILITY ) != 0 )
		{
			Inputs.Ability1				= Input.GetKeyDown ( KeyCode.Q );
			Inputs.Ability1Loop			= Input.GetKey ( KeyCode.Q );
			Inputs.Ability1Released		= Input.GetKeyUp ( KeyCode.Q );
		}

		if ( ( m_Flags & InputFlags.USE ) != 0 )
		{
			Inputs.Use					= Input.GetKeyDown ( KeyCode.F ) || Input.GetKeyDown ( KeyCode.Return );
		}

		if ( ( m_Flags & InputFlags.SWITCH ) != 0 )
		{
			Inputs.SwitchPrev			= Input.mouseScrollDelta.y > 0;
			Inputs.SwitchNext			= Input.mouseScrollDelta.y < 0;
		}

		if ( ( m_Flags & InputFlags.SELECTION ) != 0 )
		{
			Inputs.Selection1			= Input.GetKeyDown( KeyCode.Alpha1 );
			Inputs.Selection2			= Input.GetKeyDown( KeyCode.Alpha2 );
			Inputs.Selection3			= Input.GetKeyDown( KeyCode.Alpha3 );
			Inputs.Selection4			= Input.GetKeyDown( KeyCode.Alpha4 );
			Inputs.Selection5			= Input.GetKeyDown( KeyCode.Alpha5 );
			Inputs.Selection6			= Input.GetKeyDown( KeyCode.Alpha6 );
			Inputs.Selection7			= Input.GetKeyDown( KeyCode.Alpha7 );
			Inputs.Selection8			= Input.GetKeyDown( KeyCode.Alpha8 );
			Inputs.Selection9			= Input.GetKeyDown( KeyCode.Alpha9 );
		}

		if ( ( m_Flags & InputFlags.ITEM ) != 0 )
		{
			Inputs.Item1				= Input.GetKeyDown ( KeyCode.F1 ) || Input.GetKeyDown ( KeyCode.Keypad1 );
			Inputs.Item2				= Input.GetKeyDown ( KeyCode.F2 ) || Input.GetKeyDown ( KeyCode.Keypad2 );
			Inputs.Item3				= Input.GetKeyDown ( KeyCode.F3 ) || Input.GetKeyDown ( KeyCode.Keypad3 );
			Inputs.Item4				= Input.GetKeyDown ( KeyCode.F4 ) || Input.GetKeyDown ( KeyCode.Keypad4 );
		}

		if ( ( m_Flags & InputFlags.GADGET ) != 0 )
		{
			Inputs.Gadget1				= Input.GetKeyDown( KeyCode.G );
			Inputs.Gadget2				= Input.GetKeyDown( KeyCode.H );
			Inputs.Gadget3				= Input.GetKeyDown( KeyCode.J);
		}

		if ( ( m_Flags & InputFlags.FIRE1 ) != 0 )
		{
			Inputs.Fire1				= Input.GetKeyDown( KeyCode.Mouse0 );
			Inputs.Fire1Loop			= Input.GetKey( KeyCode.Mouse0 );
			Inputs.Fire1Released		= Input.GetKeyUp( KeyCode.Mouse0 );
		}

		if ( ( m_Flags & InputFlags.FIRE2 ) != 0 )
		{
			Inputs.Fire2				= Input.GetKeyDown( KeyCode.Mouse1 );
			Inputs.Fire2Loop			= Input.GetKey( KeyCode.Mouse1 );
			Inputs.Fire2Released		= Input.GetKeyUp( KeyCode.Mouse1 );
		}


		if ( ( m_Flags & InputFlags.RELOAD ) != 0 )
		{
			Inputs.Reload				= Input.GetKeyDown( KeyCode.R );
		}
		*/
		#endregion
	}

	private	void	GenerateDefaultBindings()
	{
		m_Bindings = new KeyBindings();
		{
			// Movements
			GenerateBinding( m_Bindings,	InputCommands.MOVE_FORWARD,				KeyState.HOLD,			KeyCode.W,				KeyCode.UpArrow			);
			GenerateBinding( m_Bindings,	InputCommands.MOVE_BACKWARD,			KeyState.HOLD,			KeyCode.S,				KeyCode.PageDown		);
			GenerateBinding( m_Bindings,	InputCommands.MOVE_LEFT,				KeyState.HOLD,			KeyCode.A,				KeyCode.LeftArrow		);
			GenerateBinding( m_Bindings,	InputCommands.MOVE_RIGHT,				KeyState.HOLD,			KeyCode.D,				KeyCode.RightArrow		);

			// States
			GenerateBinding( m_Bindings,	InputCommands.STATE_CROUCH,				KeyState.HOLD,			KeyCode.LeftControl,	KeyCode.RightControl	);
			GenerateBinding( m_Bindings,	InputCommands.STATE_JUMP,				KeyState.PRESS,			KeyCode.Space,			KeyCode.Keypad0			);
			GenerateBinding( m_Bindings,	InputCommands.STATE_RUN,				KeyState.HOLD,			KeyCode.LeftShift,		KeyCode.RightShift		);

			// Ability
			GenerateBinding( m_Bindings,	InputCommands.ABILITY_PRESS,			KeyState.PRESS,			KeyCode.Q,				KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.ABILITY_HOLD,				KeyState.HOLD,			KeyCode.Q,				KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.ABILITY_RELEASE,			KeyState.RELEASE,		KeyCode.Q,				KeyCode.None			);

			// Usage
			GenerateBinding( m_Bindings,	InputCommands.USAGE,					KeyState.PRESS,			KeyCode.F,				KeyCode.Return			);

			// Weapons Switch
			GenerateBinding( m_Bindings,	InputCommands.SWITCH_PREVIOUS,			KeyState.SCROLL_UP,		KeyCode.None,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SWITCH_NEXT,				KeyState.SCROLL_DOWN,	KeyCode.None,			KeyCode.None			);

			// Selection
			GenerateBinding( m_Bindings,	InputCommands.SELECTION1,				KeyState.PRESS,			KeyCode.Alpha1,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION2,				KeyState.PRESS,			KeyCode.Alpha2,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION3,				KeyState.PRESS,			KeyCode.Alpha3,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION4,				KeyState.PRESS,			KeyCode.Alpha4,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION5,				KeyState.PRESS,			KeyCode.Alpha5,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION6,				KeyState.PRESS,			KeyCode.Alpha6,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION7,				KeyState.PRESS,			KeyCode.Alpha7,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION8,				KeyState.PRESS,			KeyCode.Alpha8,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SELECTION9,				KeyState.PRESS,			KeyCode.Alpha9,			KeyCode.None			);

			// Item 
			GenerateBinding( m_Bindings,	InputCommands.ITEM1,					KeyState.PRESS,			KeyCode.F1,				KeyCode.Keypad1			);
			GenerateBinding( m_Bindings,	InputCommands.ITEM2,					KeyState.PRESS,			KeyCode.F2,				KeyCode.Keypad2			);
			GenerateBinding( m_Bindings,	InputCommands.ITEM3,					KeyState.PRESS,			KeyCode.F3,				KeyCode.Keypad3			);
			GenerateBinding( m_Bindings,	InputCommands.ITEM4,					KeyState.PRESS,			KeyCode.F4,				KeyCode.Keypad4			);

			// Gadget
			GenerateBinding( m_Bindings,	InputCommands.GADGET1,					KeyState.PRESS,			KeyCode.G,				KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.GADGET2,					KeyState.PRESS,			KeyCode.H,				KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.GADGET3,					KeyState.PRESS,			KeyCode.J,				KeyCode.None			);

			// Primary Fire
			GenerateBinding( m_Bindings,	InputCommands.PRIMARY_FIRE_PRESS,		KeyState.PRESS,			KeyCode.Mouse0,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.PRIMARY_FIRE_HOLD,		KeyState.HOLD,			KeyCode.Mouse0,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.PRIMARY_FIRE_RELEASE,		KeyState.RELEASE,		KeyCode.Mouse0,			KeyCode.None			);

			// Secondary Fire
			GenerateBinding( m_Bindings,	InputCommands.SECONDARY_FIRE_PRESS,		KeyState.PRESS,			KeyCode.Mouse1,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SECONDARY_FIRE_HOLD,		KeyState.HOLD,			KeyCode.Mouse1,			KeyCode.None			);
			GenerateBinding( m_Bindings,	InputCommands.SECONDARY_FIRE_RELEASE,	KeyState.RELEASE,		KeyCode.Mouse1,			KeyCode.None			);

			// Reload
			GenerateBinding( m_Bindings,	InputCommands.RELOAD_WPN,				KeyState.PRESS,			KeyCode.R,				KeyCode.End				);
		}

		JsonUtility.ToJson( m_Bindings, prettyPrint: true );
	}

	private	void	GenerateBinding( KeyBindings bindings, InputCommands command, KeyState keyState, KeyCode mainKey, KeyCode secondaryKey )
	{
		bindings.Pairs.Add( new KeyCommandPair()
			{	Command		= InputCommands.MOVE_BACKWARD,
				KeyState	= keyState,
				Keys		= new List<KeyCode>( new KeyCode[] {
					mainKey,	secondaryKey
				} )
			}
		);
	}

	public	void	ReadBindings()
	{
//		JsonUtility.FromJson
	}
}

[System.Serializable]
public enum KeyState {
	PRESS, HOLD, RELEASE, SCROLL_UP, SCROLL_DOWN
}

[System.Serializable]
public	enum InputCommands {
	MOVE_FORWARD, MOVE_BACKWARD, MOVE_LEFT, MOVE_RIGHT,
	STATE_CROUCH, STATE_JUMP, STATE_RUN,
	ABILITY_PRESS, ABILITY_HOLD, ABILITY_RELEASE,
	USAGE,
	SWITCH_PREVIOUS, SWITCH_NEXT,
	SELECTION1, SELECTION2, SELECTION3, SELECTION4, SELECTION5, SELECTION6, SELECTION7, SELECTION8, SELECTION9,
	ITEM1, ITEM2, ITEM3, ITEM4,
	GADGET1, GADGET2, GADGET3,
	PRIMARY_FIRE_PRESS, PRIMARY_FIRE_HOLD, PRIMARY_FIRE_RELEASE,
	SECONDARY_FIRE_PRESS, SECONDARY_FIRE_HOLD, SECONDARY_FIRE_RELEASE,
	RELOAD_WPN
}

[System.Serializable]
public	struct KeyCommandPair {

	[SerializeField]
	public	InputCommands	Command;

	[SerializeField]
	public	KeyState		KeyState;

	[SerializeField]
	public	List< KeyCode >	Keys;

}

[System.Serializable]
public	class KeyBindings {

	[SerializeField]
	public	List<KeyCommandPair> Pairs = new List<KeyCommandPair>();

};