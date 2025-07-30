/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.ActivisionPro.Containers
{
	/// <summary>
	/// Holds information about a single envelope
	/// </summary>
	internal class Envelope
	{
		public EnvelopePoint[] Points { get; } = new EnvelopePoint[6];
	}
}
