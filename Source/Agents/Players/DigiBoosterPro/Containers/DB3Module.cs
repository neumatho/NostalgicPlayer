/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Complete module
	/// </summary>
	internal class DB3Module
	{
		public string Name { get; set; }
		public uint16_t CreatorVersion { get; set; }
		public uint16_t CreatorRevision { get; set; }
		public uint16_t NumberOfInstruments { get; set; }
		public uint16_t NumberOfSamples { get; set; }
		public uint16_t NumberOfSongs { get; set; }
		public uint16_t NumberOfPatterns { get; set; }
		public uint16_t NumberOfTracks { get; set; }
		public uint16_t NumberOfVolumeEnvelopes { get; set; }
		public uint16_t NumberOfPanningEnvelopes { get; set; }
		public DB3ModuleInstrument[] Instruments { get; set; }
		public DB3ModuleSample[] Samples { get; set; }
		public DB3ModuleSong[] Songs { get; set; }
		public DB3ModulePattern[] Patterns { get; set; }
		public DB3ModuleEnvelope[] VolumeEnvelopes { get; set; }
		public DB3ModuleEnvelope[] PanningEnvelopes { get; set; }
		public DB3GlobalDsp DspDefaults { get; set; }
	}
}
