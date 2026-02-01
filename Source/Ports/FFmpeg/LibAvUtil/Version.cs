/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Version
	{
		private const int Major = 60;
		private const int Minor = 13;
		private const int Micro = 100;

		/// <summary></summary>
		public const int Version_Int = (Major << 16) | (Minor << 8) | Micro;
	}
}
