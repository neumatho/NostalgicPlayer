/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Factories
{
	/// <summary>
	/// Factory for creating progress callbacks
	/// </summary>
	public interface IProgressCallbackFactory
	{
		/// <summary>
		/// Get or set the current progress callback
		/// </summary>
		ILoadProgressCallback CurrentCallback { get; set; }
	}
}
