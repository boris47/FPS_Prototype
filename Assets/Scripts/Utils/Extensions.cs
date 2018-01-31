
using UnityEngine;

public static class Extensions {

	public static T GetComponent<T>( Transform Base ) where T : Component
	{
		if ( Base == null )
			return null;

		return Base.GetComponent<T>();
	}

}
