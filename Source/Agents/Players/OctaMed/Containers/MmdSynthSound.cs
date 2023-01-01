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
		public byte Decay;			// Only used in separate instruments
		public ushort Rpt;			// -""-
		public ushort RptLen;		// -""-
		public ushort VolTblLen;
		public ushort WfTblLen;
		public byte VolSpeed;
		public byte WfSpeed;
		public ushort NumWfs;		// Number of waveforms
	}
}
