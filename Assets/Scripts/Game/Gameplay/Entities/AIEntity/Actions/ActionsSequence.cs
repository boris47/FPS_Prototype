using System.Linq;
using System.Collections.Generic;
using UnityEngine;


namespace Entities.AI.Actions
{
	[System.Serializable]
	public class ActionsSequence : ActionComposite
	{
		//////////////////////////////////////////////////////////////////////////
		protected sealed override EActionState OnActivation() => base.OnActivation();

		//////////////////////////////////////////////////////////////////////////
		protected override EActionState OnUpdate()
		{
			EActionState OutState = EActionState.RUNNING;
			switch (m_Actions.At(m_CurrentIndex).Update())
			{
				case EActionState.INACTIVE:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
				case EActionState.FAILED:
				{
					if (m_Actions.IsValidIndex(m_CurrentIndex + 1))
					{
						++m_CurrentIndex;
					}
					else
					{
						if (m_MustRepeat)
						{
							ResetAction();
						}
						else
						{
							OutState = EActionState.FAILED;
						}
					}
					break;
				}
				case EActionState.COMPLETED:
				{
					OutState = EActionState.COMPLETED;
					break;
				}
				case EActionState.RUNNING: break;
				default:
				{
					Utils.CustomAssertions.IsTrue(false);
					break;
				}
			}
			return OutState;
		}
	}
}
