/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources.ModLand;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows.ModLibraryWindow.Sources
{
	/// <summary>
	/// Creates the different sources
	/// </summary>
	internal class SourceFactory : ISourceFactory
	{
		private readonly IApplicationContext applicationContext;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SourceFactory(IApplicationContext applicationContext)
		{
			this.applicationContext = applicationContext;
		}



		/********************************************************************/
		/// <summary>
		/// Get a ModLand source
		/// </summary>
		/********************************************************************/
		public IModLibrarySource GetModLandSource()
		{
			return new ModLandSource(applicationContext);
		}
	}
}
