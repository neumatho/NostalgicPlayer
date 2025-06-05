/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Utility
{
	/// <summary>
	/// Helper class for common things for the SC68 format
	/// </summary>
	internal static class Sc68Helper
	{
		/********************************************************************/
		/// <summary>
		/// Return the ID string for SC68 modules
		/// </summary>
		/********************************************************************/
		public static string IdString => "SC68 Music-file / (c) (BeN)jamin Gerard / SasHipA-Dev  ";



		/********************************************************************/
		/// <summary>
		/// Find all modules inside a single file
		/// </summary>
		/********************************************************************/
		public static List<Sc68DataBlockInfo> FindAllModules(ReaderStream readerStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			readerStream.Seek(64, SeekOrigin.Begin);

			List<Sc68DataBlockInfo> dataBlocks = new List<Sc68DataBlockInfo>();

			long lastScmuPosition = 0;
			int moduleLength = 0;

			for (;;)
			{
				string name = readerStream.ReadMark();
				uint size = readerStream.Read_L_UINT32();

				if (readerStream.EndOfStream)
				{
					errorMessage = Resources.IDS_ERR_LOADING_EOF_REACHED;
					return null;
				}

				// End of file
				if (name == "SCEF")
					break;

				switch (name)
				{
					// Start of new module
					case "SCMU":
					{
						lastScmuPosition = readerStream.Position - 8;
						moduleLength = 0;
						break;
					}

					// Data block
					case "SCDA":
					{
						if (lastScmuPosition == 0)
						{
							errorMessage = Resources.IDS_ERR_LOADING_NO_MUSIC_BLOCK;
							return null;
						}

						moduleLength += (int)(size + 8);

						dataBlocks.Add(new Sc68DataBlockInfo
						{
							ModuleStartPosition = lastScmuPosition,
							ModuleLength = moduleLength,
							DataStartPosition = readerStream.Position,
							DataLength = (int)size
						});

						lastScmuPosition = 0;
						break;
					}
				}

				moduleLength += (int)size + 8;
				readerStream.Seek(size, SeekOrigin.Current);
			}

			return dataBlocks;
		}
	}
}
