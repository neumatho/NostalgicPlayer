/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal static class FileReaderExt
	{
		/********************************************************************/
		/// <summary>
		/// Read a string of length srcSize into fixed-length char array
		/// destBuffer given read mode. The file cursor is advanced by
		/// "srcSize" bytes.
		/// Returns true if at least one character could be read or 0
		/// characters were requested
		/// </summary>
		/********************************************************************/
		public static bool ReadString(ReadWriteMode mode, FileCursor f, uint8[] destBuffer, size_t srcSize)//XX 49
		{
			FileCursor.PinnedView source = f.ReadPinnedView(srcSize);	// Make sure the string is cached properly
			size_t realSrcSize = source.Size();		// In case fewer bytes are available
			MptString.WriteAutoBuf(destBuffer).Assign(MptString.ReadBuf(mode, source.Data(), realSrcSize));

			return (realSrcSize > 0) || (srcSize == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Read a string of length srcSize into a std::string dest using a
		/// given read mode. The file cursor is advanced by "srcSize" bytes.
		/// Returns true if at least one character could be read or 0
		/// characters were requested
		/// </summary>
		/********************************************************************/
		public static bool ReadString(ReadWriteMode mode, FileCursor f, StdString dest, size_t srcSize)//XX 61
		{
			dest.clear();

			FileCursor.PinnedView source = f.ReadPinnedView(srcSize);	// Make sure the string is cached properly
			size_t realSrcSize = source.Size();		// In case fewer bytes are available
			dest.assign(MptString.ReadBuf(mode, source.Data(), realSrcSize));

			return (realSrcSize > 0) || (srcSize == 0);
		}



		/********************************************************************/
		/// <summary>
		/// Read a string of length srcSize into a mpt::charbuf dest using a
		/// given read mode. The file cursor is advanced by "srcSize" bytes.
		/// Returns true if at least one character could be read or 0
		/// characters were requested
		/// </summary>
		/********************************************************************/
		public static bool ReadString(ReadWriteMode mode, FileCursor f, CharBuf dest, size_t srcSize)//XX 74
		{
			FileCursor.PinnedView source = f.ReadPinnedView(srcSize);	// Make sure the string is cached properly
			size_t realSrcSize = source.Size();		// In case fewer bytes are available
			dest.Assign(MptString.ReadBuf(mode, source.Data(), realSrcSize));

			return (realSrcSize > 0) || (srcSize == 0);
		}
	}
}
