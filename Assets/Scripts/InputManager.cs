
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

public class InputManager {

	public	static	bool					HoldCrouch		{ get; set; }
	public	static	bool					HoldJump		{ get; set; }
	public	static	bool					HoldRun			{ get; set; }

	public  static	inputs_t				Inputs;
	public	static	bool					IsEnabled		= true;

	private	InputFlags						m_Flags			= InputFlags.ALL;

	private	KeyBindings						m_Bindings		= null;

	private	bool							m_IsDirty		= false;

	private class InputEventClass {

		private	InputDelegateHandler m_InputEvent = null;

		private	static	InputDelegateHandler m_EmptyMethod = () => { };

		public	InputEventClass()
		{
			m_InputEvent = () => { };	// Ensure at last one call, this avoids null check every call
		}

		public	InputEventClass	Bind( InputDelegateHandler method )
		{
			m_InputEvent = method;
			return this;
		}

		public	InputEventClass	Unbind()
		{
			m_InputEvent = m_EmptyMethod;
			return this;
		}

		public void Call()
		{
			m_InputEvent();
		}
	}

	private	Dictionary<eInputCommands, InputEventClass> m_ActionMap = new Dictionary<eInputCommands, InputEventClass>();
	

	//////////////////////////////////////////////////////////////////////////
	// ( Constructor )
	/// <summary> The default contructor </summary>
	public				InputManager()
	{
		for ( eInputCommands command = eInputCommands.NONE + 1; command < eInputCommands.COUNT; command++ )
		{
			m_ActionMap.Add( command, new InputEventClass() );
		}

		// C:/Users/Drako/AppData/LocalLow/BeWide&Co/Project Orion
		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		if ( System.IO.File.Exists( bindingsPath ) )
		{
			ReadBindings();
		}
		else
		{
			GenerateDefaultBindings( MustSave: true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// SaveBindings
	/// <summary> Save current bindings </summary>
	public		void	SaveBindings()
	{
		if ( m_IsDirty == false || m_Bindings == null )
			return;

		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		string data = JsonUtility.ToJson( m_Bindings, prettyPrint: true );
		System.IO.File.WriteAllText( bindingsPath, data );
		m_IsDirty = false;
	}


	//////////////////////////////////////////////////////////////////////////
	// LoadBindings
	/// <summary> Try to load bindings, return boolean success value </summary>
	public		bool	LoadBindings()
	{
		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		if ( System.IO.File.Exists( bindingsPath ) == false )
			return false;

		string data = System.IO.File.ReadAllText( bindingsPath );
		m_Bindings = JsonUtility.FromJson<KeyBindings>( data );

		bool bHasBeenLoaded = m_Bindings != null;
		if ( bHasBeenLoaded == false )
		{
			Debug.Log( "InputManager::LoadingBindigns:loading bings fail, using default bindings" );
			GenerateDefaultBindings( MustSave: true );
		}
		return bHasBeenLoaded;
	}


	//////////////////////////////////////////////////////////////////////////
	// ResetBindings
	/// <summary> Reset bindings and save </summary>
	public		void	ResetBindings()
	{
		GenerateDefaultBindings( MustSave: false );
	}


	//////////////////////////////////////////////////////////////////////////
	// BindCall
	/// <summary> Allow to bind a method to specified command </summary>
	public		void	BindCall( eInputCommands command, InputDelegateHandler method )
	{
		InputEventClass methodWrapper = null;
		if ( m_ActionMap.TryGetValue( command, out methodWrapper ) )
		{
			methodWrapper.Bind( method );
		}
		else
		{
			m_ActionMap.Add( command, new InputEventClass().Bind( method ) );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// UnbindCall
	/// <summary> Un bind a command from method </summary>
	public		void	UnbindCall( eInputCommands command )
	{
		InputEventClass methodWrapper = null;
		if ( m_ActionMap.TryGetValue( command, out methodWrapper ) )
		{
			methodWrapper.Unbind();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// SetFlags
	public		void	SetFlags( InputFlags flags )
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
	public		void	RemoveFlags( InputFlags flags )
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


	System.Func<KeyCode, bool> ScrollUpCheck   = ( KeyCode k ) => { return Input.mouseScrollDelta.y > 0f; };
	System.Func<KeyCode, bool> ScrollDownCheck = ( KeyCode k ) => { return Input.mouseScrollDelta.y < 0f; };
	//////////////////////////////////////////////////////////////////////////
	// Update
	/// <summary> Update everything about inputs </summary>
	public		void	Update()
	{
		if ( IsEnabled == false )
			return;

		System.Action<KeyCommandPair> commandPairCheck = ( KeyCommandPair commandPair ) =>
		{
			// Choose the check function based on the requested key state 
			System.Func<KeyCode, bool> primaryKeyCheck = null;
			System.Func<KeyCode, bool> secondaryKeyCheck = null;
			switch ( commandPair.PrimaryKeyState )
			{
				case eKeyState.PRESS:		primaryKeyCheck	= Input.GetKeyDown;		break;
				case eKeyState.HOLD:		primaryKeyCheck	= Input.GetKey;			break;
				case eKeyState.RELEASE:		primaryKeyCheck	= Input.GetKeyUp;		break;
				case eKeyState.SCROLL_UP:	primaryKeyCheck	= ScrollUpCheck;		break;
				case eKeyState.SCROLL_DOWN:	primaryKeyCheck	= ScrollDownCheck;		break;
			}
			switch ( commandPair.SecondaryKeyState )
			{
				case eKeyState.PRESS:		secondaryKeyCheck	= Input.GetKeyDown;		break;
				case eKeyState.HOLD:		secondaryKeyCheck	= Input.GetKey;			break;
				case eKeyState.RELEASE:		secondaryKeyCheck	= Input.GetKeyUp;		break;
				case eKeyState.SCROLL_UP:	secondaryKeyCheck	= ScrollUpCheck;		break;
				case eKeyState.SCROLL_DOWN:	secondaryKeyCheck	= ScrollDownCheck;		break;
			}

			// Check Primary and secondary button
			InputEventClass methodWrapper = null;

			// Key keys
			KeyCode primary = commandPair.Get( eKeys.PRIMARY );
			KeyCode secondary = commandPair.Get( eKeys.SECONDARY );

			if ( ( primaryKeyCheck( primary ) || secondaryKeyCheck( secondary ) ) && m_ActionMap.TryGetValue( commandPair.Command, out methodWrapper ) )
			{
				// call binded delegate
				methodWrapper.Call();
			}
		};
		m_Bindings.Pairs.ForEach( commandPairCheck );

		#region old
		
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
		
		#endregion
	}


	//////////////////////////////////////////////////////////////////////////
	// GenerateDefaultBindings
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings( bool MustSave )
	{
		m_Bindings = new KeyBindings();
		{
			// Movements
			GenerateBinding( m_Bindings,	eInputCommands.MOVE_FORWARD,			eKeyState.HOLD,				KeyCode.W,			null,	KeyCode.UpArrow			);
			GenerateBinding( m_Bindings,	eInputCommands.MOVE_BACKWARD,			eKeyState.HOLD,				KeyCode.S,			null,	KeyCode.PageDown		);
			GenerateBinding( m_Bindings,	eInputCommands.MOVE_LEFT,				eKeyState.HOLD,				KeyCode.A,			null,	KeyCode.LeftArrow		);
			GenerateBinding( m_Bindings,	eInputCommands.MOVE_RIGHT,				eKeyState.HOLD,				KeyCode.D,			null,	KeyCode.RightArrow		);

			// States
			GenerateBinding( m_Bindings,	eInputCommands.STATE_CROUCH,			eKeyState.HOLD,				KeyCode.LeftControl,null,	KeyCode.RightControl	);
			GenerateBinding( m_Bindings,	eInputCommands.STATE_JUMP,				eKeyState.PRESS,			KeyCode.Space,		null,	KeyCode.Keypad0			);
			GenerateBinding( m_Bindings,	eInputCommands.STATE_RUN,				eKeyState.HOLD,				KeyCode.LeftShift,	null,	KeyCode.RightShift		);

			// Ability
			GenerateBinding( m_Bindings,	eInputCommands.ABILITY_PRESS,			eKeyState.PRESS,			KeyCode.Q,			null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.ABILITY_HOLD,			eKeyState.HOLD,				KeyCode.Q,			null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.ABILITY_RELEASE,			eKeyState.RELEASE,			KeyCode.Q,			null,	KeyCode.None			);

			// Usage
			GenerateBinding( m_Bindings,	eInputCommands.USAGE,					eKeyState.PRESS,			KeyCode.F,			null,	KeyCode.Return			);

			// Weapons Switch
			GenerateBinding( m_Bindings,	eInputCommands.SWITCH_PREVIOUS,			eKeyState.SCROLL_UP,		KeyCode.None,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SWITCH_NEXT,				eKeyState.SCROLL_DOWN,		KeyCode.None,		null,	KeyCode.None			);

			// Selection
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION1,				eKeyState.PRESS,			KeyCode.Alpha1,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION2,				eKeyState.PRESS,			KeyCode.Alpha2,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION3,				eKeyState.PRESS,			KeyCode.Alpha3,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION4,				eKeyState.PRESS,			KeyCode.Alpha4,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION5,				eKeyState.PRESS,			KeyCode.Alpha5,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION6,				eKeyState.PRESS,			KeyCode.Alpha6,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION7,				eKeyState.PRESS,			KeyCode.Alpha7,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION8,				eKeyState.PRESS,			KeyCode.Alpha8,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SELECTION9,				eKeyState.PRESS,			KeyCode.Alpha9,		null,	KeyCode.None			);

			// Item 
			GenerateBinding( m_Bindings,	eInputCommands.ITEM1,					eKeyState.PRESS,			KeyCode.F1,			null,	KeyCode.Keypad1			);
			GenerateBinding( m_Bindings,	eInputCommands.ITEM2,					eKeyState.PRESS,			KeyCode.F2,			null,	KeyCode.Keypad2			);
			GenerateBinding( m_Bindings,	eInputCommands.ITEM3,					eKeyState.PRESS,			KeyCode.F3,			null,	KeyCode.Keypad3			);
			GenerateBinding( m_Bindings,	eInputCommands.ITEM4,					eKeyState.PRESS,			KeyCode.F4,			null,	KeyCode.Keypad4			);

			// Gadget
			GenerateBinding( m_Bindings,	eInputCommands.GADGET1,					eKeyState.PRESS,			KeyCode.G,			null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.GADGET2,					eKeyState.PRESS,			KeyCode.H,			null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.GADGET3,					eKeyState.PRESS,			KeyCode.J,			null,	KeyCode.None			);

			// Primary Fire
			GenerateBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_PRESS,		eKeyState.PRESS,			KeyCode.Mouse0,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse0,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse0,		null,	KeyCode.None			);

			// Secondary Fire
			GenerateBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_PRESS,	eKeyState.PRESS,			KeyCode.Mouse1,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse1,		null,	KeyCode.None			);
			GenerateBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse1,		null,	KeyCode.None			);

			// Reload
			GenerateBinding( m_Bindings,	eInputCommands.RELOAD_WPN,				eKeyState.PRESS,			KeyCode.R,			null,	KeyCode.End				);
		}

		m_IsDirty = true;

		if ( MustSave )
		{
			SaveBindings();
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	// GenerateBinding
	/// <summary> Generate default bindings </summary>
	private		void	GenerateBinding( KeyBindings bindings, eInputCommands command, eKeyState primaryKeyState, KeyCode primaryKey, eKeyState? secondaryKeyState, KeyCode secondaryKey )
	{
		eKeyState _secondaryKeyState = primaryKeyState;
		if ( secondaryKeyState.HasValue == true )
		{
			_secondaryKeyState = secondaryKeyState.Value;
		}

		KeyCommandPair commandPair = new KeyCommandPair();
		commandPair.Setup( command, primaryKeyState, primaryKey, _secondaryKeyState, secondaryKey );
		bindings.Pairs.Add( commandPair );
	}


	//////////////////////////////////////////////////////////////////////////
	// AssignNewBinding
	/// <summary> Attempt to assign a key, return a boolean success. Keys can be swapped </summary>
	public		bool	AssignNewBinding( KeyCode KeyToAssign, eKeys Key, eKeyState newKeyState, eInputCommands command, bool bMustSwap = false )
	{
		// Check if already assigned to this command
		m_Bindings.Pairs.ForEach( ( KeyCommandPair pair ) =>
		{
			eKeyState useless = eKeyState.PRESS;
			KeyCode primary = KeyCode.None, secondary = KeyCode.None;
			pair.Get( eKeys.PRIMARY,	ref primary, ref useless );
			pair.Get( eKeys.SECONDARY,	ref secondary, ref useless);
			if ( pair.Command == command && ( primary == KeyToAssign || secondary == KeyToAssign ) )
			{
				return;
			}
		});


		bool bIsKeyAvailable			= true;
		KeyCommandPair pairKeyChange	= null;
		KeyCommandPair pairAlreadyUsing = null;
		eKeys alreadyInUseKey			= eKeys.PRIMARY;

		// searching if already assigned keyCode
		foreach( KeyCommandPair pair in m_Bindings.Pairs )
		{
			// Find the pair to modify
			if ( pair.Command == command )
			{
				pairKeyChange = pair;
			}
			else
			// Only check for different commands
//			if ( pair.Command != command )
			{
				KeyCode primary = pair.Get( eKeys.PRIMARY );
				KeyCode secondary = pair.Get( eKeys.SECONDARY );
				// Others primary keyCode Check
				{
					bIsKeyAvailable &=  primary != KeyToAssign;
					if ( bIsKeyAvailable == false )
					{
						pairAlreadyUsing = pair;
						alreadyInUseKey = eKeys.PRIMARY;
						break;
					}
				}

				// Others Secondary keyCode Check
				{
					bIsKeyAvailable &= secondary != KeyToAssign;
					if ( bIsKeyAvailable == false )
					{
						pairAlreadyUsing = pair;
						alreadyInUseKey = eKeys.SECONDARY;
						break;
					}
				}
			}
		};

		// Can be assigned
		if ( bMustSwap == false & bIsKeyAvailable == true && pairKeyChange != null )
		{
			switch ( Key )
			{
				case eKeys.PRIMARY:		pairKeyChange.Assign( eKeys.PRIMARY,	newKeyState,	KeyToAssign );	break;
				case eKeys.SECONDARY:	pairKeyChange.Assign( eKeys.SECONDARY,	newKeyState,	KeyToAssign );	break;
			}
			m_IsDirty = true;
		}

		// Key Swap
		if ( bMustSwap == true & pairKeyChange != null && pairAlreadyUsing != null )
		{
			KeyCode thisKeyCode		= KeyCode.None;
			eKeyState thisKeyState	= eKeyState.PRESS;
			pairKeyChange.Get( alreadyInUseKey, ref thisKeyCode, ref thisKeyState );
			if ( alreadyInUseKey == Key )
			{
				pairKeyChange.Assign	( alreadyInUseKey,	newKeyState,	KeyToAssign		);	// current selected
				pairAlreadyUsing.Assign	( alreadyInUseKey,	thisKeyState,	thisKeyCode		);	// already set swapping
			}
			else
			{
				pairKeyChange.Assign	( Key,				newKeyState,	KeyToAssign		);	// current selected
				pairAlreadyUsing.Assign	( alreadyInUseKey,	thisKeyState,	thisKeyCode		);	// Already set swapping
			}

			m_IsDirty = true;

			// Avoid Error
			bIsKeyAvailable = true;
		}

		// Not available key
		if ( bIsKeyAvailable == false )
		{
			Utils.Msg.MSGCRT
			(
				"InputManager::AssignNewBinding: Trying to assign keycode {0}, used for command {1}, as {2} key", 
				KeyToAssign.ToString(),
				pairAlreadyUsing.Command.ToString(),
				alreadyInUseKey.ToString()
			);
			return false;
		}

		// Command not found
		if ( pairKeyChange == null )
		{
			Utils.Msg.MSGCRT
			(
				"InputManager::AssignNewBinding: Unable to find the command %s to which set the keyCode %s", 
				command.ToString(),
				KeyToAssign.ToString()
			);
			return false;
		}

		// Success
		return true;
	}
	

	//////////////////////////////////////////////////////////////////////////
	// ReadBindings
	/// <summary> Attempt to read bindigns from file </summary>
	public		void	ReadBindings()
	{
		bool bHasBeenLoaded = LoadBindings();
		if ( bHasBeenLoaded == false )
		{
			string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
			Debug.Log( "Unable to load key bindings at path " + bindingsPath );
			GenerateDefaultBindings( MustSave: false );
		}
	}


	public		KeyCommandPair[]	GetBindings()
	{
		List<KeyCommandPair> bindingsList = new List<KeyCommandPair>();
		{
			m_Bindings.Pairs.ForEach( ( p ) =>
			{
				KeyCommandPair info = new KeyCommandPair();
				info.Setup( p.Command, p.PrimaryKeyState, p.Get( eKeys.PRIMARY ), p.SecondaryKeyState, p.Get( eKeys.SECONDARY ) );
				bindingsList.Add( info );
			} );
		}
		return bindingsList.ToArray();
	}
}



[System.Serializable]
/// <summary> Enum for key state evaluation choice </summary>
public	enum eKeyState { PRESS, HOLD, RELEASE, SCROLL_UP, SCROLL_DOWN }

[System.Serializable]
/// <summary> enum for keys</summary>
public	enum eKeys { PRIMARY, SECONDARY }

[System.Serializable]
/// <summary> enum of commands to link keys at </summary>
public	enum eInputCommands {
	NONE,
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
	RELOAD_WPN,
	COUNT
}

[System.Serializable]
/// <summary> Command pair simple class </summary>
public	class KeyCommandPair {

	[SerializeField]
	public	eInputCommands		Command				= eInputCommands.NONE;

	[SerializeField]
	public	eKeyState			PrimaryKeyState		= eKeyState.PRESS;

	[SerializeField]
	public	eKeyState			SecondaryKeyState	= eKeyState.PRESS;

	[SerializeField]
	private	KeyCode				PrimaryKey			= KeyCode.None;

	[SerializeField]
	private	KeyCode				SecondaryKey		= KeyCode.None;

	//
	public	void	Setup( eInputCommands Command, eKeyState PrimaryKeyState, KeyCode PrimaryKey, eKeyState SecondaryKeyState, KeyCode SecondaryKey )
	{
		this.Command				= Command;
		this.PrimaryKeyState		= PrimaryKeyState;
		this.PrimaryKey				= PrimaryKey;
		this.SecondaryKeyState		= SecondaryKeyState;
		this.SecondaryKey			= SecondaryKey;
	}

	//
	public	void	Assign( eKeys key, eKeyState keyState, KeyCode keyCode )
	{
		switch ( key )
		{
			case eKeys.PRIMARY:		PrimaryKey		= keyCode;		PrimaryKeyState		= keyState;		break;
			case eKeys.SECONDARY:	SecondaryKey	= keyCode;		SecondaryKeyState	= keyState;		break;
			default:				break;
		}
	}

	//
	public	KeyCode	Get( eKeys key )
	{
		KeyCode code = KeyCode.None;
		switch ( key )
		{
			case eKeys.PRIMARY:		code	= PrimaryKey;				break;
			case eKeys.SECONDARY:	code	= SecondaryKey;				break;
			default:				break;
		}

		return code;
	}

	//
	public	void	Get( eKeys key, ref KeyCode keyCode, ref eKeyState keyState )
	{
		switch ( key )
		{
			case eKeys.PRIMARY:		keyCode	= PrimaryKey;		keyState = PrimaryKeyState;		break;
			case eKeys.SECONDARY:	keyCode	= SecondaryKey;		keyState = SecondaryKeyState;	break;
			default:				break;
		}
	}

}

[System.Serializable]
/// <summary> Main object that store bindings and serialized objects </summary>
public	class KeyBindings {

	[SerializeField]
	public	List<KeyCommandPair> Pairs = new List<KeyCommandPair>();

};