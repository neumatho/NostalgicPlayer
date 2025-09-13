/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Containers;
using Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Utility;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter.Formats
{
	/// <summary>
	/// Can convert SC68 container format and extract the module inside
	/// </summary>
	internal class Sc68Format : ModuleConverterAgentBase
	{
		#region IModuleConverterAgent implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;

			// First check the length
			long fileSize = moduleStream.Length;
			if (fileSize < 64)
				return AgentResult.Unknown;

			// Check ID
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.ReadMark(56) != Sc68Helper.IdString)
				return AgentResult.Unknown;

			if (moduleStream.ReadMark() != "SC68")
				return AgentResult.Unknown;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			List<Sc68DataBlockInfo> dataBlocks = Sc68Helper.FindAllModules(fileInfo.ModuleStream, out errorMessage);
			if (dataBlocks == null)
				return AgentResult.Error;

			ModuleStream moduleStream = fileInfo.ModuleStream;

			// Just take the first module
			Sc68DataBlockInfo dataBlockInfo = dataBlocks[0];

			// Just copy the whole module into the output stream
			moduleStream.Seek(dataBlockInfo.DataStartPosition, SeekOrigin.Begin);
			StreamHelper.CopyData(moduleStream, converterStream, dataBlockInfo.DataLength);

			return AgentResult.Ok;
		}
		#endregion
	}
}
