using UnityEngine;


namespace Entities.Player
{
	public class PlayerConfiguration : ConfigurationBase
	{
		public	const		float					k_CharacterHeight				= 2f;
		public	const		float					k_CharacterRadius				= 0.5f;
		public	const		float					k_HeadHeight					= 0.565f;
		public	const		float					k_MaxSlopeLimitAngle			= 90f;

		[Header("Character controller params")]
		[SerializeField][Range(1f, k_MaxSlopeLimitAngle)]
		private				float					m_SlopeLimit					= 45f;




		public				float					SlopeLimit						=> m_SlopeLimit;




		public				float					HeadHeight						=> k_HeadHeight;
		public				float					CharacterHeight					=> k_CharacterHeight;
		public				float					CharacterRadius					=> k_CharacterRadius;
		public				float					MaxSlopeLimitAngle				=> k_MaxSlopeLimitAngle;
	}
}
