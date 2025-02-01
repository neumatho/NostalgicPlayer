/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Holds data for IFF sample format
	/// </summary>
	public class IffSample
	{
		// VHDR information
		public uint OneShotHiSamples;
		public uint RepeatHiSamples;
		public uint SamplesPerHiCycle;
		public ushort SamplesPerSec;
		public byte Octaves;
		public uint Volume;

		public sbyte[] SampleData;
	}
}
