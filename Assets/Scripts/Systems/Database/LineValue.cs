
using UnityEngine;

namespace DatabaseCore
{
	//* TYPES */
	[System.Serializable]
	public enum ELineValueType : byte	{ SINGLE, MULTI };

	[System.Serializable]
	public class LineValue
	{
		[SerializeField]
		private		string						m_Key				= string.Empty;
		[SerializeField]
		private		string						m_RawValue			= string.Empty;

		[SerializeField]
		private		ELineValueType				m_Type				= 0;

		[SerializeField]
		private		MultiValue					m_MultiValue		= null;
		[SerializeField]
		private		Value						m_SingleValue		= null;


		public		ELineValueType				Type				=> m_Type;
		public		string						Key					=> m_Key;
		public		string						RawValue			=> m_RawValue;
		public		Value						Value				=> m_SingleValue;
		public		MultiValue					MultiValue			=> m_MultiValue;


		/////////////////////////////////////////////////////////////////////////////////
		public		bool						HasKey(in string InKey)	=> (m_Key == InKey);

		/////////////////////////////////////////////////////////////////////////////////
		public LineValue(in string InKey, in ELineValueType InType)
		{
			m_Type = InType;
			m_Key = InKey;
			m_RawValue = InKey;
		}

		/////////////////////////////////////////////////////////////////////////////////
		public LineValue(in LineValue InToClone) : this(InToClone.m_Key, InToClone.m_RawValue)
		{ }

		/////////////////////////////////////////////////////////////////////////////////
		// CONSTRUCTOR
		public LineValue(in string InKey, in string InLine)
		{
			m_Key = InKey;
			m_RawValue = ((InLine.Length > 0) ? InLine : "");

			if (InLine.Contains(",")) // Supposing is a MultiVal string
			{
				m_Type = ELineValueType.MULTI;
				Value[] values = Utils.RecognizeValues(InLine);
				if (values.Length >= 0)
				{
					m_MultiValue = new MultiValue(values);
				}
			}
			else // Single value
			{
				m_Type = ELineValueType.SINGLE;
				Value value = Utils.RecognizeValue(InLine);
				if (value == null)
				{
					UnityEngine.Debug.LogError($"{nameof(LineValue)}.ctor: for key {InKey} value type is undefined");
				}
				else
				{
					m_SingleValue = value;
				}
			}
		}

		/////////////////////////////////////////////////////////////////////////////////
		public bool GetAsSingle(out Value OutResult)
		{
			if (m_Type == ELineValueType.SINGLE)
			{
				OutResult = m_SingleValue;
				return true;
			}

			OutResult = null;
			return false;
		}

		/////////////////////////////////////////////////////////////////////////////////
		public bool GetAsMulti(out MultiValue OutResult)
		{
			if (m_Type == ELineValueType.MULTI)
			{
				OutResult = m_MultiValue;
				return true;
			}

			OutResult = null;
			return false;
		}

		/////////////////////////////////////////////////////////////////////////////////
		public void Destroy() => Clear();

		/////////////////////////////////////////////////////////////////////////////////
		public void Clear()
		{
			m_SingleValue = null;
			m_MultiValue = null;
		}

		/////////////////////////////////////////////////////////////////////////////////
		public LineValue Set(in Value InNewValue)
		{
			m_SingleValue = InNewValue;
			m_MultiValue = null;
			m_Type = ELineValueType.SINGLE;
			return this;
		}

		/////////////////////////////////////////////////////////////////////////////////
		public LineValue Set(in MultiValue InNewValue)
		{
			m_SingleValue = null;
			m_MultiValue = InNewValue;
			m_Type = ELineValueType.MULTI;
			return this;
		}
	}
}
