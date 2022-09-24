using UnityEngine;
using UnityEngine.InputSystem;
using TypeReferences;

namespace Entities.Player
{
	public class PlayerConfiguration : ConfigurationBase
	{
		public	const		float					k_CharacterHeight				= 2f;
		public	const		float					k_CharacterRadius				= 0.5f;
		public	const		float					k_HeadHeight					= 0.565f;
		public	const		float					k_MaxSlopeLimitAngle			= 90f;

		[SerializeField, Inherits(typeof(Components.PlayerMotionStrategyBase), AllowAbstract = false, ShowNoneElement = false)]
		private				TypeReference			m_DefaultMotionStrategyType		= typeof(Components.PlayerMotionStrategyGrounded);

		[Header("Character controller params")]
		[SerializeField][Range(1f, k_MaxSlopeLimitAngle)]
		private				float					m_SlopeLimit					= 45f;

		[Space]
		[Header("Actions")]
		[SerializeField]
		private				InputActionReference	m_UseAction						= null;

		[SerializeField, Min(0.1f)]
		private				float					m_UseDistance					= 1f;


		public				System.Type				DefaultMotionStrategyType		=> m_DefaultMotionStrategyType;

		public				float					SlopeLimit						=> m_SlopeLimit;
		public				InputActionReference	UseAction						=> m_UseAction;
		public				float					UseDistance						=> m_UseDistance;
		public				float					UseDistanceSqr					=> m_UseDistance * m_UseDistance;



		public				float					HeadHeight						=> k_HeadHeight;
		public				float					CharacterHeight					=> k_CharacterHeight;
		public				float					CharacterRadius					=> k_CharacterRadius;
		public				float					MaxSlopeLimitAngle				=> k_MaxSlopeLimitAngle;
	}
}
