

using UnityEngine;


namespace Database
{

	public partial class Section : ISection
	{
	
		//////////////////////////////////////////////////////////////////////////
		// SetValue
		public	void					SetValue( string Key, Value Value )
		{
			LineValue pLineValue = null;

			// if not exists create one
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ( byte ) ELineValueType.SINGLE );
			}

			pLineValue.Clear();
			pLineValue.Set(Value);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetMultiValue
		public	void					SetMultiValue( string Key, Value[] vValues )
		{
			LineValue pLineValue = null;

			// if not exists create one
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ELineValueType.MULTI );
			}

			pLineValue.Clear();
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// Set<T>
		public	void					Set<T>( string Key, T Value )
		{
			SetValue( Key, new Value( Value ) );
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec2
		public	void			SetVec2( string Key, Vector2 Vec )
		{
			// if not exists create one
			LineValue pLineValue = null;
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ELineValueType.MULTI );
			}

			pLineValue.Clear();
			Value[] vValues = new Value[2] { new Value(Vec.x), new Value(Vec.y) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec3
		public	void			SetVec3( string Key, Vector3 Vec )
		{
			// if not exists create one
			LineValue pLineValue = null;
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ELineValueType.MULTI );
			}

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(Vec.x), new Value(Vec.y), new Value(Vec.z) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void			SetVec4( string Key, Vector4 Vec )
		{
			// if not exists create one
			LineValue pLineValue = null;
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ELineValueType.MULTI );
			}

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(Vec.x), new Value(Vec.y), new Value(Vec.z), new Value(Vec.w) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}


		//////////////////////////////////////////////////////////////////////////
		// SetVec4
		public	void			SetColor( string Key, Color color )
		{
			// if not exists create one
			LineValue pLineValue = null;
			if (!TryGetLineValue( Key, ref pLineValue ))
			{
				pLineValue = new LineValue( Key, ELineValueType.MULTI );
			}

			pLineValue.Clear();
			Value[] vValues = new Value[] { new Value(color.r), new Value(color.g), new Value(color.b), new Value(color.a) };
			MultiValue multivalue = new MultiValue(vValues);
			pLineValue.Set(multivalue);
			m_Linevalues.Add(pLineValue);
		}
	};
}