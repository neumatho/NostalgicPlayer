/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Algorithm = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class FileDataUnseekable : IFileData
	{
		private const size_t Quantum_Size = Io.Base.BufferSize_Small;
		private const size_t Buffer_Size = Io.Base.BufferSize_Normal;

		private readonly vector<c_byte> cache = new vector<c_byte>();
		private size_t cacheSize;
		private bool streamFullyCached;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FileDataUnseekable()
		{
			cacheSize = 0;
			streamFullyCached = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool IsValid()
		{
			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool HasPinnedView()
		{
			return true;	// We have the cache which is required for seeking anyway
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override CPointer<byte> GetRawData()
		{
			CacheStream();

			return cache.data();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override size_t GetLength()
		{
			CacheStream();

			return cacheSize;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span Read(size_t pos, byte_span dst)
		{
			CacheStreamUpTo(pos, dst.Size());

			if (pos >= cacheSize)
				return dst.First(0);

			size_t cache_Avail = Math.Min(cacheSize - pos, dst.Size());
			ReadCached(pos, dst.SubSpan(0, cache_Avail).Data().AsSpan());

			return dst.SubSpan(0, cache_Avail);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override byte_span2 Read(size_t pos, byte_span2 dst)
		{
			CacheStreamUpTo(pos, dst.Size());

			if (pos >= cacheSize)
				return dst.First(0);

			size_t cache_Avail = Math.Min(cacheSize - pos, dst.Size());
			ReadCached(pos, dst.Slice(0, (c_int)cache_Avail));

			return dst.Slice(0, (c_int)cache_Avail);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override bool CanRead(size_t pos, size_t length)
		{
			CacheStreamUpTo(pos, length);

			if ((pos == cacheSize) && (length == 0))
				return true;

			if (pos >= cacheSize)
				return false;

			return length <= (cacheSize - pos);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract bool InternalEof();



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract byte_span InternalReadUnseekable(byte_span dst);
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void EnsureCacheBuffer(size_t requiredBufferSize)
		{
			if ((cache.size() - cacheSize) >= requiredBufferSize)
				return;

			if (cache.size() == 0)
				cache.resize(Numeric.Saturate_Align_Up(cacheSize + requiredBufferSize, Buffer_Size));
			else if (Algorithm.Exponential_Grow(cache.size()) < (cacheSize + requiredBufferSize))
				cache.resize(Numeric.Saturate_Align_Up(cacheSize + requiredBufferSize, Buffer_Size));
			else
				cache.resize(Algorithm.Exponential_Grow(cache.size()));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CacheStream()
		{
			if (streamFullyCached)
				return;

			while (!InternalEof())
			{
				EnsureCacheBuffer(Buffer_Size);

				size_t readCount = InternalReadUnseekable(new MptSpan<c_byte>(cache.data() + cacheSize, Buffer_Size)).Size();
				cacheSize += readCount;
			}

			streamFullyCached = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CacheStreamUpTo(size_t pos, size_t length)
		{
			if (streamFullyCached)
				return;

			if (length > (size_t.MaxValue - pos))
				length = size_t.MaxValue - pos;

			size_t target = pos + length;

			if (target <= cacheSize)
				return;

			size_t alignedPos = Numeric.Saturate_Align_Up(target, Quantum_Size);

			while (!InternalEof() && (cacheSize < alignedPos))
			{
				EnsureCacheBuffer(Buffer_Size);

				size_t readCount = InternalReadUnseekable(new MptSpan<c_byte>(cache.data() + cacheSize, Buffer_Size)).Size();
				cacheSize += readCount;
			}

			if (InternalEof())
				streamFullyCached = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ReadCached(size_t pos, byte_span2 dst)
		{
			cache.data().AsSpan(pos, dst.Size()).CopyTo(dst);
		}
		#endregion
	}
}
