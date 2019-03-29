﻿
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
	/// <summary> Secondary fire ( default: Mouse Middle Button ) </summary>
	FIRE3		= 1 << 11,
	/// <summary> Reload ( default: R ) </summary>
	RELOAD		= 1 << 12,
	/// <summary> All </summary>
	ALL			= MOVE | STATE | ABILITY | USE | SWITCH | SELECTION | ITEM | GADGET | FIRE1 | FIRE2 | FIRE3 | RELOAD
}



/*
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
	public	bool	Fire3, Fire3Loop, Fire3Released;
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
		Fire3 = Fire3Loop = Fire3Released =
		Reload = false;
	}
};
*/
public	delegate	void	InputDelegateHandler();

public class InputManager {

	public	static	bool					HoldCrouch		{ get; set; }
	public	static	bool					HoldJump		{ get; set; }
	public	static	bool					HoldRun			{ get; set; }

//	public  static	inputs_t				Inputs;
	public	static	bool					IsEnabled		= true;

	private	InputFlags						m_Flags			= InputFlags.ALL;

	private	KeyBindings						m_Bindings		= null;

	private	bool							m_IsDirty		= false;

	private	System.Action<KeyCommandPair>	m_CommandPairCheck = null;

	private	Dictionary<eInputCommands, InputEventCollection> m_ActionMap = new Dictionary<eInputCommands, InputEventCollection>();
	

	/// <summary> Return an array structure of bindings </summary>
	public	KeyCommandPair[]				Bindings
	{
		get { return m_Bindings.Pairs.ToArray(); }
	}

