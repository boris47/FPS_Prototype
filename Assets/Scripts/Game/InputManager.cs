
using System.Collections.Generic;
using UnityEngine;

[System.FlagsAttribute] 
public enum InputCategory : uint {
	NONE		= 0,
	/// <summary> Cotnrols State </summary>
	STATE		= 01,
	/// <summary> Controls movements </summary>
	MOVE		= 02,
	/// <summary> Ability input </summary>
	ABILITY		= 03,
	/// <summary> Usage input </summary>
	USE			= 04,
	/// <summary> Weapons switch </summary>
	SWITCH		= 05,
	/// <summary> Selection input </summary>
	SELECTION	= 06,
	/// <summary> Item usage </summary>
	ITEM		= 07,
	/// <summary> Accessory usage </summary>
	GADGET		= 08,
	/// <summary> Primary fire </summary>
	FIRE1		= 09,
	/// <summary> Secondary fire </summary>
	FIRE2		= 10,
	/// <summary> Secondary fire </summary>
	FIRE3		= 11,
	/// <summary> Reload </summary>
	RELOAD		= 12,
	/// <summary> In Game Interface </summary>
	INTERFACE	= 13,
	/// <summary> Categories Count </summary>
	COUNT,
	/// <summary> All categories </summary>
	ALL			= STATE | MOVE | ABILITY | USE | SWITCH | SELECTION | ITEM | GADGET | FIRE1 | FIRE2 | FIRE3 | RELOAD | INTERFACE,
}

public	delegate	void	InputDelegateHandler();

//[System.Serializable]
public class InputManager {

//	public	static	bool					HoldCrouch				{ get; set; }
//	public	static	bool					HoldJump				{ get; set; }
//	public	static	bool					HoldRun					{ get; set; }

	public	static	bool					IsEnabled				= true;

	[SerializeField]
	private	KeyBindings						m_Bindings		= null;

//	[SerializeField]
	private	InputCategory					m_InputCategories		= InputCategory.ALL;

	private	bool							m_IsDirty		= false;

	private	Dictionary<eInputCommands, InputEventCollection> m_ActionMap = new Dictionary<eInputCommands, InputEventCollection>();
	

	/// <summary> Return an array structure of bindings </summary>
	public	KeyCommandPair[]				Bindings
	{
		get { return m_Bindings.Pairs.ToArray(); }
	}

	//////////////////////////////////////////////////////////////////////////
	// ( Constructor )
	/// <summary> The default contructor </summary>
	public		void		Setup()
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
			GenerateDefaultBindings( bMustSave: true );
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
		if ( bHasBeenLoaded == true )
		{
			// Is useful when added new command
			if ( m_Bindings.Pairs.Count != (int)eInputCommands.COUNT-1 )
			{
				string[] missingCommands = System.Array.FindAll( System.Enum.GetNames( typeof( eInputCommands ) ), 
					name => {
						if ( name == "NONE" || name == "COUNT" )
							return false;

						return m_Bindings.Pairs.FindIndex( pair => pair.Command.ToString() == name ) == -1;
					}
				);

				Debug.Log( "InputManager::LoadBindings: Commands Diff:" );
				System.Array.ForEach( missingCommands, s => Debug.Log( s ) );
				
				GenerateDefaultBindings(true);
				return true;
			}


		//	Debug.Log( "InputManager::LoadBindings:loading bings fail, using default bindings" );
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
		GenerateDefaultBindings( bMustSave: false );
	}


