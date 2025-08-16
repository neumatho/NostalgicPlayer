/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Sound.Timer
{
	/// <summary>
	/// A timed event implementation
	/// </summary>
	internal interface ITimedEvent
	{
		/// <summary>
		/// Do whatever this event want to do
		/// </summary>
		void Execute(int differenceTime);
	}
}
