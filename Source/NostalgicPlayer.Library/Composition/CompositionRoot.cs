/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Library.Players;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Library.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all library specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterLibrary(this Container container)
		{
			RegisterPlayers(container);
		}



		/********************************************************************/
		/// <summary>
		/// Register all players
		/// </summary>
		/********************************************************************/
		private static void RegisterPlayers(Container container)
		{
			container.RegisterSingleton<IPlayerFactory, PlayerFactory>();

			container.Register<IModulePlayer, ModulePlayer>();
			container.Register<ISamplePlayer, SamplePlayer>();
		}
	}
}
