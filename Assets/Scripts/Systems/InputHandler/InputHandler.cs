using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[DefaultExecutionOrder(-100)]
public class InputHandler : GlobalMonoBehaviourSingleton<InputHandler>
{
	private static Dictionary<MonoBehaviour, List<ActionLogicBase>> s_RegisteredActions = new Dictionary<MonoBehaviour, List<ActionLogicBase>>();


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void RegisterButtonCallbacks(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction, in System.Action InOnPress, in System.Action InOnHold, in System.Action InOnRelease)
	{
		GetOrCreateInputActionList(InMonoBehaviour).Add(new ActionLogicButton(InInputAction, InOnPress, InOnHold, InOnRelease));
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void RegisterAxis2DCallback(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction, in System.Action<Vector2> InOnChange, in bool InTryReadRaw)
	{
		GetOrCreateInputActionList(InMonoBehaviour).Add(new ActionLogicAxis(InInputAction, InOnChange, InTryReadRaw));
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void UnRegisterCallbacks(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction)
	{
		if (s_RegisteredActions.TryGetValue(InMonoBehaviour, out List<ActionLogicBase> InputActionList))
		{
			for (int index = InputActionList.Count - 1; index >= 0; index--)
			{
				ActionLogicBase logic = InputActionList[index];
				if (logic.m_InputAction.id == InInputAction.id)
				{
					logic.Destroy();
					InputActionList.RemoveAt(index);
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private static List<ActionLogicBase> GetOrCreateInputActionList(in MonoBehaviour InMonoBehaviour)
	{
		List<ActionLogicBase> OutResult = default;
		if (!s_RegisteredActions.TryGetValue(InMonoBehaviour, out OutResult))
		{
			s_RegisteredActions.Add(InMonoBehaviour, OutResult = new List<ActionLogicBase>());
		}
		return OutResult;
	}

	//////////////////////////////////////////////////////////////////////////
	private void Update()
	{
		InputSystem.Update();
		foreach (KeyValuePair<MonoBehaviour, List<ActionLogicBase>> behaviourActionTuple in s_RegisteredActions)
		{
			foreach (ActionLogicBase actionLogic in behaviourActionTuple.Value)
			{
				actionLogic.Update();
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	private void OnDestroy()
	{
		foreach (KeyValuePair<MonoBehaviour, List<ActionLogicBase>> behaviourActionTuple in s_RegisteredActions)
		{
			foreach (ActionLogicBase actionLogic in behaviourActionTuple.Value)
			{
				actionLogic.Destroy();
			}
		}
	}



	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////

	private abstract class ActionLogicBase
	{
		public readonly InputAction m_InputAction;

		protected ActionLogicBase(in InputAction InInputAction)
		{
			if (Utils.CustomAssertions.IsNotNull(InInputAction))
			{
				m_InputAction = InInputAction;
			}
		}

		public abstract void Update();

		public virtual void Destroy() { }
	}

	/////////////////////////////////////////////////////////////////////////////
	private class ActionLogicButton : ActionLogicBase
	{
		private readonly System.Action m_OnPress = delegate { };
		private readonly System.Action m_OnHold = delegate { };
		private readonly System.Action m_OnRelease = delegate { };


		//////////////////////////////////////////////////////////////////////////
		public ActionLogicButton(in InputAction InInputAction, in System.Action InOnPress, in System.Action InOnHold, in System.Action InOnRelease) : base(InInputAction)
		{
			if (Utils.CustomAssertions.IsTrue(InOnPress.IsNotNull() || InOnHold.IsNotNull() || InOnRelease.IsNotNull()))
			{
				m_OnPress = InOnPress ?? m_OnPress;
				m_OnHold = InOnHold ?? m_OnHold;
				m_OnRelease = InOnRelease ?? m_OnRelease;

				// Old UnityEngine.input.GetButtonDown
				m_InputAction.started -= OnButtonPressed;
				m_InputAction.started += OnButtonPressed;

				// Old UnityEngine.input.GetButtonUp
				m_InputAction.canceled -= OnButtonReleased;
				m_InputAction.canceled += OnButtonReleased;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Update()
		{
			// Old UnityEngine.Input.GetButton		
			if (m_InputAction.IsPressed())
			{
				m_OnHold();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Destroy()
		{
			m_InputAction.started -= OnButtonPressed;
			m_InputAction.canceled -= OnButtonReleased;
		}

		//////////////////////////////////////////////////////////////////////////
		private void OnButtonPressed(InputAction.CallbackContext ctx) => m_OnPress();

		//////////////////////////////////////////////////////////////////////////
		private void OnButtonReleased(InputAction.CallbackContext ctx) => m_OnRelease();
	}

	/////////////////////////////////////////////////////////////////////////////
	private class ActionLogicAxis : ActionLogicBase
	{
		private readonly System.Action<Vector2> m_OnChange = delegate { };
		private readonly bool m_TryReadRaw = false;
		private bool m_CanReadValue = false;
		private InputControl<Vector2> m_InputControl = null;
		private bool m_CanReadRaw = false;


		//////////////////////////////////////////////////////////////////////////
		public ActionLogicAxis(in InputAction InInputAction, in System.Action<Vector2> InOnChange, in bool InTryReadRaw) : base(InInputAction)
		{
			if (Utils.CustomAssertions.IsNotNull(InOnChange))
			{
				m_OnChange = InOnChange;
				m_TryReadRaw = InTryReadRaw;

				m_InputAction.started -= EnableRead;
				m_InputAction.started += EnableRead;

				m_InputAction.canceled -= DisableRead;
				m_InputAction.canceled += DisableRead;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public override void Update()
		{
			if (m_CanReadValue)
			{
				m_OnChange(m_TryReadRaw && m_CanReadRaw ? m_InputControl.ReadUnprocessedValue() : m_InputAction.ReadValue<Vector2>());
			}
		}

		//////////////////////////////////////////////////////////////////////////
		private void EnableRead(InputAction.CallbackContext ctx)
		{
			m_CanReadRaw = false;
			if (ctx.control is InputControl<Vector2> inputControl)
			{
				m_InputControl = inputControl;
				m_CanReadRaw = true;
			}
			m_CanReadValue = true;
		}

		//////////////////////////////////////////////////////////////////////////
		private void DisableRead(InputAction.CallbackContext ctx)
		{
			m_CanReadValue = false;
		//	m_OnChange(Vector2.zero);
		}
	}
}
