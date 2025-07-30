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
		public uint OneShotHiSamples { get; set; }
		/// <summary></summary>
		public uint RepeatHiSamples { get; set; }
		/// <summary></summary>
		public uint SamplesPerHiCycle { get; set; }
		/// <summary></summary>
		public ushort SamplesPerSec { get; set; }
		/// <summary></summary>
		public byte Octaves { get; set; }
		/// <summary></summary>
		public uint Volume { get; set; }

		/// <summary></summary>
		public sbyte[] SampleData { get; set; }
	}
}
