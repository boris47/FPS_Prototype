
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Database {

	// PUBLIC INTERFACE
	public interface ISection {

		bool					m_bIsOK							{ get; }

		void					Destroy							();
		int						Lines							();
		string					GetName							();
		bool					Add								( cLineValue LineValue );
		bool					Remove							( string LineValueID );
		bool					HasKey							( string Key );
		bool					IsChildOf						( Section MotherSection );
		bool					IsChildOf						( string MotherName );

		System.Type				ValueType						( string Key );
		string					GetRawValue						( string Key, string Default = "" );

		T						As				<T>				( string Key );
		bool					AsBool							( string Key, bool Default = false );
		int						AsInt							( string Key, int Default = 0 );
		float					AsFloat							( string Key, float Default = 0.0f );
		string					AsString						( string Key, string Default = "" );

		cValue					OfMultiValue					( string Key, int Index );
		void					AsMultiValue	<T1,T2>			( string Key, int Idx1, int Idx2, ref T1 t1, ref T2 t2 );
		void					AsMultiValue	<T1,T2,T3>		( string Key, int Idx1, int Idx2, int Idx3, ref T1 t1, ref T2 t2, ref T3 t3 );
		void					AsMultiValue	<T1,T2,T3,T4>	( string Key, int Idx1, int Idx2, int Idx3, int Idx4, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4 );

		bool					bAs<T>							( string Key, ref T Out );
		bool					bAsBool							( string Key, ref bool	 Out, bool	 Default = false );
		bool					bAsInt							( string Key, ref int	 Out, int	 Default = 0	 );
		bool					bAsFloat						( string Key, ref float  Out, float	 Default = 0.0f  );
		bool					bAsString						( string Key, ref string Out, string Default = ""	 );

		int						GetMultiSize					( string Key );

		bool					bAsMultiValue					( string Key, int Index, out cValue Out );

		bool					bAsVec2							( string Key, ref Vector2 Out, Vector2? Default );
		bool					bAsVec3							( string Key, ref Vector3 Out, Vector3? Default );
		bool					bAsVec4							( string Key, ref Vector4 Out, Vector4? Default );

		Vector2					AsVec2							( string Key, Vector2? Default );
		Vector3					AsVec3							( string Key, Vector3? Default );
		Vector4					AsVec4							( string Key, Vector4? Default );
		Color					AsColor							( string Key, Color? Default );

		void					SetValue						( string Key, cValue Value );
		void					SetMultiValue					( string Key, cValue[] vValues );
		void					Set				<T>				( string Key, T Value );
		void					PrintSection					();
	}
	
	[System.Serializable]
	public partial class Section : ISection, IEnumerable {

		// INTERNAL VARS
		[SerializeField]
		private		string				name			= null;

		[SerializeField]
		private		string				m_Context		= "";

		[SerializeField]
		private		List<cLineValue>	m_Linevalues		= new List<cLineValue>();

		[SerializeField]
		private		List<string>		m_Mothers		= new List<string>();

		[SerializeField]
		public		bool				m_bIsOK
		{
			get; private set;
		}

		public	string	Context
		{
			get { return ( m_Context.Length > 0 ) ? (string)m_Context.Clone() :""; }
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator) GetEnumerator();
		}
	
		public List<cLineValue>.Enumerator  GetEnumerator()
		{
			return m_Linevalues.GetEnumerator();
		}

		// Indexer
		public cLineValue this[int i]
		{
			get { return ( i > -1 && i < m_Linevalues.Count ) ? m_Linevalues[i] : null; }
		}

		public Section( string sectionName, string context )
		{
			name = sectionName;
			m_Context = context;
			m_bIsOK = true;
		}


		public static bool operator !( Section obj )
		{
			return obj == null;
		}

		public static bool operator false( Section obj )
		{
			return obj == null;
		}

		public static bool operator true( Section obj )
		{
			return obj != null;
		}

		public static Section operator +( Section SecA, Section SecB )
		{
			if ( SecB.m_bIsOK == true )
			{
				foreach( cLineValue lineValue in SecB )
				{
					if ( SecA.HasKey( lineValue.Key ) == false )
					{
						SecA.Add( lineValue );
					}
				}
				SecA.m_Mothers.Add( SecB.name );
			}
			return SecA;
		}

		public	bool					IsChildOf						( Section mother )
		{
			string motherName = mother.GetName();
			return ( m_Mothers.FindIndex( m => m == motherName ) > -1 );
		}

		public	bool					IsChildOf						( string MotherName )
		{
			return ( m_Mothers.FindIndex( m => m == MotherName ) > -1 );
		}
		
		

		public void Destroy()
		{
			m_Linevalues.ForEach( ( cLineValue lv ) => lv.Destroy() );
		}

		public	int						Lines()
		{
			return m_Linevalues.Count;
		}


		public	string					GetName()				{ return ( string ) name.Clone(); }
	

		//////////////////////////////////////////////////////////////////////////
		// GetKeys
		public	string[]				GetKeys()
		{
			string[] arrayToReturn = new string[ m_Linevalues.Count ];
			for ( int i = 0; i < m_Linevalues.Count; i++ )
			{
				arrayToReturn[i] = m_Linevalues[i].Key;
			}
			return arrayToReturn;
		}

		//////////////////////////////////////////////////////////////////////////
		// Add
		public	bool				Add( cLineValue LineValue )
		{
			int index = m_Linevalues.FindIndex( ( s ) => s.Key == LineValue.Key );
			// Confirmed new linevalue
			if ( index == -1 )
			{
				m_Linevalues.Add( LineValue );
			}
			// overwrite of existing linevalue
			else
			{
				m_Linevalues[ index ] = new cLineValue( LineValue );
			}
			return index > -1;
		}


		//////////////////////////////////////////////////////////////////////////
		// Remove
		public	bool					Remove( string lineValueID )
		{
			int index = m_Linevalues.FindIndex( ( s ) => s.Key == lineValueID );
			if ( index > -1 )
			{
				m_Linevalues[index].Destroy();
				m_Linevalues.RemoveAt( index );
			}
			return index > -1;
		}
		

		//////////////////////////////////////////////////////////////////////////
		// bGetLineValue
		public	bool					bGetLineValue( string key, ref cLineValue lineValue )
		{
			int index = m_Linevalues.FindIndex( ( cLineValue lv ) => lv.IsKey( key ) == true );
			bool bHasBeenFound = index > -1;
			if ( bHasBeenFound )
			{
				lineValue = m_Linevalues[ index ];
			}
			return bHasBeenFound;
		}


		//////////////////////////////////////////////////////////////////////////
		// HasKey
		public	bool					HasKey( string Key )
		{	
			cLineValue bump = null;
			return bGetLineValue( Key, ref bump );
		}


		//////////////////////////////////////////////////////////////////////////
		// PrintSection
		public void PrintSection()
		{
			Debug.Log( "---|Section START" + name );
			foreach ( cLineValue LineValue in m_Linevalues )
			{
				string result = LineValue.Key;
				if ( LineValue.Type == LineValueType.MULTI )
				{
					cMultiValue multi = LineValue.MultiValue;
					for ( int i = 0; i < multi.Size; i++ )
					{
						result += " " + multi[ i ];
					}
					Debug.Log( "\t" + result );
				}
				else
				{
					if ( LineValue.Value.ToSystemObject() == null )
					{
						Debug.Log( result + " " + LineValue.RawValue );
					}
					else
					Debug.Log( "\t" + result + " " + LineValue.Value.ToSystemObject() + ", " + LineValue.Value.ToSystemObject().GetType() );
				}
			}
			Debug.Log( "---|Section END" );
		}


		//////////////////////////////////////////////////////////////////////////
		// SaveToBuffer
		public	void	SaveToBuffer( ref string buffer )
		{
			List<string> lines = new List<string>();

			// SECTION DEFINITION
			string sectionDefinition = "[" + name + "]";
			{
				// Concatenate mothers names
				if ( m_Mothers.Count > 0 )
				{
					sectionDefinition += ":" + m_Mothers[0];
					for ( int i = 1; i < m_Mothers.Count; i++, sectionDefinition += ',' )
					{
						string motherName = m_Mothers[i];
						sectionDefinition += motherName;
					}
				}
			}
			lines.Add( sectionDefinition );

			// Write key value pairs
			foreach ( cLineValue LineValue in m_Linevalues )
			{
				string key = LineValue.Key;
				string value = "";
				if ( LineValue.Type == LineValueType.MULTI )
				{
					cMultiValue multi = LineValue.MultiValue;
					value += multi[ 0 ];
					for ( int i = 1; i < multi.Size; i++, value += "," )
					{
						string subValue = multi[ i ];
						value += subValue;
					}
				}
				else
				{
					cValue multi = LineValue.Value;
					value += multi.ToString();
				}
				string keyValuePair = key + "=" + value;
				lines.Add( keyValuePair );
			}

			string internalBuffer = string.Join( "\n", lines.ToArray() );
			buffer += internalBuffer;
		}


	};

}