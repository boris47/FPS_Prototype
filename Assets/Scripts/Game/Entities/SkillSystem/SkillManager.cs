using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class EntitySkills {

	private		Dictionary<string, float>	m_Skills = new Dictionary<string, float>();

	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="Key"></param>
	/// <param name="Value"></param>
	/// <returns></returns>
	public	bool	AddMultiplier( string Key, float Value )
	{
		bool result = false;
		if ( m_Skills.ContainsKey( Key ) == false )
		{
			m_Skills.Add( Key, Value );
			result = true;
		}
		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="Key"></param>
	/// <returns></returns>
	public	bool	RemoveMultiplier( string Key )
	{
		bool result = false;
		if ( m_Skills.ContainsKey( Key ) == true )
		{
			m_Skills.Remove( Key );
			result = true;
		}
		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="Key"></param>
	/// <param name="Default"></param>
	/// <returns></returns>
	public	float	GetMultiplier( string Key, float Default = 1.0f )
	{
		float result = Default;
		if ( m_Skills.ContainsKey( Key ) == true )
		{
			result = m_Skills[ Key ];
		}
		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="Key"></param>
	/// <param name="Value"></param>
	/// <param name="Default"></param>
	/// <returns></returns>
	public	bool	GetMultiplier( string Key, out float Value, float Default = 1.0f )
	{
		bool result = m_Skills.TryGetValue( Key, out Value );
		if ( result == false )
		{
			Value = Default;
		}
		return result;
	}

}



public	static class SkillManager {


	private	static	readonly	Dictionary<uint, EntitySkills>		m_Data			= new Dictionary<uint, EntitySkills>();

	private	static	bool											m_bIsInitialized	= false;


	/// <summary>
	/// If not initialized, initialize blackboard data
	/// </summary>
	static	SkillManager()
	{
		if ( m_bIsInitialized == false )
		{
			m_Data = new Dictionary<uint, EntitySkills>();
			m_bIsInitialized = true;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	RegisterEntity( uint EntityID )
	{
		bool result = false;
		if ( m_Data.ContainsKey( EntityID ) == false )
		{
			m_Data.Add( EntityID, new EntitySkills() );
			result = true;
		}
		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	bool	UnregisterEntity( uint EntityID )
	{
		bool result = false;
		if ( m_Data.ContainsKey( EntityID ) == true )
		{
			m_Data[ EntityID ] = null;
			m_Data.Remove( EntityID );
			result = true;
		}
		return result;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <param name="Key"></param>
	/// <param name="Value"></param>
	/// <returns></returns>
	public	static	bool		RegisterSkill( uint EntityID, string Key, float Value )
	{
		bool result = false;
		if ( m_Data.ContainsKey( EntityID ) == true )
		{
			m_Data[ EntityID ].AddMultiplier( Key, Value );
		}
		return result;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <param name="Key"></param>
	/// <returns></returns>
	public	static	bool		UnregisterSkill( uint EntityID, string Key )
	{
		bool result = false;
		if ( m_Data.ContainsKey( EntityID ) == true )
		{
			result = m_Data[ EntityID ].RemoveMultiplier( Key );
		}
		return result;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <returns></returns>
	public	static	float		GetSkills( uint EntityID, string Key, float Default = 1.0f )
	{
		float result = Default;
		if ( m_Data.ContainsKey( EntityID ) == true )
		{
			result = m_Data[ EntityID ].GetMultiplier( Key, Default );
		}
		return result;
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="EntityID"></param>
	/// <param name="Key"></param>
	/// <param name="Value"></param>
	/// <param name="Default"></param>
	/// <returns></returns>
	public	static	bool		GetSkill( uint EntityID, string Key, out float Value, float Default = 1.0f )
	{
		bool result = false;
		Value = Default;
		if ( m_Data.ContainsKey( EntityID ) == true )
		{
			result = m_Data[ EntityID ].GetMultiplier( Key, out Value, Default );
		}
		return result;
	}
}
