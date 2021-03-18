/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// This interface helps loading extra files needed for a module
	/// </summary>
	public interface ILoader
	{
		/// <summary>
		/// Will try to open the main file
		/// </summary>
		ModuleStream OpenFile();

		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. You need to dispose the returned stream
		/// when done
		/// </summary>
		ModuleStream OpenExtraFile(string newExtension);

		/// <summary>
		/// Return the full path to the file
		/// </summary>
		string FullPath { get; }
	}
}
