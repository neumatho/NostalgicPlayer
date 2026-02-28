/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.External.Download;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Platform.Composition
{
	/// <summary>
	/// Register all classes/interfaces into the dependency injection container
	/// </summary>
	public static class CompositionRoot
	{
		/********************************************************************/
		/// <summary>
		/// Register all platform specific classes into the container
		/// </summary>
		/********************************************************************/
		public static void RegisterPlatform(this Container container)
		{
			container.RegisterSingleton<IPlatformPath, PlatformPath>();
			container.RegisterSingleton<IPictureDownloaderFactory, PictureDownloaderFactory>();
		}
	}
}