	//////////////////////////////////////////////////////////////////////////
	// ( Constructor )
	/// <summary> The default contructor </summary>
	public				InputManager()
	{
		for ( eInputCommands command = eInputCommands.NONE + 1; command < eInputCommands.COUNT; command++ )
		{
			m_ActionMap.Add( command, new InputEventCollection() );
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

		// Create lambda to use in updae
		m_CommandPairCheck = ( KeyCommandPair commandPair ) =>
		{
			// Check Primary and secondary button
			InputEventCollection inputEventCollection = null;

			// Key keys
			KeyCode primary		= commandPair.GetKeyCode( eKeys.PRIMARY );
			KeyCode secondary	= commandPair.GetKeyCode( eKeys.SECONDARY );

			if ( ( commandPair.PrimaryKeyCheck( primary ) || commandPair.SecondaryKeyCheck( secondary ) ) && m_ActionMap.TryGetValue( commandPair.Command, out inputEventCollection ) )
			{
				// call binded delegate
				inputEventCollection.Call();
			}
		};
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
		if ( bHasBeenLoaded == true )
		{
		//	Debug.Log( "InputManager::LoadingBindigns:loading bings fail, using default bindings" );
		//	GenerateDefaultBindings( MustSave: true );
			m_Bindings.Pairs.ForEach( p => p.AssignKeyChecks() );
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
	public		void	BindCall( eInputCommands command, string inputEventID, InputDelegateHandler method, System.Func<bool> condition = null )
	{
		InputEventCollection inputEventCollection = null;
		if ( m_ActionMap.TryGetValue( command, out inputEventCollection ) == false )
		{
			inputEventCollection = new InputEventCollection();
		}

		inputEventCollection.Bind( inputEventID, method, condition );
	}


	//////////////////////////////////////////////////////////////////////////
	// UnbindCall
	/// <summary> Un bind a command from method </summary>
	public		void	UnbindCall( eInputCommands command, string inputEventID )
	{
		InputEventCollection inputEventCollection = null;
		if ( m_ActionMap.TryGetValue( command, out inputEventCollection ) )
		{
			inputEventCollection.Unbind( inputEventID );
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


	//////////////////////////////////////////////////////////////////////////
	// Update
	/// <summary> Update everything about inputs </summary>
	public		void	Update()
	{
		if ( IsEnabled == false )
			return;

		m_Bindings.Pairs.ForEach( m_CommandPairCheck );
	}


	//////////////////////////////////////////////////////////////////////////
	// GenerateDefaultBindings
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings( bool MustSave )
	{
		m_Bindings = new KeyBindings();
		{
			// Movements
			GenerateDefaultBinding( m_Bindings,	eInputCommands.MOVE_FORWARD,			eKeyState.HOLD,				KeyCode.W,			null,	KeyCode.UpArrow			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.MOVE_BACKWARD,			eKeyState.HOLD,				KeyCode.S,			null,	KeyCode.PageDown		);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.MOVE_LEFT,				eKeyState.HOLD,				KeyCode.A,			null,	KeyCode.LeftArrow		);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.MOVE_RIGHT,				eKeyState.HOLD,				KeyCode.D,			null,	KeyCode.RightArrow		);

			// States
			GenerateDefaultBinding( m_Bindings,	eInputCommands.STATE_CROUCH,			eKeyState.HOLD,				KeyCode.LeftControl,null,	KeyCode.RightControl	);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.STATE_JUMP,				eKeyState.PRESS,			KeyCode.Space,		null,	KeyCode.Keypad0			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.STATE_RUN,				eKeyState.HOLD,				KeyCode.LeftShift,	null,	KeyCode.RightShift		);

			// Ability
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ABILITY_PRESS,			eKeyState.PRESS,			KeyCode.Q,			null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ABILITY_HOLD,			eKeyState.HOLD,				KeyCode.Q,			null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ABILITY_RELEASE,			eKeyState.RELEASE,			KeyCode.Q,			null,	KeyCode.None			);

			// Usage
			GenerateDefaultBinding( m_Bindings,	eInputCommands.USAGE,					eKeyState.PRESS,			KeyCode.F,			null,	KeyCode.Return			);

			// Weapons Switch
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SWITCH_PREVIOUS,			eKeyState.SCROLL_UP,		KeyCode.None,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SWITCH_NEXT,				eKeyState.SCROLL_DOWN,		KeyCode.None,		null,	KeyCode.None			);

			// Selection
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION1,				eKeyState.PRESS,			KeyCode.Alpha1,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION2,				eKeyState.PRESS,			KeyCode.Alpha2,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION3,				eKeyState.PRESS,			KeyCode.Alpha3,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION4,				eKeyState.PRESS,			KeyCode.Alpha4,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION5,				eKeyState.PRESS,			KeyCode.Alpha5,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION6,				eKeyState.PRESS,			KeyCode.Alpha6,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION7,				eKeyState.PRESS,			KeyCode.Alpha7,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION8,				eKeyState.PRESS,			KeyCode.Alpha8,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SELECTION9,				eKeyState.PRESS,			KeyCode.Alpha9,		null,	KeyCode.None			);

			// Item 
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ITEM1,					eKeyState.PRESS,			KeyCode.F1,			null,	KeyCode.Keypad1			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ITEM2,					eKeyState.PRESS,			KeyCode.F2,			null,	KeyCode.Keypad2			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ITEM3,					eKeyState.PRESS,			KeyCode.F3,			null,	KeyCode.Keypad3			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.ITEM4,					eKeyState.PRESS,			KeyCode.F4,			null,	KeyCode.Keypad4			);

			// Gadget
			GenerateDefaultBinding( m_Bindings,	eInputCommands.GADGET1,					eKeyState.PRESS,			KeyCode.G,			null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.GADGET2,					eKeyState.PRESS,			KeyCode.H,			null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.GADGET3,					eKeyState.PRESS,			KeyCode.J,			null,	KeyCode.None			);

