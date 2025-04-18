/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Iff
{
	/// <summary>
	/// Quirks
	/// </summary>
	[Flags]
	internal enum Iff_Quirk_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Little_Endian = 0x01,

		/// <summary>
		/// 
		/// </summary>
		Full_Chunk_Size = 0x02,

		/// <summary>
		/// 
		/// </summary>
		Chunk_Align2 = 0x04,

		/// <summary>
		/// 
		/// </summary>
		Chunk_Align4 = 0x08,

		/// <summary>
		/// 
		/// </summary>
		Skip_Embedded = 0x10,

		/// <summary>
		/// 
		/// </summary>
		Chunk_Trunc4 = 0x20,
	}
}
