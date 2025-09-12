/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Actionamics.Containers
{
	/// <summary>
	/// Holds information about an instrument list
	/// </summary>
	internal class InstrumentList
	{
		public byte ListNumber { get; set; }
		public byte NumberOfValuesInList { get; set; }
		public byte StartCounterDeltaValue { get; set; }
		public byte CounterEndValue { get; set; }
	}
}
