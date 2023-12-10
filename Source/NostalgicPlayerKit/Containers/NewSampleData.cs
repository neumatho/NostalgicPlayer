/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about new sample data
	/// </summary>
	public class NewSampleData
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewSampleData(int[] buffer, int numberOfChannels, bool swapSpeakers)
		{
			SampleData = buffer;
			NumberOfChannels = numberOfChannels;
			SwapSpeakers = swapSpeakers;
		}



		/********************************************************************/
		/// <summary>
		/// An array with the sample data
		/// </summary>
		/********************************************************************/
		public int[] SampleData
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the total number of channels used in the buffer
		/// </summary>
		/********************************************************************/
		public int NumberOfChannels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if you need to swap left and right speakers.
		///
		/// This option is usually used by the user, if the sounds come out
		/// of the wrong speakers. So when enabled, the samples for
		/// left/right is swapped in the SampleData buffer, but the
		/// visualization should not be swapped
		/// </summary>
		/********************************************************************/
		public bool SwapSpeakers
		{
			get;
		}
	}
}
