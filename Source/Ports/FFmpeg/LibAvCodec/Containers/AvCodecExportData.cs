/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Exported side data.
	/// These flags can be passed in AVCodecContext.export_side_data before initialization
	/// </summary>
	[Flags]
	public enum AvCodecExportData
	{
		/// <summary>
		/// Export motion vectors through frame side data
		/// </summary>
		Mvs = 1 << 0,

		/// <summary>
		/// Export encoder Producer Reference Time through packet side data
		/// </summary>
		Prft = 1 << 1,

		/// <summary>
		/// Decoding only.
		/// Export the AVVideoEncParams structure through frame side data
		/// </summary>
		Video_Enc_Params = 1 << 2,

		/// <summary>
		/// Decoding only.
		/// Do not apply film grain, export it instead
		/// </summary>
		Film_Grain = 1 << 3,

		/// <summary>
		/// Decoding only.
		/// Do not apply picture enhancement layers, export them instead
		/// </summary>
		Enhancements = 1 << 4
	}
}
