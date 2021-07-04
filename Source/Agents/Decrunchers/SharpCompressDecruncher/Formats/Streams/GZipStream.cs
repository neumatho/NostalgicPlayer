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
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;
using SharpCompress.Compressors;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Wrapper class to the SharpCompress GZipStream
	/// </summary>
	internal class GZipStream : DepackerStream
	{
		private readonly string agentName;

		private readonly SharpCompress.Compressors.Deflate.GZipStream decruncherStream;
		private readonly int unpackedLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public GZipStream(string agentName, Stream wrapperStream) : base(wrapperStream)
		{
			this.agentName = agentName;

			// Find the length of unpacked data
			byte[] buf = new byte[4];

			wrapperStream.Seek(-4, SeekOrigin.End);
			wrapperStream.Read(buf, 0, 4);

			unpackedLength = (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];

			wrapperStream.Seek(0, SeekOrigin.Begin);
			decruncherStream = new SharpCompress.Compressors.Deflate.GZipStream(wrapperStream, CompressionMode.Decompress);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			decruncherStream.Dispose();
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				return decruncherStream.Read(buffer, offset, count);
			}
			catch(Exception ex)
			{
				throw new DepackerException(agentName, Resources.IDS_SCOM_ERR_LOADING_DATA, ex);
			}
		}
		#endregion

		#region DepackerStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		/********************************************************************/
		protected override int GetDepackedLength()
		{
			return unpackedLength;
		}
		#endregion
	}
}
