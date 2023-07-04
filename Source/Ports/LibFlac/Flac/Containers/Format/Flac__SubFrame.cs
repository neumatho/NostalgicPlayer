/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// FLAC subframe structure
	/// </summary>
	public class Flac__SubFrame
	{
		/// <summary></summary>
		public Flac__SubFrameType Type;
		/// <summary></summary>
		public ISubFrame Data;
		/// <summary></summary>
		public uint32_t Wasted_Bits;
	}
}
