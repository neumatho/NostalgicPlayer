/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Homepage.Clients;
using Polycode.NostalgicPlayer.External.Homepage.Interfaces;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.External.Composition
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
		public static void RegisterExternal(this Container container)
		{
			RegisterHomepage(container);
		}



		/********************************************************************/
		/// <summary>
		/// Register all homepage
		/// </summary>
		/********************************************************************/
		private static void RegisterHomepage(Container container)
		{
			container.Register<IVersionHistoryClient, VersionHistoryClient>();
		}
	}
}
