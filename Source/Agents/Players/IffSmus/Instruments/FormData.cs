/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Holds data for IFF sample format
	/// </summary>
	internal class FormData
	{
		// VHDR information
		public uint OneShotHiSamples;
		public uint RepeatHiSamples;
		public uint SamplesPerHiCycle;
		public ushort SamplesPerSec;
		public byte Octaves;
		public uint Volume;

		public ushort NumberOfHiOctavesToSkip;
		public sbyte[] SampleData;
	}
}
