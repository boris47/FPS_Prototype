using System.Collections.Generic;
using UnityEngine;
using Entities;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
[Configurable(nameof(m_Configs), "Volumes/" + nameof(SwimVolume))]
public class SwimVolume : MonoBehaviour
{
//	private				PlayerConfigurationSwim			m_Configs							= null;

	[SerializeField, ReadOnly]
	private				Rigidbody						m_Rigidbody							= null;

	[SerializeField, ReadOnly]
	private				BoxCollider						m_Collider							= null;

	[SerializeField, ReadOnly]
	private				SwimVolumeConfigs				m_Configs							= null;

	//--------------------
	private				List<Rigidbody>					m_CurrentBodies						= new List<Rigidbody>();

	public				SwimVolumeConfigs				Configs								=> m_Configs;


	//////////////////////////////////////////////////////////////////////////
	// Awake is called when the script instance is being loaded
	private void Awake()
	{
		if (Utils.CustomAssertions.IsTrue(this.TryGetConfiguration(out m_Configs)))
		{
			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Rigidbody)))
			{
				ConfigureRigidbody(m_Rigidbody);
			}

			if (enabled &= Utils.CustomAssertions.IsTrue(gameObject.TryGetComponent(out m_Collider)))
			{
				ConfigureCollider(m_Collider);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only)
	private void OnValidate()
	{
		if (gameObject.TryGetComponent(out m_Rigidbody))
		{
			ConfigureRigidbody(m_Rigidbody);
		}
		
		if (gameObject.TryGetComponent(out m_Collider))
		{
			ConfigureCollider(m_Collider);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// This function is called every fixed framerate frame, if the MonoBehaviour is enabled
	private void FixedUpdate()
	{
		for (int i = m_CurrentBodies.Count - 1; i >= 0; --i)
		{
			Rigidbody rigidbody = m_CurrentBodies[i];
			if (rigidbody.IsNotNull())
			{
				if (m_Collider.Contains(rigidbody.worldCenterOfMass))
				{
					rigidbody.AddForce(m_Configs.FloatingForce * transform.up, ForceMode.Force);
				}
			}
			else
			{
				m_CurrentBodies.RemoveAt(i);
			}
		}
	}


	//////////////////////////////////////////////////////////////////////////
	// OnTriggerEnter is called when the Collider other enters the trigger
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.TryGetComponent(out MotionManager motionManager))
		{
			motionManager.OnSwimVolumeEnter(this);
		}

		if (!other.isTrigger && other.attachedRigidbody.IsNotNull())
		{
			m_CurrentBodies.Add(other.attachedRigidbody);
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit is called when the Collider other has stopped touching the trigger
	private void OnTriggerExit(Collider other)
	{
		if (other.attachedRigidbody.IsNotNull())
		{
			m_CurrentBodies.Remove(other.attachedRigidbody);
		}

		if (other.gameObject.TryGetComponent(out MotionManager motionManager))
		{
			motionManager.OnSwimVolumeExit(this);
		}
	}

	private static Color semiTransparent = new Color(0.3f, 0.3f, 0.3f, 0.3f);
	//////////////////////////////////////////////////////////////////////////
	// Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
	private void OnDrawGizmos()
	{
		Color prevColor = Gizmos.color;
		Matrix4x4 prevMatrix = Gizmos.matrix;
		{
			Gizmos.matrix = Matrix4x4.TRS(this.transform.TransformPoint(m_Collider.center), this.transform.rotation, this.transform.lossyScale);
			Gizmos.color = semiTransparent;
			Gizmos.DrawCube(Vector3.zero, m_Collider.size);
		}
		Gizmos.color = prevColor;
		Gizmos.matrix = prevMatrix;
	}

	//////////////////////////////////////////////////////////////////////////
	private static void ConfigureRigidbody(in Rigidbody InRigidbody)
	{
		InRigidbody.useGravity = false;
		InRigidbody.constraints = RigidbodyConstraints.FreezeAll;
		InRigidbody.mass = float.Epsilon;
	}

	//////////////////////////////////////////////////////////////////////////
	private static void ConfigureCollider(in Collider InCollider)
	{
		InCollider.isTrigger = true;
	}
}
