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
		public int MasterVolumeFadeVersion;		// -1 = None
		public bool SetDmaInSampleHandlers;
		public bool EnableCounter;
		public bool EnableSampleEffects;
		public bool EnableFinalVolumeSlide;
		public bool EnableVolumeFade;
		public bool EnablePortamento;
		public bool CheckForTicks;
		public bool ExtraTickArg;
		public bool Uses9xTrackEffects;
		public bool UsesCxTrackEffects;

		public byte MaxTrackValue;
		public bool EnableC0TrackLoop;
		public bool EnableF0TrackLoop;

		public byte MaxSampleMappingValue;
		public int GetSampleMappingVersion;
		public int SetSampleMappingVersion;
	}
}
