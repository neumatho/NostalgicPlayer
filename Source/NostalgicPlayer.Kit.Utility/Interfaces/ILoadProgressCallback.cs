/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Utility.Interfaces
{
	/// <summary>
	/// Interface for receiving progress updates during loading operations
	/// </summary>
	public interface ILoadProgressCallback
	{
		/// <summary>
		/// Called to update progress during loading
		/// </summary>
		void UpdateProgress(int loaded, int total);
	}
}
