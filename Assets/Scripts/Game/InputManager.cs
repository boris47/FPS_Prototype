
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

//[System.Serializable]
public class InputManager
{
	private static readonly string BindingFilePath = System.IO.Path.Combine(Application.persistentDataPath, "KeyBindings.json");

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
	public KeyCommandPair[] Bindings => m_Bindings.Pairs.ToArray();

	//////////////////////////////////////////////////////////////////////////
	/// <summary> The default contructor </summary>
	public InputManager()
	{
		for (EInputCommands command = EInputCommands.NONE + 1; command < EInputCommands.COUNT; command++)
		{
			m_ActionMap.Add(command, new InputEventCollection());
		}

		// C:/Users/Drako/AppData/LocalLow/BeWide&Co/Project Orion
		if (System.IO.File.Exists(BindingFilePath))
		{
			ReadBindings();
		}
		else
		{
			GenerateDefaultBindings(bMustSave: true);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Save current bindings </summary>
	public		void	SaveBindings()
	{
		if (m_IsDirty && m_Bindings.IsNotNull()) // Only save if dirty
		{
			string data = JsonUtility.ToJson(m_Bindings, prettyPrint: true);
			System.IO.File.WriteAllText(BindingFilePath, data);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Try to load bindings, return boolean success value </summary>
	public		bool	LoadBindings()
	{
		if (!System.IO.File.Exists(BindingFilePath))
		{
			return false;
		}

		string data = System.IO.File.ReadAllText(BindingFilePath);
		m_Bindings = JsonUtility.FromJson<KeyBindings>(data);

		bool bHasBeenLoaded = m_Bindings.IsNotNull();
		if (bHasBeenLoaded)
		{
			// Is useful on new command added
			if ((m_Bindings.Pairs.Count + m_Bindings.Modifiers.Count) != (int)EInputCommands.COUNT-1)
			{
				string[] missingCommands = System.Array.FindAll(System.Enum.GetNames(typeof(EInputCommands)),
					name => {
						if ( name == "NONE" || name == "COUNT" )
							return false;

						return m_Bindings.Pairs.FindIndex(pair => pair.Command.ToString() == name) == -1;
					}
				);

				Debug.Log("InputManager::LoadBindings: Commands Diff:");
				System.Array.ForEach(missingCommands, s => Debug.Log(s));

				GenerateDefaultBindings(true);
				return true;
			}


			//	Debug.Log( "InputManager::LoadBindings:loading bings fail, using default bindings" );
			//	GenerateDefaultBindings( MustSave: true );
			m_Bindings.Modifiers.ForEach(p => p.AssignKeyChecks());
			m_Bindings.Pairs.ForEach(p => p.AssignKeyChecks());
		}

		return bHasBeenLoaded;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Reset bindings and save </summary>
	public		void	ResetBindings()
	{
		GenerateDefaultBindings( bMustSave: false );
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Allow to bind a method to specified command </summary>
	public		void	BindCall(EInputCommands command, string inputEventID, System.Action action, System.Func<bool> predicate = null)
	{
		if (!m_ActionMap.TryGetValue(command, out InputEventCollection inputEventCollection))
		{
			inputEventCollection = m_ActionMap[command] = new InputEventCollection();
		}
		inputEventCollection.Bind(inputEventID, action, predicate);
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Un bind a command from method </summary>
	public		void	UnbindCall(EInputCommands command, string inputEventID)
	{
		if (m_ActionMap.TryGetValue(command, out InputEventCollection inputEventCollection))
		{
			inputEventCollection.Unbind(inputEventID);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	EnableCategory(EInputCategory category)
	{
		if (!Utils.FlagsHelper.IsSet(m_InputCategories, category))
		{
			Utils.FlagsHelper.Set(ref m_InputCategories, category);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	DisableCategory(EInputCategory category)
	{
		if (Utils.FlagsHelper.IsSet(m_InputCategories, category))
		{
			Utils.FlagsHelper.Unset(ref m_InputCategories, category);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	SetCategory(EInputCategory category, bool newState)
	{
		if (newState == true && !Utils.FlagsHelper.IsSet(m_InputCategories, category))
		{
			Utils.FlagsHelper.Set(ref m_InputCategories, category);
		}

		if (newState == false && Utils.FlagsHelper.IsSet(m_InputCategories, category))
		{
			Utils.FlagsHelper.Unset(ref m_InputCategories, category);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	HasCategoryEnabled(EInputCategory category)
	{
		return Utils.FlagsHelper.IsSet(m_InputCategories, category);
	}


	//////////////////////////////////////////////////////////////////////////
	public		void	ToggleCategory(EInputCategory category)
	{
		if (Utils.FlagsHelper.IsSet(m_InputCategories, category))
		{
			Utils.FlagsHelper.Unset(ref m_InputCategories, category);
		}
		else
		{
			Utils.FlagsHelper.Set(ref m_InputCategories, category);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private		void	CommandPairCheck(KeyCommandPair commandPair)
	{
		EInputCategory inputFlag = commandPair.Category;

		// Firstly we check if category is enabled
		if (Utils.FlagsHelper.IsSet(m_InputCategories, inputFlag))								
		{
			// If a command key check is satisfied
			if (commandPair.IsPrimaryKeyUsed() || commandPair.IsSecondaryKeyUsed())								
			{
				// if command event collection is found
				if (m_ActionMap.TryGetValue(commandPair.Command, out InputEventCollection inputEventCollection))           
				{
					// call binded delegate
					inputEventCollection.Call();
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Update everything about inputs </summary>
	public		void	Update()
	{
		if (IsEnabled)
		{
			m_Bindings.Modifiers.ForEach(CommandPairCheck);

			m_Bindings.Pairs.ForEach(CommandPairCheck);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Generate default bindings </summary>
	private		void	GenerateDefaultBinding(EInputCategory category, EInputCommands command, EKeyState primaryKeyState, KeyCode primaryKey, EKeyState? secondaryKeyState, KeyCode secondaryKey, bool bIsModifier)
	{
		EKeyState _secondaryKeyState = primaryKeyState;
		if (secondaryKeyState.HasValue)
		{
			_secondaryKeyState = secondaryKeyState.Value;
		}

		KeyCommandPair commandPair = new KeyCommandPair();
		commandPair.Setup(command, category, primaryKeyState, primaryKey, _secondaryKeyState, secondaryKey);

		if (bIsModifier)
		{
			m_Bindings.Modifiers.Add(commandPair);
		}
		else
		{
			m_Bindings.Pairs.Add(commandPair);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings(bool bMustSave)
	{
		m_Bindings = new KeyBindings();
		{   // DEFAULT BINDINGS			CATEGORY				INPUT COMMAND							PRIMARY KEY STATE			PRIMARY KEY			SECONDARY KEY		SECONDARY KEY STATE
			// Movements
			GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_FORWARD,			EKeyState.HOLD,				KeyCode.W,			null,				KeyCode.UpArrow,		false);
			GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_BACKWARD,			EKeyState.HOLD,				KeyCode.S,			null,				KeyCode.PageDown,		false);
			GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_LEFT,				EKeyState.HOLD,				KeyCode.A,			null,				KeyCode.LeftArrow,		false);
			GenerateDefaultBinding( EInputCategory.MOVE,		EInputCommands.MOVE_RIGHT,				EKeyState.HOLD,				KeyCode.D,			null,				KeyCode.RightArrow,		false);

			// States
			GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_CROUCH,			EKeyState.HOLD,				KeyCode.LeftControl,null,				KeyCode.RightControl,	true);
			GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_JUMP,				EKeyState.PRESS,			KeyCode.Space,		null,				KeyCode.Keypad0,		false);
			GenerateDefaultBinding( EInputCategory.STATE,		EInputCommands.STATE_RUN,				EKeyState.HOLD,				KeyCode.LeftShift,	null,				KeyCode.RightShift,		true);

			// Ability
			GenerateDefaultBinding( EInputCategory.ABILITY,		EInputCommands.ABILITY_PRESS,			EKeyState.PRESS,			KeyCode.Q,			null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.ABILITY,		EInputCommands.ABILITY_HOLD,			EKeyState.HOLD,				KeyCode.Q,			null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.ABILITY,		EInputCommands.ABILITY_RELEASE,			EKeyState.RELEASE,			KeyCode.Q,			null,				KeyCode.None,			false);

			// Usage
			GenerateDefaultBinding( EInputCategory.USE,			EInputCommands.USAGE,					EKeyState.PRESS,			KeyCode.F,			null,				KeyCode.Return,			false);

			// Weapons Switch
			GenerateDefaultBinding( EInputCategory.SWITCH,		EInputCommands.SWITCH_PREVIOUS,			EKeyState.SCROLL_UP,		KeyCode.None,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SWITCH,		EInputCommands.SWITCH_NEXT,				EKeyState.SCROLL_DOWN,		KeyCode.None,		null,				KeyCode.None,			false);

			// Selection
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION1,				EKeyState.PRESS,			KeyCode.Alpha1,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION2,				EKeyState.PRESS,			KeyCode.Alpha2,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION3,				EKeyState.PRESS,			KeyCode.Alpha3,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION4,				EKeyState.PRESS,			KeyCode.Alpha4,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION5,				EKeyState.PRESS,			KeyCode.Alpha5,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION6,				EKeyState.PRESS,			KeyCode.Alpha6,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION7,				EKeyState.PRESS,			KeyCode.Alpha7,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION8,				EKeyState.PRESS,			KeyCode.Alpha8,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.SELECTION,	EInputCommands.SELECTION9,				EKeyState.PRESS,			KeyCode.Alpha9,		null,				KeyCode.None,			false);

			// Item 
			GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM1,					EKeyState.PRESS,			KeyCode.F1,			null,				KeyCode.Keypad1,		false);
			GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM2,					EKeyState.PRESS,			KeyCode.F2,			null,				KeyCode.Keypad2,		false);
			GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM3,					EKeyState.PRESS,			KeyCode.F3,			null,				KeyCode.Keypad3,		false);
			GenerateDefaultBinding( EInputCategory.ITEM,		EInputCommands.ITEM4,					EKeyState.PRESS,			KeyCode.F4,			null,				KeyCode.Keypad4,		false);

			// Gadget
			GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET1,					EKeyState.PRESS,			KeyCode.G,			null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET2,					EKeyState.PRESS,			KeyCode.H,			null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.GADGET,		EInputCommands.GADGET3,					EKeyState.PRESS,			KeyCode.J,			null,				KeyCode.None,			false);

			// Primary Fire
			GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_PRESS,		EKeyState.PRESS,			KeyCode.Mouse0,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse0,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse0,		null,				KeyCode.None,			false);

			// Secondary Fire
			GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_PRESS,	EKeyState.PRESS,			KeyCode.Mouse1,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse1,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse1,		null,				KeyCode.None,			false);

			// Tertiary Fire
			GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_PRESS,		EKeyState.PRESS,			KeyCode.Mouse2,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_HOLD,		EKeyState.HOLD,				KeyCode.Mouse2,		null,				KeyCode.None,			false);
			GenerateDefaultBinding( EInputCategory.FIRE3,		EInputCommands.TERTIARY_FIRE_RELEASE,	EKeyState.RELEASE,			KeyCode.Mouse2,		null,				KeyCode.None,			false);

			// Reload
			GenerateDefaultBinding( EInputCategory.RELOAD,		EInputCommands.RELOAD_WPN,				EKeyState.PRESS,			KeyCode.R,			null,				KeyCode.End,			false);

			// Inventory
			GenerateDefaultBinding( EInputCategory.INTERFACE,	EInputCommands.INVENTORY,				EKeyState.PRESS,			KeyCode.I,			EKeyState.PRESS,	KeyCode.Backspace,		false);

			// Weapon Customization
			GenerateDefaultBinding( EInputCategory.INTERFACE,	EInputCommands.WPN_CUSTOMIZATION,		EKeyState.PRESS,			KeyCode.U,			null,				KeyCode.None,			false);
		}

		m_IsDirty = true;

		if ( bMustSave )
		{
			SaveBindings();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Change a command to modifier or not </summary>
	private		void	SetAsModifier(EInputCommands command, bool bAsModifier)
	{
		if (bAsModifier)
		{
			int index = m_Bindings.Pairs.FindIndex( p => p.Command == command );
			if (index > -1)
			{
				KeyCommandPair pair = m_Bindings.Pairs[index];
				m_Bindings.Modifiers.Add(pair);
				m_Bindings.Pairs.RemoveAt(index);
			}
		}
		else
		{
			int index = m_Bindings.Modifiers.FindIndex(p => p.Command == command);
			if (index > -1)
			{
				KeyCommandPair pair = m_Bindings.Modifiers[index];
				m_Bindings.Pairs.Add(pair);
				m_Bindings.Modifiers.RemoveAt(index);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyState </summary>
	public		void	AssignNewKeyState(EKeys Key, EKeyState NewKeyState, EInputCommands Command)
	{
		// Find the current command Pair
		KeyCommandPair pair = m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );

		// Assign new KeyState
		pair.Assign( Key, NewKeyState, pair.GetKeyCode( Key ) );
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyCode, return a boolean of success. KeyCodes can be swapped if already assigned </summary>
	public		bool	AssignNewKeyCode(EKeys Key, KeyCode NewKeyCode, EInputCommands Command, bool bMustSwap = false)
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find( ( KeyCommandPair p ) => { return p.Command == Command; } );
		
		// Already in Use vars
		KeyCommandPair	alreadyInUsePair		= null;
		EKeys			alreadyInUseKey			= EKeys.PRIMARY;
		EKeyState		alreadyInUseKeyState	= EKeyState.PRESS;
		bool			bIsAlreadyInUse			= false;

		// Find out if already in use
		{
			int alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
			{
				return p.GetKeyCode( EKeys.PRIMARY ) == NewKeyCode && p.GetKeyState( EKeys.PRIMARY ) == currentPair.PrimaryKeyState;
			} );
			// Search for primary keyCode already used
			if ( alreadyUsingPairIndex  != -1 )
			{
				alreadyInUsePair		= m_Bindings.Pairs[ alreadyUsingPairIndex ];
				alreadyInUseKey			= EKeys.PRIMARY;
				alreadyInUseKeyState	= alreadyInUsePair.GetKeyState( EKeys.PRIMARY );
			}
			bIsAlreadyInUse = alreadyUsingPairIndex != -1;

			// Search for secondary keyCode already used
			if ( bIsAlreadyInUse == false )
			{
				alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex( ( KeyCommandPair p ) => 
				{
					return p.GetKeyCode( EKeys.SECONDARY ) == NewKeyCode && p.GetKeyState( EKeys.SECONDARY ) == currentPair.PrimaryKeyState;
				} );
				if ( alreadyUsingPairIndex  != -1 )
				{
					alreadyInUsePair		= m_Bindings.Pairs[ alreadyUsingPairIndex ];
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
	/// <summary> Return boolean if a keyCode for given command at specified key can be assigned </summary>
	public		bool	CanNewKeyCodeBeAssigned(EKeys key, KeyCode NewKeyCode, EInputCommands Command)
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find((KeyCommandPair p) => p.Command == Command);

		// Result
		bool bIsAlreadyInUse = false;

		// Find out if already in use
		{
			// Search for primary keyCode already used
			int alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex((KeyCommandPair p) => 
			{
				return p.GetKeyCode(EKeys.PRIMARY) == NewKeyCode && p.GetKeyState(EKeys.PRIMARY) == currentPair.PrimaryKeyState;
			});

			bIsAlreadyInUse = alreadyUsingPairIndex > -1;

			// Search for secondary keyCode already used
			if (!bIsAlreadyInUse)
			{
				alreadyUsingPairIndex = m_Bindings.Pairs.FindIndex((KeyCommandPair p) => 
				{
					return p.GetKeyCode(EKeys.SECONDARY) == NewKeyCode && p.GetKeyState(EKeys.SECONDARY) == currentPair.PrimaryKeyState;
				});
			}
			bIsAlreadyInUse = alreadyUsingPairIndex > -1;
		}

		return bIsAlreadyInUse == false;
	}

	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to read bindigns from file </summary>
	public		void	ReadBindings()
	{
		bool bHasBeenLoaded = LoadBindings();
		if ( bHasBeenLoaded == false )
		{
			Debug.Log( $"Unable to load key bindings at path {BindingFilePath}" );
			GenerateDefaultBindings( bMustSave: false );
		}
	}

}



/// <summary> Enum for key state evaluation choice </summary>
[System.Serializable]
public	enum EKeyState { PRESS, HOLD, RELEASE, SCROLL_UP, SCROLL_DOWN }

/// <summary> enum for keys</summary>
[System.Serializable]
public	enum EKeys { PRIMARY, SECONDARY }

/// <summary> enum of commands to link keys at </summary>
[System.Serializable]
public	enum EInputCommands
{
/*CAT*/
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


/// <summary> Command pair simple class </summary>
[System.Serializable]
public	class KeyCommandPair
{
	[SerializeField]
	private	EKeyState			m_PrimaryKeyState			= EKeyState.PRESS;
	public	EKeyState			PrimaryKeyState				=> m_PrimaryKeyState;

	[SerializeField]
	private	EKeyState			m_SecondaryKeyState			= EKeyState.PRESS;
	public	EKeyState			SecondaryKeyState			=> m_SecondaryKeyState;
	
	[SerializeField]
	private	KeyCode				m_PrimaryKey				= KeyCode.None;
	public	KeyCode				PrimaryKey					=> m_PrimaryKey;

	[SerializeField]
	private	KeyCode				m_SecondaryKey				= KeyCode.None;
	public	KeyCode				SecondaryKey				=> m_SecondaryKey;

	[SerializeField]
	private	EInputCommands		m_Command					= EInputCommands.NONE;
	public	EInputCommands		Command						=> m_Command;

	[SerializeField]
	private	EInputCategory		m_Category					= EInputCategory.NONE;
	public	EInputCategory		Category					=> m_Category;
	
	[SerializeField]
	private	int					m_PrimaryCheck				= 0;

	[SerializeField]
	private	int					m_SecondaryCheck			= 0;

	private	System.Func<KeyCode, bool> PrimaryKeyCheck		= null;
	private	System.Func<KeyCode, bool> SecondaryKeyCheck	= null;

	//
	public	bool	IsPrimaryKeyUsed()
	{
		return PrimaryKeyCheck(m_PrimaryKey);
	}

	//
	public	bool	IsSecondaryKeyUsed()
	{
		return SecondaryKeyCheck(m_SecondaryKey);
	}

	//
	public	void	Setup(EInputCommands Command, EInputCategory Category, EKeyState PrimaryKeyState, KeyCode PrimaryKey, EKeyState SecondaryKeyState, KeyCode SecondaryKey)
	{
		m_Command				= Command;
		m_Category				= Category;
		m_PrimaryKeyState		= PrimaryKeyState;
		m_PrimaryKey			= PrimaryKey;
		m_SecondaryKeyState		= SecondaryKeyState;
		m_SecondaryKey			= SecondaryKey;

		m_PrimaryCheck			= (int)PrimaryKeyState;
		m_SecondaryCheck		= (int)SecondaryKeyState;

		AssignKeyChecks();
	}


	public	void	AssignKeyChecks()
	{
		EKeyState primaryKeyState	= ( EKeyState )m_PrimaryCheck;
		EKeyState secondaryKeyState = ( EKeyState )m_SecondaryCheck;

		bool ScrollUpCheck(KeyCode k) { return Input.mouseScrollDelta.y > 0f; }
		bool ScrollDownCheck(KeyCode k) { return Input.mouseScrollDelta.y < 0f; }
		switch ( primaryKeyState )
		{
			case EKeyState.PRESS:		PrimaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD:		PrimaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE:		PrimaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP:	PrimaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: PrimaryKeyCheck	= ScrollDownCheck;		break;
			default:
			{
				Debug.Log( $"WARNING: Command {Command.ToString()} has invalid \"PrimaryKeyCheck\" assigned" );
				PrimaryKeyCheck = Input.GetKeyDown;
				break;
			}
		}
		switch ( secondaryKeyState )
		{
			case EKeyState.PRESS:		SecondaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD:		SecondaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE:		SecondaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP:	SecondaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: SecondaryKeyCheck	= ScrollDownCheck;		break;
			default:
			{
				Debug.Log( $"WARNING: Command {Command.ToString()} has invalid \"SecondaryKeyCheck\" assigned" );
				SecondaryKeyCheck = Input.GetKeyDown;
				break;
			}
		}
	}

	//
	public	void	Assign( EKeys key, EKeyState? keyState, KeyCode? keyCode )
	{
		if (keyCode.HasValue)
		{
			switch (key)
			{
				case EKeys.PRIMARY:		m_PrimaryKey	= keyCode.Value; break;
				case EKeys.SECONDARY:	m_SecondaryKey	= keyCode.Value; break;
				default: break;
			}
		}

		if (keyState.HasValue)
		{
			switch (key)
			{
				case EKeys.PRIMARY:		m_PrimaryKeyState	= keyState.Value; break;
				case EKeys.SECONDARY:	m_SecondaryKeyState = keyState.Value; break;
				default: break;
			}

			AssignKeyChecks();
		}
	}

	//
	public KeyCode GetKeyCode(EKeys key)
	{
		KeyCode code = KeyCode.None;
		switch (key)
		{
			case EKeys.PRIMARY:		code = m_PrimaryKey;   break;
			case EKeys.SECONDARY:	code = m_SecondaryKey; break;
			default: break;
		}

		return code;
	}

	//
	public EKeyState GetKeyState(EKeys key)
	{
		EKeyState keyState = EKeyState.PRESS;
		switch (key)
		{
			case EKeys.PRIMARY:		keyState = m_PrimaryKeyState;   break;
			case EKeys.SECONDARY:	keyState = m_SecondaryKeyState; break;
			default: break;
		}

		return keyState;
	}

	//
	public void Get(EKeys key, ref KeyCode keyCode, ref EKeyState keyState)
	{
		switch (key)
		{
			case EKeys.PRIMARY:		keyCode = m_PrimaryKey;		keyState = m_PrimaryKeyState;   break;
			case EKeys.SECONDARY:	keyCode = m_SecondaryKey;	keyState = m_SecondaryKeyState; break;
			default: break;
		}
	}

}

/// <summary> Main object that store bindings and serialized objects </summary>
[System.Serializable]
public	class KeyBindings
{
	[SerializeField]
	public List<KeyCommandPair> Modifiers = new List<KeyCommandPair>();

	[SerializeField]
	public	List<KeyCommandPair> Pairs = new List<KeyCommandPair>();
};