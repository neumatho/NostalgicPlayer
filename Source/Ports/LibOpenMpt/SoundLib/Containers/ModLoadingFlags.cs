/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	internal enum ModLoadingFlags
	{
		/// <summary>
		/// If unset, advise loaders to not process any pattern data (if possible)
		/// </summary>
		LoadPatternData = 0x01,

		/// <summary>
		/// If unset, advise loaders to not process any sample data (if possible)
		/// </summary>
		LoadSampleData = 0x02,

		/// <summary>
		/// If unset, plugin data is not loaded (and as a consequence, plugins are not instantiated)
		/// </summary>
		LoadPluginData = 0x04,

		/// <summary>
		/// If unset, plugins are not instantiated
		/// </summary>
		LoadPluginInstance = 0x08,

		/// <summary>
		/// 
		/// </summary>
		SkipContainer = 0x10,

		/// <summary>
		/// 
		/// </summary>
		SkipModules = 0x20,

		/// <summary>
		/// Do not combine with other flags
		/// </summary>
		OnlyVerifyHeader = 0x40,

		// Shortcuts

		LoadCompleteModule = LoadSampleData | LoadPatternData | LoadPluginData | LoadPluginInstance,
		LoadNoPatternOrPluginData = LoadSampleData,
		LoadNoPluginInstance = LoadSampleData | LoadPatternData | LoadPluginData
	}
}
