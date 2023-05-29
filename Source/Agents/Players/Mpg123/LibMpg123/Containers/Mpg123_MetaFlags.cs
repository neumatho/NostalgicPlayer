/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum Mpg123_MetaFlags
	{
		Id3 = 0x3,						// < 0011 There is some ID3 info. Also matches 0010 or New_Id3
		New_Id3 = 0x1,					// < 0001 There is ID3 info that changed since last call to mpg123_id3
		Icy = 0xc,						// < 1100 There is some ICY info. Also matches 0100 or New_Icy
		New_Icy = 0x4,					// < 0100 There is ICY info that changed since last call to mpg123_icy
	}
}
