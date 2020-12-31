/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.RetroPlayer.RetroPlayerKit.Containers;

namespace Polycode.RetroPlayer.RetroPlayerKit.Streams
{
	/// <summary>
	/// This class is the base class for streams that return sound data
	/// </summary>
	public abstract class SoundStream : Stream
	{
		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports reading
		/// </summary>
		/********************************************************************/
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports writing
		/// </summary>
		/********************************************************************/
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}




		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}

			set
			{
				throw new NotSupportedException();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a new position
		/// </summary>
		/********************************************************************/
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Set new length
		/// </summary>
		/********************************************************************/
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Write data to the stream
		/// </summary>
		/********************************************************************/
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}



		/********************************************************************/
		/// <summary>
		/// Flush buffers
		/// </summary>
		/********************************************************************/
		public override void Flush()
		{
			throw new NotSupportedException();
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Set the output format
		/// </summary>
		/********************************************************************/
		public abstract void SetOutputFormat(OutputInfo outputInfo);



		/********************************************************************/
		/// <summary>
		/// Start the playing
		/// </summary>
		/********************************************************************/
		public abstract void Start();



		/********************************************************************/
		/// <summary>
		/// Stop the playing
		/// </summary>
		/********************************************************************/
		public abstract void Stop();
	}
}
