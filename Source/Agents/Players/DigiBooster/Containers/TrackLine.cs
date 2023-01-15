/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBooster.Containers
{
	/// <summary>
	/// A single track on a single row
	/// </summary>
	internal class TrackLine
	{
		public ushort Period;
		public byte SampleNumber;
		public Effect Effect;
		public byte EffectArg;
	}
}
