/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Factory implementation to create new instances of the right loader
	/// </summary>
	internal class LoaderFactory : ILoaderFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LoaderFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Get normal loader instance
		/// </summary>
		/********************************************************************/
		public Loader GetLoader()
		{
			return applicationContext.Container.GetInstance<Loader>();
		}



		/********************************************************************/
		/// <summary>
		/// Get stream loader instance
		/// </summary>
		/********************************************************************/
		public StreamLoader GetStreamLoader()
		{
			return applicationContext.Container.GetInstance<StreamLoader>();
		}
	}
}
