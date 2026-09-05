/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal static class FileCursor_StdStream
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static FileCursor Make_FileCursor<TPath>(Stream s, TPath fileName = null) where TPath : class, ISharedFileNameType, new()
		{
			if (FileDataStdStream.IsSeekable(s))
				return new FileCursor(new FileCursorTraitsFileData(), new FileCursorFileNameTraits<TPath>(), new FileDataStdStreamSeekable(s), Utility.move(fileName));
			else
				return new FileCursor(new FileCursorTraitsFileData(), new FileCursorFileNameTraits<TPath>(), new FileDataStdStreamUnseekable(s), Utility.move(fileName));
		}
	}
}
