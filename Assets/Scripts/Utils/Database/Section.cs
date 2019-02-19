
using System.Collections.Generic;
using UnityEngine;

namespace Database {

	// PUBLIC INTERFACE
	public interface ISection {

		bool					IsOK							{ get; }

		void					Destroy							();
		List<cLineValue>		GetData							();
		int						Lines							();
		string					Name							();
		void					Add								( cLineValue LineValue );
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

		cValue					AsMultiValue					( string Key, int Index );
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

		void					SetValue						( string Key, cValue Value );
		void					SetMultiValue					( string Key, cValue[] vValues );
		void					Set				<T>				( string Key, T Value );
		void					PrintSection					();
	}
	
	public partial class Section : ISection {

		// INTERNAL VARS
		private		string				sName			= "";
		private		List<cLineValue>	vSection		= new List<cLineValue>();
		private		List<string>		vMothers		= new List<string>();
		public		bool				IsOK
		{
			get; private set;
		}


		public Section( string SecName )
		{
			sName = SecName;
			IsOK = true;
		}


		public static bool operator !( Section Sec )
		{
			return Sec == null;
		}

		public static bool operator false( Section Sec )
		{
			return Sec == null;
		}

		public static bool operator true( Section Sec )
		{
			return Sec != null;
		}

		public static Section operator +( Section SecA, Section SecB )
		{
			if ( SecB.IsOK == true )
			{
				foreach( cLineValue lineValue in SecB.GetData() )
				{
					if ( SecA.HasKey( lineValue.Key ) == false )
					{
						SecA.Add( lineValue );
					}
				}
				SecA.vMothers.Add( SecB.sName );
			}
			return SecA;
		}

		public	bool					IsChildOf						( Section MotherSection )
		{
			string motherName = MotherSection.Name();
			return ( vMothers.FindIndex( m => m == motherName ) != -1 );
		}

		public	bool					IsChildOf						( string MotherName )
		{
			return ( vMothers.FindIndex( m => m == MotherName ) != -1 );
		}
		
		

		public void Destroy()
		{
			vSection.ForEach( ( cLineValue lv ) => lv.Destroy() );
		}

		public	List<cLineValue>		GetData()			{ return vSection; }
		public	int						Lines()				{ return vSection.Count; }
		public	string					Name()				{ return ( string ) sName.Clone(); }
	

		public	void					Add( cLineValue LineValue )
		{
			int index = vSection.FindIndex( ( s ) => s.Key == LineValue.Key );
			// Confirmed new linevalue
			if ( index == -1 )
			{
				vSection.Add( LineValue );
			}
			// overwrite of existing linevalue
			else
			{
				vSection[ index ] = new cLineValue( LineValue );
			}
		}


		// Indexer behaviour
		public cLineValue this[ string Key ]
		{
			get
			{
				if ( vSection.Count < 1 )
				{
					return null;
				}

				return vSection.Find( ( cLineValue lineValue ) => lineValue.IsKey( Key ) == true );
			}
		}

		public	bool					HasKey( string Key )
		{	
			return ( this[ Key ] != null );
		}





		public void PrintSection()
		{
			Debug.Log( "---|Section START" + sName );
			foreach ( cLineValue LineValue in vSection )
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


	};

}