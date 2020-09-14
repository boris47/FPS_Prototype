﻿
using System.Collections.Generic;
using UnityEngine;

[System.FlagsAttribute] 
public enum EInputCategory : uint
{
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
	/// <summary> Camera control </summary>
	CAMERA		= 14, // TODO Implementation
	/// <summary> Categories Count </summary>
	COUNT,
	/// <summary> All categories </summary>
	ALL			= STATE | MOVE | ABILITY | USE | SWITCH | SELECTION | ITEM | GADGET | FIRE1 | FIRE2 | FIRE3 | RELOAD | INTERFACE | CAMERA,
}

public	delegate	void	InputDelegateHandler();

//[System.Serializable]
public class InputManager
{
//	public	static	bool					HoldCrouch				{ get; set; }
//	public	static	bool					HoldJump				{ get; set; }
//	public	static	bool					HoldRun					{ get; set; }

	public	static	bool					IsEnabled				= true;

	[SerializeField]
	private	KeyBindings						m_Bindings				= null;

//	[SerializeField]
	private	EInputCategory					m_InputCategories		= EInputCategory.ALL;

	private	bool							m_IsDirty				= false;

	private readonly Dictionary<EInputCommands, InputEventCollection> m_ActionMap = new Dictionary<EInputCommands, InputEventCollection>();
	

