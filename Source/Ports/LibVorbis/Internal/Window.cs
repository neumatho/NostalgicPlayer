/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Window
	{
		private static readonly c_float[][] vwin =
		[
			WindowTables.VWin64,
			WindowTables.VWin128,
			WindowTables.VWin256,
			WindowTables.VWin512,
			WindowTables.VWin1024,
			WindowTables.VWin2048,
			WindowTables.VWin4096,
			WindowTables.VWin8192
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_float[] Vorbis_Window_Get(c_int n)
		{
			return vwin[n];
		}
	}
}
