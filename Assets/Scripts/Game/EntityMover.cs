using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class EntityMover : MonoBehaviour
{
	[Header("Configs")]
	[Tooltip("Move speed in meters/second")]
	[SerializeField][Range(0.001f, 5f)]
	private float moveSpeed = 1f;

	[Tooltip("Sprint multiplier to apply at movement")]
	[SerializeField][Range(1.001f, 5f)]
	private float sprintMult = 1f;

	[SerializeField][Range(0.1f, 20f)]
	private float lookSensitivity = 7f;

	[Tooltip("The height the player can jump")]
	[SerializeField]
	private float jumpHeight = 1.2f;

	[Space]
	[Header("Actions")]
	[SerializeField]
	private InputActionReference m_MoveAction;
	[SerializeField]
	private InputActionReference m_SprintAction;
	[SerializeField]
	private InputActionReference m_LookAction;
	[SerializeField]
	private InputActionReference m_JumpAction;

	[SerializeField]
	private Transform m_Head = null;
	[SerializeField]
	private Rigidbody m_Rigidbody = null;
	[SerializeField]
	private CapsuleCollider m_CapsuleCollider = null;
	[SerializeField]
	private EntityFoots m_EntityFoots = null;
	[SerializeField, ReadOnly]
	private bool m_IsGrounded = false;

	private Vector2 m_CurrentMoveInputVector = Vector2.zero;
	private Vector2 m_CurrentLookInputVector = Vector2.zero;
	private bool m_JumpRequested = false;
	private float m_CurrentMovementMultiplier = 1f;
	private float m_CurrentCameraAngle = 0f;



	private void Awake()
	{
		UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
		m_Rigidbody = m_Rigidbody.IsNotNull() ? m_Rigidbody : GetComponent<Rigidbody>();

		m_CapsuleCollider = m_CapsuleCollider.IsNotNull() ? m_CapsuleCollider : GetComponent<CapsuleCollider>();

		if (m_Head == null)
		{
			Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Head", out m_Head));
		}

		if (m_EntityFoots.IsNotNull() || Utils.CustomAssertions.IsTrue(transform.TrySearchComponentByChildName("Foots", out m_EntityFoots)))
		{
			m_EntityFoots.OnEvent_GroundedChanged += OnGroundedChanged;
		}

	}

	private void OnDestroy()
	{
		if (m_EntityFoots.IsNotNull())
		{
			m_EntityFoots.OnEvent_GroundedChanged -= OnGroundedChanged;
		}

	}

	private void OnEnable()
	{
		System.Threading.Tasks.Task.Delay(500).GetAwaiter().OnCompleted(() =>
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			InputHandler.RegisterAxis2DCallback(this, m_MoveAction, OnMoveActionUpdate, InTryReadRaw: false);
			InputHandler.RegisterAxis2DCallback(this, m_LookAction, OnLookActionUpdate, InTryReadRaw: false);
			InputHandler.RegisterButtonCallbacks(this, m_JumpAction, OnJumpRequest, null, null);
			InputHandler.RegisterButtonCallbacks(this, m_SprintAction, OnSprintStart, null, OnSprintEnd);
		});
	}

	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		InputHandler.UnRegisterCallbacks(this, m_MoveAction);
		InputHandler.UnRegisterCallbacks(this, m_LookAction);
		InputHandler.UnRegisterCallbacks(this, m_JumpAction);
		InputHandler.UnRegisterCallbacks(this, m_SprintAction);
	}


	private void OnMoveActionUpdate(Vector2 input) => m_CurrentMoveInputVector.Set(input.x, input.y);
	private void OnJumpRequest() => m_JumpRequested = true;
	private void OnSprintStart() => m_CurrentMovementMultiplier = sprintMult;
	private void OnSprintEnd() => m_CurrentMovementMultiplier = 1f;
	private void OnGroundedChanged(bool newState) => m_IsGrounded = newState;
	private void OnLookActionUpdate(Vector2 input) => m_CurrentLookInputVector.Set(input.x, input.y);

	private void FixedUpdate()
	{
		if (m_IsGrounded)
		{
			if (m_CurrentMoveInputVector.sqrMagnitude > 0f)
			{
				m_CurrentMoveInputVector.Normalize();

				Vector3 localVelocity = m_Rigidbody.transform.InverseTransformDirection(m_Rigidbody.velocity);
				{
					// Forward
					localVelocity.z = m_CurrentMoveInputVector.y * moveSpeed * m_CurrentMovementMultiplier;
					// Right
					localVelocity.x = m_CurrentMoveInputVector.x * moveSpeed * m_CurrentMovementMultiplier;

					// Consume input
					m_CurrentMoveInputVector.Set(0f, 0f);
				}
				m_Rigidbody.velocity = m_Rigidbody.transform.TransformDirection(localVelocity);
				
			}

			// Jump
			if (m_JumpRequested)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				m_Rigidbody.AddForce(Vector3.up * (Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y) - m_Rigidbody.velocity.y), ForceMode.VelocityChange);
			}
		}
		else // airborne
		{

		}
		// Always consume input
		m_JumpRequested = false;
	}

	// Restore This in a better way
	//private bool IsCurrentDeviceMouse => PlayerInput.all[0].currentControlScheme == "KeyboardMouse";
	

	private void LateUpdate()
	{
		(float right, float up) = m_CurrentLookInputVector;

	//	if (m_CurrentLookInputVector.sqrMagnitude > 0f)
		{
			// Horizontal rotation
			{
				right += Random.Range(-Mathf.Epsilon, Mathf.Epsilon);
			//	right += IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				transform.Rotate(Vector3.up, Mathf.MoveTowards(right * lookSensitivity, 0f, Time.deltaTime), Space.Self);
			}

			// Vertical clamped rotation (Head)
			{
				float verticalRotation = up * lookSensitivity;
				if (Utils.Math.ClampResult(ref m_CurrentCameraAngle, m_CurrentCameraAngle - verticalRotation, -75, 75f))
				{
					m_Head.Rotate(Vector3.right, -verticalRotation, Space.Self);
				}
			}

		}
		m_CurrentLookInputVector.Set(0f, 0f);
	}

	// Ref: https://www.immersivelimit.com/tutorials/simple-character-controller-for-unity
	//	private void UpdateGroundedState()
	//	{
	//		m_IsGrounded = false;
	//		float capsuleHeight = Mathf.Max(m_CapsuleCollider.radius * 2f, m_CapsuleCollider.height);
	//		Vector3 capsuleBottom = transform.TransformPoint(m_CapsuleCollider.center - (0.5f * capsuleHeight * Vector3.up));
	//		float radius = transform.TransformVector(m_CapsuleCollider.radius, 0f, 0f).magnitude;
	//		Ray ray = new Ray(capsuleBottom + (transform.up * .01f), -transform.up);
	//		if (Physics.Raycast(ray, out RaycastHit hit, radius * 5f))
	//		{
	//			float normalAngle = Vector3.Angle(hit.normal, transform.up);
	//			if (normalAngle < slopeLimit)
	//			{
	//				float maxDist = (radius / Mathf.Cos(Mathf.Deg2Rad * normalAngle)) - radius + .02f;
	//				if (hit.distance < maxDist)
	//				{
	//					m_IsGrounded = true;
	//				}
	//			}
	//		}
	//	}
}
