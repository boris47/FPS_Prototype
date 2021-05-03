
using System.Collections.Generic;
using UnityEngine;

// TODO Input coupled with motion type (grounded, swimm, driving, flying, ...)

//[System.FlagsAttribute]
public enum EInputCategory : uint
{
	NONE,
	/// <summary> Controls movements </summary>
	MOVE,
	/// <summary> Controls State </summary>
	STATE,
	/// <summary> Ability input </summary>
	ABILITY,
	/// <summary> Usage input </summary>
	USE,
	/// <summary> Weapons switch </summary>
	SWITCH,
	/// <summary> Selection input </summary>
	SELECTION,
	/// <summary> Item usage (Player Items) </summary>
	ITEM,
	/// <summary> Accessory usage </summary>
	GADGET,
	/// <summary> Weapon Attachments </summary>
	ATTACHMENTS,
	/// <summary> Primary fire </summary>
	FIRE1,
	/// <summary> Secondary fire </summary>
	FIRE2,
	/// <summary> Reload </summary>
	RELOAD,
	/// <summary> In Game Interface </summary>
	INTERFACE,
	/// <summary> Camera control </summary>
	CAMERA, // TODO Implementation
	/// <summary> Save, Load </summary>
	INGAME,
	/// <summary> Category never disabled ex: Menu(Escape) </summary>
	EXCLUSIVE,
	/// <summary> Categories Count </summary>
	COUNT,
	/// <summary> All categories </summary>
	ALL			= STATE | MOVE | ABILITY | USE | SWITCH | SELECTION | ITEM | GADGET | FIRE1 | FIRE2 | RELOAD | INTERFACE | CAMERA | INGAME,
}

public enum EInputPriority: short
{
	/// <summary> Input parsed after all others </summary>
	LOW,
	/// <summary> Input parsed normally </summary>
	NORMAL,
	/// <summary> Input parsed before others </summary>
	HIGH,
}

//[System.Serializable]
public class InputManager
{
	private static readonly string BindingFilePath = System.IO.Path.Combine(Application.persistentDataPath, "KeyBindings.json");

	public	static	bool					IsEnabled				= true;

	[SerializeField]
	private	KeyBindings						m_Bindings				= null;

//	[SerializeField]
	private	EInputCategory					m_InputCategories		= EInputCategory.ALL;

	private	bool							m_IsDirty				= false;

	private Dictionary<EInputCommands, InputEventCollection> m_ActionMap = new Dictionary<EInputCommands, InputEventCollection>();


