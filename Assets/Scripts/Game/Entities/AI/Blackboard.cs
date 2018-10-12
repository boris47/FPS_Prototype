using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFG_Reader;


public static class Blackboard {

	public class BlackboardSingleton {

		private	static	BlackboardSingleton m_Instance = null;
		public	static	BlackboardSingleton Instance
		{
			get { return m_Instance; }
		}

		public	BlackboardSingleton()
		{
			m_Instance = this;
		}

		private Dictionary< int, Dictionary< string, cValue > > Data = new Dictionary< int, Dictionary< string, cValue > >();

		// INDEXER
		public	cValue	this [ int EntityID, string Key ]
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


		public	bool	IsEntityRegistered( int EntityID )
		{
			return Data.ContainsKey( EntityID );
		}


		public	bool	HasValue( int EntityID, string Key )
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


		public	bool	bGetValue( int EntityID, string Key, out cValue Value )
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


		private	Dictionary< string, cValue > GetEntityData( int EntityID )
		{
			return Data[ EntityID ];
		}

	}


	public	static	cValue	GetValue( int EntityID, string Key )
	{
		return BlackboardSingleton.Instance[ EntityID, Key ];
	}

	public	static	bool	IsEntityRegistered( int EntityID )
	{
		return BlackboardSingleton.Instance.IsEntityRegistered( EntityID );
	}

	public	static	bool	HasValue( int EntityID, string Key )
	{
		return BlackboardSingleton.Instance.HasValue( EntityID, Key );
	}

	public	static	bool	bGetValue( int EntityID, string Key, out cValue Value )
	{
		return BlackboardSingleton.Instance.bGetValue( EntityID, Key, out Value );
	}
	
}
