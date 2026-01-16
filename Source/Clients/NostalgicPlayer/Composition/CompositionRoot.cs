/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Application;
using Polycode.NostalgicPlayer.Logic.Composition;
using Polycode.NostalgicPlayer.Platform.Composition;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all client specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void Register(Container container)
		{
			container.RegisterLogic();
			container.RegisterPlatform();

			container.RegisterSingleton<IApplicationHost, SingleInstanceApplication>();
		}
	}
}
