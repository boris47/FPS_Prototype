
using UnityEngine;


public class WalkerArmoredGatling : Walker {
	
	//////////////////////////////////////////////////////////////////////////
	protected override void Awake()
	{
		this.m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