	//////////////////////////////////////////////////////////////////////////
	// BindCall
	/// <summary> Allow to bind a method to specified command </summary>
	public		void	BindCall( eInputCommands command, string inputEventID, InputDelegateHandler action, System.Func<bool> predicate = null )
	{
		InputEventCollection inputEventCollection = null;
		if ( m_ActionMap.TryGetValue( command, out inputEventCollection ) == false )
		{
			inputEventCollection = new InputEventCollection();
			m_ActionMap[command] = inputEventCollection;
		}

		inputEventCollection.Bind( inputEventID, action, predicate );
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
	// EnableCategory
	public		void	EnableCategory( InputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet( m_InputCategories, category ) == false )
		{
			Utils.FlagsHelper.Set( ref m_InputCategories, category );
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	// DisableCategory
	public		void	DisableCategory( InputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet( m_InputCategories, category ) )
		{
			Utils.FlagsHelper.Unset( ref m_InputCategories, category );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ToggleCategory
	public		void	ToggleCategory( InputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet( m_InputCategories, category ) )
		{
			Utils.FlagsHelper.Unset( ref m_InputCategories, category );
		}
		else
		{
			Utils.FlagsHelper.Set( ref m_InputCategories, category );
		}
	}


	private		void	CommandPairCheck( KeyCommandPair commandPair )
	{
		InputCategory inputFlag	= commandPair.Category;
		bool bIsAvailable = Utils.FlagsHelper.IsSet( m_InputCategories, inputFlag );
		if ( bIsAvailable )																				// Firstly we check if category is enabled
		{
			if ( commandPair.IsPrimaryKeyUsed() || commandPair.IsSecondaryKeyUsed() ) // If a command key ceck is satisfied
			{
				InputEventCollection inputEventCollection = null;
				if ( m_ActionMap.TryGetValue( commandPair.Command, out inputEventCollection ) )			// if command event collection is found
				{
					inputEventCollection.Call();														// call binded delegate
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// Update
	/// <summary> Update everything about inputs </summary>
	public		void	Update()
	{
		if ( IsEnabled == false )
			return;

		m_Bindings.Pairs.ForEach( CommandPairCheck );
	}


	//////////////////////////////////////////////////////////////////////////
	// GenerateDefaultBindings
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings( bool bMustSave )
	{
		m_Bindings = new KeyBindings();
		{	// DEFAULT BINDINGS			CATEGORY				INPUT COMMAND							PRIMARY KEY STATE			PRIMARY KEY			SECONDARY KEY		SECONDARY KEY STATE
			// Movements
			GenerateDefaultBinding( InputCategory.MOVE,			eInputCommands.MOVE_FORWARD,			eKeyState.HOLD,				KeyCode.W,			null,				KeyCode.UpArrow			);
			GenerateDefaultBinding( InputCategory.MOVE,			eInputCommands.MOVE_BACKWARD,			eKeyState.HOLD,				KeyCode.S,			null,				KeyCode.PageDown		);
			GenerateDefaultBinding( InputCategory.MOVE,			eInputCommands.MOVE_LEFT,				eKeyState.HOLD,				KeyCode.A,			null,				KeyCode.LeftArrow		);
			GenerateDefaultBinding( InputCategory.MOVE,			eInputCommands.MOVE_RIGHT,				eKeyState.HOLD,				KeyCode.D,			null,				KeyCode.RightArrow		);

			// States
			GenerateDefaultBinding( InputCategory.STATE,		eInputCommands.STATE_CROUCH,			eKeyState.HOLD,				KeyCode.LeftControl,null,				KeyCode.RightControl	);
			GenerateDefaultBinding( InputCategory.STATE,		eInputCommands.STATE_JUMP,				eKeyState.PRESS,			KeyCode.Space,		null,				KeyCode.Keypad0			);
			GenerateDefaultBinding( InputCategory.STATE,		eInputCommands.STATE_RUN,				eKeyState.HOLD,				KeyCode.LeftShift,	null,				KeyCode.RightShift		);

			// Ability
			GenerateDefaultBinding( InputCategory.ABILITY,		eInputCommands.ABILITY_PRESS,			eKeyState.PRESS,			KeyCode.Q,			null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.ABILITY,		eInputCommands.ABILITY_HOLD,			eKeyState.HOLD,				KeyCode.Q,			null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.ABILITY,		eInputCommands.ABILITY_RELEASE,			eKeyState.RELEASE,			KeyCode.Q,			null,				KeyCode.None			);

			// Usage
			GenerateDefaultBinding( InputCategory.USE,			eInputCommands.USAGE,					eKeyState.PRESS,			KeyCode.F,			null,				KeyCode.Return			);

			// Weapons Switch
			GenerateDefaultBinding( InputCategory.SWITCH,		eInputCommands.SWITCH_PREVIOUS,			eKeyState.SCROLL_UP,		KeyCode.None,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SWITCH,		eInputCommands.SWITCH_NEXT,				eKeyState.SCROLL_DOWN,		KeyCode.None,		null,				KeyCode.None			);

			// Selection
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION1,				eKeyState.PRESS,			KeyCode.Alpha1,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION2,				eKeyState.PRESS,			KeyCode.Alpha2,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION3,				eKeyState.PRESS,			KeyCode.Alpha3,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION4,				eKeyState.PRESS,			KeyCode.Alpha4,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION5,				eKeyState.PRESS,			KeyCode.Alpha5,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION6,				eKeyState.PRESS,			KeyCode.Alpha6,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION7,				eKeyState.PRESS,			KeyCode.Alpha7,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION8,				eKeyState.PRESS,			KeyCode.Alpha8,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.SELECTION,	eInputCommands.SELECTION9,				eKeyState.PRESS,			KeyCode.Alpha9,		null,				KeyCode.None			);

			// Item 
			GenerateDefaultBinding( InputCategory.ITEM,			eInputCommands.ITEM1,					eKeyState.PRESS,			KeyCode.F1,			null,				KeyCode.Keypad1			);
			GenerateDefaultBinding( InputCategory.ITEM,			eInputCommands.ITEM2,					eKeyState.PRESS,			KeyCode.F2,			null,				KeyCode.Keypad2			);
			GenerateDefaultBinding( InputCategory.ITEM,			eInputCommands.ITEM3,					eKeyState.PRESS,			KeyCode.F3,			null,				KeyCode.Keypad3			);
			GenerateDefaultBinding( InputCategory.ITEM,			eInputCommands.ITEM4,					eKeyState.PRESS,			KeyCode.F4,			null,				KeyCode.Keypad4			);

			// Gadget
			GenerateDefaultBinding( InputCategory.GADGET,		eInputCommands.GADGET1,					eKeyState.PRESS,			KeyCode.G,			null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.GADGET,		eInputCommands.GADGET2,					eKeyState.PRESS,			KeyCode.H,			null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.GADGET,		eInputCommands.GADGET3,					eKeyState.PRESS,			KeyCode.J,			null,				KeyCode.None			);

			// Primary Fire
			GenerateDefaultBinding( InputCategory.FIRE1,		eInputCommands.PRIMARY_FIRE_PRESS,		eKeyState.PRESS,			KeyCode.Mouse0,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE1,		eInputCommands.PRIMARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse0,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE1,		eInputCommands.PRIMARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse0,		null,				KeyCode.None			);

			// Secondary Fire
			GenerateDefaultBinding( InputCategory.FIRE2,		eInputCommands.SECONDARY_FIRE_PRESS,	eKeyState.PRESS,			KeyCode.Mouse1,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE2,		eInputCommands.SECONDARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse1,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE2,		eInputCommands.SECONDARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse1,		null,				KeyCode.None			);

			// Tertiary Fire
			GenerateDefaultBinding( InputCategory.FIRE3,		eInputCommands.TERTIARY_FIRE_PRESS,		eKeyState.PRESS,			KeyCode.Mouse2,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE3,		eInputCommands.TERTIARY_FIRE_HOLD,		eKeyState.HOLD,				KeyCode.Mouse2,		null,				KeyCode.None			);
			GenerateDefaultBinding( InputCategory.FIRE3,		eInputCommands.TERTIARY_FIRE_RELEASE,	eKeyState.RELEASE,			KeyCode.Mouse2,		null,				KeyCode.None			);

			// Reload
			GenerateDefaultBinding( InputCategory.RELOAD,		eInputCommands.RELOAD_WPN,				eKeyState.PRESS,			KeyCode.R,			null,				KeyCode.End				);

			// Inventory
			GenerateDefaultBinding( InputCategory.INTERFACE,	eInputCommands.INVENTORY,				eKeyState.PRESS,			KeyCode.I,			eKeyState.PRESS,	KeyCode.Backspace		);

			// Weapon Customization
			GenerateDefaultBinding( InputCategory.INTERFACE,	eInputCommands.WPN_CUSTOMIZATION,		eKeyState.PRESS,			KeyCode.U,			null,				KeyCode.None			);
		}

		m_IsDirty = true;

		if ( bMustSave )
		{
			SaveBindings();
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	// GenerateBinding
	/// <summary> Generate default bindings </summary>
	private		void	GenerateDefaultBinding( InputCategory category, eInputCommands command, eKeyState primaryKeyState, KeyCode primaryKey, eKeyState? secondaryKeyState, KeyCode secondaryKey )
	{
		eKeyState _secondaryKeyState = primaryKeyState;
		if ( secondaryKeyState.HasValue == true )
		{
			_secondaryKeyState = secondaryKeyState.Value;
		}

		KeyCommandPair commandPair = new KeyCommandPair();
		commandPair.Setup( command, category, primaryKeyState, primaryKey, _secondaryKeyState, secondaryKey );
		m_Bindings.Pairs.Add( commandPair );
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
			GenerateDefaultBindings( bMustSave: false );
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
/*00*/	NONE,
/*01*/	STATE_CROUCH, STATE_JUMP, STATE_RUN,
/*02*/	MOVE_FORWARD, MOVE_BACKWARD, MOVE_LEFT, MOVE_RIGHT,
/*03*/	ABILITY_PRESS, ABILITY_HOLD, ABILITY_RELEASE,
/*04*/	USAGE,
/*05*/	SWITCH_PREVIOUS, SWITCH_NEXT,
/*06*/	SELECTION1, SELECTION2, SELECTION3, SELECTION4, SELECTION5, SELECTION6, SELECTION7, SELECTION8, SELECTION9,
/*07*/	ITEM1, ITEM2, ITEM3, ITEM4,
/*08*/	GADGET1, GADGET2, GADGET3,
/*09*/	PRIMARY_FIRE_PRESS, PRIMARY_FIRE_HOLD, PRIMARY_FIRE_RELEASE,
/*10*/	SECONDARY_FIRE_PRESS, SECONDARY_FIRE_HOLD, SECONDARY_FIRE_RELEASE,
/*11*/	TERTIARY_FIRE_PRESS, TERTIARY_FIRE_HOLD, TERTIARY_FIRE_RELEASE,
/*12*/	RELOAD_WPN,
/*13*/	INVENTORY,
/*14*/	WPN_CUSTOMIZATION,
/*  */	COUNT
}

[System.Serializable]
/// <summary> Command pair simple class </summary>
public	class KeyCommandPair {

	[SerializeField]
	private	eKeyState			m_PrimaryKeyState			= eKeyState.PRESS;
	public	eKeyState			PrimaryKeyState
	{
		get { return m_PrimaryKeyState; }
	}

	[SerializeField]
	private	eKeyState			m_SecondaryKeyState			= eKeyState.PRESS;
	public	eKeyState			SecondaryKeyState
	{
		get { return m_SecondaryKeyState; }
	}

	[SerializeField]
	private	KeyCode				m_PrimaryKey				= KeyCode.None;
	public	KeyCode				PrimaryKey
	{
		get { return m_PrimaryKey; }
	}

	[SerializeField]
	private	KeyCode				m_SecondaryKey				= KeyCode.None;
	public	KeyCode				SecondaryKey
	{
		get { return m_SecondaryKey; }
	}


	[SerializeField]
	private	eInputCommands		m_Command					= eInputCommands.NONE;
	public	eInputCommands		Command
	{
		get { return m_Command; }
	}

	[SerializeField]
	private	InputCategory		m_Category					= InputCategory.NONE;
	public	InputCategory		Category
	{
		get { return m_Category; }
	}

	[SerializeField]
	private	int					m_PrimaryCheck				= 0;

	[SerializeField]
	private	int					m_SecondaryCheck			= 0;

	private	System.Func<KeyCode, bool> PrimaryKeyCheck		= null;
	private	System.Func<KeyCode, bool> SecondaryKeyCheck	= null;

	//
	public	bool	IsPrimaryKeyUsed()
	{
		return PrimaryKeyCheck( m_PrimaryKey );
	}

	//
	public	bool	IsSecondaryKeyUsed()
	{
		return SecondaryKeyCheck( m_SecondaryKey );
	}

	//
	public	void	Setup( eInputCommands Command, InputCategory Category, eKeyState PrimaryKeyState, KeyCode PrimaryKey, eKeyState SecondaryKeyState, KeyCode SecondaryKey )
	{
		m_Command				= Command;
		m_PrimaryKeyState		= PrimaryKeyState;
		m_PrimaryKey			= PrimaryKey;
		m_SecondaryKeyState		= SecondaryKeyState;
		m_SecondaryKey			= SecondaryKey;
		m_Category				= Category;

		m_PrimaryCheck			= (int)PrimaryKeyState;
		m_SecondaryCheck		= (int)SecondaryKeyState;

		AssignKeyChecks();
	}


	public	void	AssignKeyChecks()
	{
		eKeyState primaryKeyState	= ( eKeyState )m_PrimaryCheck;
		eKeyState secondaryKeyState = ( eKeyState )m_SecondaryCheck;

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
				case eKeys.PRIMARY:		m_PrimaryKey		= keyCode.Value;	break;
				case eKeys.SECONDARY:	m_SecondaryKey	= keyCode.Value;	break;
				default:				break;
			}
		}

		if ( keyState.HasValue )
		{
			switch ( key )
			{
				case eKeys.PRIMARY:		m_PrimaryKeyState		= keyState.Value;		break;
				case eKeys.SECONDARY:	m_SecondaryKeyState	= keyState.Value;		break;
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
			case eKeys.PRIMARY:		code	= m_PrimaryKey;				break;
			case eKeys.SECONDARY:	code	= m_SecondaryKey;				break;
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
			case eKeys.PRIMARY:		keyState	= m_PrimaryKeyState;		break;
			case eKeys.SECONDARY:	keyState	= m_SecondaryKeyState;		break;
			default:				break;
		}

		return keyState;
	}

	//
	public	void	Get( eKeys key, ref KeyCode keyCode, ref eKeyState keyState )
	{
		switch ( key )
		{
			case eKeys.PRIMARY:		keyCode	= m_PrimaryKey;			keyState = m_PrimaryKeyState;		break;
			case eKeys.SECONDARY:	keyCode	= m_SecondaryKey;		keyState = m_SecondaryKeyState;		break;
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