/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Gui.Factories;
using Polycode.NostalgicPlayer.Kit.Gui.Interfaces;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Kit.Gui.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all kit specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterGuiKit(this Container container)
		{
			container.RegisterSingleton<IControlFactory, ControlFactory>();
		}
	}
}
