/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Kit.Streams;
using SharpCompress.Compressors;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Wrapper class to the SharpCompress LZipStream
	/// </summary>
	internal class LZipStream : DecruncherStream
	{
		private readonly string agentName;

		private readonly SharpCompress.Compressors.LZMA.LZipStream decruncherStream;
		private readonly int decrunchedLength;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LZipStream(string agentName, Stream wrapperStream) : base(wrapperStream, true)
		{
			this.agentName = agentName;

			// Find the length of decrunched data
			byte[] buf = new byte[4];

			wrapperStream.Seek(-16, SeekOrigin.End);
			wrapperStream.ReadExactly(buf, 0, 4);

			decrunchedLength = (buf[3] << 24) | (buf[2] << 16) | (buf[1] << 8) | buf[0];

			wrapperStream.Seek(0, SeekOrigin.Begin);
			decruncherStream = new SharpCompress.Compressors.LZMA.LZipStream(wrapperStream, CompressionMode.Decompress);
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
				throw new DecruncherException(agentName, Resources.IDS_SCOM_ERR_LOADING_DATA, ex);
			}
		}
		#endregion

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		public override int GetDecrunchedLength()
		{
			return decrunchedLength;
		}
		#endregion
	}
}
