/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/

using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ModuleConverter
{
	/// <summary>
	/// Base class for all the formats
	/// </summary>
	internal abstract class ModuleConverterWorkerBase : IModuleConverter
	{
		#region IModuleConverter implementation
		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Convert(PlayerFileInfo fileInfo, WriterStream writerStream, out string errorMessage);
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Copy length bytes from one stream to another
		/// </summary>
		/********************************************************************/
		protected void CopyData(Stream source, Stream destination, uint length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);
				if (len < 1024)
					Array.Clear(buf, len, 1024 - len);

				destination.Write(buf, 0, 1024);

				length -= 1024;
			}

			if (length > 0)
			{
				int len = source.Read(buf, 0, (int)length);
				if (len < 1024)
					Array.Clear(buf, len, 1024 - len);

				destination.Write(buf, 0, (int)length);
			}
		}
		#endregion
	}
}
