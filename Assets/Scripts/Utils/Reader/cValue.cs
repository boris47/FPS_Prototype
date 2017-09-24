


public interface ICValue {

	bool	ToBool();
	int		ToInteger();
	float	ToFloat();
	string	ToString();

}


public class cValue : ICValue {

	private object		pValue;
	private ValueTypes	iType = ValueTypes.NONE;
	private	System.Type	iSysType;

	public	object	Value {
		get { return pValue; }
	}

	public ValueTypes	Type {
		get { return iType; }
	}

	public System.Type	SysType {
		get { return iSysType; }
	}



	public	cValue()		{}

	public	cValue( bool Val )		{ pValue = Val; iType = ValueTypes.BOOLEAN;	iSysType = typeof( bool ); }
	public	cValue( int Val )		{ pValue = Val; iType = ValueTypes.INTEGER;	iSysType = typeof( int ); }
	public	cValue( float Val )		{ pValue = Val; iType = ValueTypes.FLOAT;	iSysType = typeof( float );}
	public	cValue( string Val )	{ pValue = Val; iType = ValueTypes.STRING;	iSysType = typeof( string ); }

	/*
	public cValue SetValue( object o ) {
		pValue = o;
		System.Type objType = o.GetType();
		if ( objType == typeof( bool   ) ) iType = ValueTypes.BOOLEAN;
		if ( objType == typeof( int    ) ) iType = ValueTypes.INTEGER;
		if ( objType == typeof( float  ) ) iType = ValueTypes.FLOAT;
		if ( objType == typeof( string ) ) iType = ValueTypes.STRING;

		if ( objType == typeof( cValue ) ) {
			iType = ( o as cValue ).iType;
			pValue = ( o as cValue ).pValue;
			iSysType = ( o as cValue ).iSysType;
		}
		else
			iSysType = objType;
		return this;
	}*/
	

	public T As<T>() {

		return (T) pValue;

	}

	///////////////////////////////////////////////////////////////////////////////

	public bool ToBool() {
		if ( iType == ValueTypes.BOOLEAN ) return ( bool ) pValue;
		return false;
	}

	public int ToInteger() {
		if ( iType == ValueTypes.INTEGER ) return ( int ) pValue;
		return -1;
	}

	public float ToFloat() {
		if ( iType == ValueTypes.FLOAT ) return ( float ) pValue;
		return 0.0f;
	}

	public override string ToString() {
		if ( iType == ValueTypes.STRING ) return ( string ) pValue;
		return "";
	}
	
	///////////////////////////////////////////////////////////////////////////////

	public static implicit operator bool( cValue v ) {
		return v.ToBool();
	}

	public static implicit operator int( cValue v ) {
		return v.ToInteger();
	}
	
	public static implicit operator float( cValue v ) {
		return v.ToFloat();
	}
	

	public static implicit operator string( cValue v ) {
		return v.ToString();
	}

	///////////////////////////////////////////////////////////////////////////////

	public static implicit operator cValue( bool b ) {
		return new cValue( b );
	}

	public static implicit operator cValue( int i ) {
		return new cValue( i );
	}

	public static implicit operator cValue( float f ) {
		return new cValue( f );
	}

	public static implicit operator cValue( string s ) {
		return new cValue( s );
	}

	///////////////////////////////////////////////////////////////////////////////

}
