/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// We leave some space between them for extensions (drop some
	/// keyframes for intra-only or drop just some bidir frames)
	/// </summary>
	public enum AvDiscard
	{
		/// <summary>
		/// Discard nothing
		/// </summary>
		None = -16,

		/// <summary>
		/// Discard useless packets like 0 size packets in avi
		/// </summary>
		Default = 0,

		/// <summary>
		/// Discard all non reference
		/// </summary>
		NonRef = 8,

		/// <summary>
		/// Discard all bidirectional frames
		/// </summary>
		Bidir = 16,

		/// <summary>
		/// Discard all non intra frames
		/// </summary>
		NonIntra = 24,

		/// <summary>
		/// Discard all frames except keyframes
		/// </summary>
		NonKey = 32,

		/// <summary>
		/// Discard all
		/// </summary>
		All = 48
	}
}
