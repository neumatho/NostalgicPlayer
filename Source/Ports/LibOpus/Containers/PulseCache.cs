/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class PulseCache
	{
		public c_int size;
		public CPointer<opus_int16> index;
		public CPointer<byte> bits;
		public CPointer<byte> caps;
	}
}
