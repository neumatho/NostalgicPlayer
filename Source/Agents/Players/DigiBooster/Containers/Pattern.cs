/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// Contains a single pattern
	/// </summary>
	internal class Pattern
	{
		public TrackLine[,] Rows { get; } = new TrackLine[8, 64];
	}
}
