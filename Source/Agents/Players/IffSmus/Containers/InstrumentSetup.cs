/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Indicate what to do in the next Setup() call
	/// </summary>
	internal enum InstrumentSetup : byte
	{
		Nothing = 0,
		Initialize,
		ReleaseNote,
		Mute
	}
}
