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
		/// Create new instance of the normal loader
		/// </summary>
		/********************************************************************/
		public Loader CreateLoader()
		{
			return applicationContext.Container.GetInstance<Loader>();
		}



		/********************************************************************/
		/// <summary>
		/// Create new instance of the stream loader
		/// </summary>
		/********************************************************************/
		public StreamLoader CreateStreamLoader()
		{
			return applicationContext.Container.GetInstance<StreamLoader>();
		}
	}
}
