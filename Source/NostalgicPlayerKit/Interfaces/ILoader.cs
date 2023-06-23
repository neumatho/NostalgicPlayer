/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
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
		/// Will try to open the main file.
		///
		/// You need to dispose the returned stream when done
		/// </summary>
		Stream OpenFile();

		/// <summary>
		/// Will return a collection of different kind of file names using
		/// the extension given
		/// </summary>
		IEnumerable<string> GetPossibleFileNames(string newExtension);

		/// <summary>
		/// Will try to open a file with the same name as the current module,
		/// but with a different extension. It will also try to use the
		/// extension as a prefix. Will add the file sizes to one or both of
		/// ModuleSize and CrunchedSize.
		/// 
		/// If addSize is set to true, it will add the file sizes to one or
		/// both of ModuleSize and CrunchedSize. If false, you need to call
		/// AddSizes() by yourself, if you want to include the opened file
		/// as part of the collection of loaded files. This has to be done
		/// before calling this method again.
		///
		/// You need to dispose the returned stream when done
		/// </summary>
		ModuleStream OpenExtraFileByExtension(string newExtension, bool addSize = true);

		/// <summary>
		/// Will try to open a file with the name given as extra file.
		/// 
		/// If addSize is set to true, it will add the file sizes to one or
		/// both of ModuleSize and CrunchedSize. If false, you need to call
		/// AddSizes() by yourself, if you want to include the opened file
		/// as part of the collection of loaded files. This has to be done
		/// before calling this method again.
		/// 
		/// You need to dispose the returned stream when done
		/// </summary>
		ModuleStream OpenExtraFileByFileName(string fullFileName, bool addSize);

		/// <summary>
		/// Will add the sizes of the previous opened extra file to the
		/// size properties
		/// </summary>
		void AddSizes();

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
