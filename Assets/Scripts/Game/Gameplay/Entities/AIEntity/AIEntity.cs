using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Entities.AI
{
	[RequireComponent(typeof(AIController))]
	public class AIEntity : Entity
	{
		public new AIController Controller => m_Controller as AIController;


		//////////////////////////////////////////////////////////////////
		public override bool IsInterestedAt(in Entity source)
		{
			return Controller.BrainComponent.Targets.GetEnemyFactions().Contains(source.Faction);
		//	throw new System.NotImplementedException();
		//	return false;
		}


		//////////////////////////////////////////////////////////////////
		public bool IsInterestedAt(in Entity source, in ESoundType soundType)
		{
			return Controller.BrainComponent.Targets.GetEnemyFactions().Contains(source.Faction);
		//	throw new System.NotImplementedException();
		//	return false;
		}
	}
}

