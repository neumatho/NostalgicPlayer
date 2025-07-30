/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Hold static information about the loaded module
	/// </summary>
	internal class ModuleInfo
	{
		public byte GlobalVolume { get; set; }
		public ushort TempoIndex { get; set; }
		public ushort Transpose { get; set; }
		public ushort Tune { get; set; }

		public ushort TimeSigNumerator { get; set; }
		public ushort TimeSigDenominator { get; set; }

		public ushort[] TrackVolumes { get; set; }
		public ulong[] TracksEnabled { get; set; }
		public Track[] Tracks { get; set; }

		public int[] InstrumentMapper { get; set; }
	}
}
