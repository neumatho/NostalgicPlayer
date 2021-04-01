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

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class wraps another stream and adds some helper methods to read the data
	/// </summary>
	public class ModuleStream : ReaderStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModuleStream(Stream wrapperStream) : base(wrapperStream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Will open a new handle to the current stream and return a new
		/// stream
		/// </summary>
		/********************************************************************/
		public ModuleStream Duplicate()
		{
			ModuleStream newStream = null;

			if (wrapperStream is FileStream fs)
				newStream = new ModuleStream(new FileStream(fs.Name, FileMode.Open, FileAccess.Read));

			if (newStream == null)
				throw new NotSupportedException($"Stream of type {wrapperStream.GetType()} cannot be duplicated");

			newStream.Seek(Position, SeekOrigin.Begin);

			return newStream;
		}
	}
}
