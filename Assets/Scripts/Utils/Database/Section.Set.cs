

using UnityEngine;


namespace Database {


	public partial class Section : ISection {
	
		//////////////////////////////////////////////////////////////////////////
		// SetValue
		public	void					SetValue( string Key, cValue Value )
		{
			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, ( byte ) LineValueType.SINGLE );

			pLineValue.Clear();
			pLineValue.Set( Value );
			m_Sections.Add( pLineValue );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetMultiValue
		public	void					SetMultiValue( string Key, cValue[] vValues )
		{
			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cMultiValue multivalue = new cMultiValue( vValues );
			pLineValue.Set( multivalue );
			m_Sections.Add( pLineValue );
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
			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[2] { new cValue( Vec.x ), new cValue( Vec.y ) };
			cMultiValue multivalue = new cMultiValue( vValues );
			pLineValue.Set( multivalue );
			m_Sections.Add( pLineValue );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec3
		public	void			SetVec3( string Key, Vector3 Vec ) {

			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ) };
			cMultiValue multivalue = new cMultiValue( vValues );
			pLineValue.Set( multivalue );
			m_Sections.Add( pLineValue );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void			SetVec4( string Key, Vector4 Vec ) {

			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[] { new cValue( Vec.x ), new cValue( Vec.y ), new cValue( Vec.z ), new cValue( Vec.w ) };
			cMultiValue multivalue = new cMultiValue( vValues );
			pLineValue.Set( multivalue );
			m_Sections.Add( pLineValue );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void			SetColor( string Key, Color color ) {

			cLineValue pLineValue = null;

			// if not exists create one
			if ( bGetLineValue( Key, ref pLineValue ) == false )
				pLineValue = new cLineValue( Key, LineValueType.MULTI );

			pLineValue.Clear();
			cValue[] vValues = new cValue[] { new cValue( color.r ), new cValue( color.g ), new cValue( color.b ), new cValue( color.a ) };
			cMultiValue multivalue = new cMultiValue( vValues );
			pLineValue.Set( multivalue );
			m_Sections.Add( pLineValue );
		}

	};


}