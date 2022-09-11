
using UnityEngine;

[Configurable(nameof(m_Configs), "Cameras/" + nameof(FPSCamera))]
public class FPSCamera : GameCameraBase
{
	[SerializeField]
	private Transform m_Head = null;

	[SerializeField]
	private Transform m_Body = null;

	[SerializeField, ReadOnly]
	private Configuration_FPS m_Configs = null;


	// -------------
	private Vector2 m_CurrentLookInputVector = Vector2.zero;
	private float m_CurrentCameraAngle = 0f;


	//////////////////////////////////////////////////////////////////////////
	protected sealed override void Awake()
	{
		base.Awake();

		enabled = Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs), this);

		m_Head = transform;
		
		if (Utils.CustomAssertions.IsTrue(transform.parent.IsNotNull(), this))
		{
			m_Body = transform.parent;
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnEnable()
	{
		if (Utils.CustomAssertions.IsNotNull(m_Configs, this))
		{
			InputHandler.RegisterAxis2DCallback(this, m_Configs.LookAction, OnLookActionUpdate, InTryReadRaw: false);
		}
	//	Cursor.lockState = CursorLockMode.Locked;
	//	Cursor.visible = false;
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnDisable()
	{
	//	Cursor.lockState = CursorLockMode.None;
	//	Cursor.visible = true;

		if (m_Configs.IsNotNull())
		{
			InputHandler.UnRegisterCallbacks(this, m_Configs.LookAction);
		}
	}


	//////////////////////////////////////////////////////////////////////////
	private void OnLookActionUpdate(Vector2 input) => m_CurrentLookInputVector.Set(input.x, input.y);


	//////////////////////////////////////////////////////////////////////////
	private void LateUpdate()
	{
		if (m_CurrentLookInputVector.sqrMagnitude > 0f)
		{
			// Horizontal rotation
			if (m_Body.IsNotNull())
			{
			//	TODO: right += IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				m_Body.Rotate(Vector3.up, Mathf.MoveTowards(m_CurrentLookInputVector.x * m_Configs.LookSensitivity, 0f, Time.deltaTime), Space.Self);
			}

			// Vertical clamped rotation (Head)
			if (m_Head.IsNotNull())
			{
				float verticalRotation = m_CurrentLookInputVector.y * m_Configs.LookSensitivity;
				if (Utils.Math.ClampResult(ref m_CurrentCameraAngle, m_CurrentCameraAngle - verticalRotation, m_Configs.DownCameraRotationBound, m_Configs.UpCameraRotationBound))
				{
					m_Head.Rotate(Vector3.right, -verticalRotation, Space.Self);
				}
			}
		}

		// Always consume input
		m_CurrentLookInputVector.Set(0f, 0f);
	}
}