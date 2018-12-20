using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_Bindings : MonoBehaviour {

	static		List<Dropdown.OptionData> keyStateDropDownList = null;

	private		InputManager		m_InputMgr			= null;
	private		RectTransform		m_ThisRect			= null;
	private		GameObject			m_UI_CommandRow		= null;

	private		Transform			m_ScrollContent		= null;

	// Wait For Button Press
	private		Coroutine			m_WaitForKeyCO		= null;
	private		bool				m_IsWaitingForKey	= false;

	private		Transform			m_BlockPanel		= null;


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		m_ThisRect = transform as RectTransform;
		m_InputMgr	= new InputManager();

		if ( keyStateDropDownList == null )
		{
			keyStateDropDownList = new List<Dropdown.OptionData>();
			{
				keyStateDropDownList.AddRange(
					System.Enum.GetValues( typeof( eKeyState ) ).
					Cast<eKeyState>().
					Select( (eKeyState k ) => new Dropdown.OptionData( k.ToString() ) )
				);
			}
		}

		m_UI_CommandRow = Resources.Load<GameObject>( "Prefabs/UI/UI_CommandRow" );

		m_ScrollContent = transform.GetComponentInChildren<VerticalLayoutGroup>().transform;

		m_BlockPanel = transform.Find("BlockPanel" );
		m_BlockPanel.gameObject.SetActive( false );

		FillGrid();
	}


	//////////////////////////////////////////////////////////////////////////
	private	IEnumerator	WaitForKeyCO( System.Action<KeyCode> OnKeyPressed )
	{
		KeyCode keyChosen	= KeyCode.None;
		m_IsWaitingForKey = true;

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

		m_IsWaitingForKey = false;
		if ( OnKeyPressed != null )
		{
			OnKeyPressed(keyChosen);
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	FillGrid()
	{

		foreach( Transform t in m_ScrollContent )
		{
			Destroy( t.gameObject );
		}

		Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };

		// Fill the grid
		System.Array.ForEach( m_InputMgr.Bindings, ( KeyCommandPair info ) =>
		{
			GameObject commandRow = Instantiate<GameObject>( m_UI_CommandRow );
			{
				commandRow.transform.SetParent( m_ScrollContent );
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
					(int i) => {
						OnPrimaryKeyStateChanged( info, i );
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
					() => {
						OnPrimaryKeyChoiceButtonClicked( info );
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
					(int i) => {
						OnSecondaryKeyStateChanged( info, i );
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
					() => {
						OnSecondaryKeyChoiceButtonClicked( info );
					}
				);

				Text label = secondaryKeyChoiceButton.transform.GetChild(0).GetComponent<Text>();
				label.text = info.GetKeyCode( eKeys.SECONDARY ).ToString();
			}
			
		} );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnPrimaryKeyStateChanged( KeyCommandPair info, int newValue )
	{
		eKeyState newKeyState = ( eKeyState ) newValue;
		{
			m_InputMgr.AssignNewKeyState( eKeys.PRIMARY, newKeyState, info.Command );

			if ( newKeyState == eKeyState.SCROLL_DOWN || newKeyState == eKeyState.SCROLL_UP )
			{
				info.Assign( eKeys.PRIMARY, null, KeyCode.None );
			}
		}
		FillGrid();
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	OnSecondaryKeyStateChanged( KeyCommandPair info, int newValue )
	{
		eKeyState newKeyState = ( eKeyState ) newValue;
		{
			m_InputMgr.AssignNewKeyState( eKeys.SECONDARY, newKeyState, info.Command );

			if ( newKeyState == eKeyState.SCROLL_DOWN || newKeyState == eKeyState.SCROLL_UP )
			{
				info.Assign( eKeys.SECONDARY, null, KeyCode.None );
			}
		}
		FillGrid();
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnPrimaryKeyChoiceButtonClicked( KeyCommandPair info )
	{
		// Create callback for key assigning
		System.Action<KeyCode> onKeyPressed = delegate( KeyCode keyCode )
		{
			if ( m_InputMgr.CanNewKeyCodeBeAssigned( eKeys.PRIMARY, keyCode, info.Command ) )
			{
				if ( m_InputMgr.AssignNewKeyCode( eKeys.PRIMARY, keyCode, info.Command, false ) )
				{
					m_InputMgr.SaveBindings();
				}
			}
			else
			{
				System.Action onConfirm = delegate()
				{
					if ( m_InputMgr.AssignNewKeyCode( eKeys.PRIMARY, keyCode, info.Command, true ) )
					{
						m_InputMgr.SaveBindings();
					}

					FillGrid();
				};

				UI.Instance.Confirmation.Show( "Confirm key substitution?", OnConfirm: onConfirm, OnCancel: null );
			}
		};
		StartCoroutine( WaitForKeyCO( onKeyPressed ) );
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	OnSecondaryKeyChoiceButtonClicked( KeyCommandPair info )
	{
		// Create callback for key assigning
		System.Action<KeyCode> onKeyPressed = delegate( KeyCode keyCode )
		{
			if ( m_InputMgr.CanNewKeyCodeBeAssigned( eKeys.SECONDARY, keyCode, info.Command ) )
			{
				if ( m_InputMgr.AssignNewKeyCode( eKeys.SECONDARY, keyCode, info.Command, false ) )
				{
					m_InputMgr.SaveBindings();
				}
			}
			else
			{
				System.Action onConfirm = delegate()
				{
					if ( m_InputMgr.AssignNewKeyCode( eKeys.SECONDARY, keyCode, info.Command, true ) )
					{
						m_InputMgr.SaveBindings();
					}

					FillGrid();
				};

				UI.Instance.Confirmation.Show( "Confirm key substitution?", OnConfirm: onConfirm, OnCancel: null );
			}
		};
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
