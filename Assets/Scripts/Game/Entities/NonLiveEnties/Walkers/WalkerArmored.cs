
using UnityEngine;


public class WalkerArmored : Walker {
	
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
