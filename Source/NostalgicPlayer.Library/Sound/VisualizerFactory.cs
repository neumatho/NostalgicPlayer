/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Sound
{
	/// <summary>
	/// Factory to create visualizer instances
	/// </summary>
	internal class VisualizerFactory : IVisualizerFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public VisualizerFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Get a visualizer instances
		/// </summary>
		/********************************************************************/
		public Visualizer GetVisualizer()
		{
			return applicationContext.Container.GetInstance<Visualizer>();
		}
	}
}
