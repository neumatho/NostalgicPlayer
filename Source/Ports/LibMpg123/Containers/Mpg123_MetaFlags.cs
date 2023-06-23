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
	public enum Mpg123_MetaFlags
	{
		/// <summary>
		/// 0011 There is some ID3 info. Also matches 0010 or New_Id3
		/// </summary>
		Id3 = 0x3,
		/// <summary>
		/// 0001 There is ID3 info that changed since last call to mpg123_id3
		/// </summary>
		New_Id3 = 0x1,
		/// <summary>
		/// 1100 There is some ICY info. Also matches 0100 or New_Icy
		/// </summary>
		Icy = 0xc,
		/// <summary>
		/// 0100 There is ICY info that changed since last call to mpg123_icy
		/// </summary>
		New_Icy = 0x4,
	}
}
