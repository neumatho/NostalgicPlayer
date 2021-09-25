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
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// This interface helps loading extra files needed for a module
	/// </summary>
	public interface ILoader : IDisposable
	{
		/// <summary>
		/// Will try to open the main file. You need to dispose the returned
		/// stream when done
		/// </summary>
		Stream OpenFile();

		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. Use this if you need to check extra files
		/// in the Identify() method. You need to dispose the returned stream
		/// when done
		/// </summary>
		ModuleStream OpenExtraFileForTest(string newExtension, out string newFileName);

		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. Will add the file sizes to one or both of
		/// ModuleSize and CrunchedSize. You need to dispose the returned
		/// stream when done
		/// </summary>
		ModuleStream OpenExtraFile(string newExtension);

		/// <summary>
		/// Will try to open a file with the name given as extra file. Will
		/// add the file sizes to one or both of ModuleSize and CrunchedSize.
		/// You need to dispose the returned stream when done
		/// </summary>
		ModuleStream OpenExtraFileWithName(string fullFileName);

		/// <summary>
		/// Return the full path to the file
		/// </summary>
		string FullPath { get; }

		/// <summary>
		/// Return the size of the module loaded
		/// </summary>
		long ModuleSize { get; }

		/// <summary>
		/// Return the size of the module crunched. Is zero if not crunched
		/// </summary>
		long CrunchedSize { get; }
	}
}
