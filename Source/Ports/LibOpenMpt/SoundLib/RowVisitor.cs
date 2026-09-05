/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using ChannelStates = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base.MptSpan<Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.ModChannel>;
using LoopStateSet = Polycode.NostalgicPlayer.Kit.C.Std.vector<Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.RowVisitor.LoopState>;

using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Class for recording which rows of a song has already been visited, used for detecting when a module starts to loop.
	/// Notes  : The class keeps track of rows that have been visited by the player before.
	///          This way, we can tell when the module starts to loop, i.e. we can determine the song length,
	///          or find out that a given point of the module can never be reached.
	///
	///          In some module formats, infinite loops can be achieved through pattern loops (e.g. E60 / E61 / E61 in one channel of a ProTracker MOD).
	///          To detect such loops, we store a set of loop counts across all channels encountered for each row.
	///          As soon as a set of loop counts is encountered twice for a specific row, we know that the track ends up in an infinite loop.
	///          As a result of this design, it is safe to evaluate pattern loops in CSoundFile::GetLength.
	/// </summary>
	internal class RowVisitor
	{
		#region LoopState class
		/// <summary>
		/// 
		/// </summary>
		public class LoopState : IEquatable<LoopState>
		{
			private const uint64 FNV1a_Basis = 14695981039346656037UL;
			private const uint64 FNV1a_Prime = 1099511628211UL;

			private uint64 m_Hash = FNV1a_Basis;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public LoopState()
			{
			}



			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public LoopState(ChannelStates chnState, bool ignoreRow)
			{
				// Rather than storing the exact loop count vector, we compute an FNV-1a 64-bit hash of it.
				// This means we can store the loop state in a small and fixed amount of memory.
				// In theory there is the possibility of hash collisions for different loop states, but in practice,
				// the relevant inputs for the hashing algorithm are extremely unlikely to produce collisions.
				// There may be better hashing algorithms, but many of them are much more complex and cannot be applied easily in an incremental way
				uint64 hash = FNV1a_Basis;

				if (ignoreRow)
					hash = unchecked((hash ^ 0xffU) * FNV1a_Prime);

				for (size_t chn = 0; chn < chnState.Size(); chn++)
				{
					if (chnState[chn].nPatternLoopCount != 0)
					{
						hash = (hash ^ chn) * FNV1a_Prime;
						hash = (hash ^ chnState[chn].nPatternLoopCount) * FNV1a_Prime;
					}
				}

				m_Hash = hash;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool HasLoops()
			{
				return m_Hash != FNV1a_Basis;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool operator == (LoopState me, LoopState other)
			{
				if (ReferenceEquals(me, other))
					return true;

				if ((me is null) || (other is null))
					return false;

				return me.m_Hash == other.m_Hash;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static bool operator != (LoopState me, LoopState other)
			{
				return !(me == other);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override bool Equals(object obj)
			{
				return (obj is LoopState other) && (this == other);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool Equals(LoopState other)
			{
				return this == other;
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			public override int GetHashCode()
			{
				return HashCode.Combine(m_Hash);
			}
		}
		#endregion

		protected readonly CSoundFile m_SndFile;
		protected RowIndex m_RowsSpentInLoops = 0;
		protected readonly SequenceIndex m_Sequence;

		/// <summary>
		/// Stores for every (order, row) combination in the sequence if it
		/// has been visited or not
		/// </summary>
		protected vector<vector<bool>> m_VisitedRows = new vector<vector<bool>>();

		/// <summary>
		/// Map for each row that's part of a pattern loop which loop states
		/// have been visited. Held in a separate data structure because it
		/// is sparse data in typical modules
		/// </summary>
		protected map<pair<OrderIndex, RowIndex>, LoopStateSet> m_VisitedLoopStates = new map<pair<OrderIndex, RowIndex>, LoopStateSet>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public RowVisitor(CSoundFile sndFile, SequenceIndex sequence = Snd_Def.SequenceIndex_Invalid)//XX 57
		{
			m_SndFile = sndFile;
			m_Sequence = sequence;

			Initialize(true);
		}



		/********************************************************************/
		/// <summary>
		/// Pattern loops can stack up exponentially, which can cause an
		/// effectively infinite amount of time to be spent on evaluating
		/// them. If this function returns true, module evaluation should be
		/// aborted because the pattern loops appear to be too complex
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ModuleTooComplex(RowIndex threshold)
		{
			return m_RowsSpentInLoops >= threshold;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetComplexity()
		{
			m_RowsSpentInLoops = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void MoveVisitedRowsFrom(RowVisitor other)//XX 65
		{
			m_VisitedRows = Utility.move(other.m_VisitedRows);
			m_VisitedLoopStates = Utility.move(other.m_VisitedLoopStates);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected ModSequence Order()//XX 72
		{
			if (m_Sequence >= m_SndFile.Order.GetNumSequences())
				return m_SndFile.Order.Current;
			else
				return m_SndFile.Order[m_Sequence];
		}



		/********************************************************************/
		/// <summary>
		/// Resize / clear the row vector.
		/// If reset is true, the vector is not only resized to the required
		/// dimensions, but also completely cleared (i.e. all visited rows
		/// are reset)
		/// </summary>
		/********************************************************************/
		public void Initialize(bool reset)//XX 83
		{
			ModSequence order = Order();
			OrderIndex endOrder = order.GetLengthTailTrimmed();
			bool reserveLoopStates = true;
			m_VisitedRows.resize(endOrder);

			if (reset)
			{
				reserveLoopStates = m_VisitedLoopStates.empty();

				foreach (pair<pair<OrderIndex, RowIndex>, LoopStateSet> loopState in m_VisitedLoopStates)
					loopState.second.clear();

				m_RowsSpentInLoops = 0;
			}

			vector<uint8> loopCount = new vector<uint8>();
			vector<OrderIndex> visitedPatterns = new vector<OrderIndex>(m_SndFile.Patterns.GetNumPatterns(), Snd_Def.OrderIndex_Invalid);

			for (OrderIndex ord = 0; ord < endOrder; ord++)
			{
				PatternIndex pat = order[ord];
				RowIndex numRows = VisitedRowsVectorSize(pat);
				vector<bool> visitedRows = m_VisitedRows[ord];

				if (reset)
					visitedRows.assign(numRows, false);
				else
					visitedRows.resize(numRows, false);

				if (!reserveLoopStates || !order.IsValidPat(ord))
					continue;

				RowIndex startRow = (RowIndex)Math.Min(reset ? 0 : visitedRows.size(), numRows);
				var insertionHint = m_VisitedLoopStates.end();

				if (visitedPatterns[pat] != Snd_Def.OrderIndex_Invalid)
				{
					// We visited this pattern before, copy over the results
					var begin = m_VisitedLoopStates.lower_bound(pair.make_pair(visitedPatterns[pat], startRow));
					var end = begin != m_VisitedLoopStates.end() ? m_VisitedLoopStates.lower_bound(pair.make_pair(visitedPatterns[pat], numRows)) : m_VisitedLoopStates.end();

					for (var pos = begin; pos != end; ++pos)
					{
						LoopStateSet loopStates = new LoopStateSet();
						loopStates.reserve(pos.second.capacity());

						insertionHint = m_VisitedLoopStates.insert_or_assign(insertionHint, pair.make_pair(ord, pos.first.second), Utility.move(loopStates)).Next();
					}

					continue;
				}

				// Pre-allocate loop count state
				CPattern pattern = m_SndFile.Patterns[pat];
				loopCount.assign(pattern.GetNumChannels(), 0);

				for (RowIndex i = numRows; i != startRow; i--)
				{
					RowIndex row = i - 1;
					uint32 maxLoopStates = 1;
					CPointer<ModCommand> mp = pattern.GetpModCommand(row, 0);

					// Break condition: If it's more than 16, it's probably wrong :) exact loop count depends on how loops overlap
					for (ChannelIndex chn = 0; (chn < pattern.GetNumChannels()) && (maxLoopStates < 16); chn++, mp++)
					{
						ModCommand m = mp[0];
						uint8 count = loopCount[chn];

						if (((m.Command == EffectCommand.S3MCmdEx) && ((m.Param & 0xf0) == 0xb0)) || ((m.Command == EffectCommand.ModCmdEx) && ((m.Param & 0xf0) == 0x60)))
						{
							loopCount[chn] = (uint8)(m.Param & 0x0f);

							if (loopCount[chn] != 0)
								count = loopCount[chn];
						}

						if (count != 0)
							maxLoopStates *= (uint32)(count + 1);
					}

					if (maxLoopStates > 1)
					{
						LoopStateSet loopStates = new LoopStateSet();
						loopStates.reserve(maxLoopStates);

						insertionHint = m_VisitedLoopStates.insert_or_assign(insertionHint, pair.make_pair(ord, row), Utility.move(loopStates));
					}
				}

				// Only use this order as a blueprint for other orders using the same pattern if we fully parsed the pattern
				if (startRow == 0)
					visitedPatterns[pat] = ord;
			}
		}



		/********************************************************************/
		/// <summary>
		/// TNE: Small wrapper to convert array to MptSpan
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Visit(OrderIndex ord, RowIndex row, array<ModChannel> chnState, bool ignoreRow)
		{
			return Visit(ord, row, new ChannelStates(chnState), ignoreRow);
		}



		/********************************************************************/
		/// <summary>
		/// Mark an order/row combination as visited and returns true if it
		/// was visited before
		/// </summary>
		/********************************************************************/
		public bool Visit(OrderIndex ord, RowIndex row, ChannelStates chnState, bool ignoreRow)//XX 168
		{
			ModSequence order = Order();

			if ((ord >= order.size()) || (row >= VisitedRowsVectorSize(order[ord])))
				return false;

			// The module might have been edited in the meantime - so we have to extend this a bit
			if ((ord >= m_VisitedRows.size()) || (row >= m_VisitedRows[ord].size()))
			{
				Initialize(false);

				// If it's still past the end of the vector, this means that
				// ord >= order.GetLengthTailTrimmed(), i.e. we are trying to play an empty order
				if (ord >= m_VisitedRows.size())
					return false;
			}

			LoopState newState = new LoopState(chnState.First(m_SndFile.GetNumChannels()), ignoreRow);
			map<pair<OrderIndex, RowIndex>, LoopStateSet>.iterator rowLoopState = m_VisitedLoopStates.find(new pair<OrderIndex, RowIndex>(ord, row));
			bool oldHasLoops = (rowLoopState != m_VisitedLoopStates.end()) && !rowLoopState.second.empty();
			bool newHasLoops = newState.HasLoops();
			bool wasVisited = m_VisitedRows[ord][row];

			// Check if new state is part of row state already. If so, we visited this row
			// already and thus the module must be looping
			if (!oldHasLoops && !newHasLoops && wasVisited)
				return true;

			if (oldHasLoops && Mpt.Base.Algorithm.Contains(rowLoopState.second, newState))
				return true;

			if (newHasLoops)
				m_RowsSpentInLoops++;

			if (oldHasLoops || newHasLoops)
			{
				// Convert to set representation if it isn't already
				if (!oldHasLoops && wasVisited)
					m_VisitedLoopStates[new pair<OrderIndex, RowIndex>(ord, row)].emplace_back(new LoopState());

				m_VisitedLoopStates[new pair<OrderIndex, RowIndex>(ord, row)].emplace_back(Utility.move(newState));
			}

			m_VisitedRows[ord][row] = true;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Find the first row that has not been played yet.
		/// The order and row is stored in the order and row variables on
		/// success, on failure they contain invalid values.
		/// If onlyUnplayedPatterns is true (default), only completely
		/// unplayed patterns are considered, otherwise a song can start on
		/// any unplayed row.
		/// Function returns true on success
		/// </summary>
		/********************************************************************/
		public bool GetFirstUnvisitedRow(out OrderIndex ord, out RowIndex row, bool onlyUnplayedPatterns)//XX 225
		{
			ModSequence order = Order();
			OrderIndex endOrder = order.GetLengthTailTrimmed();

			for (OrderIndex o = 0; o < endOrder; o++)
			{
				if (!order.IsValidPat(o))
					continue;

				if (o >= m_VisitedRows.size())
				{
					// Not yet initialized => unvisited
					ord = o;
					row = 0;

					return true;
				}

				vector<bool> visitedRows = m_VisitedRows[o];
				RowIndex firstUnplayedRow = 0;

				for (; firstUnplayedRow < visitedRows.size(); firstUnplayedRow++)
				{
					if (visitedRows[firstUnplayedRow] == onlyUnplayedPatterns)
						break;
				}

				if (onlyUnplayedPatterns && (firstUnplayedRow == visitedRows.size()))
				{
					// No row of this pattern has been played yet
					ord = o;
					row = 0;

					return true;
				}
				else if (!onlyUnplayedPatterns)
				{
					// Return the first unplayed row in this pattern
					if (firstUnplayedRow < visitedRows.size())
					{
						ord = o;
						row = firstUnplayedRow;

						return true;
					}

					if (visitedRows.size() < m_SndFile.Patterns[order[o]].GetNumRows())
					{
						// History is not fully initialized
						ord = o;
						row = (RowIndex)visitedRows.size();

						return true;
					}
				}
			}

			// Didn't find anything :(
			ord = Snd_Def.OrderIndex_Invalid;
			row = Snd_Def.RowIndex_Invalid;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Get the needed vector size for a given pattern
		/// </summary>
		/********************************************************************/
		protected RowIndex VisitedRowsVectorSize(PatternIndex pattern)//XX 212
		{
			if (m_SndFile.Patterns.IsValidPat(pattern))
				return m_SndFile.Patterns[pattern].GetNumRows();
			else
				return 1;	// Non-existing patterns consist of a "fake" row
		}
	}
}
