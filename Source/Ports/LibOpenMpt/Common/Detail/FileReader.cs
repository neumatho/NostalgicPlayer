/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Detail
{
	/// <summary>
	/// A basic class for transparent reading of memory-based files
	/// </summary>
	internal class FileReader<TByte> : FileCursor where TByte : unmanaged
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileReader(ITraits traits, IFileNameTraits fileNameTraits) : base(traits, fileNameTraits)//XX 181
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// 
		/// Initialize file reader object with pointer to data and data
		/// length
		/// </summary>
		/********************************************************************/
		public FileReader(ITraits traits, IFileNameTraits fileNameTraits, MptSpan<TByte> byteData, ISharedFileNameType fileName = null) : base(traits, fileNameTraits, Memory.Byte_Cast<TByte, c_byte>(byteData), fileName)//XX 189
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool Read<T>(ref T target) where T : unmanaged//XX 205
		{
			return Mpt.Io_Read.FileReader.Read(this, ref target);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint32 ReadUInt32LE()//XX 234
		{
			return Mpt.Io_Read.FileReader.ReadUInt32LE(this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadArray<T>(T[] destArray) where T : unmanaged//XX 342
		{
			return Mpt.Io_Read.FileReader.ReadArray(this, destArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadArray<T>(CPointer<T> destArray) where T : unmanaged
		{
			return Mpt.Io_Read.FileReader.ReadArray<T>(this, destArray);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public array<T> ReadArray<T>(size_t destSize) where T : unmanaged//XX 353
		{
			return Mpt.Io_Read.FileReader.ReadArray<T>(this, destSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadMagic(string magic)
		{
			return Mpt.Io_Read.FileReader.ReadMagic(this, magic);
		}
	}
}
