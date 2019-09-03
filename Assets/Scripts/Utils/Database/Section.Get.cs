
using System.Collections.Generic;

namespace Database {

	public partial class Section {


		//////////////////////////////////////////////////////////////////////////
		// ValueType
		public	global::System.Type		ValueType( string Key )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				if ( pLineValue.Type == LineValueType.SINGLE )
				{
					return pLineValue.Value.GetType();
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// GetRawValue
		public	string					GetRawValue( string Key, string Default = "" )
		{
			cLineValue pLineValue = null;
			return ( bGetLineValue( Key, ref pLineValue ) ) ? pLineValue.RawValue : Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// As<T>
		public	T						As<T>( string Key )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::As: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<T>();
			}
			return default( T );
		}


		//////////////////////////////////////////////////////////////////////////
		// AsBool
		public	bool					AsBool( string Key, bool Default = false )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::AsBool: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<bool>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsInt
		public	int						AsInt( string Key, int Default = 0 )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::AsInt: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<int>();
			}
			return Default;
		}

		//////////////////////////////////////////////////////////////////////////
		// AsInt ( UInt )
		public	uint					AsUInt( string Key, uint Default = 0u )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::AsUInt: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<uint>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsFloat
		public	float					AsFloat( string Key, float Default = 0.0f )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::AsUInt: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.ToFloat();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsString
		public	string					AsString( string Key, string Default = "" )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == LineValueType.SINGLE,
					"Database::Section::AsString: Line value for section " + GetName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<string>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue
		public	cValue					OfMultiValue( string Key, int Index )
		{
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) && Index > 0 )
			{
				cMultiValue pMultiValue = null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					return pMultiValue [Index - 1 ];
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2>
		public	void					AsMultiValue<T1,T2>( string Key, int Idx1, int Idx2, ref T1 t1, ref T2 t2 )
		{
			cLineValue pLineValue		= null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue = null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3>
		public	void					AsMultiValue<T1,T2,T3>( string Key, int Idx1, int Idx2, int Idx3, ref T1 t1, ref T2 t2, ref T3 t3 )
		{
			cLineValue pLineValue		= null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue = null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
					t3 = pMultiValue [Idx3 - 1].As<T3>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3,T4>
		public	void					AsMultiValue<T1,T2,T3,T4>( string Key, int Idx1, int Idx2, int Idx3, int Idx4, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4 )
		{
			cLineValue pLineValue		= null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue = null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					t1 = pMultiValue [Idx1 - 1].As<T1>();
					t2 = pMultiValue [Idx2 - 1].As<T2>();
					t3 = pMultiValue [Idx3 - 1].As<T3>();
					t4 = pMultiValue [Idx4 - 1].As<T4>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetMultiSize
		public	int						GetMultiSize( string Key )
		{
			cLineValue pLineValue = null; cMultiValue pMultiValue = null;
			return
				( 
					bGetLineValue( Key, ref pLineValue ) && 
					pLineValue.GetAsMulti( ref pMultiValue ) 
				) ? pMultiValue.Size : 0;
		}


		//////////////////////////////////////////////////////////////////////////
		// bGetMultiAsArray
		public	bool						bGetMultiAsArray<T>( string Key, ref T[] array )
		{
			bool bResult = false;

			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				// If is single value
				cValue value = null;
				if ( pLineValue.GetAsSingle( ref value ) )
				{
					array = new T[1] { value.As<T>() };
					bResult = true;
				}

				// If is multi value
				cMultiValue multiValue = null;
				if ( pLineValue.GetAsMulti( ref multiValue ) )
				{
					array = multiValue.ValueList
					.ConvertAll	// Get a list of converted cvalues to requested type
					(
						new System.Converter<cValue, T> ( ( cValue v ) => { return v.As<T>(); } )
					)
					.ToArray(); // return as array

					bResult = true;
				}	
			}
			
			return bResult;
		}






















		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	UnityEngine.Vector2				AsVec2( string Key, UnityEngine.Vector2? Default )
		{
			UnityEngine.Vector2 Out = Default.GetValueOrDefault();
			cLineValue pLineValue = null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					cValue pValue1				= pMultiValue[ 0 ];
					cValue pValue2				= pMultiValue[ 1 ];

					if ( ( pValue1 != null ) && ( pValue2 != null ) )
					{
						Out = new UnityEngine.Vector2( pValue1.ToFloat(), pValue2.ToFloat() );
					}
				}
			}
			return Out;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	UnityEngine.Vector3					AsVec3( string Key, UnityEngine.Vector3? Default )
		{
			UnityEngine.Vector3 Out = Default.GetValueOrDefault();
			cLineValue pLineValue		= null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					float x = pMultiValue[ 0 ];
					float y = pMultiValue[ 1 ];
					float z = pMultiValue[ 2 ];

					Out = new UnityEngine.Vector3( x, y, z );
				}
			}
			return Out;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	UnityEngine.Vector4					AsVec4( string Key, UnityEngine.Vector4? Default )
		{
			UnityEngine.Vector4 Out = Default.GetValueOrDefault();
			cLineValue pLineValue		= null;
			if ( bGetLineValue( Key, ref pLineValue ) )
			{
				cMultiValue pMultiValue	= null;
				if ( pLineValue.GetAsMulti( ref pMultiValue ) )
				{
					float x = pMultiValue[ 0 ];
					float y = pMultiValue[ 1 ];
					float z = pMultiValue[ 2 ];
					float w = pMultiValue[ 3 ];

					Out = new UnityEngine.Vector4( x, y, z, w );
				}
			}
			return Out;
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsColor
		public	UnityEngine.Color					AsColor( string Key, UnityEngine.Color? Default )
		{
			UnityEngine.Color Out = Default.GetValueOrDefault();
			UnityEngine.Vector4 refVec = UnityEngine.Vector4.zero;
			if ( bAsVec4( Key, ref refVec, UnityEngine.Vector4.zero ) == true )
			{
				Out = refVec;
			}
			return Out;
		}

	};

}