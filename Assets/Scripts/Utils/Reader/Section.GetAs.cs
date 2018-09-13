

using UnityEngine;

namespace CFG_Reader {

	public partial class Section {


		//////////////////////////////////////////////////////////////////////////
		// bAs<T>
		public	bool					bAs<T>( string Key, ref T Out )
		{
			cLineValue pLineValue = null;
			if ( ( pLineValue = this[ Key ] ) != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.As<T>();
					return true;
				}
			}
			Out = default(T);
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsBool
		public	bool					bAsBool( string Key, ref bool Out, bool Default = false )
		{
			cLineValue pLineValue = null;
			if ( ( pLineValue = this[ Key ] ) != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToBool();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsInt
		public	bool					bAsInt( string Key, ref int Out, int Default = 0 )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToInteger();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsFloat
		public	bool					bAsFloat( string Key, ref float Out, float Default = 0.0f )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToFloat();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsString
		public	bool					bAsString( string Key, ref string Out, string Default = "" )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					Out = pLineValue.Value.ToString();
					return true;
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsMultiValue
		public	bool					bAsMultiValue( string Key, int Index, out cValue Out )
		{
			cLineValue pLineValue = this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue = pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					cValue pValue = pMultiValue[ Index - 1 ];
					if ( pValue != null )
					{
						Out = pValue;
						return true;
					}
				}
			}
			Out = null;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	bool					bAsVec2( string Key, ref Vector2 Out, Vector2 Default )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue		= pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					cValue pValue1				= pMultiValue[ 0 ];
					cValue pValue2				= pMultiValue[ 1 ];

					if ( ( pValue1 != null ) && ( pValue2 != null ) )
					{
						Out = new Vector2( pValue1.ToFloat(), pValue2.ToFloat() );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	bool					bAsVec3( string Key, ref Vector3 Out, Vector3 Default )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue		= pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					cValue pValue1				= pMultiValue[ 0 ];
					cValue pValue2				= pMultiValue[ 1 ];
					cValue pValue3				= pMultiValue[ 2 ];

					if ( ( pValue1 != null ) && ( pValue2 != null ) && ( pValue3 != null ) )
					{
						Out = new Vector3( pValue1.ToFloat(), pValue2.ToFloat(), pValue3.ToFloat() );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	bool					bAsVec4( string Key, ref Vector4 Out, Vector4 Default )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue		= pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					cValue pValue1				= pMultiValue[ 0 ];
					cValue pValue2				= pMultiValue[ 1 ];
					cValue pValue3				= pMultiValue[ 2 ];
					cValue pValue4				= pMultiValue[ 3 ];

					if ( ( pValue1 != null ) && ( pValue2 != null ) && ( pValue3 != null ) && ( pValue4 != null ) )
					{
						Out = new Vector4( pValue1.ToFloat(), pValue2.ToFloat(), pValue3.ToFloat(), pValue4.ToFloat() );
						return true;
					}
				}
			}
			Out = Default;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsColor
		public	bool					bAsColor( string Key, ref Color Out, Color Default )
		{
			Vector4 refVec = Vector4.zero;
			bool result = bAsVec4( Key, ref refVec, Vector4.zero );

			if ( result == true )
			{
				Out = new Color( refVec[0], refVec[1], refVec[2], refVec[3] );
			}
			else
			{
				Out = Color.clear;
			}
			return result;
		}
	
	};

}