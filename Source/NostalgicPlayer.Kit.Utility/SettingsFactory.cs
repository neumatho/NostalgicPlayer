/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Factory for creating ISettings instances
	/// </summary>
	internal class SettingsFactory : ISettingsFactory
	{
		private readonly IPlatformPath platformPath;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SettingsFactory(IPlatformPath platformPath)
		{
			this.platformPath = platformPath;
		}



		/********************************************************************/
		/// <summary>
		/// Creates a new ISettings instance
		/// </summary>
		/********************************************************************/
		public ISettings CreateSettings()
		{
			return new Settings(platformPath);
		}
	}
}
