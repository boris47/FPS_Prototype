public class WalkerStandard : Walker {

	//////////////////////////////////////////////////////////////////////////
	protected	override	void	Awake()
	{
		m_SectionName = GetType().FullName;

		base.Awake();
	}

}
