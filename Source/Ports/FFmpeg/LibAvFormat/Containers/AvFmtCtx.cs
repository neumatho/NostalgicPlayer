/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvFmtCtx
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Signal that no header is present
		/// (streams are added dynamically)
		/// </summary>
		NoHeader = 0x0001,

		/// <summary>
		/// signal that the stream is definitely
		/// seekable, and attempts to call the
		/// seek function will fail. For some
		/// network protocols (e.g. HLS), this can
		/// change dynamically at runtime
		/// </summary>
		Unseekable = 0x0002
	}
}
