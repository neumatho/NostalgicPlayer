/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// Stream that wraps a Hio object
	/// </summary>
	internal class HioStream : Stream
	{
		private readonly Hio hio;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public HioStream(Hio f)
		{
			hio = f;
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
		/// Indicate if the stream supports writing
		/// </summary>
		/********************************************************************/
		public override bool CanWrite => false;



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => true;



		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => hio.Hio_Size();



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => hio.Hio_Tell();

			set
			{
				if (hio.Hio_Seek((c_long)value, SeekOrigin.Begin) < 0)
					throw new IOException("Seek failed");
			}
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			int result = hio.Hio_Seek((c_long)offset, origin);
			if (result < 0)
				throw new IOException("Seek failed");

			return hio.Hio_Tell();
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
			return (int)hio.Hio_Read(new CPointer<byte>(buffer, offset), 1, (size_t)count);
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
