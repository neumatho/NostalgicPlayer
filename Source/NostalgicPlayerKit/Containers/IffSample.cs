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
		//
		// VHDR information
		//

		/// <summary></summary>
		public uint OneShotHiSamples;
		/// <summary></summary>
		public uint RepeatHiSamples;
		/// <summary></summary>
		public uint SamplesPerHiCycle;
		/// <summary></summary>
		public ushort SamplesPerSec;
		/// <summary></summary>
		public byte Octaves;
		/// <summary></summary>
		public uint Volume;

		/// <summary></summary>
		public sbyte[] SampleData;
	}
}
