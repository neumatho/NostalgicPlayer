/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class OctaMedIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "med", "mmd0", "mmd1", "mmd2", "mmd3", "mmdc", "omed", "ocss", "md0", "md1", "md2", "md3" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			if (moduleStream.Length < 840)
				return ModuleType.Unknown;

			// Now check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);
			uint mark = moduleStream.Read_B_UINT32();

			// Check the mark
			if ((mark & 0xffffff00) != 0x4d4d4400)	// MMD\0
				return ModuleType.Unknown;

			// Mark out the mark and leave the version
			byte markVersion = (byte)(mark & 0x000000ff);

			if (((markVersion < '0') || (markVersion > '3')) && (markVersion != 'C'))
				return ModuleType.Unknown;

			if (markVersion == '0')
			{
				// Well, it's either a MED or OctaMED module, find out which one
				//
				// Skip module length
				moduleStream.Seek(4, SeekOrigin.Current);

				// Seek to the song structure + skip until the flags argument
				uint temp = moduleStream.Read_B_UINT32();
				if (temp == 0)
					return ModuleType.Unknown;

				moduleStream.Seek(temp + 767, SeekOrigin.Begin);

				if (((MmdFlag)moduleStream.Read_UINT8() & MmdFlag.EightChannel) != 0)
					return ModuleType.OctaMed;

				return ModuleType.Med210_MMD0;
			}

			if (markVersion == '1')
				return ModuleType.OctaMed_Professional4;

			if (markVersion == '2')
				return ModuleType.OctaMed_Professional6;

			if (markVersion == '3')
				return ModuleType.OctaMed_SoundStudio;

			if (markVersion == 'C')
				return ModuleType.MedPacker;

			return ModuleType.Unknown;
		}
	}
}
