/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum ReaderFlags
	{
		Fd_Opened = 0x1,
		Id3Tag = 0x2,
		Seekable = 0x4,
		Buffered = 0x8,
		NonBlock = 0x20,
		HandleIo = 0x40
	}
}
