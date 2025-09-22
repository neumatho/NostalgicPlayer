/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// The different states the envelope processing can be in
	/// </summary>
	internal enum EnvelopeState : byte
	{
		Attack,
		Decay,
		Sustain,
		Release
	}
}
