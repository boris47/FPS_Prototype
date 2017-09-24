
using System.Collections.Generic;
using UnityEngine;


// PUBLIC INTERFACE
interface ISection {

	/// <summary>
	/// asd
	/// </summary>
	void					Destroy();
	bool					IsOK();
	List < cLineValue >		GetData();
	int						Lines();
	string					Name();
	void					Add( cLineValue LineValue );
	bool					HasKey( string Key );

	int						KeyType( string Key );
	string					GetRawValue ( string Key, string Default = "" );

	bool					AsBool( string Key, bool Default = false );
	int						AsInt( string Key, int Default = 0 );
	float					AsFloat( string Key, float Default = 0.0f );
	string					AsString( string Key, string Default = "" );

	cValue					AsMultiValue( string Key, int Index );

	bool					bAsBool( string Key, ref bool Out, bool Default = false );
	bool					bAsInt( string Key, ref int Out, int Default = 0 );
	bool					bAsFloat( string Key, ref float Out, float Default = 0.0f );
	bool					bAsString( string Key, ref string Out, string Default = "" );

	int						GetMultiSize( string Key );

	bool					bAsMultiValue( string Key, int Index, out cValue Out );

	bool					bAsVec2( string Key, ref Vector2 Out, Vector2 Default );
	bool					bAsVec3( string Key, ref Vector3 Out, Vector3 Default );
	bool					bAsVec4( string Key, ref Vector4 Out, Vector4 Default );

	void					SetValue( string Key, cValue Value );
	void					SetMultiValue( string Key, List < cValue > vValues );
	void					SetInt( string Key, int Value );
	void					SetBool( string Key, bool Value );
	void					SetFloat( string Key, float Value );
	void					SetString ( string Key, string Value );

}

public class Section : ISection {


	// INTERNAL VARS
	string sName = "";
	List < cLineValue > vSection;

	bool bIsOK = false;



	// Indexer behaviour
	public cLineValue this[ string Key ] {

		get {

			if ( vSection == null ) return null;

			if ( vSection.Count < 1 ) return null;

			foreach ( cLineValue LineValue in vSection )  {
				if ( LineValue.IsKey( Key ) )
					return LineValue;
			}
			return null;
		}

	}



	public static bool operator !( Section Sec ) {
		return Sec == null;
	}

	public static bool operator false( Section Sec ) {
		return Sec == null;
	}

	public static bool operator true( Section Sec ) {
		return Sec != null;
	}



	public Section( string SecName, Section Mother = null ) {

		sName = SecName;
		if ( Mother != null ) vSection = Mother.GetData();
		else vSection = new List < cLineValue >();
		bIsOK = true;

	}

	public void Destroy() {

		foreach( cLineValue pLineValue in vSection ) pLineValue.Destroy();
		vSection.Clear();

	}

	public bool						IsOK()				{ return bIsOK; }

	public	List < cLineValue >		GetData()			{ return vSection; }
	public	int						Lines()				{ return vSection.Count; }
	public	string					Name()				{ return ( string ) sName.Clone(); }

	public	void					Add( cLineValue LineValue ) { vSection.Add( LineValue ); }


	public	bool					HasKey( string Key ) {
		
		return ( this[ Key ] != null );

	}

	// Return the type of a value assigned to a key, or -1
	public	int						KeyType( string Key ) {

		if ( vSection.Count < 1 ) return -1;

		cLineValue pLineValue = null; cValue Value = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			LineValueType iType = pLineValue.Type;
			switch( iType ) {
				case LineValueType.SINGLE: {
					if ( ( Value = pLineValue.Value ) != null ) { return ( int ) Value.Type; }
					Debug.LogError( "cSection::KeyType:WARNING! In section " + sName + " a key has no value but designed as SINGLE !!!" );
					break;
				}
				case LineValueType.MULTI: {
						if ( ( pLineValue.MultiValue != null ) ) return ( int ) iType;
						Debug.LogError( "cSection::KeyType:WARNING! In section " + sName + " a key has no value but designed as MULTI !!!" );
						break;
				}
			}
		}

