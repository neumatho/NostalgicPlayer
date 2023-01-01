/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// FLAC subframe structure
	/// </summary>
	internal class Flac__SubFrame
	{
		public Flac__SubFrameType Type;
		public ISubFrame Data;
		public uint32_t Wasted_Bits;
	}
}
