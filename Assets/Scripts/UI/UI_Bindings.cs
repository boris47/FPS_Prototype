using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using OptionData = UnityEngine.UI.Dropdown.OptionData;

public sealed class UI_Bindings : UI_Base, IStateDefiner
{
	static				List<OptionData>	m_KeyStateDropDownList				= null;
	private				GameObject			m_UI_CommandRow						= null;
	private				GameObject			m_UI_CommandRow_Separator			= null;
	private				Transform			m_ScrollContentTransform			= null;
	private				Transform			m_BlockPanel						= null;
	private				Button				m_ApplyButton						= null;
	private				Button				m_BackButton						= null;

	private				bool				m_IsInitialized						= false;
						bool				IStateDefiner.IsInitialized			=> m_IsInitialized;

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.PreInit()
	{

	}

	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.Initialize()
	{
		if (!m_IsInitialized)
		{
			// Preload KeyState list
			m_KeyStateDropDownList = new List<OptionData>
			(
				System.Enum.GetValues(typeof(EKeyState)).Cast<EKeyState>().Select(keyState => new OptionData(keyState.ToString()))
			);

			// Load Command UI Row
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Prefabs/UI/UI_CommandRow", out m_UI_CommandRow));

			// Load Command Separator
			CustomAssertions.IsTrue(ResourceManager.LoadResourceSync("Prefabs/UI/UI_CommandRow_Separator", out m_UI_CommandRow_Separator));

			// Find Vertical Layout Group component
			CustomAssertions.IsTrue(transform.TrySearchComponent(ESearchContext.LOCAL_AND_CHILDREN, out m_ScrollContentTransform, c => c.HasComponent<VerticalLayoutGroup>()));

			// Find Block Panel (Used in key assignment)
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("BlockPanel", out m_BlockPanel)))
			{
				m_BlockPanel.gameObject.SetActive(false);
			}

			// Apply button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Apply", out m_ApplyButton)))
			{
				m_ApplyButton.onClick.AddListener(GlobalManager.InputMgr.SaveBindings);
				m_ApplyButton.interactable = false;
			}

