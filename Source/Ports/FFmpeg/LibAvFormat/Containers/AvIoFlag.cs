/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// URL open modes.
	///
	/// The flags argument to avio_open must be one of the following
	/// constants, optionally ORed with other flags
	/// </summary>
	[Flags]
	public enum AvIoFlag
	{
		/// <summary>
		/// Read-only
		/// </summary>
		Read = 1,

		/// <summary>
		/// Write-only
		/// </summary>
		Write = 2,

		/// <summary>
		/// Read-write pseudo flag
		/// </summary>
		ReadWrite = Read | Write,

		/// <summary>
		/// Use non-blocking mode.
		/// If this flag is set, operations on the context will return
		/// AVERROR(EAGAIN) if they can not be performed immediately.
		/// If this flag is not set, operations on the context will never return
		/// AVERROR(EAGAIN).
		/// Note that this flag does not affect the opening/connecting of the
		/// context. Connecting a protocol will always block if necessary (e.g. on
		/// network protocols) but never hang (e.g. on busy devices).
		/// Warning: non-blocking protocols is work-in-progress; this flag may be
		/// silently ignored
		/// </summary>
		NonBlock = 8,

		/// <summary>
		/// Use direct mode.
		/// avio_read and avio_write should if possible be satisfied directly
		/// instead of going through a buffer, and avio_seek will always
		/// call the underlying seek function directly
		/// </summary>
		Direct = 0x8000
	}
}
