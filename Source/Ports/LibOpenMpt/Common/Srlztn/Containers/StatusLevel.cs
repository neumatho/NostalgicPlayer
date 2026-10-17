/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum StatusLevel : uint8
	{
		Failure = 0x2,
		Note = 0x1,
		None = 0x0
	}
}