	/// <summary> Return an array structure of bindings </summary>
	public	KeyCommandPair[]				Bindings
	{
		get { return this.m_Bindings.Pairs.ToArray(); }
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> The default contructor </summary>
	public		void		Setup()
	{
		for ( EInputCommands command = EInputCommands.NONE + 1; command < EInputCommands.COUNT; command++ )
		{
			this.m_ActionMap.Add( command, new InputEventCollection() );
		}
		
		// C:/Users/Drako/AppData/LocalLow/BeWide&Co/Project Orion
		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		if ( System.IO.File.Exists( bindingsPath ) )
		{
			this.ReadBindings();
		}
		else
		{
			this.GenerateDefaultBindings( bMustSave: true );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Save current bindings </summary>
	public		void	SaveBindings()
	{
		if (this.m_IsDirty == false || this.m_Bindings == null )
			return;

		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		string data = JsonUtility.ToJson(this.m_Bindings, prettyPrint: true );
		System.IO.File.WriteAllText( bindingsPath, data );
		this.m_IsDirty = false;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Try to load bindings, return boolean success value </summary>
	public		bool	LoadBindings()
	{
		string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
		if ( System.IO.File.Exists( bindingsPath ) == false )
			return false;

		string data = System.IO.File.ReadAllText( bindingsPath );
		this.m_Bindings = JsonUtility.FromJson<KeyBindings>( data );

		bool bHasBeenLoaded = this.m_Bindings != null;
		if ( bHasBeenLoaded == true )
		{
			// Is useful when added new command
			if (this.m_Bindings.Pairs.Count != (int)EInputCommands.COUNT-1 )
			{
				string[] missingCommands = System.Array.FindAll( System.Enum.GetNames( typeof( EInputCommands ) ), 
					name => {
						if ( name == "NONE" || name == "COUNT" )
							return false;

						return this.m_Bindings.Pairs.FindIndex( pair => pair.Command.ToString() == name ) == -1;
					}
				);

				Debug.Log( "InputManager::LoadBindings: Commands Diff:" );
				System.Array.ForEach( missingCommands, s => Debug.Log( s ) );

				this.GenerateDefaultBindings(true);
				return true;
			}


			//	Debug.Log( "InputManager::LoadBindings:loading bings fail, using default bindings" );
			//	GenerateDefaultBindings( MustSave: true );
			this.m_Bindings.Pairs.ForEach( p => p.AssignKeyChecks() );
		}

		return bHasBeenLoaded;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Reset bindings and save </summary>
	public		void	ResetBindings()
	{
		this.GenerateDefaultBindings( bMustSave: false );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to bind a method to specified command </summary>
	public		void	BindCall( EInputCommands command, string inputEventID, InputDelegateHandler action, System.Func<bool> predicate = null )
	{
		if (this.m_ActionMap.TryGetValue(command, out InputEventCollection inputEventCollection) == false)
		{
			inputEventCollection = new InputEventCollection();
			this.m_ActionMap[command] = null;
		}
		inputEventCollection.Bind( inputEventID, action, predicate );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Un bind a command from method </summary>
	public		void	UnbindCall( EInputCommands command, string inputEventID )
	{
		if (this.m_ActionMap.TryGetValue(command, out InputEventCollection inputEventCollection))
		{
			inputEventCollection.Unbind(inputEventID);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	EnableCategory( EInputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet(this.m_InputCategories, category ) == false )
		{
			Utils.FlagsHelper.Set( ref this.m_InputCategories, category );
		}
		
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	DisableCategory( EInputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet(this.m_InputCategories, category ) )
		{
			Utils.FlagsHelper.Unset( ref this.m_InputCategories, category );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	SetCategory( EInputCategory category, bool newState )
	{
		if ( newState == true && Utils.FlagsHelper.IsSet(this.m_InputCategories, category ) == false )
		{
			Utils.FlagsHelper.Set( ref this.m_InputCategories, category );
		}

		if ( newState == false && Utils.FlagsHelper.IsSet(this.m_InputCategories, category ) == true )
		{
			Utils.FlagsHelper.Unset( ref this.m_InputCategories, category );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	HasCategoryEnabled( EInputCategory category )
	{
		return Utils.FlagsHelper.IsSet(this.m_InputCategories, category );
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	ToggleCategory( EInputCategory category )
	{
		if ( Utils.FlagsHelper.IsSet(this.m_InputCategories, category ) )
		{
			Utils.FlagsHelper.Unset( ref this.m_InputCategories, category );
		}
		else
		{
			Utils.FlagsHelper.Set( ref this.m_InputCategories, category );
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private		void	CommandPairCheck( KeyCommandPair commandPair )
	{
		EInputCategory inputFlag	= commandPair.Category;
		bool bIsAvailable = Utils.FlagsHelper.IsSet(this.m_InputCategories, inputFlag );
		if ( bIsAvailable )																				// Firstly we check if category is enabled
		{
			if ( commandPair.IsPrimaryKeyUsed() || commandPair.IsSecondaryKeyUsed() ) // If a command key ceck is satisfied
			{
				if (this.m_ActionMap.TryGetValue(commandPair.Command, out InputEventCollection inputEventCollection))           // if command event collection is found
				{
					inputEventCollection.Call();                                                        // call binded delegate
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Update everything about inputs </summary>
	public		void	Update()
	{
		if ( IsEnabled == false )
			return;

		this.m_Bindings.Pairs.ForEach(this.CommandPairCheck );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings( bool bMustSave )
	{
		this.m_Bindings = new KeyBindings();
		{   // DEFAULT BINDINGS			CATEGORY				INPUT COMMAND							PRIMARY KEY STATE			PRIMARY KEY			SECONDARY KEY		SECONDARY KEY STATE
			// Movements
			this.GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_FORWARD,			EKeyState.HOLD,				KeyCode.W,			null,				KeyCode.UpArrow			);
			this.GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_BACKWARD,			EKeyState.HOLD,				KeyCode.S,			null,				KeyCode.PageDown		);
			this.GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_LEFT,				EKeyState.HOLD,				KeyCode.A,			null,				KeyCode.LeftArrow		);
			this.GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_RIGHT,				EKeyState.HOLD,				KeyCode.D,			null,				KeyCode.RightArrow		);

			// States
			this.GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_CROUCH,			EKeyState.HOLD,				KeyCode.LeftControl,null,				KeyCode.RightControl	);
			this.GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_JUMP,				EKeyState.PRESS,			KeyCode.Space,		null,				KeyCode.Keypad0			);
			this.GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_RUN,				EKeyState.HOLD,				KeyCode.LeftShift,	null,				KeyCode.RightShift		);

			// Ability
			this.GenerateDefaultBinding( EInputCategory.ABILITY,	EInputCommands.ABILITY_PRESS,			EKeyState.PRESS,			KeyCode.Q,			null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.ABILITY,	EInputCommands.ABILITY_HOLD,			EKeyState.HOLD,				KeyCode.Q,			null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.ABILITY,	EInputCommands.ABILITY_RELEASE,			EKeyState.RELEASE,			KeyCode.Q,			null,				KeyCode.None			);

			// Usage
			this.GenerateDefaultBinding( EInputCategory.USE,		EInputCommands.USAGE,					EKeyState.PRESS,			KeyCode.F,			null,				KeyCode.Return			);

			// Weapons Switch
			this.GenerateDefaultBinding( EInputCategory.SWITCH,		EInputCommands.SWITCH_PREVIOUS,			EKeyState.SCROLL_UP,		KeyCode.None,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SWITCH,		EInputCommands.SWITCH_NEXT,				EKeyState.SCROLL_DOWN,		KeyCode.None,		null,				KeyCode.None			);

			// Selection
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION1,				EKeyState.PRESS,			KeyCode.Alpha1,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION2,				EKeyState.PRESS,			KeyCode.Alpha2,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION3,				EKeyState.PRESS,			KeyCode.Alpha3,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION4,				EKeyState.PRESS,			KeyCode.Alpha4,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION5,				EKeyState.PRESS,			KeyCode.Alpha5,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION6,				EKeyState.PRESS,			KeyCode.Alpha6,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION7,				EKeyState.PRESS,			KeyCode.Alpha7,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION8,				EKeyState.PRESS,			KeyCode.Alpha8,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION9,				EKeyState.PRESS,			KeyCode.Alpha9,		null,				KeyCode.None			);

			// Item 
			this.GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM1,					EKeyState.PRESS,			KeyCode.F1,			null,				KeyCode.Keypad1			);
			this.GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM2,					EKeyState.PRESS,			KeyCode.F2,			null,				KeyCode.Keypad2			);
			this.GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM3,					EKeyState.PRESS,			KeyCode.F3,			null,				KeyCode.Keypad3			);
			this.GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM4,					EKeyState.PRESS,			KeyCode.F4,			null,				KeyCode.Keypad4			);

			// Gadget
			this.GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET1,					EKeyState.PRESS,			KeyCode.G,			null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET2,					EKeyState.PRESS,			KeyCode.H,			null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET3,					EKeyState.PRESS,			KeyCode.J,			null,				KeyCode.None			);

			// Primary Fire
			this.GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_PRESS,		EKeyState.PRESS,			KeyCode.Mouse0,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse0,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse0,		null,				KeyCode.None			);

			// Secondary Fire
			this.GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_PRESS,	EKeyState.PRESS,			KeyCode.Mouse1,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse1,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse1,		null,				KeyCode.None			);

			// Tertiary Fire
			this.GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_PRESS,		EKeyState.PRESS,			KeyCode.Mouse2,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse2,		null,				KeyCode.None			);
			this.GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse2,		null,				KeyCode.None			);

			// Reload
			this.GenerateDefaultBinding( EInputCategory.RELOAD,		EInputCommands.RELOAD_WPN,				EKeyState.PRESS,			KeyCode.R,			null,				KeyCode.End				);

			// Inventory
			this.GenerateDefaultBinding( EInputCategory.INTERFACE,	EInputCommands.INVENTORY,				EKeyState.PRESS,			KeyCode.I,			EKeyState.PRESS,	KeyCode.Backspace		);

			// Weapon Customization
			this.GenerateDefaultBinding( EInputCategory.INTERFACE,	EInputCommands.WPN_CUSTOMIZATION,		EKeyState.PRESS,			KeyCode.U,			null,				KeyCode.None			);
		}

		this.m_IsDirty = true;

		if ( bMustSave )
		{
			this.SaveBindings();
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////
	/// <summary> Generate default bindings </summary>
	private		void	GenerateDefaultBinding( EInputCategory category, EInputCommands command, EKeyState primaryKeyState, KeyCode primaryKey, EKeyState? secondaryKeyState, KeyCode secondaryKey )
	{
		EKeyState _secondaryKeyState = primaryKeyState;
		if ( secondaryKeyState.HasValue == true )
		{
			_secondaryKeyState = secondaryKeyState.Value;
		}

		KeyCommandPair commandPair = new KeyCommandPair();
		commandPair.Setup( command, category, primaryKeyState, primaryKey, _secondaryKeyState, secondaryKey );
		this.m_Bindings.Pairs.Add( commandPair );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyState </summary>
	public		void	AssignNewKeyState( EKeys Key, EKeyState NewKeyState, EInputCommands Command )
	{
		// Find the current command Pair
		KeyCommandPair pair = this.m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );

		// Assign new KeyState
		pair.Assign( Key, NewKeyState, pair.GetKeyCode( Key ) );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyCode, return a boolean of success. KeyCodes can be swapped if already assigned </summary>
	public		bool	AssignNewKeyCode( EKeys Key, KeyCode NewKeyCode, EInputCommands Command, bool bMustSwap = false )
	{
		// Find the current command Pair
		KeyCommandPair currentPair = this.m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );
		
		// Already in Use vars
		KeyCommandPair	alreadyInUsePair		= null;
		EKeys			alreadyInUseKey			= EKeys.PRIMARY;
		EKeyState		alreadyInUseKeyState	= EKeyState.PRESS;
		bool			bIsAlreadyInUse			= false;

		// Find out if already in use
		{
			int alreadyUsingPairIndex = this.m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
			{
				return p.GetKeyCode( EKeys.PRIMARY ) == NewKeyCode && p.GetKeyState( EKeys.PRIMARY ) == currentPair.PrimaryKeyState;
			} );
			// Search for primary keyCode already used
			if ( alreadyUsingPairIndex  != -1 )
			{
				alreadyInUsePair		= this.m_Bindings.Pairs[ alreadyUsingPairIndex ];
				alreadyInUseKey			= EKeys.PRIMARY;
				alreadyInUseKeyState	= alreadyInUsePair.GetKeyState( EKeys.PRIMARY );
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;

			// Search for secondary keyCode already used
			if ( bIsAlreadyInUse == false )
			{
				alreadyUsingPairIndex = this.m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
				{
					return p.GetKeyCode( EKeys.SECONDARY ) == NewKeyCode && p.GetKeyState( EKeys.SECONDARY ) == currentPair.PrimaryKeyState;
				} );
				if ( alreadyUsingPairIndex  != -1 )
				{
					alreadyInUsePair		= this.m_Bindings.Pairs[ alreadyUsingPairIndex ];
					alreadyInUseKey			= EKeys.SECONDARY;
					alreadyInUseKeyState	= alreadyInUsePair.GetKeyState( EKeys.SECONDARY );
				}
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;
		}

		// Swapping KeyCode and keyState
		if ( bIsAlreadyInUse == true && bMustSwap == true )
		{
			KeyCode thisKeyCode		= currentPair.GetKeyCode( alreadyInUseKey );
			EKeyState thiskeyState	= currentPair.GetKeyState( alreadyInUseKey );
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

			this.m_IsDirty = true;
		}

		// Can be assigned
		if ( bIsAlreadyInUse == false )
		{
			currentPair.Assign( Key,	null,	NewKeyCode );

			this.m_IsDirty = true;
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
	/// <summary> Return boolean if a keyCode for given command at specified key can be assigned </summary>
	public		bool	CanNewKeyCodeBeAssigned( EKeys key, KeyCode NewKeyCode, EInputCommands Command )
	{
		// Find the current command Pair
		KeyCommandPair currentPair = this.m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );

		// Result
		bool			bIsAlreadyInUse			= false;

		// Find out if already in use
		{
			// Search for primary keyCode already used
			int alreadyUsingPairIndex = this.m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
			{
				return p.GetKeyCode( EKeys.PRIMARY ) == NewKeyCode && p.GetKeyState( EKeys.PRIMARY ) == currentPair.PrimaryKeyState;
			} );

			bIsAlreadyInUse = alreadyUsingPairIndex != -1;

			// Search for secondary keyCode already used
			if ( bIsAlreadyInUse == false )
			{
				alreadyUsingPairIndex = this.m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
				{
					return p.GetKeyCode( EKeys.SECONDARY ) == NewKeyCode && p.GetKeyState( EKeys.SECONDARY ) == currentPair.PrimaryKeyState;
				} );
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;
		}

		return bIsAlreadyInUse == false;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to read bindigns from file </summary>
	public		void	ReadBindings()
	{
		bool bHasBeenLoaded = this.LoadBindings();
		if ( bHasBeenLoaded == false )
		{
			string bindingsPath = Application.persistentDataPath + "/KeyBindings.json";
			Debug.Log( "Unable to load key bindings at path " + bindingsPath );
			this.GenerateDefaultBindings( bMustSave: false );
		}
	}

}



[System.Serializable]
/// <summary> Enum for key state evaluation choice </summary>
public	enum EKeyState { PRESS, HOLD, RELEASE, SCROLL_UP, SCROLL_DOWN }

[System.Serializable]
/// <summary> enum for keys</summary>
public	enum EKeys { PRIMARY, SECONDARY }

[System.Serializable]
/// <summary> enum of commands to link keys at </summary>
public	enum EInputCommands
{
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
public	class KeyCommandPair
{
	[SerializeField]
	private	EKeyState			m_PrimaryKeyState			= EKeyState.PRESS;
	public	EKeyState			PrimaryKeyState
	{
		get { return this.m_PrimaryKeyState; }
	}

	[SerializeField]
	private	EKeyState			m_SecondaryKeyState			= EKeyState.PRESS;
	public	EKeyState			SecondaryKeyState
	{
		get { return this.m_SecondaryKeyState; }
	}

	[SerializeField]
	private	KeyCode				m_PrimaryKey				= KeyCode.None;
	public	KeyCode				PrimaryKey
	{
		get { return this.m_PrimaryKey; }
	}

	[SerializeField]
	private	KeyCode				m_SecondaryKey				= KeyCode.None;
	public	KeyCode				SecondaryKey
	{
		get { return this.m_SecondaryKey; }
	}


	[SerializeField]
	private	EInputCommands		m_Command					= EInputCommands.NONE;
	public	EInputCommands		Command
	{
		get { return this.m_Command; }
	}

	[SerializeField]
	private	EInputCategory		m_Category					= EInputCategory.NONE;
	public	EInputCategory		Category
	{
		get { return this.m_Category; }
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
		return this.PrimaryKeyCheck(this.m_PrimaryKey );
	}

	//
	public	bool	IsSecondaryKeyUsed()
	{
		return this.SecondaryKeyCheck(this.m_SecondaryKey );
	}

	//
	public	void	Setup( EInputCommands Command, EInputCategory Category, EKeyState PrimaryKeyState, KeyCode PrimaryKey, EKeyState SecondaryKeyState, KeyCode SecondaryKey )
	{
		this.m_Command				= Command;
		this.m_PrimaryKeyState		= PrimaryKeyState;
		this.m_PrimaryKey			= PrimaryKey;
		this.m_SecondaryKeyState		= SecondaryKeyState;
		this.m_SecondaryKey			= SecondaryKey;
		this.m_Category				= Category;

		this.m_PrimaryCheck			= (int)PrimaryKeyState;
		this.m_SecondaryCheck		= (int)SecondaryKeyState;

		this.AssignKeyChecks();
	}


	public	void	AssignKeyChecks()
	{
		EKeyState primaryKeyState	= ( EKeyState )this.m_PrimaryCheck;
		EKeyState secondaryKeyState = ( EKeyState )this.m_SecondaryCheck;

		bool ScrollUpCheck(KeyCode k) { return Input.mouseScrollDelta.y > 0f; }
		bool ScrollDownCheck(KeyCode k) { return Input.mouseScrollDelta.y < 0f; }
		switch ( primaryKeyState )
		{
			case EKeyState.PRESS: this.PrimaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD: this.PrimaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE: this.PrimaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP: this.PrimaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: this.PrimaryKeyCheck	= ScrollDownCheck;		break;
			default:
				{
					Debug.Log( "WARNING: Command " + this.Command.ToString() + " has invalid \"PrimaryKeyCheck\" assigned" );
					this.PrimaryKeyCheck = Input.GetKeyDown;
					break;
				}
		}
		switch ( secondaryKeyState )
		{
			case EKeyState.PRESS: this.SecondaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD: this.SecondaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE: this.SecondaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP: this.SecondaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: this.SecondaryKeyCheck	= ScrollDownCheck;		break;
			default:
				{
					Debug.Log( "WARNING: Command " + this.Command.ToString() + " has invalid \"SecondaryKeyCheck\" assigned" );
					this.SecondaryKeyCheck = Input.GetKeyDown;
					break;
				}
		}
	}

	//
	public	void	Assign( EKeys key, EKeyState? keyState, KeyCode? keyCode )
	{
		if ( keyCode.HasValue )
		{
			switch ( key )
			{
				case EKeys.PRIMARY: this.m_PrimaryKey		= keyCode.Value;	break;
				case EKeys.SECONDARY: this.m_SecondaryKey	= keyCode.Value;	break;
				default:				break;
			}
		}

		if ( keyState.HasValue )
		{
			switch ( key )
			{
				case EKeys.PRIMARY: this.m_PrimaryKeyState		= keyState.Value;		break;
				case EKeys.SECONDARY: this.m_SecondaryKeyState	= keyState.Value;		break;
				default:				break;
			}

			this.AssignKeyChecks();
		}
	}

	//
	public	KeyCode	GetKeyCode( EKeys key )
	{
		KeyCode code = KeyCode.None;
		switch ( key )
		{
			case EKeys.PRIMARY:		code	= this.m_PrimaryKey;				break;
			case EKeys.SECONDARY:	code	= this.m_SecondaryKey;				break;
			default:				break;
		}

		return code;
	}

	//
	public	EKeyState	GetKeyState( EKeys key )
	{
		EKeyState keyState = EKeyState.PRESS;
		switch ( key )
		{
			case EKeys.PRIMARY:		keyState	= this.m_PrimaryKeyState;		break;
			case EKeys.SECONDARY:	keyState	= this.m_SecondaryKeyState;		break;
			default:				break;
		}

		return keyState;
	}

	//
	public	void	Get( EKeys key, ref KeyCode keyCode, ref EKeyState keyState )
	{
		switch ( key )
		{
			case EKeys.PRIMARY:		keyCode	= this.m_PrimaryKey;			keyState = this.m_PrimaryKeyState;		break;
			case EKeys.SECONDARY:	keyCode	= this.m_SecondaryKey;		keyState = this.m_SecondaryKeyState;		break;
			default:				break;
		}
	}

}

[System.Serializable]
/// <summary> Main object that store bindings and serialized objects </summary>
public	class KeyBindings
{
	[SerializeField]
	public	List<KeyCommandPair> Pairs = new List<KeyCommandPair>();

};