/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// Helper class to enable test
	/// </summary>
	public static class UnitTest
	{
		/// <summary></summary>
		public delegate uint32_t Random_Delegate();

		private static bool unitTestEnabled = false;
		private static Random_Delegate randomMethod = null;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void EnableUnitTest()
		{
			unitTestEnabled = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool IsUnitTestEnabled()
		{
			return unitTestEnabled;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void SetRandomMethod(Random_Delegate method)
		{
			randomMethod = method;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Random_Delegate GetRandomMethod()
		{
			return randomMethod;
		}
	}
}
