/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Exceptions;
using Polycode.NostalgicPlayer.Ports.Ancient.Exceptions;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.AncientDecruncher.Streams
{
	/// <summary>
	/// Wrapper stream which read from an IEnumerable
	/// </summary>
	internal class AncientStream : Stream
	{
		private readonly string agentName;
		private readonly IEnumerator<byte[]> data;
		private readonly int length;
		private long position;

		private byte[] decrunchedBuffer;
		private int decrunchedBufferIndex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public AncientStream(string agentName, IEnumerable<byte[]> decrunchedData, int decrunchedSize)
		{
			this.agentName = agentName;

			data = decrunchedData.GetEnumerator();
			length = decrunchedSize;
			position = 0;

			decrunchedBuffer = null;
			decrunchedBufferIndex = 0;
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
		public override bool CanSeek => false;



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports writing
		/// </summary>
		/********************************************************************/
		public override bool CanWrite => false;



		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => length;



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => position;

			set => throw new NotSupportedException("Set Position not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Set new length
		/// </summary>
		/********************************************************************/
		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Read data from the stream
		/// </summary>
		/********************************************************************/
		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				int bytesRead = 0;

				while (count > 0)
				{
					if (decrunchedBuffer == null)
					{
						if (!data.MoveNext())
							return bytesRead;

						decrunchedBuffer = data.Current;
						decrunchedBufferIndex = 0;
					}

					int todo = Math.Min(count, decrunchedBuffer.Length - decrunchedBufferIndex);
					Array.Copy(decrunchedBuffer, decrunchedBufferIndex, buffer, offset, todo);

					count -= todo;
					bytesRead += todo;
					offset += todo;
					position += todo;
					decrunchedBufferIndex += todo;

					if (decrunchedBufferIndex == decrunchedBuffer.Length)
						decrunchedBuffer = null;
				}

				return bytesRead;
			}
			catch (DecompressionException)
			{
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CORRUPT_DATA);
			}
			catch (VerificationException)
			{
				throw new DecruncherException(agentName, Resources.IDS_ANC_ERR_CHECKSUM_MISMATCH);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write data to the stream
		/// </summary>
		/********************************************************************/
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write not supported");
		}



		/********************************************************************/
		/// <summary>
		/// Flush buffers
		/// </summary>
		/********************************************************************/
		public override void Flush()
		{
			throw new NotSupportedException("Flush not supported");
		}
		#endregion
	}
}
