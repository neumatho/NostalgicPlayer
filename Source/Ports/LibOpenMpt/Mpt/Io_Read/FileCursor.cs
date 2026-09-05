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
			public PinnedView(FileCursor file, size_t size)
			{
				Init(file, size);
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public byte_span GetSpan()//XX 298
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
			public byte_span Span()//XX 308
			{
				return GetSpan();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public CPointer<byte> Data()//XX 316
			{
				return Span().Data();
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public size_t Size()//XX 319
			{
				return size_;
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
		public FileCursor(ITraits traits, IFileNameTraits fileNameTraits, byte_span byteData, ISharedFileNameType fileName = null)//XX 94
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
		public FileCursor(ITraits traits, IFileNameTraits fileNameTraits, IFileData other, ISharedFileNameType fileName = null)//XX 102
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
		public IFileData DataContainer()//XX 61
		{
			return Traits_Type.Get_Ref(m_Data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IFileData DataInitializer()//XX 65
		{
			return Traits_Type.Make_Data();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected IFileData DataInitializer(byte_span data)//XX 68
		{
			return Traits_Type.Make_Data(data);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the object points to a valid (non-empty) stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValid()//XX 120
		{
			return DataContainer().IsValid();
		}



		/********************************************************************/
		/// <summary>
		/// Seek to a position in the mapped file.
		/// Returns false if position is invalid
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Seek(size_t position)//XX 131
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
		public bool Skip(size_t skipBytes)//XX 146
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
		public bool SkipBack(size_t skipBytes)//XX 158
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
		public size_t GetPosition()//XX 169
		{
			return streamPos;
		}



		/********************************************************************/
		/// <summary>
		/// Returns size of the mapped file in bytes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t GetLength()//XX 181
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
		public size_t BytesLeft()//XX 187
		{
			// deprecated because in case of an unseekable std::istream, this triggers caching of the whole file
			return DataContainer().GetLength() - streamPos;
		}



		/********************************************************************/
		/// <summary>
		/// Check if "amount" bytes can be read from the current position in
		/// the stream
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanRead(size_t amount)//XX 201
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
		public bool LengthIsAtLeast(size_t size)//XX 206
		{
			return DataContainer().CanRead(0, size);
		}



		/********************************************************************/
		/// <summary>
		/// Returns a pinned view into the remeining raw data from cursor
		/// position, clamped at size
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PinnedView GetPinnedView(size_t size)//XX 341
		{
			return new PinnedView(this, size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetRaw<T>(Span<T> dst) where T : unmanaged//XX 362
		{
			return Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> GetRaw<T>(MptSpan<T> dst) where T : unmanaged//XX 362
		{
			return Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MptSpan<T> ReadRaw<T>(MptSpan<T> dst) where T : unmanaged//XX 366
		{
			MptSpan<T> result = Memory.Byte_Cast<c_byte, T>(DataContainer().Read(streamPos, Memory.Byte_Cast<T, c_byte>(dst)));
			streamPos += result.Size();

			return result;
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
