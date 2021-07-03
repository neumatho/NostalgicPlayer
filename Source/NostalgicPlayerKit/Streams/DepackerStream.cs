﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class is used for the depacker agent interfaces
	/// </summary>
	public abstract class DepackerStream : ReaderStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DepackerStream(Stream wrapperStream) : base(wrapperStream)
		{
		}

		#region Stream implementation
		/********************************************************************/
		/// <summary>
		/// Indicate if the stream supports seeking
		/// </summary>
		/********************************************************************/
		public override bool CanSeek => false;




		/********************************************************************/
		/// <summary>
		/// Return the length of the data
		/// </summary>
		/********************************************************************/
		public override long Length => GetDepackedLength();



		/********************************************************************/
		/// <summary>
		/// Return the current position
		/// </summary>
		/********************************************************************/
		public override long Position
		{
			get => wrapperStream.Position;

			set => throw new NotSupportedException("Set position not supported");
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
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the size of the depacked data
		/// </summary>
		/********************************************************************/
		protected abstract int GetDepackedLength();
	}
}