			// Primary Fire
			GenerateDefaultBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_PRESS,		eKeyState.PRESS,			KeyCode.Mouse0,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse0,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.PRIMARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse0,		null,	KeyCode.None			);

			// Secondary Fire
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_PRESS,	eKeyState.PRESS,			KeyCode.Mouse1,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse1,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.SECONDARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse1,		null,	KeyCode.None			);

			// Secondary Fire
			GenerateDefaultBinding( m_Bindings,	eInputCommands.TERTIARY_FIRE_PRESS,		eKeyState.PRESS,			KeyCode.Mouse2,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.TERTIARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse2,		null,	KeyCode.None			);
			GenerateDefaultBinding( m_Bindings,	eInputCommands.TERTIARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse2,		null,	KeyCode.None			);

			// Reload
			GenerateDefaultBinding( m_Bindings,	eInputCommands.RELOAD_WPN,				eKeyState.PRESS,			KeyCode.R,			null,	KeyCode.End				);
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
	private		void	GenerateDefaultBinding( KeyBindings bindings, eInputCommands command, eKeyState primaryKeyState, KeyCode primaryKey, eKeyState? secondaryKeyState, KeyCode secondaryKey )
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
	// AssignNewKeyState
	/// <summary> Attempt to assign a keyState </summary>
	public		void	AssignNewKeyState( eKeys Key, eKeyState NewKeyState, eInputCommands Command )
	{
		// Find the current command Pair
		KeyCommandPair pair = m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );

		// Assign new KeyState
		pair.Assign( Key, NewKeyState, pair.GetKeyCode( Key ) );
	}

	//////////////////////////////////////////////////////////////////////////
	// AssignNewBinding
	/// <summary> Attempt to assign a keyCode, return a boolean of success. KeyCodes can be swapped if already assigned </summary>
	public		bool	AssignNewKeyCode( eKeys Key, KeyCode NewKeyCode, eInputCommands Command, bool bMustSwap = false )
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );
		
		// Already in Use vars
		KeyCommandPair	alreadyInUsePair		= null;
		eKeys			alreadyInUseKey			= eKeys.PRIMARY;
		eKeyState		alreadyInUseKeyState	= eKeyState.PRESS;
		bool			bIsAlreadyInUse			= false;

		// Find out if already in use
		{
			int alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
			{
				return p.GetKeyCode( eKeys.PRIMARY ) == NewKeyCode && p.GetKeyState( eKeys.PRIMARY ) == currentPair.PrimaryKeyState;
			} );
			// Search for primary keyCode already used
			if ( alreadyUsingPairIndex  != -1 )
			{
				alreadyInUsePair		= m_Bindings.Pairs[ alreadyUsingPairIndex ];
				alreadyInUseKey			= eKeys.PRIMARY;
				alreadyInUseKeyState	= alreadyInUsePair.GetKeyState( eKeys.PRIMARY );
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;

			// Search for secondary keyCode already used
			if ( bIsAlreadyInUse == false )
			{
				alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
				{
					return p.GetKeyCode( eKeys.SECONDARY ) == NewKeyCode && p.GetKeyState( eKeys.SECONDARY ) == currentPair.PrimaryKeyState;
				} );
				if ( alreadyUsingPairIndex  != -1 )
				{
					alreadyInUsePair		= m_Bindings.Pairs[ alreadyUsingPairIndex ];
					alreadyInUseKey			= eKeys.SECONDARY;
					alreadyInUseKeyState	= alreadyInUsePair.GetKeyState( eKeys.SECONDARY );
				}
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;
		}

		// Swapping KeyCode and keyState
		if ( bIsAlreadyInUse == true && bMustSwap == true )
		{
			KeyCode thisKeyCode		= currentPair.GetKeyCode( alreadyInUseKey );
			eKeyState thiskeyState	= currentPair.GetKeyState( alreadyInUseKey );
			if ( alreadyInUseKey == Key )
			{
				currentPair.Assign		( alreadyInUseKey,	thiskeyState,			NewKeyCode		);	// current selected
				alreadyInUsePair.Assign	( alreadyInUseKey,	alreadyInUseKeyState,	thisKeyCode		);	// already set swapping
			}
			else
			{
				currentPair.Assign		( Key,				thiskeyState,			NewKeyCode		);	// current selected
				alreadyInUsePair.Assign	( alreadyInUseKey,	alreadyInUseKeyState,	thisKeyCode		);	// Already set swapping
			}

			m_IsDirty = true;
		}

		// Can be assigned
		if ( bIsAlreadyInUse == false )
		{
			currentPair.Assign( Key,	null,	NewKeyCode );
			
			m_IsDirty = true;
		}

		bool result = !bIsAlreadyInUse;

		// Not available key
		if ( bIsAlreadyInUse == true && bMustSwap == false )
		{
			Utils.Msg.MSGCRT
			(
				"InputManager::AssignNewBinding: Trying to assign keycode {0}, used for command {1}, as {2} key", 
				NewKeyCode.ToString(),
				alreadyInUsePair.Command.ToString(),
				alreadyInUseKey.ToString()
			);
			result = false;
		}

		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	// CanNewKeyCodeBeAssigned
	/// <summary> Return boolean if a keyCode for given command at specified key can be assigned </summary>
	public		bool	CanNewKeyCodeBeAssigned( eKeys key, KeyCode NewKeyCode, eInputCommands Command )
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );

		// Result
		bool			bIsAlreadyInUse			= false;

		// Find out if already in use
		{
			// Search for primary keyCode already used
			int alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
			{
				return p.GetKeyCode( eKeys.PRIMARY ) == NewKeyCode && p.GetKeyState( eKeys.PRIMARY ) == currentPair.PrimaryKeyState;
			} );

			bIsAlreadyInUse = alreadyUsingPairIndex != -1;

			// Search for secondary keyCode already used
			if ( bIsAlreadyInUse == false )
			{
				alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
				{
					return p.GetKeyCode( eKeys.SECONDARY ) == NewKeyCode && p.GetKeyState( eKeys.SECONDARY ) == currentPair.PrimaryKeyState;
				} );
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;
		}

		return bIsAlreadyInUse == false;
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
	STATE_CROUCH, STATE_JUMP, STATE_RUN,
	MOVE_FORWARD, MOVE_BACKWARD, MOVE_LEFT, MOVE_RIGHT,
	ABILITY_PRESS, ABILITY_HOLD, ABILITY_RELEASE,
	USAGE,
	SWITCH_PREVIOUS, SWITCH_NEXT,
	SELECTION1, SELECTION2, SELECTION3, SELECTION4, SELECTION5, SELECTION6, SELECTION7, SELECTION8, SELECTION9,
	ITEM1, ITEM2, ITEM3, ITEM4,
	GADGET1, GADGET2, GADGET3,
	PRIMARY_FIRE_PRESS, PRIMARY_FIRE_HOLD, PRIMARY_FIRE_RELEASE,
	SECONDARY_FIRE_PRESS, SECONDARY_FIRE_HOLD, SECONDARY_FIRE_RELEASE,
	TERTIARY_FIRE_PRESS, TERTIARY_FIRE_HOLD, TERTIARY_FIRE_RELEASE,
	RELOAD_WPN,
	COUNT
}

