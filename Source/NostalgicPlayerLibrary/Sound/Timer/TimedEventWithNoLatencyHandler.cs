/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Timer
{
	/// <summary>
	/// Specific implementation of timed events where no latency
	/// at all is included
	/// </summary>
	internal class TimedEventWithNoLatencyHandler : TimedEventHandler
	{
		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public override void SetOutputFormat(OutputInfo outputInformation)
		{
			base.SetOutputFormat(outputInformation);

			// No latency at all
			outputLatencyInFrames = 0;

			CalculateLatency();
		}



		/********************************************************************/
		/// <summary>
		/// Set the latency
		/// </summary>
		/********************************************************************/
		public override void SetLatency(int latency)
		{
		}
	}
}
