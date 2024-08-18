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
		public byte GlobalVolume;
		public ushort TempoIndex;
		public ushort Transpose;
		public ushort Tune;

		public ushort TimeSigNumerator;
		public ushort TimeSigDenominator;

		public ushort[] TrackVolumes;
		public ulong[] TracksEnabled;
		public Track[] Tracks;

		public int[] InstrumentMapper;
	}
}
