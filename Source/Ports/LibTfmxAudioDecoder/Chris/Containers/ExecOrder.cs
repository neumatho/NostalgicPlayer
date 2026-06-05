/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
{
	/// <summary>
	/// SEQ: Sequencer
	/// MAC: Macro processing
	/// MOD: Modulation/effects processing
	/// </summary>
	internal enum ExecOrder
	{
		Seq_Mod_Mac,
		Mod_Mac_Seq,
		Mac_Mod_Seq
	}
}
