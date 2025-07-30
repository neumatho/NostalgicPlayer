/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.BenDaglish.Containers
{
	/// <summary>
	/// Holds which features are enabled or not
	/// </summary>
	internal class Features
	{
		public int MasterVolumeFadeVersion { get; set; }		// -1 = None
		public bool SetDmaInSampleHandlers { get; set; }
		public bool EnableCounter { get; set; }
		public bool EnableSampleEffects { get; set; }
		public bool EnableFinalVolumeSlide { get; set; }
		public bool EnableVolumeFade { get; set; }
		public bool EnablePortamento { get; set; }
		public bool CheckForTicks { get; set; }
		public bool ExtraTickArg { get; set; }
		public bool Uses9xTrackEffects { get; set; }
		public bool UsesCxTrackEffects { get; set; }

		public byte MaxTrackValue { get; set; }
		public bool EnableC0TrackLoop { get; set; }
		public bool EnableF0TrackLoop { get; set; }

		public byte MaxSampleMappingValue { get; set; }
		public int GetSampleMappingVersion { get; set; }
		public int SetSampleMappingVersion { get; set; }
	}
}
