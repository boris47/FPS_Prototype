

using UnityEngine;


namespace CFG_Reader {

	public partial class Section : ISection {
	
		//////////////////////////////////////////////////////////////////////////
		// SetValue
		public	void					SetValue( string Key, cValue Value )
		{
			cLineValue pLineValue = this[ Key ];

			// if not exists create one
			if ( pLineValue == null )
				pLineValue = new cLineValue( Key, ( byte ) LineValueType.SINGLE );

			pLineValue.Clear();
			pLineValue.Set( ref Value );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetMultiValue
		public	void					SetMultiValue( string Key, cValue[] vValues )
		{
			cLineValue pLineValue = this[ Key ];

			// if not exists create one
			if ( pLineValue == null )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cMultiValue multivalue = new cMultiValue( ref vValues );
			pLineValue.Set( ref multivalue );
		}


		//////////////////////////////////////////////////////////////////////////
		// Set<T>
		public	void					Set<T>( string Key, T Value )
		{
			SetValue( Key, new cValue( Value ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec2
		public	void			SetVec2( string Key, Vector2 Vec )
		{
			cLineValue pLineValue = this[ Key ];

			// if not exists create one
			if ( pLineValue == null )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[2] { new cValue( Vec.x ), new cValue( Vec.y ) };
			cMultiValue multivalue = new cMultiValue( ref vValues );
			pLineValue.Set( ref multivalue );

		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec3
		public	void			SetVec3( string Key, Vector3 Vec ) {

			cLineValue pLineValue = this[ Key ];

			// if not exists create one
			if ( pLineValue == null )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ) };
			cMultiValue multivalue = new cMultiValue( ref vValues );
			pLineValue.Set( ref multivalue );

		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void			SetVec4( string Key, Vector4 Vec ) {

			cLineValue pLineValue = this[ Key ];

			// if not exists create one
			if ( pLineValue == null )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ), new cValue( Vec.w ) };
			cMultiValue multivalue = new cMultiValue( ref vValues );
			pLineValue.Set( ref multivalue );

		}

	};


}