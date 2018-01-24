
using System.Collections.Generic;

public class cMultiValue {

	private List< cValue >	vValues = new List<cValue>();

	public int				Size {
		get {
			return vValues.Count;
		}
	}




	public cMultiValue() { vValues = new List<cValue>(); }

	public cMultiValue( List < cValue > vValues ) {

//		vValues = new List<cValue>(vValues);
		this.vValues = vValues;

	}

	public void Destroy() {

		vValues.Clear();

	}


	// Indexer behaviour
	public cValue this[ int Index ] {

		get {
			if ( ( vValues != null ) && Index < vValues.Count )
				return vValues[ Index ];
			return null;
		}

	}

	/////////////////////////////////////////////////////////
	public void		Add( cValue pValue ) {

		vValues.Add( pValue );

	}

	/////////////////////////////////////////////////////////
	public void		Set( List< cValue > vValues )	{

		this.vValues = vValues;

	}

	/////////////////////////////////////////////////////////
	public cValue	At( int Index ) {

		if ( Index < vValues.Count )
				return vValues[ Index ];
		return null;

	}
	
}