	public KeyCommandPair[]					Bindings				=> m_Bindings.Pairs.ToArray();

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
			if (m_Bindings.Pairs.Count != (int)EInputCommands.COUNT-1)
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
		if ((Utils.FlagsHelper.IsSet(m_InputCategories, inputFlag) && IsEnabled) || inputFlag == EInputCategory.EXCLUSIVE)
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
		m_Bindings.Pairs.ForEach(CommandPairCheck);
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Generates default bindings, optionally can save </summary>
	private		void	GenerateDefaultBindings(bool bMustSave)
	{
		// Reset all bindings
		m_Bindings = new KeyBindings();

		// Modifier are checked before the other keys in order to allow manipulation of values
		void GenerateDefaultBinding(EInputCategory category, EInputCommands command, KeyCode primaryKey, KeyCode secondaryKey, EKeyState keyState, EInputPriority priority)
		{
			KeyCommandPair commandPair = new KeyCommandPair();
			commandPair.Setup(command, category, keyState, primaryKey, keyState, secondaryKey, priority);
			m_Bindings.Pairs.Add(commandPair);
		}

		{   // DEFAULT BINDINGS			CATEGORY				INPUT COMMAND							PRIMARY KEY				SECONDARY KEY			KEY STATE					IS MODIFIER
			// Movements
			GenerateDefaultBinding(EInputCategory.MOVE,			EInputCommands.MOVE_FORWARD,			KeyCode.W,				KeyCode.UpArrow,		EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.MOVE,			EInputCommands.MOVE_BACKWARD,			KeyCode.S,				KeyCode.DownArrow,		EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.MOVE,			EInputCommands.MOVE_LEFT,				KeyCode.A,				KeyCode.LeftArrow,		EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.MOVE,			EInputCommands.MOVE_RIGHT,				KeyCode.D,				KeyCode.RightArrow,		EKeyState.HOLD,				EInputPriority.NORMAL);

			// States
			GenerateDefaultBinding(EInputCategory.STATE,		EInputCommands.STATE_CROUCH,			KeyCode.LeftControl,	KeyCode.RightControl,	EKeyState.HOLD,				EInputPriority.HIGH);
			GenerateDefaultBinding(EInputCategory.STATE,		EInputCommands.STATE_JUMP,				KeyCode.Space,			KeyCode.Keypad0,		EKeyState.PRESS,			EInputPriority.LOW);
			GenerateDefaultBinding(EInputCategory.STATE,		EInputCommands.STATE_RUN,				KeyCode.LeftShift,		KeyCode.RightShift,		EKeyState.HOLD,				EInputPriority.HIGH);

			// Ability
			GenerateDefaultBinding(EInputCategory.ABILITY,		EInputCommands.ABILITY_PRESS,			KeyCode.Q,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ABILITY,		EInputCommands.ABILITY_HOLD,			KeyCode.Q,				KeyCode.None,			EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ABILITY,		EInputCommands.ABILITY_RELEASE,			KeyCode.Q,				KeyCode.None,			EKeyState.RELEASE,			EInputPriority.NORMAL);

			// Usage
			GenerateDefaultBinding(EInputCategory.USE,			EInputCommands.USAGE,					KeyCode.F,				KeyCode.Return,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// Weapons Switch
			GenerateDefaultBinding(EInputCategory.SWITCH,		EInputCommands.SWITCH_PREVIOUS,			KeyCode.None,			KeyCode.None,			EKeyState.SCROLL_UP,		EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SWITCH,		EInputCommands.SWITCH_NEXT,				KeyCode.None,			KeyCode.None,			EKeyState.SCROLL_DOWN,		EInputPriority.NORMAL);

			// Selection
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION1,				KeyCode.Alpha1,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION2,				KeyCode.Alpha2,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION3,				KeyCode.Alpha3,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION4,				KeyCode.Alpha4,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION5,				KeyCode.Alpha5,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION6,				KeyCode.Alpha6,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION7,				KeyCode.Alpha7,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION8,				KeyCode.Alpha8,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.SELECTION,	EInputCommands.SELECTION9,				KeyCode.Alpha9,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// Player Items
			GenerateDefaultBinding(EInputCategory.ITEM,			EInputCommands.ITEM1,					KeyCode.F1,				KeyCode.Keypad1,		EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ITEM,			EInputCommands.ITEM2,					KeyCode.F2,				KeyCode.Keypad2,		EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ITEM,			EInputCommands.ITEM3,					KeyCode.F3,				KeyCode.Keypad3,		EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ITEM,			EInputCommands.ITEM4,					KeyCode.F4,				KeyCode.Keypad4,		EKeyState.PRESS,			EInputPriority.NORMAL);
			
			// Gadget
			GenerateDefaultBinding(EInputCategory.GADGET,		EInputCommands.GADGET1,					KeyCode.G,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.GADGET,		EInputCommands.GADGET2,					KeyCode.H,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.GADGET,		EInputCommands.GADGET3,					KeyCode.J,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// Weapon Attachments
			GenerateDefaultBinding(EInputCategory.ATTACHMENTS,	EInputCommands.ATTACHMENT1,				KeyCode.Mouse1,			KeyCode.Delete,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ATTACHMENTS,	EInputCommands.ATTACHMENT2,				KeyCode.T,				KeyCode.PageDown,		EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.ATTACHMENTS,	EInputCommands.ATTACHMENT3,				KeyCode.Y,				KeyCode.End,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// Primary Fire
			GenerateDefaultBinding(EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_PRESS,		KeyCode.Mouse0,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_HOLD,		KeyCode.Mouse0,			KeyCode.None,			EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.FIRE1,		EInputCommands.PRIMARY_FIRE_RELEASE,	KeyCode.Mouse0,			KeyCode.None,			EKeyState.RELEASE,			EInputPriority.NORMAL);

			// Secondary Fire
			GenerateDefaultBinding(EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_PRESS,	KeyCode.Mouse2,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_HOLD,		KeyCode.Mouse2,			KeyCode.None,			EKeyState.HOLD,				EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.FIRE2,		EInputCommands.SECONDARY_FIRE_RELEASE,	KeyCode.Mouse2,			KeyCode.None,			EKeyState.RELEASE,			EInputPriority.NORMAL);

			// Reload
			GenerateDefaultBinding(EInputCategory.RELOAD,		EInputCommands.RELOAD_WPN,				KeyCode.R,				KeyCode.End,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// Inventory
			GenerateDefaultBinding(EInputCategory.INTERFACE,	EInputCommands.INVENTORY,				KeyCode.I,				KeyCode.Backspace,		EKeyState.PRESS,			EInputPriority.NORMAL);

			// Weapon Customization
			GenerateDefaultBinding(EInputCategory.INTERFACE,	EInputCommands.WPN_CUSTOMIZATION,		KeyCode.U,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);

			// System
			GenerateDefaultBinding(EInputCategory.EXCLUSIVE,	EInputCommands.INGAME_MENU,				KeyCode.Escape,			KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.INGAME,		EInputCommands.INGAME_SAVE,				KeyCode.F5,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
			GenerateDefaultBinding(EInputCategory.INGAME,		EInputCommands.INGAME_LOAD,				KeyCode.F9,				KeyCode.None,			EKeyState.PRESS,			EInputPriority.NORMAL);
		}

		// Sorting by priority
		m_Bindings.Pairs.Sort((KeyCommandPair pair1, KeyCommandPair pair2) => pair2.Priority - pair1.Priority);

		m_IsDirty = true;

		if (bMustSave)
		{
			SaveBindings();
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyState </summary>
	public		void	AssignNewKeyState(EKeySlot key, EKeyState newKeyState, EInputCommands command)
	{
		// Find the current command Pair
		KeyCommandPair pair = m_Bindings.Pairs.Find(p => p.Command == command);

		// Assign new KeyState
		if (pair.IsNotNull())
		{
			pair.Assign(key, newKeyState, pair.GetKeyCode(key));
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to assign a keyCode, return a boolean of success. KeyCodes can be swapped if already assigned </summary>
	public		bool	TryAssignNewKeyCode(EInputCommands command, EKeySlot destinationKeySlot, KeyCode newKeyCode, bool bMustSwap)
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find(p => p.Command == command);
		
		// Find out if already in use
		if (HasKeyCodeAlreadyBeenAssigned(command, destinationKeySlot, newKeyCode, out KeyCommandPair alreadyInUsePair, out EKeySlot alreadyInUseKeySlot))
		{
			EKeyState alreadyInUseKeyState = alreadyInUsePair.GetKeyStateByKeyCode(newKeyCode);

			if (bMustSwap)
			{
				if (alreadyInUseKeySlot == destinationKeySlot)
				{
					KeyCode thisKeyCode		= currentPair.GetKeyCode(alreadyInUseKeySlot);
					EKeyState thiskeyState	= currentPair.GetKeyState(alreadyInUseKeySlot);
					currentPair.Assign		( alreadyInUseKeySlot,	thiskeyState,			newKeyCode	);	// current selected
					alreadyInUsePair.Assign	( alreadyInUseKeySlot,	alreadyInUseKeyState,	thisKeyCode	);	// already set swapping
				}
				else
				{
					KeyCode thisKeyCode		= currentPair.GetKeyCode(destinationKeySlot);
					EKeyState thiskeyState	= currentPair.GetKeyState(alreadyInUseKeySlot);
					currentPair.Assign		( destinationKeySlot,	thiskeyState,			newKeyCode	);	// current selected
					alreadyInUsePair.Assign	( alreadyInUseKeySlot,	alreadyInUseKeyState,	thisKeyCode	);	// Already set swapping
				}
				m_IsDirty = true;
			}
			else
			{
				Debug.LogWarning($"Trying to assign keycode {newKeyCode.ToString()}, used for command {alreadyInUsePair.Command.ToString()}, as {alreadyInUseKeySlot.ToString()} key");
			}
		}
		else // Can be assigned
		{
			currentPair.Assign(destinationKeySlot, null, newKeyCode);

			m_IsDirty = true;
		}
		return m_IsDirty;
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Return boolean if a keyCode for given command at specified key can be assigned </summary>
	public		bool	HasKeyCodeAlreadyBeenAssigned(EInputCommands command, EKeySlot keySlot, KeyCode newKeyCode, out KeyCommandPair alreadyInUsePair, out EKeySlot alreadyInUseKeySlot)
	{
		// Find the current command Pair
		KeyCommandPair currentPair = m_Bindings.Pairs.Find(p => p.Command == command);

		bool PrimaryPredicate  (KeyCommandPair otherPair) => otherPair.GetKeyCode(EKeySlot.PRIMARY)   == newKeyCode && otherPair.GetKeyState(EKeySlot.PRIMARY)   == currentPair.GetKeyState(EKeySlot.PRIMARY);
		bool SecondaryPredicate(KeyCommandPair otherPair) => otherPair.GetKeyCode(EKeySlot.SECONDARY) == newKeyCode && otherPair.GetKeyState(EKeySlot.SECONDARY) == currentPair.GetKeyState(EKeySlot.SECONDARY);

		alreadyInUsePair = m_Bindings.Pairs.Find(PrimaryPredicate) ?? m_Bindings.Pairs.Find(SecondaryPredicate);
		alreadyInUseKeySlot = alreadyInUsePair?.GetKeySlotByKeyCode(newKeyCode) ?? default;

		return alreadyInUsePair.IsNotNull();
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary> Attempt to read bindigns from file </summary>
	public		void	ReadBindings()
	{
		bool bHasBeenLoaded = LoadBindings();
		if ( bHasBeenLoaded == false )
		{
			Debug.Log( $"Unable to load key bindings at path {BindingFilePath}" );
			GenerateDefaultBindings( bMustSave: true );
		}
	}
}



/// <summary> Enum for key state evaluation choice </summary>
[System.Serializable]
public	enum EKeyState: byte { PRESS, HOLD, RELEASE, SCROLL_UP, SCROLL_DOWN }

/// <summary> enum for keys</summary>
[System.Serializable]
public	enum EKeySlot: byte { PRIMARY, SECONDARY }

/// <summary> enum of commands to link keys at </summary>
[System.Serializable]
public	enum EInputCommands: byte
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
/*09*/	ATTACHMENT1, ATTACHMENT2, ATTACHMENT3,
/*09*/	PRIMARY_FIRE_PRESS, PRIMARY_FIRE_HOLD, PRIMARY_FIRE_RELEASE,
/*10*/	SECONDARY_FIRE_PRESS, SECONDARY_FIRE_HOLD, SECONDARY_FIRE_RELEASE,
/*12*/	RELOAD_WPN,
/*13*/	INVENTORY,
/*14*/	WPN_CUSTOMIZATION,
/*15*/	INGAME_SAVE, INGAME_LOAD, INGAME_MENU,
/*  */	COUNT
}


/// <summary> Command pair simple class </summary>
[System.Serializable]
public	class KeyCommandPair
{
	[SerializeField]
	private		EKeyState						m_PrimaryKeyState			= EKeyState.PRESS;

	[SerializeField]
	private		EKeyState						m_SecondaryKeyState			= EKeyState.PRESS;
	
	[SerializeField]
	private		KeyCode							m_PrimaryKey				= KeyCode.None;

	[SerializeField]
	private		KeyCode							m_SecondaryKey				= KeyCode.None;

	[SerializeField]
	private		EInputCommands					m_Command					= EInputCommands.NONE;

	[SerializeField]
	private		EInputCategory					m_Category					= EInputCategory.NONE;
	
	[SerializeField]
	private		int								m_PrimaryCheck				= 0;

	[SerializeField]
	private		int								m_SecondaryCheck			= 0;

	[SerializeField]
	private		EInputPriority					m_Priority					= EInputPriority.NORMAL;


	private		System.Func<KeyCode, bool>		m_PrimaryKeyCheck			= null;
	private		System.Func<KeyCode, bool>		m_SecondaryKeyCheck			= null;
	private		UnityEngine.UI.Text				m_PrimaryLabel				= null;
	private		UnityEngine.UI.Text				m_SecondaryLabel			= null;

	public		EInputCategory					Category					=> m_Category;
	public		EInputCommands					Command						=> m_Command;
	public		EKeyState						PrimaryKeyState				=> m_PrimaryKeyState;
	public		EKeyState						SecondaryKeyState			=> m_SecondaryKeyState;
	public		KeyCode							SecondaryKey				=> m_SecondaryKey;
	public		KeyCode							PrimaryKey					=> m_PrimaryKey;
	public		EInputPriority					Priority					=> m_Priority;
	public		UnityEngine.UI.Text				PrimaryLabel				=> m_PrimaryLabel;
	public		UnityEngine.UI.Text				SecondaryLabel				=> m_SecondaryLabel;


	//
	public	bool	IsPrimaryKeyUsed() => m_PrimaryKeyCheck(m_PrimaryKey);

	//
	public	bool	IsSecondaryKeyUsed() => m_SecondaryKeyCheck(m_SecondaryKey);

	//
	public void		SetUIPrimaryLabel(UnityEngine.UI.Text label) => m_PrimaryLabel = label;

	//
	public void		SetUISecondaryLabel(UnityEngine.UI.Text label) => m_SecondaryLabel = label;

	//
	public	void	Setup(EInputCommands Command, EInputCategory Category, EKeyState PrimaryKeyState, KeyCode PrimaryKey, EKeyState SecondaryKeyState, KeyCode SecondaryKey, EInputPriority Priority)
	{
		m_Command				= Command;
		m_Category				= Category;
		m_PrimaryKeyState		= PrimaryKeyState;
		m_PrimaryKey			= PrimaryKey;
		m_SecondaryKeyState		= SecondaryKeyState;
		m_SecondaryKey			= SecondaryKey;
		m_Priority				= Priority;

		m_PrimaryCheck			= (int)PrimaryKeyState;
		m_SecondaryCheck		= (int)SecondaryKeyState;

		AssignKeyChecks();
	}

	//
	public	void	AssignKeyChecks()
	{
		EKeyState primaryKeyState	= (EKeyState)m_PrimaryCheck;
		EKeyState secondaryKeyState = (EKeyState)m_SecondaryCheck;

		bool ScrollUpCheck(KeyCode k) { return Input.mouseScrollDelta.y > 0f; }
		bool ScrollDownCheck(KeyCode k) { return Input.mouseScrollDelta.y < 0f; }
		switch ( primaryKeyState )
		{
			case EKeyState.PRESS:		m_PrimaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD:		m_PrimaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE:		m_PrimaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP:	m_PrimaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: m_PrimaryKeyCheck	= ScrollDownCheck;		break;
			default:
			{
				Debug.Log($"WARNING: Command {Command.ToString()} has invalid \"PrimaryKeyCheck\" assigned");
				m_PrimaryKeyCheck = Input.GetKeyDown;
				break;
			}
		}
		switch ( secondaryKeyState )
		{
			case EKeyState.PRESS:		m_SecondaryKeyCheck	= Input.GetKeyDown;		break;
			case EKeyState.HOLD:		m_SecondaryKeyCheck	= Input.GetKey;			break;
			case EKeyState.RELEASE:		m_SecondaryKeyCheck	= Input.GetKeyUp;		break;
			case EKeyState.SCROLL_UP:	m_SecondaryKeyCheck	= ScrollUpCheck;		break;
			case EKeyState.SCROLL_DOWN: m_SecondaryKeyCheck	= ScrollDownCheck;		break;
			default:
			{
				Debug.Log($"WARNING: Command {Command.ToString()} has invalid \"SecondaryKeyCheck\" assigned");
				m_SecondaryKeyCheck = Input.GetKeyDown;
				break;
			}
		}
	}

	//
	public	void	Assign( EKeySlot keySlot, EKeyState? keyState, KeyCode? keyCode )
	{
		if (keyCode.HasValue)
		{
			switch (keySlot)
			{
				case EKeySlot.PRIMARY:		m_PrimaryKey	= keyCode.Value; break;
				case EKeySlot.SECONDARY:	m_SecondaryKey	= keyCode.Value; break;
				default: break;
			}
		}

		if (keyState.HasValue)
		{
			switch (keySlot)
			{
				case EKeySlot.PRIMARY:		m_PrimaryKeyState	= keyState.Value; break;
				case EKeySlot.SECONDARY:	m_SecondaryKeyState = keyState.Value; break;
				default: break;
			}

			AssignKeyChecks();
		}
	}

	//
	public KeyCode GetKeyCode(EKeySlot keySlot)
	{
		KeyCode code = KeyCode.None;
		switch (keySlot)
		{
			case EKeySlot.PRIMARY:		code = m_PrimaryKey;   break;
			case EKeySlot.SECONDARY:	code = m_SecondaryKey; break;
			default: break;
		}

		return code;
	}

	//
	public EKeyState GetKeyState(EKeySlot keySlot)
	{
		EKeyState keyState = EKeyState.PRESS;
		switch (keySlot)
		{
			case EKeySlot.PRIMARY:		keyState = m_PrimaryKeyState;   break;
			case EKeySlot.SECONDARY:	keyState = m_SecondaryKeyState; break;
			default: break;
		}

		return keyState;
	}

	//
	public void Get(EKeySlot keySlot, ref KeyCode keyCode, ref EKeyState keyState)
	{
		switch (keySlot)
		{
			case EKeySlot.PRIMARY:		keyCode = m_PrimaryKey;		keyState = m_PrimaryKeyState;   break;
			case EKeySlot.SECONDARY:	keyCode = m_SecondaryKey;	keyState = m_SecondaryKeyState; break;
			default: break;
		}
	}

	//
	public EKeySlot GetKeySlotByKeyCode(KeyCode keyCode)
	{
		if (m_PrimaryKey == keyCode)
		{
			return EKeySlot.PRIMARY;
		}
		return EKeySlot.SECONDARY;
	}

	//
	public EKeyState GetKeyStateByKeyCode(KeyCode keyCode)
	{
		if (m_PrimaryKey == keyCode)
		{
			return m_PrimaryKeyState;
		}
		return m_SecondaryKeyState;
	}
}



/// <summary> Main object that store bindings and serialized objects </summary>
[System.Serializable]
public	class KeyBindings
{
	[SerializeField]
	public	List<KeyCommandPair> Pairs = new List<KeyCommandPair>();
};