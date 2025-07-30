/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	internal class Track
	{
		public byte Note { get; set; }
		public byte Instrument { get; set; }
		public TrackEffect Effect { get; set; }
		public byte EffectArgument { get; set; }
		public byte RowsToWait { get; set; }
	}
}
