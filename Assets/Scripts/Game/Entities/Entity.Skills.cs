
using UnityEngine;
using System.Collections.Generic;

public interface IEntitySkills
{
	bool		EnableSkill			( string Key );
	bool		DisableSkill		( string Key );
	bool		IsSkillEnabled		( string Key );
	bool		HasSkill			( string key );
	float		GetSkill			( string Key, float Default = 0.0f );
	bool		GetSkill			( string Key, out float Value, float Default = 0.0f );
}



public	abstract	partial	class Entity : IEntitySkills {

	private			IEntitySkills				m_SkillInstance			= null;
	public			IEntitySkills				Skills					{ get { return m_SkillInstance; } }

	private class MultiplierData<T> {
		public		bool	IsEnabled	= true;
		public		T		Value		= default(T);
	}

	private	readonly	Dictionary<string, MultiplierData<float>>	m_SkillsMap		= new Dictionary<string, MultiplierData<float>>();



	// Private methods:
	//	AddSkill( string Key, float Value )
	//	LoseSkil( string key )
	//////////////////////////////////////////////////////////////////////////
	private		bool	AddSkill( string Key, float Value )
	{
		bool result = false;
		if (m_SkillsMap.ContainsKey( Key ) )
		{
			m_SkillsMap.Add( Key, new MultiplierData<float>() { Value = Value } );
			result = true;
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	EnableSkill( string Key )
	{
		return HasSkill( Key ) && (m_SkillsMap[ Key ].IsEnabled = true );
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	DisableSkill( string Key )
	{
		return HasSkill( Key ) && !(m_SkillsMap[ Key ].IsEnabled = false );
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	IsSkillEnabled( string Key )
	{
		return HasSkill( Key ) && m_SkillsMap[Key].IsEnabled;
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	HasSkill( string Key )
	{
		return m_SkillsMap.ContainsKey( Key );
	}


	//////////////////////////////////////////////////////////////////////////
	public		float	GetSkill( string Key, float Default = 0.0f )
	{
		return ( (HasSkill( Key ) && m_SkillsMap[Key].IsEnabled ) ? m_SkillsMap[Key].Value : Default );
	}


	//////////////////////////////////////////////////////////////////////////
	public		bool	GetSkill( string Key, out float Value, float Default )
	{
		Value = Default;
		bool result = false;
		if (HasSkill( Key ) && m_SkillsMap[ Key ].IsEnabled )
		{
			Value = m_SkillsMap[ Key ].Value;
			result = true;
		}
		return result;
	}


	//////////////////////////////////////////////////////////////////////////
	private		bool	LoseSkill( string Key )
	{
		bool result = false;
		if (HasSkill( Key ) == true )
		{
			m_SkillsMap.Remove( Key );
		}
		return result;
	}

}


