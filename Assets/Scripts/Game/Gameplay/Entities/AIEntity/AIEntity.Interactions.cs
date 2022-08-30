
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.AI
{
	using Components;

	[RequireComponent(typeof(NavMeshAgent))]
	[RequireComponent(typeof(AIController))]
	[RequireComponent(typeof(AIMotionManager))]
	public partial class AIEntity : Entity
	{
	//	//////////////////////////////////////////////////////////////////////////
	//	public override bool GrabObject(GrabInteractable grabbable)
	//	{
	//		return true;
	//	}
	//
	//	//////////////////////////////////////////////////////////////////////////
	//	public override bool DropGrabbedObject()
	//	{
	//		return false;
	//	}
	}
}