[System.Serializable]
/// <summary> Command pair simple class </summary>
public	class KeyCommandPair {

	[SerializeField]
	public	eKeyState			PrimaryKeyState		= eKeyState.PRESS;

	[SerializeField]
	public	eKeyState			SecondaryKeyState	= eKeyState.PRESS;

	[SerializeField]
	private	KeyCode				PrimaryKey			= KeyCode.None;

	[SerializeField]
	private	KeyCode				SecondaryKey		= KeyCode.None;

	[SerializeField]
	public	eInputCommands		Command				= eInputCommands.NONE;

	[SerializeField]
	public	int					PrimaryCheck		= 0;

	[SerializeField]
	public	int					SecondaryCheck		= 0;

	public	System.Func<KeyCode, bool> PrimaryKeyCheck		= null;
	public	System.Func<KeyCode, bool> SecondaryKeyCheck	= null;

	//
	public	void	Setup( eInputCommands Command, eKeyState PrimaryKeyState, KeyCode PrimaryKey, eKeyState SecondaryKeyState, KeyCode SecondaryKey )
	{
		this.Command				= Command;
		this.PrimaryKeyState		= PrimaryKeyState;
		this.PrimaryKey				= PrimaryKey;
		this.SecondaryKeyState		= SecondaryKeyState;
		this.SecondaryKey			= SecondaryKey;

		PrimaryCheck = (int)PrimaryKeyState;
		SecondaryCheck = (int)SecondaryKeyState;

		AssignKeyChecks();
	}


