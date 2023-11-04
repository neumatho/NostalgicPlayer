/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader
{
	/// <summary>
	/// Test name flags
	/// </summary>
	[Flags]
	internal enum Test_Name
	{
		None = 0,
		Ignore_After_0 = 0x0001,
		Ignore_After_Cr = 0x0002
	}
}
