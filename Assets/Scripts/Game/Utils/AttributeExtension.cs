﻿
using UnityEngine;


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
////////////////   READ ONLY ATTRIBUTE   ////////////////
/////////////////       [ReadOnly]      /////////////////
public class ReadOnlyAttribute : PropertyAttribute
{}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
/////////////////   OBSOLE ATTRIBUTE   //////////////////
//////////////     [ObsoleteInspector]     //////////////

public class ObsoleteInspectorAttribute : PropertyAttribute
{}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
/////////////////   OBSOLE ATTRIBUTE   //////////////////
//////////////     [ObsoleteInspector]     //////////////
public class EnumBitFieldAttribute : PropertyAttribute
{
	public readonly System.Type EnumType;

	public EnumBitFieldAttribute(System.Type InEnumType)
	{
		EnumType = InEnumType;
	}
}


/////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////
/////////////////   OBSOLE ATTRIBUTE   //////////////////
//////////////     [ObsoleteInspector]     //////////////
public class ConditionalPropertyAttribute : PropertyAttribute
{
	public readonly string ConditionalFieldName;

	public ConditionalPropertyAttribute(string InConditionalFieldName)
	{
		this.ConditionalFieldName = InConditionalFieldName;
	}
}