	public	void	AssignKeyChecks()
	{
		eKeyState primaryKeyState	= ( eKeyState )PrimaryCheck;
		eKeyState secondaryKeyState = ( eKeyState )SecondaryCheck;

		System.Func<KeyCode, bool> ScrollUpCheck   = ( KeyCode k ) => { return Input.mouseScrollDelta.y > 0f; };
		System.Func<KeyCode, bool> ScrollDownCheck = ( KeyCode k ) => { return Input.mouseScrollDelta.y < 0f; };
		switch ( primaryKeyState )
		{
			case eKeyState.PRESS:		PrimaryKeyCheck	= Input.GetKeyDown;		break;
			case eKeyState.HOLD:		PrimaryKeyCheck	= Input.GetKey;			break;
			case eKeyState.RELEASE:		PrimaryKeyCheck	= Input.GetKeyUp;		break;
			case eKeyState.SCROLL_UP:	PrimaryKeyCheck	= ScrollUpCheck;		break;
			case eKeyState.SCROLL_DOWN:	PrimaryKeyCheck	= ScrollDownCheck;		break;
			default:
				{
					Debug.Log( "WARNING: Command " + Command.ToString() + " has invalid \"PrimaryKeyCheck\" assigned" );
					PrimaryKeyCheck = Input.GetKeyDown;
					break;
				}
		}
		switch ( secondaryKeyState )
		{
			case eKeyState.PRESS:		SecondaryKeyCheck	= Input.GetKeyDown;		break;
			case eKeyState.HOLD:		SecondaryKeyCheck	= Input.GetKey;			break;
			case eKeyState.RELEASE:		SecondaryKeyCheck	= Input.GetKeyUp;		break;
			case eKeyState.SCROLL_UP:	SecondaryKeyCheck	= ScrollUpCheck;		break;
			case eKeyState.SCROLL_DOWN:	SecondaryKeyCheck	= ScrollDownCheck;		break;
			default:
				{
					Debug.Log( "WARNING: Command " + Command.ToString() + " has invalid \"SecondaryKeyCheck\" assigned" );
					SecondaryKeyCheck = Input.GetKeyDown;
					break;
				}
		}
	}

	//
	public	void	Assign( eKeys key, eKeyState? keyState, KeyCode? keyCode )
	{
		if ( keyCode.HasValue )
		{
			switch ( key )
			{
				case eKeys.PRIMARY:		PrimaryKey		= keyCode.Value;	break;
				case eKeys.SECONDARY:	SecondaryKey	= keyCode.Value;	break;
				default:				break;
			}
		}

		if ( keyState.HasValue )
		{
			switch ( key )
			{
				case eKeys.PRIMARY:		PrimaryKeyState		= keyState.Value;		break;
				case eKeys.SECONDARY:	SecondaryKeyState	= keyState.Value;		break;
				default:				break;
			}

			AssignKeyChecks();
		}
	}

	//
	public	KeyCode	GetKeyCode( eKeys key )
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
	public	eKeyState	GetKeyState( eKeys key )
	{
		eKeyState keyState = eKeyState.PRESS;
		switch ( key )
		{
			case eKeys.PRIMARY:		keyState	= PrimaryKeyState;			break;
			case eKeys.SECONDARY:	keyState	= SecondaryKeyState;		break;
			default:				break;
		}

		return keyState;
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