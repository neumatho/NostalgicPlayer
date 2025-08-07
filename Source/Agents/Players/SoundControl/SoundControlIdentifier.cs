/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/

using System;
using System.IO;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class SoundControlIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions = [ "sc", "sct" ];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static ModuleType TestModule(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			if (moduleStream.Length < 576)
				return ModuleType.Unknown;

			moduleStream.Seek(16, SeekOrigin.Begin);

			uint offset = moduleStream.Read_B_UINT32();

			if ((offset >= 0x8000) || (offset & 0x1) != 0)
				return ModuleType.Unknown;

			offset += 64 - 2;
			if (offset >= moduleStream.Length)
				return ModuleType.Unknown;

			moduleStream.Seek(offset, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT16() != 0xffff)
				return ModuleType.Unknown;

			if (moduleStream.Read_B_UINT32() != 0x00000400)
				return ModuleType.Unknown;

			moduleStream.Seek(28, SeekOrigin.Begin);

			offset = moduleStream.Read_B_UINT32();
			ushort version = moduleStream.Read_B_UINT16();

			if ((version == 2) && (offset == 0))
				return ModuleType.SoundControl3x;

			if ((version == 3) && (offset != 0))
			{
				moduleStream.Seek(16, SeekOrigin.Begin);

				uint totalLength = 64 +
								   moduleStream.Read_B_UINT32() +
								   moduleStream.Read_B_UINT32() +
								   moduleStream.Read_B_UINT32() +
								   moduleStream.Read_B_UINT32();

				if (SoundControl40_50Player.IsVersion40Module(totalLength))
					return ModuleType.SoundControl40;

				return ModuleType.SoundControl50;
			}

			return ModuleType.Unknown;
		}
	}
}
