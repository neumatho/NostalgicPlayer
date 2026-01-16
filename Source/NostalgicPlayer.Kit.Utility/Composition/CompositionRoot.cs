/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Kit.Utility.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all utility specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterUtility(this Container container)
		{
			container.Register<ISettings, Settings>();
		}
	}
}
