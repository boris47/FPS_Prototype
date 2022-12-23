using System.Collections.Generic;
using UnityEngine;
using Entities;
using System;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class SwimVolume : MotionVolume
{
	[SerializeField, ReadOnly]
	private				Rigidbody						m_Rigidbody							= null;

	[SerializeField, ReadOnly]
	private				BoxCollider						m_Collider							= null;

	[SerializeField]
	private				float							m_Density							= 1.027f;
	/*
	[SerializeField]
	private				float							m_DeepestDrag						= 1f;

	[SerializeField]
	private				float							m_DeepestAngularDrag				= 1f;
	*/
	[SerializeField]


	//--------------------
	private				List<BodyData>					m_CurrentBodies						= new List<BodyData>();


	//////////////////////////////////////////////////////////////////////////
	public override bool IsPointInside(in Vector3 InPoint) => m_Collider.Contains(InPoint);


	//////////////////////////////////////////////////////////////////////////
	// Awake is called when the script instance is being loaded
	private void Awake()
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
			BodyData bodyData = m_CurrentBodies[i];
			if (bodyData.FloatingObject.IsNotNull())
			{
				bodyData.Update(Time.fixedDeltaTime);
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
		if (!other.isTrigger && other.attachedRigidbody.IsNotNull() && other.TryGetComponent(out FloatingObject OutFloatingObject))
		{
			if (!m_CurrentBodies.TryFind(out BodyData _, out int _, bd => bd.FloatingObject == OutFloatingObject))
			{
				m_CurrentBodies.Add(new BodyData(this, OutFloatingObject));

				if (other.gameObject.TryGetComponent(out IMotionManager motionManager))
				{
					motionManager.OnMotionVolumeEnter(this);
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////
	// OnTriggerExit is called when the Collider other has stopped touching the trigger
	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger && other.attachedRigidbody.IsNotNull() && other.TryGetComponent(out FloatingObject OutFloatingObject))
		{
			if (m_CurrentBodies.TryFind(out BodyData bodyData, out int index, bd => bd.FloatingObject == OutFloatingObject))
			{
				bodyData.Restore();
				m_CurrentBodies.RemoveAt(index);

				if (other.gameObject.TryGetComponent(out IMotionManager motionManager))
				{
					motionManager.OnMotionVolumeExit(this);
				}
			}
		}

	}

	//////////////////////////////////////////////////////////////////////////
	// Implement this OnDrawGizmos if you want to draw gizmos that are also pickable and always drawn
	private void OnDrawGizmos()
	{
		Color semiTransparent = new Color(0.3f, 0.3f, 0.3f, 0.3f);

		using (new Utils.Editor.GizmosHelper.UseGizmoColor(semiTransparent))
		{
			Matrix4x4 transformMatrix = Matrix4x4.TRS(transform.TransformPoint(m_Collider.center), transform.rotation, transform.lossyScale);
			using (new Utils.Editor.GizmosHelper.UseGizmoMatrix(transformMatrix))
			{
				Gizmos.DrawCube(Vector3.zero, m_Collider.size);
			}
		}
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




	//////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////
	private class BodyData
	{
		public readonly FloatingObject FloatingObject = null;
		private readonly SwimVolume m_SwimVolume = null;
		private readonly Rigidbody m_Rigidbody = null;

		private readonly float m_OriginalDrag = 0f;
		private readonly float m_OriginalAngularDrag = 0f;
		
		private readonly float m_DragInWater = 0f;
		private readonly float m_AngularDragInWater = 0f;

		private float m_CurrentDrag = 0f;
		private float m_CurrentAngularDrag = 0f;

		/**
		 * The idea is to recognize the areo of the object hittin the pluind and manage forces base on that area
		 * Maybe transform linear velocity into torque velocity
		*/

		//////////////////////////////////////////////////////////////////////////
		public BodyData(in SwimVolume InVolume, in FloatingObject InFloatingObject)
		{
			FloatingObject = InFloatingObject;
			m_SwimVolume = InVolume;
			m_Rigidbody = InFloatingObject.Rigidbody;

			m_OriginalDrag = m_Rigidbody.drag;
			m_OriginalAngularDrag = m_Rigidbody.angularDrag;

			Bounds objectBounds = Utils.Math.GetBoundsOf(FloatingObject);
			float objectVolume = objectBounds.size.magnitude;
			float impactArea = Mathf.Max(objectBounds.size.x, objectBounds.size.z);

		//	float objectVolume = m_Rigidbody.mass / FloatingObject.Density;

		//	Debug.Log(impactArea);

			m_Rigidbody.velocity /= Mathf.Max(impactArea, 1f);

			m_DragInWater = m_OriginalDrag + MathF.Pow(m_SwimVolume.m_Density, objectVolume);
			m_AngularDragInWater = m_OriginalAngularDrag + MathF.Pow(m_SwimVolume.m_Density, objectVolume);

			m_CurrentDrag = MathF.Pow(m_SwimVolume.m_Density, objectVolume);
			m_CurrentAngularDrag = 0.05f;
		}

		//////////////////////////////////////////////////////////////////////////
		public void Update(in float InDeltaTime)
		{
			Voxel[] voxels = FloatingObject.Voxels;
			int voxelsCount = voxels.Length;

			// No voxel? Create one for the entire object
			if (voxelsCount == 0)
			{
				voxels = new Voxel[1]
				{
					new Voxel(FloatingObject, Vector3.zero, Utils.Math.GetBoundsOf(FloatingObject).size)
				};
				voxelsCount = 1;
			}
			
			// TODO: For weaving fluids ti coordinates may change. Handle it!
			Vector3 waterUpperPoint = m_SwimVolume.m_Collider.bounds.center + (Vector3.up * m_SwimVolume.m_Collider.bounds.extents.y);

			float objectVolume = m_Rigidbody.mass / FloatingObject.Density;
			float maxBuoyancyForce = m_SwimVolume.m_Density * objectVolume * -Physics.gravity.y;
			float forceAtSingleVoxel = maxBuoyancyForce / voxelsCount;

			float submergedVolume = 0f;

			for (int i = 0; i < voxelsCount; i++)
			{
				Voxel voxel = voxels[i];
				float submergedFactor = Utils.Math.ScaleBetween(waterUpperPoint.y, voxel.Position.y - voxel.Size.y, voxel.Position.y + voxel.Size.y, 0f, 1f);
				submergedVolume += submergedFactor;
				
				Vector3 finalVoxelForce = forceAtSingleVoxel * submergedFactor * m_SwimVolume.transform.up;
				m_Rigidbody.AddForceAtPosition(finalVoxelForce, voxel.Position);

			//	Debug.DrawLine(voxel.Position, voxel.Position + finalVoxelForce, Color.blue);
			}

			submergedVolume /= voxelsCount; // 0 - object is fully out of the water, 1 - object is fully submerged

			float newDrag = Mathf.Lerp(m_OriginalDrag, m_DragInWater, submergedVolume);
			float newAngularDrag = Mathf.Lerp(m_OriginalAngularDrag, m_AngularDragInWater, submergedVolume);

			m_Rigidbody.drag = m_CurrentDrag = Mathf.MoveTowards(m_CurrentDrag, newDrag, InDeltaTime);
			m_Rigidbody.angularDrag = m_CurrentAngularDrag = Mathf.MoveTowards(m_CurrentAngularDrag, newAngularDrag, InDeltaTime);

		//	m_Rigidbody.drag = Mathf.Lerp(0f, m_DragInWater, submergedVolume);
		//	m_Rigidbody.angularDrag = Mathf.Lerp(0.05f, m_AngularDragInWater, submergedVolume);
			m_Rigidbody.angularVelocity = Vector3.MoveTowards(m_Rigidbody.angularVelocity, Vector3.zero, InDeltaTime * FloatingObject.AngularTimeDrag);
		}

		//////////////////////////////////////////////////////////////////////////
		public void Restore()
		{
			m_Rigidbody.drag = m_OriginalDrag;
			m_Rigidbody.angularDrag = m_OriginalAngularDrag;
		}
	}
}