		return -1;
	}

	public	string					GetRawValue ( string Key, string Default = "" ) {

		cLineValue pLineValue = null;
		return ( ( pLineValue = this[ Key ] ) != null ) ? pLineValue.RawValue : Default;

	}


	public	bool					AsBool( string Key, bool Default = false ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				return pLineValue.Value.ToBool();
			}
		}
		return Default;

	}

	public	int						AsInt( string Key, int Default = 0 ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				return pLineValue.Value.ToInteger();
			}
		}
		return Default;

	}

	public	float					AsFloat( string Key, float Default = 0.0f ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				return pLineValue.Value.ToFloat();
			}
		}
		return Default;

	}

	public	string					AsString( string Key, string Default = "" ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				return pLineValue.Value.ToString();
			}
		}
		return Default;

	}

	public	cValue					AsMultiValue( string Key, int Index ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null; cValue pValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( ( pMultiValue = pLineValue.MultiValue ) != null ) {
				if ( ( pValue = pMultiValue [Index - 1 ] ) != null ) {
					return pValue;
				}
			}
		}

		return null;
	}


	public	void					AsMultiValue<T1,T2>( string Key, int Idx1, int Idx2, ref T1 t1, ref T2 t2 ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		if ( ( ( pLineValue = this[ Key ] ) != null ) && ( ( pMultiValue = pLineValue.MultiValue ) != null ) ) {

			t1 = pMultiValue [Idx1 - 1].As<T1>();
			t2 = pMultiValue [Idx2 - 1].As<T2>();
		}

	}

	public	void					AsMultiValue<T1,T2,T3>( string Key, int Idx1, int Idx2, int Idx3, ref T1 t1, ref T2 t2, ref T3 t3 ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		if ( ( ( pLineValue = this[ Key ] ) != null ) && ( ( pMultiValue = pLineValue.MultiValue ) != null ) ) {

			t1 = pMultiValue [Idx1 - 1].As<T1>();
			t2 = pMultiValue [Idx2 - 1].As<T2>();
			t3 = pMultiValue [Idx3 - 1].As<T3>();
		}

	}

	public	void					AsMultiValue<T1,T2,T3,T4>( string Key, int Idx1, int Idx2, int Idx3, int Idx4, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4 ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		if ( ( ( pLineValue = this[ Key ] ) != null ) && ( ( pMultiValue = pLineValue.MultiValue ) != null ) ) {

			t1 = pMultiValue [Idx1 - 1].As<T1>();
			t2 = pMultiValue [Idx2 - 1].As<T2>();
			t3 = pMultiValue [Idx3 - 1].As<T3>();
			t4 = pMultiValue [Idx4 - 1].As<T4>();
		}

	}


	public	bool					bAsBool( string Key, ref bool Out, bool Default = false ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				Out = pLineValue.Value.ToBool();
				return true;
			}
		}

		Out = Default;
		return false;
	}

	public	bool					bAsInt( string Key, ref int Out, int Default = 0 ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				Out = pLineValue.Value.ToInteger();
				return true;
			}
		}

		Out = Default;
		return false;
	}

	public	bool					bAsFloat( string Key, ref float Out, float Default = 0.0f ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				Out = pLineValue.Value.ToFloat();
				return true;
			}
		}

		Out = Default;
		return false;
	}

	public	bool					bAsString( string Key, ref string Out, string Default = "" ) {

		cLineValue pLineValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( pLineValue.Type == LineValueType.SINGLE ) {
				Out = pLineValue.Value.ToString();
				return true;
			}
		}

		Out = Default;
		return false;
	}

	public	int						GetMultiSize( string Key ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		return (
			( ( pLineValue = this[ Key ] )			!= null ) && 
			( ( pMultiValue = pLineValue.MultiValue )	!= null ) ) ? 
			( pMultiValue.Size + 1 ) : 0;

	}

	public	bool					bAsMultiValue( string Key, int Index, out cValue Out ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null; cValue pValue = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( ( pMultiValue = pLineValue.MultiValue ) != null ) {
				if ( ( pValue = pMultiValue[ Index - 1 ] ) != null ) {
					Out = pValue;
					return true;
				}
			}
		}

		Out = null;
		return false;
	}

	public	bool					bAsVec2( string Key, ref Vector2 Out, Vector2 Default ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		cValue pValue1 = null; cValue pValue2 = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( ( pMultiValue = pLineValue.MultiValue ) != null ) {
				if ( ( ( pValue1 = pMultiValue[ 0 ] ) != null ) && 
					   ( pValue2 = pMultiValue[ 1 ] ) != null ) {
					Out = new Vector2( pValue1.ToFloat(), pValue2.ToFloat() );
					return true;
				}
			}
		}

		Out = Default;
		return false;
	}


	public	bool					bAsVec3( string Key, ref Vector3 Out, Vector3 Default ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		cValue pValue1 = null; cValue pValue2 = null; cValue pValue3 = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( ( pMultiValue = pLineValue.MultiValue ) != null ) {
				if ( (	( pValue1 = pMultiValue[ 0 ] ) != null ) && 
					 (	( pValue2 = pMultiValue[ 1 ] ) != null ) &&
					 (  ( pValue3 = pMultiValue[ 2 ] ) != null )  ){
					Out = new Vector3( pValue1.ToFloat(), pValue2.ToFloat(), pValue3.ToFloat() );
					return true;
				}
			}
		}

		Out = Default;
		return false;
	}

	public	bool					bAsVec4( string Key, ref Vector4 Out, Vector4 Default ) {

		cLineValue pLineValue = null; cMultiValue pMultiValue = null;
		cValue pValue1 = null; cValue pValue2 = null; cValue pValue3 = null; cValue pValue4 = null;
		if ( ( pLineValue = this[ Key ] ) != null ) {
			if ( ( pMultiValue = pLineValue.MultiValue ) != null ) {
				if ( (	( pValue1 = pMultiValue[ 0 ] ) != null ) && 
					 (	( pValue2 = pMultiValue[ 1 ] ) != null ) &&
					 (	( pValue3 = pMultiValue[ 2 ] ) != null ) &&
					 (  ( pValue4 = pMultiValue[ 3 ] ) != null )  ){
					Out = new Vector4( pValue1.ToFloat(), pValue2.ToFloat(), pValue3.ToFloat(), pValue4.ToFloat() );
					return true;
				}
			}
		}

		Out = Default;
		return false;
	}



	public	void					SetValue( string Key, cValue Value ) {

		cLineValue pLineValue = this[ Key ];
		// if not exists create one
		if ( pLineValue == null ) pLineValue = new cLineValue( Key, ( byte ) LineValueType.SINGLE );
		pLineValue.Clear();
		pLineValue.Set( Value );

	}

	public	void					SetMultiValue( string Key, List < cValue > vValues ) {

		cLineValue pLineValue = this[ Key ];
		// if not exists create one
		if ( pLineValue == null ) pLineValue = new cLineValue( Key, LineValueType.MULTI );
		pLineValue.Clear();
		pLineValue.Set( new cMultiValue( vValues ) );

	}


	public	void					SetInt( string Key, int Value ) {

		SetValue( Key, new cValue( Value ) );

	}

	public	void					SetBool( string Key, bool Value ) {

		SetValue( Key, new cValue( Value ) );

	}

	public	void					SetFloat( string Key, float Value ) {

		SetValue( Key, new cValue( Value ) );

	}
	
	public	void					SetString ( string Key, string Value ) {

		SetValue( Key, new cValue( Value ) );

	}



	public	void			SetVec2( string Key, Vector2 Vec ) {

		cLineValue pLineValue = this[ Key ];
		// if not exists create one
		if ( pLineValue == null ) pLineValue = new cLineValue( Key, LineValueType.MULTI );
		pLineValue.Clear();

		List < cValue > vValues = new List<cValue> { new cValue( Vec.x ), new cValue( Vec.y ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}

	public	void			SetVec3( string Key, Vector3 Vec ) {

		cLineValue pLineValue = this[ Key ];
		// if not exists create one
		if ( pLineValue == null ) pLineValue = new cLineValue( Key, LineValueType.MULTI );
		pLineValue.Clear();

		List < cValue > vValues = new List<cValue> { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}

	public	void			SetVec4( string Key, Vector4 Vec ) {

		cLineValue pLineValue = this[ Key ];
		// if not exists create one
		if ( pLineValue == null ) pLineValue = new cLineValue( Key, LineValueType.MULTI );
		pLineValue.Clear();

		List < cValue > vValues = new List<cValue> { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ), new cValue( Vec.w ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}



};