/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
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
		public T ReadSizedIntLE<T>(size_t size)//XX 229
		{
			return Mpt.Io_Read.FileReader.ReadSizedIntLE<T>(this, size);
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
		public uint16 ReadUInt16LE()//XX 264
		{
			return Mpt.Io_Read.FileReader.ReadUInt16LE(this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8 ReadUInt8()//XX 289
		{
			return Mpt.Io_Read.FileReader.ReadUInt8(this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadStruct<T>(ref T target) where T : unmanaged, IClearable//XX 320
		{
			return Mpt.Io_Read.FileReader.ReadStruct(this, ref target);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public size_t ReadStructPartial<T>(ref T target, size_t partialSize) where T : unmanaged//XX 326
		{
			return Mpt.Io_Read.FileReader.ReadStructPartial(this, ref target, partialSize);
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
			return Mpt.Io_Read.FileReader.ReadArray(this, destArray);
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
		public bool ReadVector<T>(vector<T> destVector, size_t destSize) where T : unmanaged//XX 360
		{
			return Mpt.Io_Read.FileReader.ReadVector(this, destVector, destSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadString(ReadWriteMode mode, uint8[] dest, size_t srcSize)//XX 401
		{
			return FileReaderExt.ReadString(mode, this, dest, srcSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadString(ReadWriteMode mode, StdString dest, size_t srcSize)//XX 408
		{
			return FileReaderExt.ReadString(mode, this, dest, srcSize);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool ReadString(ReadWriteMode mode, CharBuf dest, size_t srcSize)//XX 414
		{
			return FileReaderExt.ReadString(mode, this, dest, srcSize);
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
