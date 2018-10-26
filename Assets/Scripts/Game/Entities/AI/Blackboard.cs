using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFG_Reader;


public static class Blackboard {


	private	static	bool	m_bIsInitialized = false;


	public	static	void	Initialize()
	{
		if ( m_bIsInitialized == false )
		{
			new BlackboardSingleton();
		}
	}

	public	static	bool	Register( uint EntityID )
	{
		Initialize();
		return BlackboardSingleton.Instance.Register( EntityID );
	}

	public	static	bool	UnRegister( uint EntityID )
	{
		Initialize();
		return BlackboardSingleton.Instance.UnRegister( EntityID );
	}

	public	static	cValue	GetValue( uint EntityID, string Key )
	{
		return m_bIsInitialized ? BlackboardSingleton.Instance[ EntityID, Key ] : null;
	}

	public	static	bool	IsEntityRegistered( uint EntityID )
	{
		return m_bIsInitialized ? BlackboardSingleton.Instance.IsEntityRegistered( EntityID ) : false;
	}

	public	static	bool	HasValue( uint EntityID, string Key )
	{
		return m_bIsInitialized ? BlackboardSingleton.Instance.HasValue( EntityID, Key ) : false;
	}

	public	static	bool	bGetValue( uint EntityID, string Key, out cValue Value )
	{
		Value = null;
		return m_bIsInitialized ? BlackboardSingleton.Instance.bGetValue( EntityID, Key, out Value ) : false;
	}



	protected class BlackboardSingleton {

		private	static	BlackboardSingleton m_Instance = null;
		public	static	BlackboardSingleton Instance
		{
			get { return m_Instance; }
		}

		private Dictionary< uint, Dictionary< string, cValue > > Data = new Dictionary< uint, Dictionary< string, cValue > >();


		public	BlackboardSingleton()
		{
			m_Instance = this;
		}


		// INDEXER
		public	cValue	this [ uint EntityID, string Key ]
		{
			get
			{
				if ( IsEntityRegistered( EntityID ) )
				{
					var entityData = this.GetEntityData( EntityID );
					if ( entityData.ContainsKey( Key ) )
					{
						return entityData[ Key ];
					}
				}

				return null;	
			}
		}


		public	bool	Register( uint EntityID )
		{
			if ( Data.ContainsKey( EntityID ) )
			{
				return false;
			}

			var entityData = new Dictionary< string, cValue > ();
			Data.Add( EntityID, entityData );
			return true;
		}


		public	bool	UnRegister( uint EntityID )
		{
			if ( IsEntityRegistered( EntityID ) )
			{
				return Data.Remove( EntityID );
			}
			return false;
		}


		public	bool	IsEntityRegistered( uint EntityID )
		{
			return Data.ContainsKey( EntityID );
		}


		public	bool	HasValue( uint EntityID, string Key )
		{
			if ( IsEntityRegistered( EntityID ) )
			{
				var entityData = this.GetEntityData( EntityID );
				if ( entityData.ContainsKey( Key ) )
				{
					return true;
				}
			}
			return true;
		}


		public	bool	bGetValue( uint EntityID, string Key, out cValue Value )
		{
			bool result = false;
			Value = null;
			if ( IsEntityRegistered( EntityID ) )
			{
				var entityData = this.GetEntityData( EntityID );
				if ( entityData.ContainsKey( Key ) )
				{
					Value = entityData[ Key ];
					result = true;
				}
			}
			return result;
		}


		private	Dictionary< string, cValue > GetEntityData( uint EntityID )
		{
			return Data[ EntityID ];
		}

	}
	
}
