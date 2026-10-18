/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Alternative sample tuning
	/// </summary>
	internal class CTuning : IEquatable<CTuning>
	{
		public const TuningRatioType s_DefaultFallbackRatio = 1.0f;
		public const TuningNoteIndexType s_NoteMinDefault = -64;
		public const TuningUNoteIndexType s_RatioTableSizeDefault = 128;
		public const TuningUStepIndexType s_RatioTableFineSizeMaxDefault = 1000;

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

		private Type m_TuningType = Type.General;

		/// <summary>
		/// Noteratios
		/// </summary>
		private vector<TuningRatioType> m_RatioTable = new vector<TuningRatioType>();

		/// <summary>
		/// 'Fineratios'
		/// </summary>
		private readonly vector<TuningRatioType> m_RatioTableFine = new vector<TuningRatioType>();

		/// <summary>
		/// The lowest index of note in the table
		/// </summary>
		private TuningNoteIndexType m_NoteMin = s_NoteMinDefault;

		/// <summary>
		/// For groupgeometric tunings, tells the 'group size' and 'group ratio'
		/// m_GroupSize should always be ›= 0.
		/// </summary>
		private TuningNoteIndexType m_GroupSize = 0;
		private TuningRatioType m_GroupRatio = 0;

		/// <summary>
		/// invariant: 0 ‹= m_FineStepCount ‹= FINESTEPCOUNT_MAX
		/// </summary>
		private TuningUStepIndexType m_FineStepCount = 0;
		private string m_TuningName = string.Empty;
		private map<TuningNoteIndexType, string> m_NoteNameMap = new map<TuningNoteIndexType, string>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public CTuning()
		{
			m_RatioTable.resize(s_RatioTableSizeDefault, 1);
		}



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
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TuningRatioType GetGroupRatio()
		{
			return m_GroupRatio;
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
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CTuning CreateDeserialize(ReaderStream f, Encoding defaultCharset)
		{
			CTuning pT = new CTuning();

			if (pT.InitDeserialize(f, defaultCharset) != SerializationResult.Success)
				return null;

			return pT;
		}



		/********************************************************************/
		/// <summary>
		/// Try to read old version (v.3) and return pointer to new instance
		/// if successfully, else nullptr
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CTuning CreateDeserializeOld(ReaderStream f, Encoding defaultCharset)
		{
			CTuning pT = new CTuning();

			if (pT.InitDeserializeOld(f, defaultCharset) != SerializationResult.Success)
				return null;

			return pT;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CTuning CreateGeometric(string name, TuningUNoteIndexType groupSize, TuningRatioType groupRatio, TuningUStepIndexType fineStepCount)
		{
			CTuning pT = new CTuning();
			pT.SetName(name);

			if (!pT.CreateGeometric(groupSize, groupRatio))
				return null;

			pT.SetFineStepCount(fineStepCount);

			return pT;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetName(string s)
		{
			m_TuningName = s;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public string GetName()
		{
			return m_TuningName;
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
		/// 
		/// </summary>
		/********************************************************************/
		private static bool IsValidRatio(TuningRatioType ratio)
		{
			// Arbitrary epsilon > 0 to avoid NaNs and infinite values in ratio calculation
			return ratio > 0.02f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static TuningRatioType SanitizeGroupRatio(TuningRatioType ratio)
		{
			return Math.Clamp(CMath.abs(ratio), 1e-15f, 1e+07f);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetNoteName(TuningNoteIndexType n, string str)
		{
			TuningNoteIndexType pos = GetGroupSize() < 1 ? n : Wrapping_Divide.Wrapping_Modulo(n, m_GroupSize);

			if (!string.IsNullOrEmpty(str))
				m_NoteNameMap[pos] = str;
			else
			{
				map<TuningNoteIndexType, string>.iterator iter = m_NoteNameMap.find(pos);

				if (iter != m_NoteNameMap.end())
					m_NoteNameMap.erase(iter);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Without finetune
		/// </summary>
		/********************************************************************/
		public TuningRatioType GetRatio(TuningNoteIndexType note)
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
		public TuningRatioType GetRatio(TuningNoteIndexType baseNote, TuningStepIndexType baseFineSteps)
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
		/// To set finestepcount between two consecutive mainsteps.
		/// Finestep count == 0 means that stepdistances become the same as
		/// note distances
		/// </summary>
		/********************************************************************/
		public void SetFineStepCount(TuningUStepIndexType fs)
		{
			m_FineStepCount = (TuningUStepIndexType)Math.Clamp(TuningStepIndexType.CreateSaturating(fs), 0, TuningBase.FineStepCount_Max);
			UpdateFineStepTable();
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
				   (me.m_TuningName == other.m_TuningName) && (me.m_NoteNameMap == other.m_NoteNameMap);
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
			hash.Add(m_RatioTable);
			hash.Add(m_RatioTableFine);
			hash.Add(m_TuningName);
			hash.Add(m_NoteNameMap);

			return hash.ToHashCode();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private bool CreateGroupGeometric(vector<TuningRatioType> v, TuningRatioType r, NoteRange range, TuningNoteIndexType ratioStartPos)
		{
			if ((range.First > range.Last) || (v.size() == 0))
				return false;

			if ((ratioStartPos < range.First) || (range.Last < ratioStartPos) || (TuningUNoteIndexType)(range.Last - ratioStartPos) < (TuningUNoteIndexType)(v.size() - 1))
				return false;

			if (GetFineStepCount() > TuningBase.FineStepCount_Max)
				return false;

			for (size_t i = 0; i < v.size(); i++)
			{
				if (v[i] < 0)
					return false;
			}

			if (r <= 0)
				return false;

			m_TuningType = Type.GroupGeometric;
			m_NoteMin = range.First;
			m_GroupSize = TuningNoteIndexType.CreateSaturating(v.size());
			m_GroupRatio = CMath.abs(r);
			m_RatioTable.resize((size_t)(range.Last - range.First + 1));
			Algorithm.copy<forward_iterator<TuningRatioType>, forward_iterator<TuningRatioType>, TuningRatioType>(v.begin(), v.end(), m_RatioTable.begin() + (ratioStartPos - range.First));

			for (int32 i = ratioStartPos - 1; (i >= m_NoteMin) && (ratioStartPos > TuningBase.NoteIndexType_Min); i--)
				m_RatioTable[i - m_NoteMin] = m_RatioTable[i - m_NoteMin + m_GroupSize] / m_GroupRatio;

			for (int32 i = ratioStartPos + m_GroupSize; (i <= range.Last) && (ratioStartPos <= (TuningBase.NoteIndexType_Max - m_GroupSize)); i++)
				m_RatioTable[i - m_NoteMin] = m_GroupRatio * m_RatioTable[i - m_NoteMin - m_GroupSize];

			UpdateFineStepTable();

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private bool CreateGeometric(TuningUNoteIndexType p, TuningRatioType r)
		{
			return CreateGeometric(p, r, GetNoteRange());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private bool CreateGeometric(TuningUNoteIndexType s, TuningRatioType r, NoteRange range)
		{
			if (range.First > range.Last)
				return false;

			if ((s < 1) || !IsValidRatio(r))
				return false;

			if ((range.Last - range.First + 1) > TuningBase.NoteIndexType_Max)
				return false;

			m_TuningType = Type.Geometric;
			m_RatioTable.clear();
			m_NoteMin = s_NoteMinDefault;
			m_RatioTable.resize(s_RatioTableSizeDefault, 1.0f);
			m_GroupSize = 0;
			m_GroupRatio = 0;
			m_RatioTableFine.clear();
			m_NoteMin = range.First;
			m_GroupSize = TuningNoteIndexType.CreateSaturating(s);
			m_GroupRatio = CMath.abs(r);
			TuningRatioType stepRatio = CMath.pow(m_GroupRatio, 1.0f / m_GroupSize);
			m_RatioTable.resize((size_t)(range.Last - range.First + 1));

			for (int32 i = range.First; i <= range.Last; i++)
				m_RatioTable[i - m_NoteMin] = CMath.pow(stepRatio, i);

			UpdateFineStepTable();

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private void UpdateFineStepTable()
		{
			if (m_FineStepCount <= 0)
			{
				m_RatioTableFine.clear();
				return;
			}

			if (GetType_() == Type.Geometric)
			{
				if (m_FineStepCount > s_RatioTableFineSizeMaxDefault)
				{
					m_RatioTableFine.clear();
					return;
				}

				m_RatioTableFine.resize(m_FineStepCount);
				TuningRatioType q = GetRatio((TuningNoteIndexType)(GetNoteRange().First + 1)) / GetRatio(GetNoteRange().First);
				TuningRatioType rFineStep = CMath.pow(q, 1.0f / (m_FineStepCount + 1));

				for (TuningUStepIndexType i = 1; i <= m_FineStepCount; i++)
					m_RatioTableFine[i - 1] = CMath.pow(rFineStep, i);

				return;
			}

			if (GetType_() == Type.GroupGeometric)
			{
				TuningUNoteIndexType p = GetGroupSize();

				if (p > (s_RatioTableFineSizeMaxDefault / m_FineStepCount))
				{
					// In case fineratiotable would become too large, not using
					// table for it
					m_RatioTableFine.clear();
					return;
				}
				else
				{
					// Creating 'geometric' finestepping between notes
					m_RatioTableFine.resize(p * m_FineStepCount);
					TuningNoteIndexType startNote = GetRefNote(GetNoteRange().First);

					for (TuningUNoteIndexType i = 0; i < p; i++)
					{
						TuningNoteIndexType refNote = GetRefNote((TuningNoteIndexType)(startNote + i));
						TuningRatioType rFineStep = CMath.pow(GetRatio((TuningNoteIndexType)(refNote + 1)) / GetRatio(refNote), 1.0f / (m_FineStepCount + 1));

						for (TuningUNoteIndexType j = 1; j <= m_FineStepCount; j++)
							m_RatioTableFine[(size_t)((m_FineStepCount * refNote) + (j - 1))] = CMath.pow(rFineStep, j);
					}

					return;
				}
			}

			if (GetType_() == Type.General)
			{
				// Not using table with tuning of type general
				m_RatioTableFine.clear();
				return;
			}

			// Should not reach here
			m_RatioTableFine.clear();
			m_FineStepCount = 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private SerializationResult InitDeserialize(ReaderStream iStrm, Encoding defaultCharset)
		{
			// Note: OpenMPT since at least r323 writes version number (4<<24)+4 while it
			// reads version number (5<<24)+4 or earlier.
			// We keep this behaviour

			if (iStrm.EndOfStream)
				return SerializationResult.Failure;

			SsbRead ssb = new SsbRead(iStrm);
			ssb.BeginRead("CTB244RTI", (5 << 24) + 4);		// Version

			int8 use_Utf8 = 0;
			ssb.ReadItem(ref use_Utf8, "UTF8");
			Encoding charset = use_Utf8 != 0 ? Encoding.UTF8 : defaultCharset;

			ssb.ReadItem(ref m_TuningName, "0", (strm, ref ustr, dummy) => CTuningS11n.ReadStr(strm, ref ustr, dummy, charset));

			uint16 dummyEditMask = 0xffff;
			ssb.ReadItem(ref dummyEditMask, "1");

			uint16 type = 0;
			ssb.ReadItem(ref type, "2");
			m_TuningType = (Type)type;

			ssb.ReadItem(ref m_NoteNameMap, "3", (strm, ref m, dummy) => CTuningS11n.ReadNoteMap(strm, m, dummy, charset));
			ssb.ReadItem(ref m_FineStepCount, "4");

			// RTI entries
			ssb.ReadItem(ref m_RatioTable, "RTI0", CTuningS11n.ReadRatioTable);
			ssb.ReadItem(ref m_NoteMin, "RTI1");
			ssb.ReadItem(ref m_GroupSize, "RTI2");
			ssb.ReadItem(ref m_GroupRatio, "RTI3");

			TuningUNoteIndexType ratioTableSize = 0;
			ssb.ReadItem(ref ratioTableSize, "RTI4");

			m_GroupRatio = SanitizeGroupRatio(m_GroupRatio);

			if (!CMath.isfinite(m_GroupRatio))
				return SerializationResult.Failure;

			foreach (TuningRatioType ratio in m_RatioTable)
			{
				if (!CMath.isfinite(ratio))
					return SerializationResult.Failure;
			}

			// If reader status is ok and m_NoteMin is somewhat reasonable, process data
			if (ssb.HasFailed() || (m_NoteMin < -300) || (m_NoteMin > 300))
				return SerializationResult.Failure;

			// Reject unknown types
			if ((m_TuningType != Type.General) && (m_TuningType != Type.GroupGeometric) && (m_TuningType != Type.Geometric))
				return SerializationResult.Failure;

			if (m_GroupSize < 0)
				return SerializationResult.Failure;

			m_FineStepCount = (TuningUStepIndexType)Math.Clamp(TuningStepIndexType.CreateSaturating(m_FineStepCount), 0, TuningBase.FineStepCount_Max);

			if (m_RatioTable.size() > (size_t)TuningBase.NoteIndexType_Max)
				return SerializationResult.Failure;

			if ((GetType_() == Type.GroupGeometric) || (GetType_() == Type.Geometric))
			{
				if ((ratioTableSize < 1) || (ratioTableSize > TuningBase.NoteIndexType_Max))
					return SerializationResult.Failure;

				if (GetType_() == Type.Geometric)
				{
					if (!CreateGeometric(GetGroupSize(), GetGroupRatio(), new NoteRange { First = m_NoteMin, Last = (TuningNoteIndexType)(m_NoteMin + ratioTableSize - 1) }))
						return SerializationResult.Failure;
				}
				else
				{
					if (!CreateGroupGeometric(m_RatioTable, GetGroupRatio(), new NoteRange { First = m_NoteMin, Last = (TuningNoteIndexType)(m_NoteMin + ratioTableSize - 1) }, m_NoteMin))
						return SerializationResult.Failure;
				}
			}
			else
				UpdateFineStepTable();

			return SerializationResult.Success;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static bool VectorFromBinaryStream<T, TValue, TSizeType>(ReaderStream inStrm, vector<TuningRatioType> v) where T : unmanaged, IEndianNumber<TValue> where TValue : unmanaged, INumberBase<TValue> where TSizeType : INumber<TSizeType>, IMinMaxValue<TSizeType>
		{
			return VectorFromBinaryStream<T, TValue, TSizeType>(inStrm, v, TSizeType.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private static bool VectorFromBinaryStream<T, TValue, TSizeType>(ReaderStream inStrm, vector<TuningRatioType> v, TSizeType maxSize) where T : unmanaged, IEndianNumber<TValue> where TValue : unmanaged, INumberBase<TValue> where TSizeType : INumber<TSizeType>
		{
			if (inStrm.EndOfStream)
				return false;

			Io_.ReadIntLE(inStrm, out TSizeType size);

			if (size > maxSize)
				return false;

			v.resize(size_t.CreateTruncating(size));

			for (size_t i = 0; i < size_t.CreateTruncating(size); i++)
			{
				T tmp = default;
				Io_.Read(inStrm, ref tmp);
				v[i] = TuningRatioType.CreateTruncating(tmp.Value);
			}

			return !inStrm.EndOfStream;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		private SerializationResult InitDeserializeOld(ReaderStream inStrm, Encoding defaultCharset)
		{
			if (inStrm.EndOfStream)
				return SerializationResult.Failure;

			long startPos = inStrm.Position;

			// First checking is there expected begin sequence
			uint8[] begin = new uint8[8];
			inStrm.ReadInto(begin, 0, begin.Length);

			if (CMemory.memcmp(begin, "CTRTI_B.", 8) != 0)
			{
				// Returning stream position if beginmarker was not found
				inStrm.Seek(startPos, SeekOrigin.Begin);

				return SerializationResult.Failure;
			}

			// Version
			Io_.ReadIntLE(inStrm, out int16 version);

			if ((version != 2) && (version != 3))
				return SerializationResult.Failure;

			uint8[] begin2 = new uint8[8];
			inStrm.ReadInto(begin2, 0, begin2.Length);

			if (CMemory.memcmp(begin2, "CT<sfs>B", 8) != 0)
				return SerializationResult.Failure;

			Io_.ReadIntLE(inStrm, out int16 version2);

			if ((version2 != 3) && (version2 != 4))
				return SerializationResult.Failure;

			// Tuning name
			if (version2 <= 3)
			{
				if (!Io_.ReadSizedStringLE<uint32>(inStrm, out StdString tmpName, 0xffff))
					return SerializationResult.Failure;

				m_TuningName = defaultCharset.GetString(tmpName.Span());
			}
			else
			{
				if (!Io_.ReadSizedStringLE<uint8>(inStrm, out StdString tmpName))
					return SerializationResult.Failure;

				m_TuningName = defaultCharset.GetString(tmpName.Span());
			}

			// Const mask
			Io_.ReadIntLE(inStrm, out int16 em);

			// Tuning type
			Io_.ReadIntLE(inStrm, out int16 tt);
			m_TuningType = (Type)tt;

			// Notemap
			uint16 size = 0;

			if (version2 <= 3)
			{
				Io_.ReadIntLE(inStrm, out uint32 tempSize);

				if (tempSize > 0xffff)
					return SerializationResult.Failure;

				size = uint16.CreateSaturating(tempSize);
			}
			else
				Io_.ReadIntLE(inStrm, out size);

			for (TuningUNoteIndexType i = 0; i < size; i++)
			{
				StdString str;

				Io_.ReadIntLE(inStrm, out int16 n);

				if (version2 <= 3)
				{
					if (!Io_.ReadSizedStringLE<uint32>(inStrm, out str, 0xffff))
						return SerializationResult.Failure;
				}
				else
				{
					if (!Io_.ReadSizedStringLE<uint8>(inStrm, out str))
						return SerializationResult.Failure;
				}

				m_NoteNameMap[n] = defaultCharset.GetString(str.Span());
			}

			// End marker
			uint8[] end2 = new uint8[8];
			inStrm.ReadInto(end2, 0, end2.Length);

			if (CMemory.memcmp(end2, "CT<sfs>E", 8) != 0)
				return SerializationResult.Failure;

			// Reject unknown types
			if ((m_TuningType != Type.General) && (m_TuningType != Type.GroupGeometric) && (m_TuningType != Type.Geometric))
				return SerializationResult.Failure;

			// Ratiotable
			if (version <= 2)
			{
				if (!VectorFromBinaryStream<Ieee754Binary32LE, c_float, uint32>(inStrm, m_RatioTable, 0xffff))
					return SerializationResult.Failure;
			}
			else
			{
				if (!VectorFromBinaryStream<Ieee754Binary32LE, c_float, uint16>(inStrm, m_RatioTable))
					return SerializationResult.Failure;
			}

			foreach (TuningRatioType ratio in m_RatioTable)
			{
				if (!CMath.isfinite(ratio))
					return SerializationResult.Failure;
			}

			// Fineratios
			if (version <= 2)
			{
				if (!VectorFromBinaryStream<Ieee754Binary32LE, c_float, uint32>(inStrm, m_RatioTableFine, 0xffff))
					return SerializationResult.Failure;
			}
			else
			{
				if (!VectorFromBinaryStream<Ieee754Binary32LE, c_float, uint16>(inStrm, m_RatioTableFine))
					return SerializationResult.Failure;
			}

			foreach (TuningRatioType ratio in m_RatioTableFine)
			{
				if (!CMath.isfinite(ratio))
					return SerializationResult.Failure;
			}

			m_FineStepCount = TuningUStepIndexType.CreateSaturating(m_RatioTableFine.size());

			// m_NoteMin
			Io_.ReadIntLE(inStrm, out int16 noteMin);
			m_NoteMin = noteMin;

			if ((m_NoteMin < -200) || (m_NoteMin > 200))
				return SerializationResult.Failure;

			// m_GroupSize
			Io_.ReadIntLE(inStrm, out int16 groupSize);
			m_GroupSize = groupSize;

			if (m_GroupSize < 0)
				return SerializationResult.Failure;

			// m_GroupRatio
			Ieee754Binary32LE groupRatio = (Ieee754Binary32LE)0;
			Io_.Read(inStrm, ref groupRatio);
			m_GroupRatio = SanitizeGroupRatio(groupRatio);

			if (!CMath.isfinite(m_GroupRatio))
				return SerializationResult.Failure;

			array<uint8> end = new array<uint8>(8);
			Io_.Read(inStrm, end);

			if (CMemory.memcmp(end.data(), "CTRTI_E.", 8) != 0)
				return SerializationResult.Failure;

			// Reject corrupt tunings
			if (m_RatioTable.size() > (size_t)TuningBase.NoteIndexType_Max)
				return SerializationResult.Failure;

			if (((m_GroupSize <= 0) || (m_GroupRatio <= 0)) && (m_TuningType != Type.General))
				return SerializationResult.Failure;

			if ((m_TuningType == Type.GroupGeometric) || (m_TuningType == Type.Geometric))
			{
				if (m_RatioTable.size() < (size_t)m_GroupSize)
					return SerializationResult.Failure;
			}

			// Convert old finestepcount
			if (m_FineStepCount > 0)
				m_FineStepCount -= 1;

			m_FineStepCount = (TuningUStepIndexType)Math.Clamp(TuningStepIndexType.CreateSaturating(m_FineStepCount), 0, TuningBase.FineStepCount_Max);
			UpdateFineStepTable();

			if (m_TuningType == Type.Geometric)
			{
				// Convert old geometric to new groupgeometric because old geometric tunings
				// can have ratio(0) != 1.0, which would get lost when saving nowadays
				TuningNoteIndexType baseNote = 0;
				baseNote = Math.Clamp(baseNote, GetNoteRange().First, GetNoteRange().Last);

				if (IsValidNote(baseNote) && IsValidNote(TuningNoteIndexType.CreateSaturating(baseNote + (TuningNoteIndexType)(GetGroupSize()) - 1)))
				{
					vector<TuningRatioType> ratios = new vector<TuningRatioType>();

					for (TuningNoteIndexType note = baseNote; note < (m_GroupSize + baseNote); ++note)
						ratios.push_back(GetRatio(note));

					// Ignore return value, keep as geometric if conversion fails
					CreateGroupGeometric(ratios, m_GroupRatio, GetNoteRange(), baseNote);
				}
			}

			return SerializationResult.Success;
		}
		#endregion
	}
}
