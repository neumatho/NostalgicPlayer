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

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Base class that decompressor streams which cannot detect the
	/// length of the decompressed data can derive from, so it is
	/// possible to get the length
	/// </summary>
	internal abstract class NoLengthStream : DecruncherStream
	{
		private readonly MemoryStream bufferStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected NoLengthStream(string agentName, Stream wrapperStream, bool leaveOpen) : base(wrapperStream, leaveOpen)
		{
			bufferStream = new MemoryStream();

			ReadAndDecrunch(agentName);
		}



		/********************************************************************/
		/// <summary>
		/// Dispose our self
		/// </summary>
		/********************************************************************/
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			bufferStream.Dispose();
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports reading
		/// </summary>
		/********************************************************************/
		public override bool CanRead => true;



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => bufferStream.Position;

			set => bufferStream.Position = value;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			return bufferStream.Seek(offset, origin);
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			return bufferStream.Read(buffer, offset, count);
		}
		#endregion

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		protected override int GetDecrunchedLength()
		{
			return (int)bufferStream.Length;
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the stream holding the crunched data
		/// </summary>
		/********************************************************************/
		protected abstract Stream OpenCrunchedDataStream();

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read and decrunch data
		/// </summary>
		/********************************************************************/
		private void ReadAndDecrunch(string agentName)
		{
			try
			{
				// Because the decrunched length is not stored anywhere, we need
				// to decrunch the whole file into a buffer and use that to read from
				wrapperStream.Seek(0, SeekOrigin.Begin);

				using (Stream decruncherStream = OpenCrunchedDataStream())
				{
					decruncherStream.CopyTo(bufferStream);
				}

				bufferStream.Seek(0, SeekOrigin.Begin);
			}
			catch(Exception ex)
			{
				throw new DecruncherException(agentName, Resources.IDS_SCOM_ERR_LOADING_DATA, ex);
			}
		}
		#endregion
	}
}
