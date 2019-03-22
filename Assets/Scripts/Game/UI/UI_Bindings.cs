using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_Bindings : MonoBehaviour, IStateDefiner {

	static		List<Dropdown.OptionData> keyStateDropDownList = null;

	private		InputManager		m_InputMgr			= null;
	private		RectTransform		m_ThisRect			= null;
	private		GameObject			m_UI_CommandRow		= null;

	private		Transform			m_ScrollContent		= null;

	private		Transform			m_BlockPanel		= null;


	private	bool			m_bIsInitialized			= false;
	bool IStateDefiner.IsInitialized
	{
		get { return m_bIsInitialized; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	bool IStateDefiner.Initialize()
	{
		m_bIsInitialized = true;
		{
			m_ThisRect = transform as RectTransform;
			m_bIsInitialized &= m_ThisRect != null;

			keyStateDropDownList = new List<Dropdown.OptionData>();
			{
				keyStateDropDownList.AddRange
				(
					System.Enum.GetValues( typeof( eKeyState ) ).
					Cast<eKeyState>().
					Select( (eKeyState k ) => new Dropdown.OptionData( k.ToString() ) )
				);
			}


			m_UI_CommandRow = Resources.Load<GameObject>( "Prefabs/UI/UI_CommandRow" );
			m_bIsInitialized &= m_UI_CommandRow != null;

			m_ScrollContent = transform.GetComponentInChildren<VerticalLayoutGroup>().transform;
			m_bIsInitialized &= m_ScrollContent != null;

			m_BlockPanel = transform.Find("BlockPanel" );
			m_bIsInitialized &= m_BlockPanel != null;

			if ( m_bIsInitialized )
			{
				m_BlockPanel.gameObject.SetActive( false );
			}
			else
			{
				Debug.LogError( "UI_Bindings: Bad initialization!!!" );
			}
		}
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	bool IStateDefiner.ReInit()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return m_bIsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if ( GameManager.InputMgr != null )
		{
			m_InputMgr = GameManager.InputMgr;
		}
		else
		{
			m_InputMgr	= new InputManager();
		}

		FillGrid();
	}


	//////////////////////////////////////////////////////////////////////////
	private	IEnumerator	WaitForKeyCO( System.Action<KeyCode> OnKeyPressed )
	{
		KeyCode keyChosen	= KeyCode.None;

		m_BlockPanel.gameObject.SetActive( true );

		bool bIsWaiting	= true;
		while( bIsWaiting == true )
		{
			for ( KeyCode key = 0; key < KeyCode.JoystickButton0 && bIsWaiting == true; key++ )
			{
				bIsWaiting &= !Input.GetKeyDown( key );
				keyChosen = key;
			}
			yield return null;
		}

		m_BlockPanel.gameObject.SetActive( false );

		if ( OnKeyPressed != null )
		{
			OnKeyPressed(keyChosen);
		}
	}

	private Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
	private	void CreateGridElement( KeyCommandPair info )
	{
		GameObject commandRow = Instantiate<GameObject>( m_UI_CommandRow );
		{
			commandRow.transform.SetParent( m_ScrollContent );
			commandRow.transform.localScale  = Vector3.one;
			commandRow.name = info.Command.ToString();
		}

		// Command Label
		{
			Text commandLabel = commandRow.transform.GetChild( 0 ).GetComponent<Text>();
			commandLabel.text = info.Command.ToString();
		}

		// Primary KeyState Dropdown
		{
			Dropdown primaryKeyStateDropdown = commandRow.transform.GetChild( 1 ).GetComponent<Dropdown>();
			primaryKeyStateDropdown.AddOptions( keyStateDropDownList );
			primaryKeyStateDropdown.value = (int)info.PrimaryKeyState;
			primaryKeyStateDropdown.onValueChanged.AddListener( 
				delegate( int i )
				{
					OnKeyStateChanged( info, eKeys.PRIMARY, i );
				}
			);

			Text label = primaryKeyStateDropdown.transform.GetChild(0).GetComponent<Text>();
			label.text = info.PrimaryKeyState.ToString();
		}

		// Primary Key Choice Button
		{
			Button primaryKeyChoiceButton = commandRow.transform.GetChild( 2 ).GetComponent<Button>();
			primaryKeyChoiceButton.navigation = noNavigationMode;
			primaryKeyChoiceButton.onClick.AddListener( 
				delegate() 
				{
					OnKeyChoiceButtonClicked( info, eKeys.PRIMARY );
				}
			);

			Text label = primaryKeyChoiceButton.transform.GetChild(0).GetComponent<Text>();
			label.text = info.GetKeyCode( eKeys.PRIMARY ).ToString();
		}

		// Secondary KeyState Dropdown
		{
			Dropdown secondaryKeyStateDropdown = commandRow.transform.GetChild( 3 ).GetComponent<Dropdown>();
			secondaryKeyStateDropdown.AddOptions( keyStateDropDownList );
			secondaryKeyStateDropdown.value = (int)info.SecondaryKeyState;
			secondaryKeyStateDropdown.onValueChanged.AddListener( 
				delegate( int i )
				{
					OnKeyStateChanged( info, eKeys.SECONDARY, i );
				}
			);

			Text label = secondaryKeyStateDropdown.transform.GetChild(0).GetComponent<Text>();
			label.text = info.SecondaryKeyState.ToString();
		}

		// Secondary Key Choice Button
		{
			Button secondaryKeyChoiceButton = commandRow.transform.GetChild( 4 ).GetComponent<Button>();
			secondaryKeyChoiceButton.navigation = noNavigationMode;
			secondaryKeyChoiceButton.onClick.AddListener( 
				delegate()
				{
					OnKeyChoiceButtonClicked( info, eKeys.SECONDARY );
				}
			);

			Text label = secondaryKeyChoiceButton.transform.GetChild(0).GetComponent<Text>();
			label.text = info.GetKeyCode( eKeys.SECONDARY ).ToString();
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	FillGrid()
	{
		// Clear the content of scroll view
		foreach( Transform t in m_ScrollContent )
		{
			Destroy( t.gameObject );
		}

		// Fill the grid
		System.Array.ForEach( m_InputMgr.Bindings, CreateGridElement );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnKeyStateChanged( KeyCommandPair info, eKeys Key, int newValue )
	{
		eKeyState newKeyState = ( eKeyState ) newValue;

		// skip for identical assignment
		if ( newKeyState == info.GetKeyState( Key ) )
			return;

		{
			m_InputMgr.AssignNewKeyState( Key, newKeyState, info.Command );

			if ( newKeyState == eKeyState.SCROLL_DOWN || newKeyState == eKeyState.SCROLL_UP )
			{
				info.Assign( Key, null, KeyCode.None );
			}
		}
		FillGrid();
	}
	
	//////////////////////////////////////////////////////////////////////////
	private	void	OnKeyChoiceButtonClicked( KeyCommandPair info, eKeys Key )
	{
		// Create callback for key assigning
		System.Action<KeyCode> onKeyPressed = delegate( KeyCode keyCode )
		{
			// skip for identical assignment
			if ( keyCode == info.GetKeyCode( Key ) )
				return;

			if ( m_InputMgr.CanNewKeyCodeBeAssigned( Key, keyCode, info.Command ) )
			{
				if ( m_InputMgr.AssignNewKeyCode( Key, keyCode, info.Command, false ) )
				{
					m_InputMgr.SaveBindings();
				}
			}
			else
			{
				System.Action onConfirm = delegate()
				{
					if ( m_InputMgr.AssignNewKeyCode( Key, keyCode, info.Command, true ) )
					{
						m_InputMgr.SaveBindings();
					}

					FillGrid();
				};

				UI.Instance.Confirmation.Show( "Confirm key substitution?", OnConfirm: onConfirm, OnCancel: null );
			}
		};

		StartCoroutine( WaitForKeyCO( onKeyPressed ));
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Apply()
	{
		m_InputMgr.SaveBindings();
		FillGrid();
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		m_InputMgr	= null;
	}


}