			// Back button
			if (CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Button_Back", out m_BackButton)))
			{
				m_BackButton.onClick.AddListener(() => UIManager.Instance.GoBack());
			}

			FillGrid();

			// disable navigation for everything
			Navigation noNavigationMode = new Navigation() { mode = Navigation.Mode.None };
			foreach (Selectable s in GetComponentsInChildren<Selectable>())
			{
				s.navigation = noNavigationMode;
			}

			m_IsInitialized = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	void IStateDefiner.ReInit()
	{
		
	}


	//////////////////////////////////////////////////////////////////////////
	bool	 IStateDefiner.Finalize()
	{
		return m_IsInitialized;
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		CustomAssertions.IsTrue(m_IsInitialized);

		m_ApplyButton.interactable = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void CreateGridElement(KeyCommandPair info)
	{
		GameObject commandRow = Instantiate(m_UI_CommandRow);
		{
			commandRow.transform.SetParent(m_ScrollContentTransform);
			commandRow.transform.localScale = Vector3.one;
			commandRow.name = info.Command.ToString();
		}

		// Command Label
		if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(0, out Text commandLabel)))
		{
			commandLabel.text = info.Command.ToString();
		}

		// Primary KeyState Dropdown
		if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(1, out Dropdown primaryKeyStateDropdown)))
		{
			primaryKeyStateDropdown.AddOptions(m_KeyStateDropDownList);
			primaryKeyStateDropdown.value = (int)info.PrimaryKeyState;

			if (CustomAssertions.IsTrue(primaryKeyStateDropdown.transform.TrySearchComponentByChildIndex(0, out Text label)))
			{
				label.text = info.PrimaryKeyState.ToString();
				primaryKeyStateDropdown.onValueChanged.AddListener(newValue => OnKeyStateChanged(info, EKeySlot.PRIMARY, newValue, label));
			}
		}

		// Primary Key Choice Button
		if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(2, out Button primaryKeyChoiceButton)))
		{
			if (CustomAssertions.IsTrue(primaryKeyChoiceButton.transform.TrySearchComponentByChildIndex(0, out Text label)))
			{
				label.text = info.PrimaryKey.ToString();
				primaryKeyChoiceButton.onClick.AddListener(() => OnKeyChoiceButtonClicked(info, EKeySlot.PRIMARY, label));
			}
		}

		// Secondary KeyState Dropdown
		if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(3, out Dropdown secondaryKeyStateDropdown)))
		{
			secondaryKeyStateDropdown.AddOptions(m_KeyStateDropDownList);
			secondaryKeyStateDropdown.value = (int)info.SecondaryKeyState;

			if (CustomAssertions.IsTrue(secondaryKeyStateDropdown.transform.TrySearchComponentByChildIndex(0, out Text label)))
			{
				label.text = info.SecondaryKeyState.ToString();
				secondaryKeyStateDropdown.onValueChanged.AddListener(newValue => OnKeyStateChanged(info, EKeySlot.SECONDARY, newValue, label));
			}
		}

		// Secondary Key Choice Button
		if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(4, out Button secondaryKeyChoiceButton)))
		{
			if (CustomAssertions.IsTrue(secondaryKeyChoiceButton.transform.TrySearchComponentByChildIndex(0, out Text label)))
			{
				label.text = info.SecondaryKey.ToString();
				secondaryKeyChoiceButton.onClick.AddListener(() => OnKeyChoiceButtonClicked(info, EKeySlot.SECONDARY, label));
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void FillGrid()
	{
		// Clear the content of scroll view
		foreach (Transform t in m_ScrollContentTransform)
		{
			Destroy(t.gameObject);
		}

		for (EInputCategory category = EInputCategory.NONE + 1; category < EInputCategory.ALL; category++)
		{
			GameObject separator = Instantiate(m_UI_CommandRow_Separator);
			{
				separator.transform.SetParent(m_ScrollContentTransform);
				separator.transform.localScale = Vector3.one;
				separator.name = category.ToString();

				if (CustomAssertions.IsTrue(separator.transform.TrySearchComponentByChildIndex(0, out Text textComponent)))
				{
					textComponent.text = textComponent.text.Replace("TO_REPLACE", category.ToString());
				}
			}

			GlobalManager.InputMgr.Bindings.Where(p => p.Category == category).ToList().ForEach(CreateGridElement);
		}


		// Fill the grid
	//	System.Array.ForEach(GlobalManager.InputMgr.Bindings, CreateGridElement);
	}


	private static bool IsLegalKeyCode(in KeyCode keyCode)
	{
		return keyCode > KeyCode.None
			&& keyCode < KeyCode.F1 && keyCode > KeyCode.F15
			&& keyCode != KeyCode.Escape
			&& keyCode != KeyCode.LeftWindows && keyCode != KeyCode.RightWindows
			&& keyCode != KeyCode.LeftCommand && keyCode != KeyCode.RightCommand
			;
	}


	//////////////////////////////////////////////////////////////////////////
	private IEnumerator WaitForKeyCO(KeyCode currentKey, System.Action<KeyCode> OnKeyPressed)
	{
		m_BlockPanel.gameObject.SetActive(true);
		{
			bool bIsWaiting = true;
			while (bIsWaiting)
			{
				if (Input.anyKeyDown) // Input inserted, but not backspace (Backspace has been choosen as assignment cancellation)
				{
					if (Input.GetKeyDown(KeyCode.Backspace))
					{
						break;
					}

					for (KeyCode key = 0; key < KeyCode.JoystickButton0; key++) // Find the inserted input
					{
						if (Input.GetKeyDown(key) && IsLegalKeyCode(key) && key != currentKey) // Pressed key found, skipping the already assinged one
						{
							OnKeyPressed(key);
							bIsWaiting = false;
							break;
						}
					}
				}
				yield return null;
			}
		}
		m_BlockPanel.gameObject.SetActive(false);
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnKeyStateChanged(KeyCommandPair info, EKeySlot Key, int newValue, Text buttonLabel)
	{
		EKeyState newKeyState = (EKeyState)newValue;

		// skip for identical key state
		if (newKeyState != info.GetKeyState(Key))
		{
			GlobalManager.InputMgr.AssignNewKeyState(Key, newKeyState, info.Command);

			if (newKeyState == EKeyState.SCROLL_DOWN || newKeyState == EKeyState.SCROLL_UP)
			{
				info.Assign(Key, null, KeyCode.None);
			}

			buttonLabel.text = info.GetKeyState(Key).ToString();

			RefreshLabels();

			m_ApplyButton.interactable = true;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnKeyChoiceButtonClicked(KeyCommandPair info, EKeySlot keySlot, Text buttonLabel)
	{
		KeyCode currentKey = info.GetKeyCode(keySlot);

		void OnKeyAssigned()
		{
			buttonLabel.text = currentKey.ToString();

			RefreshLabels();

			m_ApplyButton.interactable = true;
		}

		// Create callback for key assigning
		void OnKeyPressed(KeyCode keyCode)
		{
			if (!GlobalManager.InputMgr.HasKeyCodeAlreadyBeenAssigned(info.Command, keySlot, keyCode, out KeyCommandPair otherPair))
			{
				CustomAssertions.IsTrue(GlobalManager.InputMgr.TryAssignNewKeyCode(info.Command, keySlot, keyCode, bMustSwap: false));

				OnKeyAssigned();
			}
			else
			{
				void OnConfirm()
				{
					CustomAssertions.IsTrue(GlobalManager.InputMgr.TryAssignNewKeyCode(info.Command, keySlot, keyCode, bMustSwap: true));

					OnKeyAssigned();
				};
				UIManager.Confirmation.Show("Confirm key substitution?", OnConfirm);
			}
		};
		CoroutinesManager.Start(WaitForKeyCO(currentKey, OnKeyPressed), "UI_Bindings::OnKeyChoiceButtonClicked: Waiting for button press");
	}


	//////////////////////////////////////////////////////////////////////////
	private void RefreshLabels()
	{
		void UpdateCommandRow(Transform commandRow, KeyCommandPair info)
		{
			// Primary KeyState Dropdown
			if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(1, out Dropdown primaryKeyStateDropdown)))
			{
				if (CustomAssertions.IsTrue(primaryKeyStateDropdown.transform.TrySearchComponentByChildIndex(0, out Text label)))
				{
					label.text = info.PrimaryKeyState.ToString();
				}
			}

			// Primary Key Choice Button
			if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(2, out Button primaryKeyChoiceButton)))
			{
				if (CustomAssertions.IsTrue(primaryKeyChoiceButton.transform.TrySearchComponentByChildIndex(0, out Text label)))
				{
					label.text = info.PrimaryKey.ToString();
				}
			}

			// Secondary KeyState Dropdown
			if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(3, out Dropdown secondaryKeyStateDropdown)))
			{
				if (CustomAssertions.IsTrue(secondaryKeyStateDropdown.transform.TrySearchComponentByChildIndex(0, out Text label)))
				{
					label.text = info.SecondaryKeyState.ToString();
				}
			}

			// Secondary Key Choice Button
			if (CustomAssertions.IsTrue(commandRow.transform.TrySearchComponentByChildIndex(4, out Button secondaryKeyChoiceButton)))
			{
				if (CustomAssertions.IsTrue(secondaryKeyChoiceButton.transform.TrySearchComponentByChildIndex(0, out Text label)))
				{
					label.text = info.SecondaryKey.ToString();
				}
			}
		}

		KeyCommandPair[] bindings = GlobalManager.InputMgr.Bindings;
		for (int i = 0; i < bindings.Length; i++)
		{
			KeyCommandPair binding = bindings[i];
			Transform commandRow = m_ScrollContentTransform.GetChild(i);
			UpdateCommandRow(commandRow, binding);
		}
	}
}
