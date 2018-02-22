

using UnityEngine;


public partial class Section : ISection {
	

	public    void                    SetValue( string Key, cValue Value )
	{
		cLineValue pLineValue = this[ Key ];

		// if not exists create one
		if ( pLineValue == null )
			pLineValue = new cLineValue( Key, ( byte ) LineValueType.SINGLE );

		pLineValue.Clear();
		pLineValue.Set( Value );
	}

	public    void                    SetMultiValue( string Key, cValue[] vValues )
	{
		cLineValue pLineValue = this[ Key ];

		// if not exists create one
		if ( pLineValue == null )
			pLineValue = new cLineValue( Key, LineValueType.MULTI );

		pLineValue.Clear();
		pLineValue.Set( new cMultiValue( vValues ) );
	}


	public	void					Set<T>( string Key, T Value )
	{
		SetValue( Key, new cValue( Value ) );
	}



	public	void			SetVec2( string Key, Vector2 Vec )
	{
		cLineValue pLineValue = this[ Key ];

		// if not exists create one
		if ( pLineValue == null )
			pLineValue = new cLineValue( Key, LineValueType.MULTI );

		pLineValue.Clear();
		cValue[] vValues = new cValue[2] { new cValue( Vec.x ), new cValue( Vec.y ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}

	public	void			SetVec3( string Key, Vector3 Vec ) {

		cLineValue pLineValue = this[ Key ];

		// if not exists create one
		if ( pLineValue == null )
			pLineValue = new cLineValue( Key, LineValueType.MULTI );

		pLineValue.Clear();
		cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}

	public	void			SetVec4( string Key, Vector4 Vec ) {

		cLineValue pLineValue = this[ Key ];

		// if not exists create one
		if ( pLineValue == null )
			pLineValue = new cLineValue( Key, LineValueType.MULTI );

		pLineValue.Clear();
		cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ), new cValue( Vec.w ) };
		pLineValue.Set( new cMultiValue( vValues ) );

	}

};