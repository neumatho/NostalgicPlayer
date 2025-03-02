/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.PlayerLibrary.Sound.Mixer.Containers
{
	/// <summary>
	/// Holds information about a single mixer buffer
	/// </summary>
	internal class MixerBufferInfo
	{
		/********************************************************************/
		/// <summary>
		/// Holds the buffer to mix into for this channel
		/// </summary>
		/********************************************************************/
		public int[] Buffer;



		/********************************************************************/
		/// <summary>
		/// The previous filter value in the last round
		/// </summary>
		/********************************************************************/
		public int FilterPrevValue;
	}
}
