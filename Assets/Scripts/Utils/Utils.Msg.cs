
namespace Utils {

	public static class Msg {

		public static void MSGDBG( string format, params object[] args )
		{
			UnityEngine.Debug.Log( "DBG: " + global::System.String.Format( format, args ) );
		}


		public static void MSGCRT( string format, params object[] args )
		{
			UnityEngine.Debug.Log( "CRT:	 " + global::System.String.Format( format, args ) );
		}
	}

}
