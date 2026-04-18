/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Factory to create visualizer instances
	/// </summary>
	internal interface IVisualizerFactory
	{
		/// <summary>
		/// Get a visualizer instances
		/// </summary>
		Visualizer GetVisualizer();
	}
}
