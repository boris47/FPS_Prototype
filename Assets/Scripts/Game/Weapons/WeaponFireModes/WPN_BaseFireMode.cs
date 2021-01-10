
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public	abstract	class	WPN_BaseFireMode : MonoBehaviour, IModifiable
{	
	public delegate	void FireFunctionDel( float baseFireDispersion, float baseCamDeviation );

	public	abstract	EFireMode					FireMode					{ get; }

	protected			WPN_FireModule				m_FireModule				= null;

	protected			List<Database.Section>		m_Modifiers					= new List<Database.Section>();
	[SerializeField]
	protected			float						m_FireDelay					= 1.0f;
	[SerializeField]
	protected			float						m_CurrentDelay				= 0.0f;

	[SerializeField]
	protected			float						m_DispersionMultiplier		= 1.0f;
	[SerializeField]
	protected			float						m_DeviationMultiplier		= 1.0f;


	protected			FireFunctionDel				m_FireFunction				= delegate { };

	private void Awake()
	{
		m_DispersionMultiplier = m_DeviationMultiplier = 1.0f;
	}

	public				void	Setup				( in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction )
	{
		if ( fireFunction != null )
		{
			m_FireDelay = shotDelay;
			m_FireFunction = fireFunction;
			m_FireModule = fireModule;
		}

		string moduleSectionName = GetType().Name;
		Database.Section fireModeSection = null;
		if ( GlobalManager.Configs.GetSection( moduleSectionName, ref fireModeSection ) )
		{
			InternalSetup( fireModeSection, fireModule, shotDelay, fireFunction );

			m_DispersionMultiplier = fireModeSection.AsFloat( "DispersionMultiplier", m_DispersionMultiplier );
			m_DeviationMultiplier  = fireModeSection.AsFloat( "DeviationMultiplier", m_DeviationMultiplier );
		}
	}

	protected abstract	void	InternalSetup		(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction);

	public	abstract	void	ApplyModifier		( Database.Section modifier );
	public	abstract	void	ResetBaseConfiguration	();
	public	abstract	void	RemoveModifier		( Database.Section modifier );

	public	abstract	bool	OnSave				( StreamUnit streamUnit );
	public	abstract	bool	OnLoad				( StreamUnit streamUnit );

	public	abstract	void	OnWeaponChange		();

	public	abstract	void	InternalUpdate		( float DeltaTime, uint magazineSize );

	//
	public	abstract	void	OnStart				( float baseFireDispersion, float baseCamDeviation );
	public	abstract	void	OnUpdate			( float baseFireDispersion, float baseCamDeviation );
	public	abstract	void	OnEnd				( float baseFireDispersion, float baseCamDeviation );

}

public	class WPN_FireMode_Empty : WPN_BaseFireMode
{
	public override		EFireMode	FireMode		=> EFireMode.NONE;

	protected	override	void	InternalSetup	(in Database.Section fireModeSection, in WPN_FireModule fireModule, in float shotDelay, in FireFunctionDel fireFunction)
	{ }

	public		override	void	ApplyModifier	( Database.Section modifier )
	{ }

	public		override	void	ResetBaseConfiguration()
	{ }

	public		override	void	RemoveModifier	( Database.Section modifier )
	{ }

	public		override	bool	OnSave			( StreamUnit streamUnit ) => true;


	public		override	bool	OnLoad			( StreamUnit streamUnit )  => true;


	public		override	void	OnWeaponChange	()
	{ }

	public		override	void	InternalUpdate	( float DeltaTime, uint magazineSize )
	{ }

	public		override	void	OnStart			( float baseFireDispersion, float baseCamDeviation )
	{ }

	public		override	void	OnUpdate		( float baseFireDispersion, float baseCamDeviation )
	{ }

	public		override	void	OnEnd			( float baseFireDispersion, float baseCamDeviation )
	{ }
}
