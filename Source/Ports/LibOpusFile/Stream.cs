/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOpusFile.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpusFile
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Stream
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Op_Stream_Read(object _stream, CPointer<byte> _ptr, c_int _buf_size)
		{
			// Check for empty read
			if (_buf_size <= 0)
				return 0;

			System.IO.Stream stream = (System.IO.Stream)_stream;

			int ret = stream.Read(_ptr.Buffer, _ptr.Offset, _buf_size);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Op_Stream_Seek(object _stream, opus_int64 _offset, SeekOrigin _whence)
		{
			System.IO.Stream stream = (System.IO.Stream)_stream;
			if (!stream.CanSeek)
				return -1;

			// Translate the seek to an absolute one
			opus_int64 pos;

			if (_whence == SeekOrigin.Current)
				pos = stream.Position;
			else if (_whence == SeekOrigin.End)
				pos = stream.Length;
			else if (_whence == SeekOrigin.Begin)
				pos = 0;
			else
				return -1;

			// Check for errors or overflow
			if ((pos < 0) || (_offset < -pos) || (_offset > (opus_int64.MaxValue - pos)))
				return -1;

			pos += _offset;

			stream.Seek(pos, SeekOrigin.Begin);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static opus_int64 Op_Stream_Tell(object _stream)
		{
			System.IO.Stream stream = (System.IO.Stream)_stream;

			return stream.Position;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Op_Stream_Close(object _stream)
		{
			System.IO.Stream stream = (System.IO.Stream)_stream;
			stream.Close();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static OpusFileCallbacks Op_Get_Stream_Callbacks(System.IO.Stream _stream, bool _leaveOpen)
		{
			bool canSeek = _stream.CanSeek;

			return new OpusFileCallbacks
			{
				Read = Op_Stream_Read,
				Seek = canSeek ? Op_Stream_Seek : null,
				Tell = canSeek ? Op_Stream_Tell : null,
				Close = _leaveOpen ? null : Op_Stream_Close
			};
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static System.IO.Stream Op_IO_Stream_Create(out OpusFileCallbacks _cb, System.IO.Stream _stream, bool _leaveOpen)
		{
			_cb = Op_Get_Stream_Callbacks(_stream, _leaveOpen);

			return _stream;
		}
	}
}
