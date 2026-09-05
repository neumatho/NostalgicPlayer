/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Alternative sample tuning
	/// </summary>
	internal class CTuning : IEquatable<CTuning>
	{
		public const TuningRatioType s_DefaultFallbackRatio = 1.0f;
		public const TuningNoteIndexType s_NoteMinDefault = -64;

		public struct NoteRange
		{
			public TuningNoteIndexType First;
			public TuningNoteIndexType Last;
		}

		public enum Type : uint16
		{
			General = 0,
			GroupGeometric = 1,
			Geometric = 3
		}

		private readonly Type m_TuningType = Type.General;

		/// <summary>
		/// Noteratios
		/// </summary>
		private readonly vector<TuningRatioType> m_RatioTable = new vector<TuningRatioType>();

		/// <summary>
		/// 'Fineratios'
		/// </summary>
		private readonly vector<TuningRatioType> m_RatioTableFine = new vector<TuningRatioType>();

		/// <summary>
		/// The lowest index of note in the table
		/// </summary>
		private readonly TuningNoteIndexType m_NoteMin = s_NoteMinDefault;

		/// <summary>
		/// For groupgeometric tunings, tells the 'group size' and 'group ratio'
		/// m_GroupSize should always be ›= 0.
		/// </summary>
		private readonly TuningNoteIndexType m_GroupSize = 0;
		private readonly TuningRatioType m_GroupRatio = 0;

		/// <summary>
		/// invariant: 0 ‹= m_FineStepCount ‹= FINESTEPCOUNT_MAX
		/// </summary>
		private readonly TuningUStepIndexType m_FineStepCount = 0;
//XX		private string m_TuningName;
		private readonly map<TuningNoteIndexType, string> m_NoteNameMap = new map<TuningNoteIndexType, string>();

		/********************************************************************/
		/// <summary>
		/// Tuning might not be valid for arbitrarily large range, so this
		/// can be used to ask where it is valid. Tells the lowest and
		/// highest note that are valid
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NoteRange GetNoteRange()
		{
			return new NoteRange
			{
				First = m_NoteMin,
				Last = (TuningNoteIndexType)(m_NoteMin + (TuningNoteIndexType)m_RatioTable.size() - 1)
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return true if note is within note range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidNote(TuningNoteIndexType n)
		{
			return (GetNoteRange().First <= n) && (n <= GetNoteRange().Last);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TuningUNoteIndexType GetGroupSize()
		{
			return (TuningUNoteIndexType)m_GroupSize;
		}



		/********************************************************************/
		/// <summary>
		/// To return (fine)stepcount between two consecutive mainsteps
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TuningUStepIndexType GetFineStepCount()
		{
			return m_FineStepCount;
		}



		/********************************************************************/
		/// <summary>
		/// To return 'directed distance' between given notes
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TuningStepIndexType GetStepDistance(TuningNoteIndexType from, TuningNoteIndexType to)
		{
			return (to - from) * ((TuningNoteIndexType)GetFineStepCount() + 1);
		}



		/********************************************************************/
		/// <summary>
		/// To return 'directed distance' between given steps
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TuningStepIndexType GetStepDistance(TuningNoteIndexType noteFrom, TuningStepIndexType stepDistFrom, TuningNoteIndexType noteTo, TuningStepIndexType stepDistTo)
		{
			return GetStepDistance(noteFrom, noteTo) + stepDistTo - stepDistFrom;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Type GetType_()
		{
			return m_TuningType;
		}



		/********************************************************************/
		/// <summary>
		/// GroupPeriodic-specific.
		/// Get the corresponding note in [0, period-1].
		/// For example GetRefNote(-1) is to return note :'groupsize-1'
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TuningNoteIndexType GetRefNote(TuningNoteIndexType note)
		{
			return Wrapping_Divide.Wrapping_Modulo(note, (TuningNoteIndexType)GetGroupSize());
		}



		/********************************************************************/
		/// <summary>
		/// Without finetune
		/// </summary>
		/********************************************************************/
		public TuningRatioType GetRatio(TuningNoteIndexType note)//XX 274
		{
			if (!IsValidNote(note))
				return s_DefaultFallbackRatio;

			TuningRatioType ratio = m_RatioTable[note - m_NoteMin];

			if (ratio <= 1e-15f)
				return s_DefaultFallbackRatio;

			return ratio;
		}



		/********************************************************************/
		/// <summary>
		/// With finetune
		/// </summary>
		/********************************************************************/
		public TuningRatioType GetRatio(TuningNoteIndexType baseNote, TuningStepIndexType baseFineSteps)//XX 290
		{
			TuningStepIndexType fineStepCount = (TuningStepIndexType)GetFineStepCount();

			if ((fineStepCount == 0) || (baseFineSteps == 0))
				return GetRatio((TuningNoteIndexType)(baseNote + baseFineSteps));

			// If baseFineSteps is more than the number of finesteps between notes, note is increased.
			// So first figuring out what note and fineStep values to actually use.
			// Interpreting finestep==-1 on note x so that it is the same as finestep==fineStepCount on note x-1.
			// Note: If fineStepCount is n, n+1 steps are needed to get to next note
			TuningNoteIndexType note = (TuningNoteIndexType)(baseNote + Wrapping_Divide.Wrapping_Divide_(baseFineSteps, fineStepCount + 1));
			TuningStepIndexType fineStep = Wrapping_Divide.Wrapping_Modulo(baseFineSteps, fineStepCount + 1);

			if (!IsValidNote(note))
				return s_DefaultFallbackRatio;

			if (fineStep == 0)
				return m_RatioTable[note - m_NoteMin];

			TuningRatioType fineRatio = (TuningRatioType)1.0;

			if ((GetType_() == Type.Geometric) && (m_RatioTableFine.size() > 0))
				fineRatio = m_RatioTableFine[fineStep - 1];
			else if ((GetType_() == Type.GroupGeometric) && (m_RatioTableFine.size() > 0))
				fineRatio = m_RatioTableFine[(GetRefNote(note) * fineStepCount) + fineStep - 1];
			else
			{
				// Geometric finestepping
				fineRatio = CMath.pow(GetRatio((TuningNoteIndexType)(note + 1)) / GetRatio(note), (TuningRatioType)fineStep / (fineStepCount + 1));
			}

			return m_RatioTable[note - m_NoteMin] * fineRatio;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (CTuning me, CTuning other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.m_TuningType == other.m_TuningType) && (me.m_NoteMin == other.m_NoteMin) && (me.m_GroupSize == other.m_GroupSize) && (me.m_GroupRatio == other.m_GroupRatio) &&
				   (me.m_FineStepCount == other.m_FineStepCount) && (me.m_RatioTable == other.m_RatioTable) && (me.m_RatioTableFine == other.m_RatioTableFine) &&
				   /*(me.m_TuningName == other.m_TuningName) &&*/ (me.m_NoteNameMap == other.m_NoteNameMap);//XX
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (CTuning me, CTuning other)
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
			return (obj is CTuning other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(CTuning other)
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

			hash.Add(m_TuningType);
			hash.Add(m_NoteMin);
			hash.Add(m_GroupSize);
			hash.Add(m_GroupRatio);
			hash.Add(m_FineStepCount);
			hash.Add(m_RatioTable.GetHashCode());
			hash.Add(m_RatioTableFine.GetHashCode());
//XX			hash.Add(m_TuningName);
			hash.Add(m_NoteNameMap.GetHashCode());

			return hash.ToHashCode();
		}
	}
}
