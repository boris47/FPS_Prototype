
//////////////////////////////////////////////////////////////////////////
// WPN_FireMode_Single
using UnityEngine;

public class WPN_FireMode_Single : WPN_BaseFireMode
{
	public	override	EFireMode FireMode		=> EFireMode.SINGLE;
	

	protected	override	void	InternalSetup	(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction)
	{
		
	}
	

	public	override	void	ApplyModifier	( Database.Section modifier )
	{
		m_Modifiers.Add( modifier );
	}

	public	override	void	ResetBaseConfiguration()
	{

	}

	public	override	void	RemoveModifier( Database.Section modifier )
	{
		m_Modifiers.Remove( modifier );
	}


	public	override	bool	OnSave			( StreamUnit streamUnit )
	{
		return true;
	}


	public	override	bool	OnLoad			( StreamUnit streamUnit )
	{
		m_CurrentDelay = 0.0f;
		return true;
	}

	public	override	void	OnWeaponChange	()
	{
		m_CurrentDelay = 0.0f;
	}
	

	//	INTERNAL UPDATE
	public	override	void	InternalUpdate( float DeltaTime, uint magazineSize )
	{
		m_CurrentDelay = Mathf.Max( m_CurrentDelay - DeltaTime, 0.0f );
	}

	//	START
	public override		void	OnStart( float baseFireDispersion, float baseCamDeviation )
	{
		if ( m_CurrentDelay <= 0.0f )
		{
			m_FireFunction( baseFireDispersion * m_DispersionMultiplier, baseCamDeviation * m_DeviationMultiplier );
			m_CurrentDelay = m_FireDelay;
		}
	}

	//	INTERNAL UPDATE
	public	override	void	OnUpdate( float baseFireDispersion, float baseCamDeviation )
	{
		
	}

	//	END
	public override		void	OnEnd( float baseFireDispersion, float baseCamDeviation )
	{
		
	}
}
