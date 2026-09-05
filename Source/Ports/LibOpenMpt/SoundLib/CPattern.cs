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
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Module Pattern header class
	/// </summary>
	internal class CPattern : IEquatable<CPattern>, IDeepCloneable<CPattern>
	{
		/// <summary>
		/// 
		/// </summary>
		public vector<ModCommand> m_ModCommands = new vector<ModCommand>();

		/// <summary>
		/// 
		/// </summary>
		protected RowIndex m_Rows = 0;

		/// <summary>
		/// Patterns-specific time signature. if != 0, the time signature is used automatically
		/// </summary>
		protected RowIndex m_RowsPerBeat = 0;

		/// <summary>
		/// Ditto
		/// </summary>
		protected RowIndex m_RowsPerMeasure = 0;

		/// <summary>
		/// 
		/// </summary>
		protected TempoSwing m_TempoSwing = new TempoSwing();

		/// <summary>
		/// 
		/// </summary>
		protected StdString m_PatternName;

		/// <summary>
		/// 
		/// </summary>
		protected CPatternContainer m_rPatternContainer;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CPattern(CPatternContainer patCont)
		{
			m_rPatternContainer = patCont;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CPointer<ModCommand> GetpModCommand(RowIndex r, ChannelIndex c)
		{
			return m_ModCommands.data() + ((r * GetNumChannels()) + c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RowIndex GetNumRows()
		{
			return m_Rows;
		}



		/********************************************************************/
		/// <summary>
		/// Pattern-specific rows per beat
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RowIndex GetRowsPerBeat()
		{
			return m_RowsPerBeat;
		}



		/********************************************************************/
		/// <summary>
		/// Pattern-specific rows per measure
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RowIndex GetRowsPerMeasure()
		{
			return m_RowsPerMeasure;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetOverrideSignature()
		{
			return (m_RowsPerBeat + m_RowsPerMeasure) > 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if pattern data can be accessed at given row, false
		/// otherwise
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidRow(RowIndex row)
		{
			return row < GetNumRows();
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if any pattern data is present
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValid()
		{
			return !m_ModCommands.empty();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveSignature()
		{
			m_RowsPerBeat = m_RowsPerMeasure = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasTempoSwing()
		{
			return !m_TempoSwing.empty();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TempoSwing GetTempoSwing()
		{
			return m_TempoSwing;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StdString GetName()
		{
			return m_PatternName;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CSoundFile GetSoundFile()//XX 26
		{
			return m_rPatternContainer.GetSoundFile();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ChannelIndex GetNumChannels()//XX 30
		{
			return GetSoundFile().GetNumChannels();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ClearCommands()//XX 119
		{
			Algorithm.fill(m_ModCommands.begin(), m_ModCommands.end(), new ModCommand());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool AllocatePattern(RowIndex rows)//XX 125
		{
			size_t newSize = GetNumChannels() * rows;

			if (rows == 0)
				return false;
			else if ((rows == GetNumRows()) && (m_ModCommands.size() == newSize))
			{
				// Re-use allocated memory
				ClearCommands();

				return true;
			}
			else
			{
				// Do this in two steps in order to keep the old pattern data in case of OOM
				vector<ModCommand> newPattern = new vector<ModCommand>(newSize, new ModCommand());
				m_ModCommands = Utility.move(newPattern);
			}

			m_Rows = rows;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool SetName(StdString newName)//XX 278
		{
			m_PatternName = Utility.move(newName);

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (CPattern me, CPattern other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.GetNumRows() == other.GetNumRows()) && (me.GetNumChannels() == other.GetNumChannels()) && (me.GetOverrideSignature() == other.GetOverrideSignature()) &&
			       (me.GetRowsPerBeat() == other.GetRowsPerBeat()) && (me.GetRowsPerMeasure() == other.GetRowsPerMeasure()) && (me.GetTempoSwing() == other.GetTempoSwing()) &&
				   (me.m_ModCommands == other.m_ModCommands);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (CPattern me, CPattern other)
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
			return (obj is CPattern other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(CPattern other)
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

			hash.Add(m_ModCommands.GetHashCode());
			hash.Add(m_Rows);
			hash.Add(m_RowsPerBeat);
			hash.Add(m_RowsPerMeasure);
			hash.Add(m_TempoSwing);
			hash.Add(m_PatternName);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public CPattern MakeDeepClone()
		{
			CPattern clone = (CPattern)MemberwiseClone();

			clone.m_ModCommands = m_ModCommands.MakeDeepClone();
			clone.m_TempoSwing = m_TempoSwing.MakeDeepClone();

			return clone;
		}
	}
}
