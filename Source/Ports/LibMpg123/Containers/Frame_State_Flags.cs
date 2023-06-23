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
	internal enum Frame_State_Flags
	{
		Accurate = 0x1,					// < 0001 Positions are considered accurate
		Frankenstein = 0x2,				// < 0010 This stream is concatenated
		Fresh_Decoder = 0x4,			// < 0100 Decoder is fleshly initialized
		Decoder_Live = 0x8,				// < 1000 Decoder can be used
	}
}
