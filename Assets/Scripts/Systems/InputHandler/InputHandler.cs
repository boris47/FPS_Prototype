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
	public static void RegisterButtonCallbacks(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction, in System.Action InOnPress, in System.Action<float> InOnHold, in System.Action InOnRelease)
	{
		if (Utils.CustomAssertions.IsNotNull(InInputAction))
		{
			GetOrCreateInputActionList(InMonoBehaviour).Add(new ActionLogicButton(InInputAction, InOnPress, InOnHold, InOnRelease));
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void RegisterAxis2DCallback(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction, in System.Action<float, Vector2> InOnChange, in bool InTryReadRaw)
	{
		if (Utils.CustomAssertions.IsNotNull(InInputAction))
		{
			GetOrCreateInputActionList(InMonoBehaviour).Add(new ActionLogicAxis(InInputAction, InOnChange, InTryReadRaw));
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void UnRegisterCallbacks(in MonoBehaviour InMonoBehaviour, in InputAction InInputAction)
	{
		if (Utils.CustomAssertions.IsNotNull(InInputAction) && s_RegisteredActions.TryGetValue(InMonoBehaviour, out List<ActionLogicBase> InputActionList))
		{
			for (int index = InputActionList.Count - 1; index >= 0; index--)
			{
				ActionLogicBase logic = InputActionList[index];
				if (logic.InputAction.id == InInputAction.id)
				{
					logic.Destroy();
					InputActionList.RemoveAt(index);
				}
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	/// <summary>  </summary>
	public static void UnRegisterAllCallbacks(in MonoBehaviour InMonoBehaviour)
	{
		if (s_RegisteredActions.TryGetValue(InMonoBehaviour, out List<ActionLogicBase> InputActionList))
		{
			for (int index = InputActionList.Count - 1; index >= 0; index--)
			{
				InputActionList[index].Destroy();
				InputActionList.RemoveAt(index);
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
				actionLogic.Update(Time.deltaTime);
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
		private readonly InputAction m_InputAction = null;

		public InputAction InputAction => m_InputAction;

		protected ActionLogicBase(in InputAction InInputAction)
		{
			if (Utils.CustomAssertions.IsNotNull(InInputAction))
			{
				m_InputAction = InInputAction;
			}
		}

		//////////////////////////////////////////////////////////////////////////
		public void Update(float deltaTime)
		{
			OnUpdate(deltaTime);
		}

		//////////////////////////////////////////////////////////////////////////
		public void Destroy()
		{
			m_InputAction.started -= OnActionStarted;
			m_InputAction.canceled -= OnActioCanceled;
			OnDestroy();
		}

		protected abstract void OnUpdate(float deltaTime);

		//////////////////////////////////////////////////////////////////////////
		protected virtual void OnDestroy()
		{

		}

		//////////////////////////////////////////////////////////////////////////
		protected void RegisterEvents()
		{
			// Old UnityEngine.input.GetButtonDown
			m_InputAction.started -= OnActionStarted;
			m_InputAction.started += OnActionStarted;

			// Old UnityEngine.input.GetButtonUp
			m_InputAction.canceled -= OnActioCanceled;
			m_InputAction.canceled += OnActioCanceled;
		}

		//////////////////////////////////////////////////////////////////////////
		protected abstract void OnActionStarted(InputAction.CallbackContext ctx);
		protected abstract void OnActioCanceled(InputAction.CallbackContext ctx);
	}

	/////////////////////////////////////////////////////////////////////////////
	private class ActionLogicButton : ActionLogicBase
	{
		private readonly System.Action m_OnPress = delegate { };
		private readonly System.Action<float> m_OnHold = delegate { };
		private readonly System.Action m_OnRelease = delegate { };

		//////////////////////////////////////////////////////////////////////////
		public ActionLogicButton(in InputAction InInputAction, in System.Action InOnPress, in System.Action<float> InOnHold, in System.Action InOnRelease) : base(InInputAction)
		{
			if (Utils.CustomAssertions.IsTrue(InOnPress.IsNotNull() || InOnHold.IsNotNull() || InOnRelease.IsNotNull()))
			{
				m_OnPress = InOnPress ?? m_OnPress;
				m_OnHold = InOnHold ?? m_OnHold;
				m_OnRelease = InOnRelease ?? m_OnRelease;

				RegisterEvents();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnUpdate(float deltaTime)
		{
			// Old UnityEngine.Input.GetButton		
			if (InputAction.IsPressed())
			{
				m_OnHold(deltaTime);
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnActionStarted(InputAction.CallbackContext ctx) => m_OnPress();

		//////////////////////////////////////////////////////////////////////////
		protected override void OnActioCanceled(InputAction.CallbackContext ctx) => m_OnRelease();
	}


	/////////////////////////////////////////////////////////////////////////////
	private class ActionLogicAxis : ActionLogicBase
	{
		private readonly System.Action<float, Vector2> m_OnChange = delegate { };
		private readonly bool m_TryReadRaw = false;
		private bool m_CanReadValue = false;
		private InputControl<Vector2> m_InputControl = null;
		private bool m_CanReadRaw = false;


		//////////////////////////////////////////////////////////////////////////
		public ActionLogicAxis(in InputAction InInputAction, in System.Action<float, Vector2> InOnChange, in bool InTryReadRaw) : base(InInputAction)
		{
			if (Utils.CustomAssertions.IsNotNull(InOnChange))
			{
				m_OnChange = InOnChange;
				m_TryReadRaw = InTryReadRaw;

				RegisterEvents();
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnUpdate(float deltaTime)
		{
			if (m_CanReadValue)
			{
				m_OnChange(deltaTime, m_TryReadRaw && m_CanReadRaw ? m_InputControl.ReadUnprocessedValue() : InputAction.ReadValue<Vector2>());
			}
		}

		//////////////////////////////////////////////////////////////////////////
		protected override void OnActionStarted(InputAction.CallbackContext ctx)
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
		protected override void OnActioCanceled(InputAction.CallbackContext ctx)
		{
			m_CanReadValue = false;
		}
	}
}
