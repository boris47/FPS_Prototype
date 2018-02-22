

using UnityEngine;


// PUBLIC INTERFACE
interface ISection {

	bool					IsOK { get; }

	void					Destroy();
	cLineValue[]			GetData();
	int						Lines();
	string					Name();
	void					Add( cLineValue LineValue );
	bool					HasKey( string Key );

	System.Type				ValueType( string Key );
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
	void					SetMultiValue( string Key, cValue[] vValues );
	void					Set<T>( string Key, T Value );
	void					PrintSection();

}

public partial class Section : ISection {

	// INTERNAL VARS
	private		string			sName			= "";
	private		cLineValue[]	vSection		= null;
	public		bool			IsOK
	{
		get; private set;
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



	public Section( string SecName, Section Mother = null )
	{
		sName = SecName;
		if ( Mother != null )
			vSection = Mother.GetData();

		IsOK = true;
	}

	public void Destroy()
	{
		System.Array.ForEach<cLineValue>( vSection, ( cLineValue lv ) => lv.Destroy() );
	}

	public	cLineValue[]			GetData()			{ return vSection; }
	public	int						Lines()				{ return vSection.Length; }
	public	string					Name()				{ return ( string ) sName.Clone(); }
	

	public	void					Add( cLineValue LineValue )
	{
		if ( vSection == null )
			vSection = new cLineValue[ 1 ];
		else
			System.Array.Resize<cLineValue>( ref vSection, vSection.Length + 1 );

		vSection[ vSection.Length - 1 ] = LineValue;
	}


	// Indexer behaviour
	public cLineValue this[ string Key ]
	{
		get
		{
			if ( vSection == null || vSection.Length < 1 )
				return null;

			return System.Array.Find<cLineValue>( vSection, ( lv ) => lv.IsKey( Key ) == true );
		}
	}

	public	bool					HasKey( string Key )
	{	
		return ( this[ Key ] != null );
	}





	public void PrintSection()
	{
		Debug.Log( "Section " + sName );
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
				Debug.Log( result );
			}
			else
			{
				if ( LineValue.Value.Value == null )
				{
					Debug.Log( result + " " + LineValue.RawValue );
				}
				else
				Debug.Log( result + " " + LineValue.Value.Value + ", " + LineValue.Value.Value.GetType() );
			}
		}
	}


};