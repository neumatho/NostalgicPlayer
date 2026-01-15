/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Helpers;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Kit.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void RegisterKit(this Container container)
		{
			container.Register<ISettings, Settings>();
		}
	}
}
