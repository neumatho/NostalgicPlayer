/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers
{
	/// <summary>
	/// Synth sound structure
	/// </summary>
	internal class MmdSynthSound
	{
		public byte Decay { get; set; }			// Only used in separate instruments
		public ushort Rpt { get; set; }			// -""-
		public ushort RptLen { get; set; }		// -""-
		public ushort VolTblLen { get; set; }
		public ushort WfTblLen { get; set; }
		public byte VolSpeed { get; set; }
		public byte WfSpeed { get; set; }
		public ushort NumWfs { get; set; }		// Number of waveforms
	}
}
