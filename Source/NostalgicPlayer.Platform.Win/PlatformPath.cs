/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Platform
{
	/// <summary>
	/// Holds different system paths
	/// </summary>
	internal class PlatformPath : IPlatformPath
	{
		/********************************************************************/
		/// <summary>
		/// Return the path to where the settings should be stored
		/// </summary>
		/********************************************************************/
		public string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Polycode\NostalgicPlayer");

	}
}
