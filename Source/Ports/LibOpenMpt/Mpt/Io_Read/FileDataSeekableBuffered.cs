/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io_Read
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class FileDataSeekableBuffered : FileDataSeekable
	{
		private const size_t Chunk_Size = Io.Base.BufferSize_Small;
		private const size_t Buffer_Size = Io.Base.BufferSize_Normal;

		private const size_t Num_Chunks = Buffer_Size / Chunk_Size;

		private struct Chunk_Info
		{
			public size_t ChunkOffset = 0;
			public size_t ChunkLength = 0;
			public bool ChunkValid = false;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Chunk_Info()
			{
			}
		}

		private vector<byte> m_Buffer = new vector<byte>(Buffer_Size);

		private array<Chunk_Info> m_ChunkInfo = new array<Chunk_Info>(Num_Chunks);
		private array<size_t> m_ChunkIndexLru = new array<size_t>(Num_Chunks);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected FileDataSeekableBuffered(size_t streamLength_) : base(streamLength_)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override byte_span InternalReadSeekable(size_t pos, byte_span dst)
		{
			size_t totalRead = 0;
			CPointer<byte> pDst = dst.Data();
			size_t count = dst.Size();

			while (count > 0)
			{
				size_t chunkIndex = InternalFillPageAndReturnIndex(pos);
				size_t pageSkip = pos - m_ChunkInfo[chunkIndex].ChunkOffset;
				size_t chunkWanted = Math.Min(Chunk_Size - pageSkip, count);
				size_t chunkGot = m_ChunkInfo[chunkIndex].ChunkLength > pageSkip ? m_ChunkInfo[chunkIndex].ChunkLength - pageSkip : 0;
				size_t chunk = Math.Min(chunkWanted, chunkGot);

				Algorithm.copy<CPointer<byte>, CPointer<byte>, byte>(Chunk_Data(chunkIndex).Data() + pageSkip, Chunk_Data(chunkIndex).Data() + pageSkip + chunk, pDst);

				pos += chunk;
				pDst += chunk;
				totalRead += chunk;
				count -= chunk;

				if (chunkWanted > chunk)
					return dst.First(totalRead);
			}

			return dst.First(totalRead);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override byte_span2 InternalReadSeekable(size_t pos, byte_span2 dst)
		{
			size_t totalRead = 0;
			byte_span2 pDst = dst;
			size_t count = dst.Size();

			while (count > 0)
			{
				size_t chunkIndex = InternalFillPageAndReturnIndex(pos);
				size_t pageSkip = pos - m_ChunkInfo[chunkIndex].ChunkOffset;
				size_t chunkWanted = Math.Min(Chunk_Size - pageSkip, count);
				size_t chunkGot = m_ChunkInfo[chunkIndex].ChunkLength > pageSkip ? m_ChunkInfo[chunkIndex].ChunkLength - pageSkip : 0;
				size_t chunk = Math.Min(chunkWanted, chunkGot);

				(Chunk_Data(chunkIndex).Data() + pageSkip).AsSpan(chunk).CopyTo(pDst);

				pos += chunk;
				pDst = pDst.Slice((int)chunk);
				totalRead += chunk;
				count -= chunk;

				if (chunkWanted > chunk)
					return dst.First(totalRead);
			}

			return dst.First(totalRead);
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract byte_span InternalReadBuffered(size_t pos, byte_span dst);
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte_span Chunk_Data(size_t chunkIndex)
		{
			return new byte_span(m_Buffer.data() + (chunkIndex * Chunk_Size), Chunk_Size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private size_t InternalFillPageAndReturnIndex(size_t pos)
		{
			pos = Numeric.Align_Down(pos, Chunk_Size);

			for (size_t chunkLruIndex = 0; chunkLruIndex < Num_Chunks; ++chunkLruIndex)
			{
				size_t chunkIndex = m_ChunkIndexLru[chunkLruIndex];

				if (m_ChunkInfo[chunkIndex].ChunkValid && (m_ChunkInfo[chunkIndex].ChunkOffset == pos))
				{
					size_t chunk = Utility.move(m_ChunkIndexLru[chunkLruIndex]);
					Algorithm.move_backward<forward_iterator<size_t>, forward_iterator<size_t>, size_t>(m_ChunkIndexLru.begin(), m_ChunkIndexLru.begin() + chunkLruIndex, m_ChunkIndexLru.begin() + (chunkLruIndex + 1));
					m_ChunkIndexLru[0] = Utility.move(chunk);

					return chunkIndex;
				}
			}

			{
				size_t chunk = Utility.move(m_ChunkIndexLru[Num_Chunks - 1]);
				Algorithm.move_backward<forward_iterator<size_t>, forward_iterator<size_t>, size_t>(m_ChunkIndexLru.begin(), m_ChunkIndexLru.begin() + (Num_Chunks - 1), m_ChunkIndexLru.begin() + Num_Chunks);
				m_ChunkIndexLru[0] = Utility.move(chunk);
			}

			size_t chunkIndex_ = m_ChunkIndexLru[0];
			ref Chunk_Info chunk_ = ref m_ChunkInfo[chunkIndex_];
			chunk_.ChunkOffset = pos;
			chunk_.ChunkLength = InternalReadBuffered(pos, Chunk_Data(chunkIndex_)).Size();
			chunk_.ChunkValid = true;

			return chunkIndex_;
		}
		#endregion
	}
}
