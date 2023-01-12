
using UnityEngine;


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ReadOnlyAttribute : PropertyAttribute
{}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ObsoleteInspectorAttribute : PropertyAttribute
{}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class EnumBitFieldAttribute : PropertyAttribute
{
	public readonly System.Type EnumType = null;

	public EnumBitFieldAttribute(System.Type InEnumType)
	{
		EnumType = InEnumType;
	}
}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////

public enum EOperators
{
	GREATER,
	GREATER_OR_EQUAL,
	EQUAL,
	NOT_EQUAL,
	LESS_OR_EQUAL,
	LESS
}

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ConditionalPropertyAttribute : PropertyAttribute
{
	public readonly string FieldName = string.Empty;
	public readonly EOperators Operator = EOperators.EQUAL;
	public readonly bool BoolValue = false;
	public readonly int IntValue = 0;
	public readonly float FloatValue = 0f;
	public readonly string StringValue = string.Empty;
	public readonly bool IsOtherFieldName = false;

	private ConditionalPropertyAttribute(string InFieldName, EOperators InOperator)
	{
		FieldName = InFieldName;
		Operator = InOperator;
	}

	public ConditionalPropertyAttribute(string InFieldName, EOperators InOperator, bool InValue) : this(InFieldName, InOperator)
	{
		BoolValue = InValue;
	}

	public ConditionalPropertyAttribute(string InFieldName, EOperators InOperator, int InValue) : this(InFieldName, InOperator)
	{
		IntValue = InValue;
	}

	public ConditionalPropertyAttribute(string InFieldName, EOperators InOperator, float InValue) : this(InFieldName, InOperator)
	{
		FloatValue = InValue;
	}

	public ConditionalPropertyAttribute(string InFieldName, EOperators InOperator, string InValue, bool IsOtherFieldName = false) : this(InFieldName, InOperator)
	{
		StringValue = InValue;
		this.IsOtherFieldName = IsOtherFieldName;
	}
}
