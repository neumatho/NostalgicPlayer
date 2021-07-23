/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;
using SharpCompress.Common;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Wrapper class to the SharpCompress entry stream
	/// </summary>
	internal class ArchiveEntryStream : ArchiveStream
	{
		private readonly IEntry entry;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ArchiveEntryStream(IEntry entry, Stream entryStream) : base(entryStream, false)
		{
			this.entry = entry;
		}

		#region DecruncherStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the decrunched data
		/// </summary>
		/********************************************************************/
		protected override int GetDecrunchedLength()
		{
			return (int)entry.Size;
		}
		#endregion

		#region ArchiveStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the size of the crunched data
		/// </summary>
		/********************************************************************/
		public override int GetCrunchedLength()
		{
			return entry.CompressedSize == 0 ? -1 : (int)entry.CompressedSize;
		}
		#endregion
	}
}
