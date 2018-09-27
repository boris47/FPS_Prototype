
using UnityEngine;


public class WalkerStandard : Walker {

	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		m_SectionName = this.GetType().FullName;

		base.Awake();
	}

}
