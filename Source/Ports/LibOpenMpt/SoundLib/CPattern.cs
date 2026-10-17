/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Iterators;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;
using Utility = Polycode.NostalgicPlayer.Kit.C.Std.Utility;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Module Pattern header class
	/// </summary>
	internal class CPattern : IEquatable<CPattern>, IDeepCloneable<CPattern>
	{
		private const string FileIdPattern = "mptP";

		#region MaskBits
		[Flags]
		private enum MaskBits
		{
			NoteBit = 1 << 0,
			InstrBit = 1 << 1,
			VolCmdBit = 1 << 2,
			VolBit = 1 << 3,
			CommandBit = 1 << 4,
			EffectParamBit = 1 << 5,
			ExtraData = 1 << 6
		}
		#endregion

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
		public MptSpan<ModCommand> GetRow(RowIndex row)
		{
			return new MptSpan<ModCommand>(GetpModCommand(row, 0), GetNumChannels());
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
		public void SetTempoSwing(TempoSwing swing)
		{
			m_TempoSwing = swing;
			m_TempoSwing.Normalize();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetName(CPointer<uint8> buffer)
		{
			return SetName(buffer, buffer.Size());
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
		public bool SetSignature(RowIndex rowsPerBeat, RowIndex rowsPerMeasure)//XX 66
		{
			if (!IsValidSignature(rowsPerBeat, rowsPerMeasure))
				return false;

			m_RowsPerBeat = rowsPerBeat;
			m_RowsPerMeasure = rowsPerMeasure;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool IsValidSignature(RowIndex rowsPerBeat, RowIndex rowsPerMeasure)//XX 76
		{
			return (rowsPerBeat > 0) && (rowsPerBeat <= Snd_Def.Max_Rows_Per_Beat) && (rowsPerBeat <= rowsPerMeasure) && (rowsPerMeasure <= Snd_Def.Max_Rows_Per_Measure);
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
		public bool SetName(CPointer<uint8> newName, size_t maxChars)//XX 285
		{
			if (newName.IsNull || (maxChars == 0))
				return false;

			CPointer<uint8> nameEnd = Algorithm.find(newName, newName + maxChars, (uint8)'\0');
			m_PatternName.assign(newName, nameEnd);

			return true;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static void ReadModPattern(ReaderStream iStrm, ref CPattern pat, size_t _)//XX 517
		{
			SsbRead ssb = new SsbRead(iStrm);
			ssb.BeginRead(FileIdPattern, Version.Current().GetRawVersion());

			if (ssb.HasFailed())
				return;

			ssb.ReadItem(ref pat, "data", ReadData);

			// Pattern time signature
			uint32 rpb = 0, rpm = 0;

			ssb.ReadItem(ref rpb, "RPB.");
			ssb.ReadItem(ref rpm, "RPM.");
			pat.SetSignature(rpb, rpm);

			TempoSwing swing = new TempoSwing();
			ssb.ReadItem(ref swing, "SWNG", TempoSwing.Deserialize);

			if (!swing.empty())
				swing.resize(pat.GetRowsPerBeat());

			pat.SetTempoSwing(swing);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadData(ReaderStream iStrm, ref CPattern pat, size_t _)//XX 612
		{
			if (!pat.IsValid())	// Expecting patterns to be allocated and resized properly
				return;

			ChannelIndex chns = pat.GetNumChannels();
			RowIndex rows = pat.GetNumRows();

			vector<ModCommand> lastChnMc = new vector<ModCommand>(chns);

			RowIndex row = 0;

			while ((row < rows) && !iStrm.EndOfStream)
			{
				Io_.ReadIntLE(iStrm, out uint8 t);

				if (t == 0)
				{
					row++;
					continue;
				}

				ChannelIndex ch = (ChannelIndex)(t & ItTools.It_BitMask_PatternChanField);

				if (ch > 0)
					ch--;

				uint8 diffMask = 0;

				if ((t & ItTools.It_BitMask_PatternChanEnabled) != 0)
					Io_.ReadIntLE(iStrm, out diffMask);

				uint8 temp = 0;

				ModCommand dummy = new ModCommand();
				ModCommand m = ch < chns ? pat.GetpModCommand(row, ch)[0] : dummy;

				if ((diffMask & (uint8)MaskBits.NoteBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].Note = (ModCommandNote)temp;
				}

				if (ch < chns)
					m.Note = lastChnMc[ch].Note;

				if ((diffMask & (uint8)MaskBits.InstrBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].Instr = (ModCommandInstr)temp;
				}

				if (ch < chns)
					m.Instr = lastChnMc[ch].Instr;

				if ((diffMask & (uint8)MaskBits.VolCmdBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].VolCmd = (ModCommandVolCmd)temp;
				}

				if (ch < chns)
					m.VolCmd = lastChnMc[ch].VolCmd;

				if ((diffMask & (uint8)MaskBits.VolBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].Vol = (ModCommandVol)temp;
				}

				if (ch < chns)
					m.Vol = lastChnMc[ch].Vol;

				if ((diffMask & (uint8)MaskBits.CommandBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].Command = (ModCommandCommand)temp;
				}

				if (ch < chns)
					m.Command = lastChnMc[ch].Command;

				if ((diffMask & (uint8)MaskBits.EffectParamBit) != 0)
				{
					Io_.ReadIntLE(iStrm, out temp);

					if (ch < chns)
						lastChnMc[ch].Param = (ModCommandParam)temp;
				}

				if (ch < chns)
					m.Param = lastChnMc[ch].Param;

				if ((diffMask & (uint8)MaskBits.ExtraData) != 0)
				{
					// Ignore additional data
					Io_.ReadIntLE(iStrm, out uint8 size);
					iStrm.Seek(size, SeekOrigin.Current);
				}

				if (!m.IsPcNote())
					m.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public forward_iterator<ModCommand> Begin()
		{
			return m_ModCommands.begin();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public forward_iterator<ModCommand> End()
		{
			return m_ModCommands.end();
		}



		/********************************************************************/
		/// <summary>
		/// Returns an enumerator over the commands of the pattern, so that
		/// it can be used in a C# foreach loop (C++ begin()/end())
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Buffer_Enumerator<ModCommand> GetEnumerator()
		{
			return m_ModCommands.GetEnumerator();
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
