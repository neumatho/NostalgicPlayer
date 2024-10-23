/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The maximum number of channels in an Ogg Opus stream
		/// </summary>
		public const int Opus_Channel_Count_Max = 255;

		/// <summary>
		/// Indicates that the decoding callback did not decode anything, and that
		/// libopusfile should decode normally instead
		/// </summary>
		public const int Op_Dec_Use_Default = 6720;

		/// <summary>
		/// The maximum channel count for any mapping we'll actually decode
		/// </summary>
		internal const int Op_NChannels_Max = 8;
	}
}
