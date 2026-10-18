/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Order and sequence handling
	/// </summary>
	internal class ModSequence : vector<PatternIndex>, IEquatable<ModSequence>, IDeepCloneable<ModSequence>
	{
		public const string FileIdSequences = "mptSeqC";
		private const string FileIdSequence = "mptSeq";

		/// <summary>
		/// Sequence name
		/// </summary>
		protected string m_Name;

		/// <summary>
		/// Associated CSoundFile
		/// </summary>
		protected readonly CSoundFile m_SndFile;

		/// <summary>
		/// Restart position when playback of this order ended
		/// </summary>
		protected OrderIndex m_RestartPos = 0;

		/// <summary>
		/// Default tempo at start of sequence
		/// </summary>
		protected Tempo m_DefaultTempo = new Tempo(125, 0);

		/// <summary>
		/// Default ticks per row at start of sequence
		/// </summary>
		protected uint32 m_DefaultSpeed = 6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModSequence(CSoundFile sndFile)
		{
			m_SndFile = sndFile;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OrderIndex GetLength()
		{
			return OrderIndex.CreateSaturating(size());
		}



		/********************************************************************/
		/// <summary>
		/// Replaces all occurences of oldPat with newPat
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Replace(PatternIndex oldPat, PatternIndex newPat)
		{
			if (oldPat != newPat)
				Algorithm.replace(begin(), end(), oldPat, newPat);
		}



		/********************************************************************/
		/// <summary>
		/// Removes any "---" patterns at the end of the list
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Shrink()
		{
			resize(GetLengthTailTrimmed());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetName(string newName)
		{
			m_Name = Utility.move(newName);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetName()
		{
			return m_Name;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetRestartPos(OrderIndex restartPos)
		{
			m_RestartPos = restartPos;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OrderIndex GetRestartPos()
		{
			return m_RestartPos;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDefaultTempoInt(uint32 tempo)
		{
			SetDefaultTempo(new Tempo(uint16.CreateSaturating(tempo), 0));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Tempo GetDefaultTempo()
		{
			return m_DefaultTempo;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetDefaultSpeed(uint32 speed)
		{
			m_DefaultSpeed = speed != 0 ? speed : 6;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 GetDefaultSpeed()
		{
			return m_DefaultSpeed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetDefaultTempo(Tempo tempo)
		{
			if (tempo.GetInt() == 0)
				tempo.Set(125);
			else
				tempo = tempo > new Tempo(uint16.MaxValue, 0) ? new Tempo(uint16.MaxValue, 0) : tempo;

			m_DefaultTempo = tempo;
		}



		/********************************************************************/
		/// <summary>
		/// Returns length of sequence without counting trailing '---' items
		/// </summary>
		/********************************************************************/
		public OrderIndex GetLengthTailTrimmed()
		{
			if (empty())
				return 0;

			reverse_iterator<PatternIndex> last = Algorithm.find_if(rbegin(), rend(), (PatternIndex pat) => pat != Snd_Def.PatternIndex_Invalid);

			return (OrderIndex)Iterator.distance(begin(), last.@base());
		}



		/********************************************************************/
		/// <summary>
		/// Remove all references to a given pattern index from the order
		/// list. Jump commands are updated accordingly
		/// </summary>
		/********************************************************************/
		public void RemovePattern(PatternIndex pat)
		{
			// First, calculate the offset that needs to be applied to jump commands
			OrderIndex orderLength = GetLengthTailTrimmed();
			vector<OrderIndex> newPosition = new vector<OrderIndex>(orderLength);
			OrderIndex maxJump = 0;

			for (OrderIndex i = 0; i < orderLength; i++)
			{
				newPosition[i] = (OrderIndex)(i - maxJump);

				if (this[i] == pat)
					maxJump++;
			}

			if (maxJump == 0)
				return;

			erase(Algorithm.remove(begin(), end(), pat), end());

			// Only apply to patterns actually found in this sequence
			foreach (PatternIndex p in this)
			{
				if (!m_SndFile.Patterns.IsValidPat(p))
					continue;

				foreach (ModCommand m in m_SndFile.Patterns[p])
				{
					if ((m.Command == EffectCommand.PositionJump) && (m.Param < newPosition.size()))
						m.Param = (ModCommandParam)newPosition[m.Param];
				}
			}

			if (m_RestartPos < newPosition.size())
				m_RestartPos = newPosition[m_RestartPos];
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void assign(OrderIndex newSize, PatternIndex pat)
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public OrderIndex insert(OrderIndex pos, OrderIndex count)
		{
			return insert(pos, count, Snd_Def.PatternIndex_Invalid, true);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public OrderIndex insert(OrderIndex pos, OrderIndex count, PatternIndex fill, bool enforeFormatLimits = true)
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// Check if pattern at sequence position ord is valid
		/// </summary>
		/********************************************************************/
		public bool IsValidPat(OrderIndex ord)
		{
			if (ord < size())
				return m_SndFile.Patterns.IsValidPat(this[ord]);

			return false;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void push_back()
		{
			push_back(Snd_Def.PatternIndex_Invalid);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new void push_back(PatternIndex pat)
		{
			if (GetLength() < Snd_Def.Max_Orders)
				base.push_back(pat);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void resize(OrderIndex newSize)
		{
			resize(newSize, Snd_Def.PatternIndex_Invalid);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void resize(OrderIndex newSize, PatternIndex pat)
		{
			base.resize(Math.Min(Snd_Def.Max_Orders, newSize), pat);
		}



		/********************************************************************/
		/// <summary>
		/// Find an order item that contains a given pattern number
		/// </summary>
		/********************************************************************/
		public OrderIndex FindOrder(PatternIndex pat, OrderIndex startSearchAt, bool searchForward = true)
		{
			OrderIndex length = GetLength();

			if (startSearchAt >= length)
				return Snd_Def.OrderIndex_Invalid;

			OrderIndex ord = startSearchAt;

			for (OrderIndex p = 0; p < length; p++)
			{
				if (this[ord] == pat)
					return ord;

				if (searchForward)
				{
					if (++ord >= length)
						ord = 0;
				}
				else
				{
					if (ord-- == 0)
						ord = (OrderIndex)(length - 1);
				}
			}

			return Snd_Def.OrderIndex_Invalid;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static void ReadModSequenceOld(Stream iStrm, ref ModSequenceSet seq, size_t _)
		{
			Io_.ReadIntLE(iStrm, out uint16 size);

			if (size > ModSpecs.Mptm.OrdersMax)
				size = ModSpecs.Mptm.OrdersMax;

			seq[0].resize(size);

			for (size_t i = 0; i < seq[0].size(); i++)
			{
				Io_.ReadIntLE(iStrm, out uint16 temp);
				seq[0][i] = temp;
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static void ReadModSequence(ReaderStream iStrm, ModSequence seq, size_t _, Encoding defaultCharset)
		{
			SsbRead ssb = new SsbRead(iStrm);
			ssb.BeginRead(FileIdSequence, Version.Current().GetRawVersion());

			if (ssb.HasFailed())
				return;

			int8 useUtf8 = 0;
			ssb.ReadItem(ref useUtf8, "u");

			StdString str = new StdString();
			ssb.ReadItem(ref str, "n");
			seq.SetName((useUtf8 != 0 ? Encoding.UTF8 : defaultCharset).GetString(str.Span()));

			OrderIndex nSize = 0;
			ssb.ReadItem(ref nSize, "l");
			OpenMpt.LimitMax(ref nSize, ModSpecs.Mptm.OrdersMax);

			ssb.ReadItem(ref seq, "a", (strm, ref data, size) => new VectorReader<uint16>(nSize).Read(strm, data));

			OrderIndex restartPos = Snd_Def.OrderIndex_Invalid;
			if (ssb.ReadItem(ref restartPos, "r") && (restartPos < nSize))
				seq.SetRestartPos(restartPos);

			uint32 defaultTempo = 0;
			if (ssb.ReadItem(ref defaultTempo, "t") && (defaultTempo > 0))
			{
				Tempo tempo = new Tempo();
				tempo.SetRaw(defaultTempo);
				seq.SetDefaultTempo(tempo);
			}

			uint32 defaultSpeed = 0;
			if (ssb.ReadItem(ref defaultSpeed, "s") && (defaultSpeed > 0))
				seq.SetDefaultSpeed(defaultSpeed);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static void ReadModSequences(ReaderStream iStrm, ModSequenceSet seq, size_t _, Encoding defaultCharset)
		{
			SsbRead ssb = new SsbRead(iStrm);
			ssb.BeginRead(FileIdSequences, Version.Current().GetRawVersion());

			if (ssb.HasFailed())
				return;

			SequenceIndex seqs = 0;
			uint8 currentSeq = 0;
			ssb.ReadItem(ref seqs, "n");

			if (seqs == 0)
				return;

			OpenMpt.LimitMax(ref seqs, Snd_Def.Max_Sequences);

			ssb.ReadItem(ref currentSeq, "c");

			if (seq.GetNumSequences() < seqs)
				seq.m_Sequences.resize(seqs, new ModSequence(seq.m_SndFile));

			// There used to be only one restart position / tempo / speed for all sequences
			OrderIndex legacyRestartPos = seq[0].GetRestartPos();
			Tempo legacyTempo = seq[0].GetDefaultTempo();
			uint32 legacySpeed = seq[0].GetDefaultSpeed();

			for (SequenceIndex i = 0; i < seqs; i++)
			{
				seq[i].SetRestartPos(legacyRestartPos);
				seq[i].SetDefaultTempo(legacyTempo);
				seq[i].SetDefaultSpeed(legacySpeed);

				ssb.ReadItem(ref seq[i], Id.FromInt(i), (strm, ref seq_, dummy) => ReadModSequence(strm, seq_, dummy, defaultCharset));
			}

			seq.m_CurrentSeq = (SequenceIndex)((currentSeq < seq.GetNumSequences()) ? currentSeq : 0);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (ModSequence me, ModSequence other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return ((vector<PatternIndex>)me == (vector<PatternIndex>)other) &&
			       (me.m_Name == other.m_Name) && (me.m_RestartPos == other.m_RestartPos) &&
			       (me.m_DefaultTempo == other.m_DefaultTempo) && (me.m_DefaultSpeed == other.m_DefaultSpeed);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (ModSequence me, ModSequence other)
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
			return (obj is ModSequence other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ModSequence other)
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
			HashCode hash = new HashCode();

			hash.Add(base.GetHashCode());
			hash.Add(m_Name);
			hash.Add(m_RestartPos);
			hash.Add(m_DefaultTempo);
			hash.Add(m_DefaultSpeed);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public override ModSequence MakeDeepClone()
		{
			ModSequence clone = new ModSequence(m_SndFile);

			clone.m_Name = m_Name;
			clone.m_RestartPos = m_RestartPos;
			clone.m_DefaultTempo = m_DefaultTempo;
			clone.m_DefaultSpeed = m_DefaultSpeed;
			CopyTo(clone);

			return clone;
		}
	}
}
