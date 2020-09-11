

namespace Database {

	public partial class Section
	{
		//////////////////////////////////////////////////////////////////////////
		// ValueType
		public	System.Type		ValueType( string Key )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				if (pLineValue.Type == ELineValueType.SINGLE)
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
			LineValue pLineValue = null;
			return (this.bGetLineValue( Key, ref pLineValue ) ) ? pLineValue.RawValue : Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// As<T>
		public	T						As<T>( string Key )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::As: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<T>();
			}
			return default(T);
		}


		//////////////////////////////////////////////////////////////////////////
		// AsBool
		public	bool					AsBool( string Key, bool Default = false )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::AsBool: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<bool>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsInt
		public	int						AsInt( string Key, int Default = 0 )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::AsInt: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<int>();
			}
			return Default;
		}

		//////////////////////////////////////////////////////////////////////////
		// AsInt ( UInt )
		public	uint					AsUInt( string Key, uint Default = 0u )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::AsUInt: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<uint>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsFloat
		public	float					AsFloat( string Key, float Default = 0.0f )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::AsUInt: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.ToFloat();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsString
		public	string					AsString( string Key, string Default = "" )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				UnityEngine.Assertions.Assert.IsTrue
				(
					pLineValue.Type == ELineValueType.SINGLE,
					"Database::Section::AsString: Line value for section " + this.GetSectionName() + " at key " + Key + " is not of single type"
				);

				return pLineValue.Value.As<string>();
			}
			return Default;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue
		public	Value					OfMultiValue( string Key, int Index )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue) && Index > 0)
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					return pMultiValue[Index - 1];
				}
			}
			return null;
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2>
		public	void					AsMultiValue<T1,T2>( string Key, int Idx1, int Idx2, ref T1 t1, ref T2 t2 )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					t1 = pMultiValue[Idx1 - 1].As<T1>();
					t2 = pMultiValue[Idx2 - 1].As<T2>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3>
		public	void					AsMultiValue<T1,T2,T3>( string Key, int Idx1, int Idx2, int Idx3, ref T1 t1, ref T2 t2, ref T3 t3 )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					t1 = pMultiValue[Idx1 - 1].As<T1>();
					t2 = pMultiValue[Idx2 - 1].As<T2>();
					t3 = pMultiValue[Idx3 - 1].As<T3>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// AsMultiValue<T1,T2,T3,T4>
		public	void					AsMultiValue<T1,T2,T3,T4>( string Key, int Idx1, int Idx2, int Idx3, int Idx4, ref T1 t1, ref T2 t2, ref T3 t3, ref T4 t4 )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					t1 = pMultiValue[Idx1 - 1].As<T1>();
					t2 = pMultiValue[Idx2 - 1].As<T2>();
					t3 = pMultiValue[Idx3 - 1].As<T3>();
					t4 = pMultiValue[Idx4 - 1].As<T4>();
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////
		// GetMultiSize
		public	int						GetMultiSize( string Key )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					return pMultiValue.Size;
				}
			}
			return 0;
		}


		//////////////////////////////////////////////////////////////////////////
		// bGetMultiAsArray
		public	bool						bGetMultiAsArray<T>( string Key, ref T[] array )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				Value value = null;
				if (pLineValue.GetAsSingle(ref value))
				{
					array = new T[1] { value.As<T>() };
					return true;
				}

				MultiValue multiValue = null;
				if (pLineValue.GetAsMulti(ref multiValue))
				{
					array = System.Array.ConvertAll(multiValue.ValueList, (Value v) => v.As<T>());
					return true;
				}
			}
			return false;
		}






















		//////////////////////////////////////////////////////////////////////////
		// bAsVec2
		public	UnityEngine.Vector2				AsVec2( string Key, UnityEngine.Vector2? Default )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					Value pValue1 = pMultiValue[0], pValue2 = pMultiValue[1];
					if ((pValue1 != null) && (pValue2 != null))
					{
						return new UnityEngine.Vector2(pValue1.ToFloat(), pValue2.ToFloat());
					}
				}
			}
			return Default.GetValueOrDefault();
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec3
		public	UnityEngine.Vector3					AsVec3( string Key, UnityEngine.Vector3? Default )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					float x = pMultiValue[0], y = pMultiValue[1], z = pMultiValue[2];
					return new UnityEngine.Vector3(x, y, z);
				}
			}
			return Default.GetValueOrDefault();
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsVec4
		public	UnityEngine.Vector4					AsVec4( string Key, UnityEngine.Vector4? Default )
		{
			LineValue pLineValue = null;
			if (this.bGetLineValue(Key, ref pLineValue))
			{
				MultiValue pMultiValue = null;
				if (pLineValue.GetAsMulti(ref pMultiValue))
				{
					float x = pMultiValue[0], y = pMultiValue[1], z = pMultiValue[2], w = pMultiValue[3];
					return new UnityEngine.Vector4(x, y, z, w);
				}
			}
			return Default.GetValueOrDefault();
		}


		//////////////////////////////////////////////////////////////////////////
		// bAsColor
		public	UnityEngine.Color					AsColor( string Key, UnityEngine.Color? Default )
		{
			UnityEngine.Vector4 refVec = UnityEngine.Vector4.zero;
			if (this.bAsVec4(Key, ref refVec, UnityEngine.Vector4.zero))
			{
				return refVec;
			}
			return Default.GetValueOrDefault();
		}

	};

}