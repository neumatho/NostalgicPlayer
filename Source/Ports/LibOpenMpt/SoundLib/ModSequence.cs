/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Order and sequence handling
	/// </summary>
	internal class ModSequence : vector<PatternIndex>, IEquatable<ModSequence>, IDeepCloneable<ModSequence>
	{
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
		public ModSequence(CSoundFile sndFile)//XX 23
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
		public void SetDefaultTempo(Tempo tempo)//XX 52
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
		public OrderIndex GetLengthTailTrimmed()//XX 104
		{
			if (empty())
				return 0;

			reverse_iterator<PatternIndex> last = Algorithm.find_if(rbegin(), rend(), (PatternIndex pat) => pat != Snd_Def.PatternIndex_Invalid);

			return (OrderIndex)Iterator.distance(begin(), last.@base());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void assign(OrderIndex newSize, PatternIndex pat)//XX 219
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
		public OrderIndex insert(OrderIndex pos, OrderIndex count, PatternIndex fill, bool enforeFormatLimits = true)//XX 226
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// Check if pattern at sequence position ord is valid
		/// </summary>
		/********************************************************************/
		public bool IsValidPat(OrderIndex ord)//XX 254
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
		public OrderIndex FindOrder(PatternIndex pat, OrderIndex startSearchAt, bool searchForward = true)//XX 270
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
