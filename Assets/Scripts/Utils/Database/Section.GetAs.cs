

using System.Collections.Generic;
using UnityEngine;

namespace Database {

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
		// bAs<T>
		public	bool					bAs<T>( string Key, ref T[] Out )
		{
			cLineValue pLineValue = null;
			if ( ( pLineValue = this[ Key ] ) != null )
			{
				if ( pLineValue.Type == LineValueType.MULTI )
				{
					List<cValue> values = pLineValue.MultiValue.ValueArray;
					bool bAreValidValues = true;
					values.ForEach( ( cValue value ) => bAreValidValues &= typeof(T) == value.GetType() );
					if ( bAreValidValues )
					{
						Out = values.ConvertAll( ( s ) => s.As<T>() ).ToArray();
						return true;
					}
				}
			}
			Out = null;
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
					Out = pMultiValue[ Index - 1 ];
					return true;
				}
			}
			Out = null;
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	bool					bAsVec2( string Key, ref Vector2 Out, Vector2? Default )
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
			Out = Default.GetValueOrDefault();
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	bool					bAsVec3( string Key, ref Vector3 Out, Vector3? Default )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue		= pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					float x = pMultiValue[ 0 ];
					float y = pMultiValue[ 1 ];
					float z = pMultiValue[ 2 ];

					Out = new Vector3( x, y, z );
					return true;
				}
			}
			Out = Default.GetValueOrDefault();
			return false;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	bool					bAsVec4( string Key, ref Vector4 Out, Vector4? Default )
		{
			cLineValue pLineValue		= this[ Key ];
			if ( pLineValue != null )
			{
				cMultiValue pMultiValue		= pLineValue.MultiValue;
				if ( pMultiValue != null )
				{
					float x = pMultiValue[ 0 ];
					float y = pMultiValue[ 1 ];
					float z = pMultiValue[ 2 ];
					float w = pMultiValue[ 3 ];

					Out = new Vector4( x, y, z, w );
					return true;
				}
			}
			Out = Default.GetValueOrDefault();
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
				Out = Default;
			}
			return result;
		}
	
	};

}