/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic20.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument
	{
		public short Number { get; set; }

		public ushort SampleLength { get; set; }
		public ushort RepeatStart { get; set; }
		public ushort RepeatLength { get; set; }
		public VolumeInfo[] VolumeTable { get; } = new VolumeInfo[5];
		public VibratoInfo[] VibratoTable { get; } = new VibratoInfo[5];
		public ushort PitchBend { get; set; }
		public bool IsSample { get; set; }
		public byte SampleNumber { get; set; }
		public byte[] Table { get; } = new byte[48];

		public sbyte[] SampleData { get; set; }
	}
}
