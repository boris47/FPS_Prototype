
using UnityEngine;


public class WalkerArmored : Walker {
	
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();
	}

}
