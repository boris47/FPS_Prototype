using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public sealed class UI_Bindings : MonoBehaviour, IStateDefiner {

	static		List<Dropdown.OptionData> keyStateDropDownList		= null;
	private		InputManager		m_InputMgr						= null;
	private		GameObject			m_UI_CommandRow					= null;
	private		Transform			m_ScrollContentTransform		= null;
	private		Transform			m_BlockPanel					= null;
	private		Button				m_ApplyButton					= null;


	private	bool			m_IsInitialized					= false;
	bool IStateDefiner.IsInitialized
	{
		get { return this.m_IsInitialized; }
	}

	string IStateDefiner.StateName
	{
		get { return this.name; }
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	IEnumerator IStateDefiner.Initialize()
	{
		if (this.m_IsInitialized == true )
			yield break;

		CoroutinesManager.AddCoroutineToPendingCount( 1 );

		this.m_IsInitialized = true;
		{
			keyStateDropDownList = new List<Dropdown.OptionData>
			(
				System.Enum.GetValues( typeof( EKeyState ) ).
				Cast<EKeyState>().
				Select( ( EKeyState k ) => new Dropdown.OptionData( k.ToString() ) )
			);

			yield return null;

			if (this.m_IsInitialized &= this.transform.SearchComponentInChild( "Button_Apply", ref this.m_ApplyButton ) )
			{
				this.m_ApplyButton.interactable = false;
			}

			yield return null;
			
			// UI Command Row Prefab
			ResourceManager.LoadedData<GameObject> loadedResource = new ResourceManager.LoadedData<GameObject>();
			yield return ResourceManager.LoadResourceAsyncCoroutine
			(
				ResourcePath:			"Prefabs/UI/UI_CommandRow",
				loadedResource:			loadedResource,
				OnResourceLoaded:		(a) => { this.m_IsInitialized &= true; this.m_UI_CommandRow = a; },
				OnFailure:				(p) => this.m_IsInitialized &= false
			);

			// Scroll content conmponent
			this.m_IsInitialized &= this.transform.SearchComponent( ref this.m_ScrollContentTransform, ESearchContext.CHILDREN, c => c.HasComponent<VerticalLayoutGroup>() );

			this.m_BlockPanel = this.transform.Find("BlockPanel" );
			this.m_IsInitialized &= this.m_BlockPanel != null;

			yield return null;

			if (this.m_IsInitialized )
			{
				this.m_BlockPanel.gameObject.SetActive( false );
				CoroutinesManager.RemoveCoroutineFromPendingCount( 1 );
			}
			else
			{
				Debug.LogError( "UI_Bindings: Bad initialization!!!" );
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// ReInit
	IEnumerator IStateDefiner.ReInit()
	{
		yield return null;
	}


	//////////////////////////////////////////////////////////////////////////
	// Finalize
	bool	 IStateDefiner.Finalize()
	{
		return this.m_IsInitialized;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if ( GlobalManager.InputMgr != null )
		{
			this.m_InputMgr = GlobalManager.InputMgr;
		}
		else
		{
			this.m_InputMgr	= new InputManager();
			this.m_InputMgr.Setup();
			Debug.Log( "UI_Bindings::OnEnable: Cannot fin input manager in global manager" );
		}

		this.FillGrid();
	}


	//////////////////////////////////////////////////////////////////////////
	private	IEnumerator	WaitForKeyCO( System.Action<KeyCode> OnKeyPressed )
	{
		KeyCode keyChosen	= KeyCode.None;

		this.m_BlockPanel.gameObject.SetActive( true );

		bool bIsWaiting	= true;
		while( bIsWaiting == true )
		{
			for ( KeyCode key = 0; key < KeyCode.JoystickButton0 && bIsWaiting == true; key++ )
			{
				bIsWaiting &= !Input.GetKeyDown( key );
				if ( bIsWaiting == false && key != KeyCode.Backspace )
				{
					keyChosen = key;
				}
			}
			yield return null;
		}

		this.m_BlockPanel.gameObject.SetActive( false );

		if ( OnKeyPressed != null && keyChosen != KeyCode.None )
		{
			OnKeyPressed(keyChosen);
		}
	}

	private Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
	private	void CreateGridElement( KeyCommandPair info )
	{
		GameObject commandRow = Instantiate<GameObject>(this.m_UI_CommandRow );
		{
			commandRow.transform.SetParent(this.m_ScrollContentTransform );
			commandRow.transform.localScale  = Vector3.one;
			commandRow.name = info.Command.ToString();
		}

		// Command Label
		{
			Text commandLabel = null;
			if ( commandRow.transform.SearchComponentInChild( 0, ref commandLabel ) )
				commandLabel.text = info.Command.ToString();
		}

		// Primary KeyState Dropdown
		{
			Dropdown primaryKeyStateDropdown = null;
			if ( commandRow.transform.SearchComponentInChild( 1, ref primaryKeyStateDropdown ) )
			{
				primaryKeyStateDropdown.AddOptions( keyStateDropDownList );
				primaryKeyStateDropdown.value = (int)info.PrimaryKeyState;
				primaryKeyStateDropdown.onValueChanged.AddListener( 
					delegate( int i )
					{
						this.OnKeyStateChanged( info, EKeys.PRIMARY, i );
					}
				);
			}

			Text label = null;
			if ( primaryKeyStateDropdown.transform.SearchComponentInChild( 0, ref label ) )
				label.text = info.PrimaryKeyState.ToString();
		}

		// Primary Key Choice Button
		{
			Button primaryKeyChoiceButton = null;
			if (commandRow.transform.SearchComponentInChild( 2, ref primaryKeyChoiceButton ) )
			{
				primaryKeyChoiceButton.navigation = this.noNavigationMode;
				primaryKeyChoiceButton.onClick.AddListener( 
					delegate() 
					{
						this.OnKeyChoiceButtonClicked( info, EKeys.PRIMARY );
					}
				);
			}

			Text label = null;
			if ( primaryKeyChoiceButton.transform.SearchComponentInChild( 0, ref label ) )
				label.text = info.GetKeyCode( EKeys.PRIMARY ).ToString();
		}

		// Secondary KeyState Dropdown
		{
			Dropdown secondaryKeyStateDropdown = null;
			if ( commandRow.transform.SearchComponentInChild( 3, ref secondaryKeyStateDropdown ) )
			{
				secondaryKeyStateDropdown.AddOptions( keyStateDropDownList );
				secondaryKeyStateDropdown.value = (int)info.SecondaryKeyState;
				secondaryKeyStateDropdown.onValueChanged.AddListener( 
					delegate( int i )
					{
						this.OnKeyStateChanged( info, EKeys.SECONDARY, i );
					}
				);
			}

			Text label = null;
			if ( secondaryKeyStateDropdown.transform.SearchComponentInChild( 0, ref label ) )
				label.text = info.SecondaryKeyState.ToString();
		}

		// Secondary Key Choice Button
		{
			Button secondaryKeyChoiceButton = null;
			if ( commandRow.transform.SearchComponentInChild( 4, ref secondaryKeyChoiceButton ) )
			{
				secondaryKeyChoiceButton.navigation = this.noNavigationMode;
				secondaryKeyChoiceButton.onClick.AddListener( 
					delegate()
					{
						this.OnKeyChoiceButtonClicked( info, EKeys.SECONDARY );
					}
				);
			}

			Text label = null;
			if ( secondaryKeyChoiceButton.transform.SearchComponentInChild( 0, ref label ) )
				label.text = info.GetKeyCode( EKeys.SECONDARY ).ToString();
		}
	}

	
	//////////////////////////////////////////////////////////////////////////
	private	void	FillGrid()
	{
		// Clear the content of scroll view
		foreach( Transform t in this.m_ScrollContentTransform )
		{
			Destroy( t.gameObject );
		}

		// Fill the grid
		System.Array.ForEach(this.m_InputMgr.Bindings, this.CreateGridElement );
	}


	//////////////////////////////////////////////////////////////////////////
	private	void	OnKeyStateChanged( KeyCommandPair info, EKeys Key, int newValue )
	{
		EKeyState newKeyState = ( EKeyState ) newValue;

		// skip for identical assignment
		if ( newKeyState == info.GetKeyState( Key ) )
			return;

		{
			this.m_InputMgr.AssignNewKeyState( Key, newKeyState, info.Command );

			if ( newKeyState == EKeyState.SCROLL_DOWN || newKeyState == EKeyState.SCROLL_UP )
			{
				info.Assign( Key, null, KeyCode.None );
			}
		}

		this.FillGrid();
		this.m_ApplyButton.interactable = true;
	}
	
	//////////////////////////////////////////////////////////////////////////
	private	void	OnKeyChoiceButtonClicked( KeyCommandPair info, EKeys Key )
	{
		// Create callback for key assigning
		System.Action<KeyCode> onKeyPressed = delegate( KeyCode keyCode )
		{
			// skip for identical assignment
			if ( keyCode == info.GetKeyCode( Key ) )
				return;

			if (this.m_InputMgr.CanNewKeyCodeBeAssigned( Key, keyCode, info.Command ) )
			{
				if (this.m_InputMgr.AssignNewKeyCode( Key, keyCode, info.Command, false ) )
				{
//					m_InputMgr.SaveBindings();
				}

				this.FillGrid();
				this.m_ApplyButton.interactable = true;
			}
			else
			{
				System.Action onConfirm = delegate()
				{
					if (this.m_InputMgr.AssignNewKeyCode( Key, keyCode, info.Command, true ) )
					{
//						m_InputMgr.SaveBindings();
					}

					this.FillGrid();
					this.m_ApplyButton.interactable = true;
				};

				UIManager.Confirmation.Show( "Confirm key substitution?", OnConfirm: onConfirm, OnCancel: null );
			}
		};

		CoroutinesManager.Start(this.WaitForKeyCO( onKeyPressed ),
			"UI_Bindings::OnKeyChoiceButtonClicked: Waiting for button press"
			);
	}


	//////////////////////////////////////////////////////////////////////////
	public	void	Apply()
	{
		System.Action onConfirm = delegate()
		{
			this.m_InputMgr.SaveBindings();

			this.FillGrid();
			this.m_ApplyButton.interactable = false;
		};

		UIManager.Confirmation.Show( "Confirm bindings?", OnConfirm: onConfirm, OnCancel: null );
	}


	//////////////////////////////////////////////////////////////////////////
	public void ResetBindinggs()
	{
		System.Action onConfirm = delegate()
		{
			this.m_InputMgr.ResetBindings();

			this.FillGrid();
			this.m_ApplyButton.interactable = false;
		};

		UIManager.Confirmation.Show( "Do you really want to reset bindings?", OnConfirm: onConfirm, OnCancel: null );
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
		this.m_InputMgr	= null;
	}


}
