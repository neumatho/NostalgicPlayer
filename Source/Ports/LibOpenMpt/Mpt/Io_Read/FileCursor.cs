/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	internal class FileCursor : ICopyTo<FileCursor>
	{
		#region PinnedView class
		public class PinnedView
		{
			private size_t size_;
			private CPointer<byte> pinnedData;
			private readonly vector<byte> cache = new vector<byte>();

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PinnedView()
			{
				size_ = 0;
				pinnedData.SetToNull();
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PinnedView(FileCursor file)
			{
				Init(file, file.BytesLeft());
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PinnedView(FileCursor file, size_t size)
			{
				Init(file, size);
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PinnedView(FileCursor file, bool advance)
			{
				Init(file, file.BytesLeft());

				if (advance)
					file.Skip(size_);
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public PinnedView(FileCursor file, size_t size, bool advance)
			{
				Init(file, size);

				if (advance)
					file.Skip(size_);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public byte_span GetSpan()
			{
				if (pinnedData.IsNotNull)
					return new byte_span(pinnedData, size_);
				else if (!cache.empty())
					return new byte_span(cache);
				else
					return new byte_span();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public byte_span Span()
			{
				return GetSpan();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public CPointer<byte> Data()
			{
				return Span().Data();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public size_t Size()
			{
				return size_;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public CPointer<byte> Begin()
			{
				return Span().Data();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public CPointer<byte> End()
			{
				return Span().Data() + Span().Size();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			private void Init(FileCursor file, size_t size)
			{
				size_ = 0;
				pinnedData.SetToNull();

				if (!file.CanRead(size))
					size = file.BytesLeft();

				size_ = size;

				if (file.DataContainer().HasPinnedView())
					pinnedData = file.DataContainer().GetRawData() + file.GetPosition();
				else
				{
					cache.resize(size_);

					if (!cache.empty())
						file.GetRaw(new MptSpan<byte>(cache));
				}
			}
		}
		#endregion

		public readonly ITraits Traits_Type;
		public readonly IFileNameTraits FileName_Traits_Type;

		private IFileData m_Data;
		private uint64_t streamPos;				// Cursor location in the file
		private ISharedFileNameType m_FileName;	// Filename that corresponds to this FileCursor. It is only set if this FileCursor represents the whole contents of fileName. May be nullopt

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FileCursor(ITraits traits, IFileNameTraits fileNameTraits)
		{
			Traits_Type = traits;
			FileName_Traits_Type = fileNameTraits;

			m_Data = DataInitializer();
			streamPos = 0;
			m_FileName = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize file reader object with pointer to data and data
		/// length
		/// </summary>
		/********************************************************************/
		public FileCursor(ITraits traits, IFileNameTraits fileNameTraits, byte_span byteData, ISharedFileNameType fileName = null)
		{
			Traits_Type = traits;
			FileName_Traits_Type = fileNameTraits;

			m_Data = DataInitializer(byteData);
			streamPos = 0;
			m_FileName = Utility.move(fileName ?? fileNameTraits.CreateSharedFileName());
		}



		/********************************************************************/
		/// <summary>
		/// Initialize file reader object based on an existing file reader
		/// object window
		/// </summary>
		/********************************************************************/
		public FileCursor(ITraits traits, IFileNameTraits fileNameTraits, IFileData other, ISharedFileNameType fileName = null)
		{
			Traits_Type = traits;
			FileName_Traits_Type = fileNameTraits;

			m_Data = other;
			streamPos = 0;
			m_FileName = Utility.move(fileName ?? fileNameTraits.CreateSharedFileName());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IFileData SharedDataContainer()
		{
			return Traits_Type.Get_Shared(m_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IFileData DataContainer()
		{
			return Traits_Type.Get_Ref(m_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IFileData DataInitializer()
		{
			return Traits_Type.Make_Data();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IFileData DataInitializer(byte_span data)
		{
			return Traits_Type.Make_Data(data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IFileData CreateChunkImpl(IFileData data, size_t position, size_t size)
		{
			return Traits_Type.Make_Chunk(data, position, size);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the object points to a valid (non-empty) stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValid()
		{
			return DataContainer().IsValid();
		}



		/********************************************************************/
		/// <summary>
		/// Reset cursor to first byte in file
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Rewind()
		{
			streamPos = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a position in the mapped file.
		/// Returns false if position is invalid
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Seek(size_t position)
		{
			if (position <= streamPos)
			{
				streamPos = position;
				return true;
			}

			if (DataContainer().CanRead(0, position))
			{
				streamPos = position;
				return true;
			}
			else
				return false;
		}



		/********************************************************************/
		/// <summary>
		/// Increases position by skipBytes.
		/// Returns true if skipBytes could be skipped or false if the file
		/// end was reached earlier
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Skip(size_t skipBytes)
		{
			if (CanRead(skipBytes))
			{
				streamPos += skipBytes;
				return true;
			}
			else
			{
				streamPos = DataContainer().GetLength();
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Decreases position by skipBytes.
		/// Returns true if skipBytes could be skipped or false if the file
		/// start was reached earlier
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SkipBack(size_t skipBytes)
		{
			if (streamPos >= skipBytes)
			{
				streamPos -= skipBytes;
				return true;
			}
			else
			{
				streamPos = 0;
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns cursor position in the mapped file
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t GetPosition()
		{
			return streamPos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns size of the mapped file in bytes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t GetLength()
		{
			// deprecated because in case of an unseekable std::istream, this triggers caching of the whole file
			return DataContainer().GetLength();
		}



		/********************************************************************/
		/// <summary>
		/// Return byte count between cursor position and end of file, i.e.
		/// how many bytes can still be read
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t BytesLeft()
		{
			// deprecated because in case of an unseekable std::istream, this triggers caching of the whole file
			return DataContainer().GetLength() - streamPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool NoBytesLeft()
		{
			return !CanRead(1);
		}



		/********************************************************************/
		/// <summary>
		/// Check if "amount" bytes can be read from the current position in
		/// the stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanRead(size_t amount)
		{
			return DataContainer().CanRead(streamPos, amount);
		}



		/********************************************************************/
		/// <summary>
		/// Check if file size is at least size, without potentially caching
		/// the whole file to query the exact file length
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool LengthIsAtLeast(size_t size)
		{
			return DataContainer().CanRead(0, size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected FileCursor CreateChunk(size_t position, size_t length)
		{
			size_t readableLength = DataContainer().GetReadableLength(position, length);

			if (readableLength == 0)
				return new FileCursor(Traits_Type, FileName_Traits_Type);

			return new FileCursor(Traits_Type, FileName_Traits_Type, CreateChunkImpl(SharedDataContainer(), position, readableLength));
		}



		/********************************************************************/
		/// <summary>
		/// Create a new FileCursor object for parsing a sub chunk at the
		/// current position with a given length.
		/// The file cursor is advanced by "length" bytes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FileCursor ReadChunk(size_t length)
		{
			size_t position = streamPos;

			Skip(length);

			return CreateChunk(position, length);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pinned view into the remaining raw data from cursor
		/// position
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PinnedView GetPinnedView()
		{
			return new PinnedView(this);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pinned view into the remaining raw data from cursor
		/// position, clamped at size
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PinnedView GetPinnedView(size_t size)
		{
			return new PinnedView(this, size);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pinned view into the remaining raw data from cursor
		/// position.
		/// The file cursor is advanced by the size of the returned pinned
		/// view
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PinnedView ReadPinnedView(size_t size)
		{
			return new PinnedView(this, size, true);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pinned view into the remaining raw data from cursor
		/// position.
		/// File cursor is advanced by the size of the returned pinned view
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PinnedView ReadPinnedView()
		{
			return new PinnedView(this, true);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetRaw<T>(Span<T> dst) where T : unmanaged
		{
			return Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> GetRaw<T>(MptSpan<T> dst) where T : unmanaged
		{
			return Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> ReadRaw<T>(MptSpan<T> dst) where T : unmanaged
		{
			MptSpan<T> result = Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
			streamPos += result.Size();

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public vector<uint8> GetRawDataAsByteVector()
		{
			PinnedView view = GetPinnedView();

			return new vector<byte>(view.Begin(), view.End());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public vector<byte> ReadRawDataAsByteVector()
		{
			PinnedView view = ReadPinnedView();

			return new vector<byte>(view.Begin(), view.End());
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(FileCursor destination)
		{
			if ((destination.Traits_Type != Traits_Type) || (destination.FileName_Traits_Type != FileName_Traits_Type))
				throw new ArgumentException();

			destination.m_Data = m_Data;
			destination.streamPos = streamPos;
			destination.m_FileName = m_FileName;
		}
	}
}
