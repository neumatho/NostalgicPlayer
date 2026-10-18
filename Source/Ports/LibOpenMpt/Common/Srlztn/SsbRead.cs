/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using ReadIterator = Polycode.NostalgicPlayer.Kit.C.Std.Iterators.forward_iterator<Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers.ReadEntry>;

using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Utility = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn
{
	/// <summary>
	/// Serializing data from MPTM files
	/// </summary>
	internal class SsbRead : Ssb
	{
		// mapData is a cache that facilitates faster access to the stored data
		// without having to reparse on every access.
		//  Iterator invalidation in CacheMap() is not a problem because every code
		// path that ever returns an iterator into mapData does CacheMap exactly once
		// beforehand. Following calls use this already cached map. As the data is
		// immutable when reading, there is no need to ever invalidate the cache and
		// redo CacheMap()

		private readonly ReaderStream iStrm;

		/// <summary>
		/// Read: Holds entry ids
		/// </summary>
		private readonly vector<uint8> m_IdArray = new vector<uint8>();

		/// <summary>
		/// Read: Contains map information
		/// </summary>
		private readonly vector<ReadEntry> mapData = new vector<ReadEntry>();

		/// <summary>
		/// Read: Version is placed here when reading
		/// </summary>
		private uint64 m_nReadVersion;

		/// <summary>
		/// Read: If map exists, rpos of map begin, else m_rposEndofHdrData
		/// </summary>
		private long m_RPosMapBegin;

		/// <summary>
		/// Read: If map exists, map end position, else pos of end of hdrData
		/// </summary>
		private long m_PosMapEnd;

		/// <summary>
		/// Read: Data begin position
		/// </summary>
		private long m_PosDataBegin;

		/// <summary>
		/// Read: rpos of end of header data
		/// </summary>
		private long m_RPosEndOfHdrData;

		/// <summary>
		/// Read: Number of entries
		/// </summary>
		private size_t m_nReadEntryCount;

		/// <summary>
		/// Read: Hint where to start looking for the next read entry
		/// </summary>
		private size_t m_nNextReadHint;

		/// <summary>
		/// 
		/// </summary>
		public delegate void ReadItemDelegate<T>(ReaderStream iStrm, ref T data, size_t nSize);

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SsbRead(ReaderStream @is)
		{
			iStrm = @is;
			m_nReadVersion = 0;
			m_RPosMapBegin = 0;
			m_PosMapEnd = 0;
			m_PosDataBegin = 0;
			m_RPosEndOfHdrData = 0;
			m_nReadEntryCount = 0;
			m_nNextReadHint = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Read item using default read implementation
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadItem<T>(ref T obj, Id id)
		{
			return ReadItem(ref obj, id, Serialization_Utils.ReadItem);
		}



		/********************************************************************/
		/// <summary>
		/// Read string item using default read implementation. C# has no
		/// template specialization, so this overload takes the place of
		/// ReadItem‹std::string›
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadItem(ref StdString obj, Id id)
		{
			return ReadItem(ref obj, id, Serialization_Utils.ReadItemString);
		}



		/********************************************************************/
		/// <summary>
		/// Read item using given function
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadItem<T>(ref T obj, Id id, ReadItemDelegate<T> func)
		{
			ReadEntry pE = Find(id);
			long pos = iStrm.Position;
			bool entryFound = (pE != null) || !m_Flags[Rwf.RMapHasId];

			if (entryFound)
				func(iStrm, ref obj, pE != null ? pE.nSize : Serialization_Utils.InvalidDataSize);

			OnReadEntry(pE, id, pos);

			return entryFound;
		}



		/********************************************************************/
		/// <summary>
		/// Read item using read iterator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadIterItem<T>(ReadIterator iter, ref T obj)
		{
			return ReadIterItem(iter, ref obj, Serialization_Utils.ReadItem);
		}



		/********************************************************************/
		/// <summary>
		/// Read item using read iterator
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadIterItem<T>(ReadIterator iter, ref T obj, ReadItemDelegate<T> func)
		{
			if (iter.Value.RPosStart != 0)
				iStrm.Seek(m_PosStart + iter.Value.RPosStart, SeekOrigin.Begin);

			long pos = iStrm.Position;
			func(iStrm, ref obj, iter.Value.nSize);

			OnReadEntry(iter.Value, new Id(m_IdArray.data() + iter.Value.nIdPos, iter.Value.nIdLength), pos);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Compares given id with read entry id
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MatchesId(ReadIterator iter, Id id)
		{
			if (iter.Value.nIdPos >= m_IdArray.size())
				return false;

			return id == new Id(m_IdArray.data() + iter.Value.nIdPos, iter.Value.nIdLength);
		}



		/********************************************************************/
		/// <summary>
		/// Returns read iterator to the beginning of entries. The behaviour
		/// of read iterators is undefined if map doesn't contain entry ids
		/// or data begin positions
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadIterator GetReadBegin()
		{
			if (!m_Flags[Rwf.RMapCached])
				CacheMap();

			return mapData.begin();
		}



		/********************************************************************/
		/// <summary>
		/// Returns read iterator to the end (one past last) of entries
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadIterator GetReadEnd()
		{
			if (!m_Flags[Rwf.RMapCached])
				CacheMap();

			return mapData.end();
		}



		/********************************************************************/
		/// <summary>
		/// Call this to begin reading: must be called before other read
		/// functions
		/// </summary>
		/********************************************************************/
		public void BeginRead(Id id, uint64 nVersion)
		{
			ResetReadStatus();

			m_PosStart = iStrm.Position;

			// Start bytes
			{
				uint8[] temp = new uint8[s_EntryId.Length];
				new ArrayReader<uint8>((size_t)s_EntryId.Length).Read(iStrm, temp);

				if (CMemory.memcmp(temp, s_EntryId, (size_t)s_EntryId.Length) != 0)
				{
					AddReadNote(Serialization_Utils.Snr_StartByte_Mismatch);
					return;
				}
			}

			// Compare IDs
			Serialization_Utils.BinaryRead(iStrm, out uint8 storedIdLen);
			array<uint8> storedIdBuf = new array<uint8>(256);

			if (storedIdLen > 0)
				iStrm.ReadInto(storedIdBuf.data().AsSpan(storedIdLen));

			if (!(id == new Id(storedIdBuf.data(), storedIdLen)))
				AddReadNote(Serialization_Utils.Snr_ObjectClass_IdMismatch);

			if (HasFailed())
				return;

			// Header
			Serialization_Utils.BinaryRead(iStrm, out uint8 tempU8);
			uint8 header = tempU8;
			m_nIdBytes = (uint16)(((header & 3) == 3) ? 4 : (header & 3));

			if (Serialization_Utils.TestBit(header, 6))
				m_Flags[Rwf.RTwoBytesDescChar] = true;

			// Read header data size
			Io_.ReadAdaptiveInt32LE(iStrm, out uint32 tempU32);
			uint32 headerDataSize = tempU32;

			// If headerdatasize != 0, read known headerdata and ignore rest
			uint8 flagByte = s_DefaultFlagByte;

			if (headerDataSize >= 2)
			{
				Serialization_Utils.BinaryRead(iStrm, out tempU8);

				if (tempU8 == Serialization_Utils.HeaderId_FlagByte)
					Serialization_Utils.BinaryRead(iStrm, out flagByte);

				iStrm.Seek((tempU8 == Serialization_Utils.HeaderId_FlagByte) ? headerDataSize - 2 : headerDataSize - 1, SeekOrigin.Current);
			}

			uint64 tempU64 = 0;

			// Read version numeric if available
			if (Serialization_Utils.TestBit(header, 4))
			{
				Io_.ReadAdaptiveInt64LE(iStrm, out tempU64);
				m_nReadVersion = tempU64;

				if (tempU64 > nVersion)
					AddReadNote(Serialization_Utils.Snr_Loading_Object_With_Larger_Version);
			}

			if (Serialization_Utils.TestBit(header, 5))
			{
				Serialization_Utils.BinaryRead(iStrm, out tempU8);
				iStrm.Seek(tempU8, SeekOrigin.Current);
			}

			if (Serialization_Utils.TestBit(flagByte, 0))   // Custom ID?
			{
				Serialization_Utils.BinaryRead(iStrm, out tempU8);

				if ((tempU8 & 1) != 0)
					m_nIdBytes = Serialization_Utils.IdSizeVariable;
				else
					m_nIdBytes = (uint16)(tempU8 >> 1);

				if (m_nIdBytes == 0)
					AddReadNote(Serialization_Utils.Snr_No_EntryIds_With_CustomId_Defined);
			}

			m_nFixedEntrySize = 0;

			if (Serialization_Utils.TestBit(flagByte, 1))   // Fixedsize entries?
				Io_.ReadAdaptiveInt32LE(iStrm, out m_nFixedEntrySize);

			m_Flags[Rwf.RMapHasStartPos] = Serialization_Utils.TestBit(header, 2);
			m_Flags[Rwf.RMapHasSize] = Serialization_Utils.TestBit(header, 3);
			m_Flags[Rwf.RMapHasId] = m_nIdBytes > 0;
			m_Flags[Rwf.RMapHasDesc] = Serialization_Utils.TestBit(header, 7);
			m_Flags[Rwf.RwHasMap] = m_Flags[Rwf.RMapHasId] || m_Flags[Rwf.RMapHasStartPos] || m_Flags[Rwf.RMapHasSize] || m_Flags[Rwf.RMapHasDesc];

			if (Serialization_Utils.TestBit(flagByte, 2))	// Object description?
			{
				Io_.ReadAdaptiveInt16LE(iStrm, out uint16 size);
				iStrm.Seek(size * (m_Flags[Rwf.RTwoBytesDescChar] ? 2 : 1), SeekOrigin.Current);
			}

			if (Serialization_Utils.TestBit(flagByte, 3))
				iStrm.Seek(5, SeekOrigin.Current);

			// Read entrycount
			Io_.ReadAdaptiveInt64LE(iStrm, out tempU64);

			if (tempU64 > 16000)
			{
				// The current code can only write 16383 entries because it uses a Adaptive64LE with a fixed size=2
				// Additionally, 16000 is an arbitrary limit to avoid an out-of-memory DoS when caching the map
				AddReadNote(Serialization_Utils.Snr_Too_Many_Entries_To_Read);
				return;
			}

			m_nReadEntryCount = tempU64;

			if (m_nReadEntryCount == 0)
				AddReadNote(Serialization_Utils.Snr_ZeroEntryCount);

			// Read map rpos if map exists
			if (m_Flags[Rwf.RwHasMap])
			{
				Io_.ReadAdaptiveInt64LE(iStrm, out tempU64);

				if (!Utility.In_Range<long, uint64>(tempU64))
				{
					AddReadNote(Serialization_Utils.Snr_Insufficient_Stream_OffType);
					return;
				}
			}

			long rawEndOfHdrData = iStrm.Position - m_PosStart;

			if (rawEndOfHdrData < 0)
			{
				AddReadNote(Serialization_Utils.Snr_Insufficient_RPosType);
				return;
			}

			m_RPosEndOfHdrData = rawEndOfHdrData;
			m_RPosMapBegin = m_Flags[Rwf.RwHasMap] ? (long)tempU64 : m_RPosEndOfHdrData;

			if (!m_Flags[Rwf.RwHasMap])
				m_PosMapEnd = m_PosStart + m_RPosEndOfHdrData;

			m_Flags[Rwf.RHeaderIsRead] = true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AddReadNote(Status s)
		{
			m_Status.Level |= s.Level;
			m_Status.Messages |= s.Messages;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ResetReadStatus()
		{
			m_Status = new Status();
			m_IdArray.reserve(32);
			m_IdArray.push_back(0);
		}



		/********************************************************************/
		/// <summary>
		/// Called after reading an object
		/// </summary>
		/********************************************************************/
		private void OnReadEntry(ReadEntry pE, Id id, long posReadBegin)
		{
			bool entryFound = (pE != null) || !m_Flags[Rwf.RMapHasId];

			if (entryFound)
				m_nCounter++;
		}



		/********************************************************************/
		/// <summary>
		/// Reads map to cache
		/// </summary>
		/********************************************************************/
		private void CacheMap()
		{
			if (m_Flags[Rwf.RwHasMap] || (m_nFixedEntrySize > 0))
			{
				iStrm.Seek(m_PosStart + m_RPosMapBegin, SeekOrigin.Begin);

				if (iStrm.EndOfStream)
				{
					AddReadNote(Serialization_Utils.Snr_BadStream_After_MapHeaderSeek);
					return;
				}

				mapData.resize(m_nReadEntryCount);
				m_IdArray.reserve(m_nReadEntryCount * 4);

				// Read map
				for (size_t i = 0; i < m_nReadEntryCount; i++)
				{
					if (iStrm.EndOfStream)
					{
						AddReadNote(Serialization_Utils.Snr_BadStream_At_Map_Read);
						return;
					}

					// Read ID
					uint16 nIdSize = m_nIdBytes;

					if (nIdSize == Serialization_Utils.IdSizeVariable)	// Variablesize ID
						Io_.ReadAdaptiveInt16LE(iStrm, out nIdSize);

					size_t nOldEnd = m_IdArray.size();

					if ((nIdSize > 0) && ((size_t.MaxValue - nOldEnd) >= nIdSize))
					{
						m_IdArray.resize(nOldEnd + nIdSize);
						iStrm.ReadInto(m_IdArray.data().AsSpan(nOldEnd, nIdSize));
					}

					mapData[i].nIdLength = nIdSize;
					mapData[i].nIdPos = nOldEnd;

					// Read position
					if (m_Flags[Rwf.RMapHasStartPos])
					{
						Io_.ReadAdaptiveInt64LE(iStrm, out uint64 tempU64);

						if (!Utility.In_Range<long, uint64>(tempU64))
						{
							AddReadNote(Serialization_Utils.Snr_Insufficient_Stream_OffType);
							return;
						}

						mapData[i].RPosStart = (long)tempU64;
					}

					// Read entry size
					if (m_nFixedEntrySize > 0)
						mapData[i].nSize = m_nFixedEntrySize;
					else if (m_Flags[Rwf.RMapHasSize])	// Map has datasize field
					{
						Io_.ReadAdaptiveInt64LE(iStrm, out uint64 tempU64);

						if (!Utility.In_Range<long, uint64>(tempU64))
						{
							AddReadNote(Serialization_Utils.Snr_Insufficient_Stream_OffType);
							return;
						}

						mapData[i].nSize = tempU64;
					}

					// If there's no entry startpos in map, count start pos from datasizes.
					// Here readentry.rposStart is set to relative position from databegin
					if ((mapData[i].nSize != Serialization_Utils.InvalidDataSize) && !m_Flags[Rwf.RMapHasStartPos])
						mapData[i].RPosStart = (i > 0) ? mapData[i - 1].RPosStart + (long)mapData[i - 1].nSize : 0;

					if (m_Flags[Rwf.RMapHasDesc])	// Map has entrydescriptions?
					{
						Io_.ReadAdaptiveInt16LE(iStrm, out uint16 size);

						if (m_Flags[Rwf.RTwoBytesDescChar])
							iStrm.Seek(size * 2, SeekOrigin.Current);
						else
							iStrm.Seek(size, SeekOrigin.Current);
					}
				}

				m_PosMapEnd = iStrm.Position;
			}

			m_Flags[Rwf.RMapCached] = true;
			m_PosDataBegin = (m_RPosMapBegin == m_RPosEndOfHdrData) ? m_PosMapEnd : m_PosStart + m_RPosEndOfHdrData;
			iStrm.Seek(m_PosDataBegin, SeekOrigin.Begin);

			// If there are no positions in the map but there are entry sizes, rposStart will
			// be relative to data start. Now that posDataBegin is known, make them relative to 
			// startpos
			if (!m_Flags[Rwf.RMapHasStartPos] && (m_Flags[Rwf.RMapHasSize] || (m_nFixedEntrySize > 0)))
			{
				long offset = m_PosDataBegin - m_PosStart;

				for (size_t i = 0; i < m_nReadEntryCount; i++)
					mapData[i].RPosStart += offset;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Searches for entry with given ID. If found, returns pointer to
		/// corresponding entry, else returns null
		/// </summary>
		/********************************************************************/
		private ReadEntry Find(Id id)
		{
			if (!m_Flags[Rwf.RMapCached])
				CacheMap();

			if ((m_nFixedEntrySize > 0) && !m_Flags[Rwf.RMapHasStartPos] && !m_Flags[Rwf.RMapHasSize])
				iStrm.Seek(m_PosDataBegin + (long)(m_nFixedEntrySize * m_nCounter), SeekOrigin.Begin);

			if (m_Flags[Rwf.RMapHasId])
			{
				size_t nEntries = mapData.size();

				for (size_t i0 = 0; i0 < nEntries; i0++)
				{
					size_t i = (i0 + m_nNextReadHint) % nEntries;

					if ((mapData[i].nIdPos < m_IdArray.size()) && (id == new Id(m_IdArray.data() + mapData[i].nIdPos, mapData[i].nIdLength)))
					{
						m_nNextReadHint = (i + 1) % nEntries;

						if (mapData[i].RPosStart != 0)
							iStrm.Seek(m_PosStart + mapData[i].RPosStart, SeekOrigin.Begin);

						return mapData[i];
					}
				}
			}

			return null;
		}
		#endregion
	}
}
