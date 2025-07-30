/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FutureComposer.Containers
{
	/// <summary>
	/// Voice sequence
	/// </summary>
	internal class VoiceSeq
	{
		public byte Pattern { get; set; }
		public sbyte Transpose { get; set; }
		public sbyte SoundTranspose { get; set; }
	}
}
