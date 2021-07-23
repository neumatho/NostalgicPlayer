/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Streams
{
	/// <summary>
	/// This class is used for the archive decruncher agent interfaces
	/// </summary>
	public abstract class ArchiveStream : DecruncherStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ArchiveStream(Stream wrapperStream, bool leaveOpen) : base(wrapperStream, leaveOpen)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the size of the crunched data
		/// </summary>
		/********************************************************************/
		public abstract int GetCrunchedLength();
	}
}
