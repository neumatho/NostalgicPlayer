/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Indicate in which state the voice is in
	/// </summary>
	internal enum VoiceStatus : byte
	{
		Silence = 0,
		Playing,
		Stopping
	}
}
