using System;
using System.Collections .Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Utils {

	public static class Msg {

		public static void MSGDBG( string format, params object[] args ) {
			Debug.Log( "DBG: " + global::System.String.Format( format, args ) );
		}


		public static void MSGCRT( string format, params object[] args ) {
			Debug.Log( "CRT:	 " + global::System.String.Format( format, args ) );
		}
	}

}
