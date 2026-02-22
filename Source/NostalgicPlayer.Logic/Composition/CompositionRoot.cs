/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Logic.Databases;
using Polycode.NostalgicPlayer.Logic.Playlists;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Logic.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all client logic specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterLogic(this Container container)
		{
			container.RegisterSingleton<IPlaylistFactory, PlaylistFactory>();
			container.RegisterSingleton<IModuleDatabase, ModuleDatabase>();
		}
	}
}
