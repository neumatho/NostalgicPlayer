/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Archive decrunchers need to implement this interface
	/// when returning the worker class on the archive
	/// </summary>
	public interface IArchive
	{
		/// <summary>
		/// Open the entry with the name given
		/// </summary>
		ArchiveStream OpenEntry(string entryName);

		/// <summary>
		/// Return all entries
		/// </summary>
		IEnumerable<string> GetEntries();
	}
}
