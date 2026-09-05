/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// File header following the sample headers
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 130)]
	internal struct ModFileHeader
	{
		public uint8be NumOrders;
		public uint8be RestartPos;		// Tempo (early SoundTracker) or restart position (only PC trackers?)
		public Array128 OrderList;
	}
}
