

using System.Collections.Generic;
using UnityEngine;

namespace Database
{
	public partial class Section
	{
		//////////////////////////////////////////////////////////////////////////
		// bAs<T>
		public	bool					bAs<T>( string Key, ref T Out )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.MULTI )
				{
					Value[] values = pLineValue.MultiValue.ValueList;
					System.Type requestedType = typeof(T);
					if (System.Array.TrueForAll(values, (Value value) => value.GetType().IsEquivalentTo(requestedType)))
					{
						Out = System.Array.ConvertAll(values, (s) => s.As<T>());
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == ELineValueType.SINGLE )
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
		public	bool					bAsMultiValue( string Key, int Index, out Value Out )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				MultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
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
			LineValue pLineValue = null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				MultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					Value pValue1 = pMultiValue[ 0 ], pValue2 = pMultiValue[ 1 ];
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
			LineValue pLineValue		= null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				MultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					float x = pMultiValue[ 0 ], y = pMultiValue[ 1 ], z = pMultiValue[ 2 ];
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
			LineValue pLineValue		= null;
			if (this.bGetLineValue( Key, ref pLineValue ) )
			{
				MultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					float x = pMultiValue[0], y = pMultiValue[1], z = pMultiValue[2], w = pMultiValue[3];
					Out = new Vector4( x:x, y:y, z:z, w:w );
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
			if (this.bAsVec4(Key, ref refVec, Vector4.zero))
			{
				float r = refVec[0], g = refVec[1], b = refVec[2], a = refVec[3];
				Out = new Color( r:r, g:g, b:b, a:a );
			}
			Out = Default;
			return false;
		}
	
	};

}