
using UnityEngine;

public abstract partial class LiveEntity  {

	public		void	EvaluateFall( Vector3 fallDistance )
	{
		if ( IsFalling == false )
			return;

		float sqrMagnitude = fallDistance.sqrMagnitude;
//		print( "Fall evaluation, magnitude " + sqrMagnitude );
		if ( sqrMagnitude > m_FallDistanceThreshold * m_FallDistanceThreshold )
		{
			float rapport = ( 1f - ( ( m_FallDistanceThreshold * m_FallDistanceThreshold ) / sqrMagnitude ) );

			HurtInfo info = new HurtInfo()
			{
//				Damage = ( magnitude + ( magnitude * rapport ) ) * 3f
				Damage = rapport * sqrMagnitude
			};

//			print( "Extimated damage " + info.Damage );

			this.OnHurt( info );
		}

	}


}