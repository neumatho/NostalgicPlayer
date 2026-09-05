/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Detail;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Algorithm = Polycode.NostalgicPlayer.Kit.C.Std.Algorithm;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Helper methods for MOD files
	/// </summary>
	internal static class ModTools
	{
		private static readonly EffectCommand[] effTrans =
		[
			// MOD effects
			EffectCommand.Arpeggio, EffectCommand.PortamentoUp, EffectCommand.PortamentoDown, EffectCommand.TonePortamento,	// 0123
			EffectCommand.Vibrato, EffectCommand.TonePortaVol, EffectCommand.VibratoVol, EffectCommand.Tremolo,				// 4567
			EffectCommand.Panning8, EffectCommand.Offset, EffectCommand.VolumeSlide, EffectCommand.PositionJump,			// 89AB
			EffectCommand.Volume, EffectCommand.PatternBreak, EffectCommand.ModCmdEx, EffectCommand.Tempo,					// CDEF

			// XM extended effects
			EffectCommand.GlobalVolume, EffectCommand.GlobalVolSlide, EffectCommand.None, EffectCommand.None,				// GHIJ
			EffectCommand.KeyOff, EffectCommand.SetEnvPosition, EffectCommand.None, EffectCommand.None,						// KLMN
			EffectCommand.None, EffectCommand.PanningSlide, EffectCommand.None, EffectCommand.Retrig,						// OPQR
			EffectCommand.None, EffectCommand.Tremor, EffectCommand.None, EffectCommand.None,								// STUV
			EffectCommand.Dummy, EffectCommand.XFinePortaUpDown, EffectCommand.Panbrello, EffectCommand.Midi,				// WXYZ
			EffectCommand.SmoothMidi, EffectCommand.SmoothMidi, EffectCommand.XParam,										// \\# (BeRoTracker uses command 37 instead of 36 for smooth MIDI macros; in old OpenMPT versions this was reserved for the unimplemented "velocity" command)
		];

		/********************************************************************/
		/// <summary>
		/// Check if header magic equals a given string
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsMagic(CPointer<byte> magic1, string magic2)
		{
			return CMemory.memcmp(magic1, magic2, 4) == 0;
		}



		/********************************************************************/
		/// <summary>
		/// For .DTM files from Apocalypse Abyss, where the first 2108 bytes
		/// are swapped
		/// </summary>
		/********************************************************************/
		public static class ReadAndSwap<T> where T : unmanaged
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static T From<TByte>(FileReader<TByte> file, bool swapBytes) where TByte : unmanaged
			{
				T value = new T();

				if (file.Read(ref value) && swapBytes)
				{
					byte_span2 byteView = Memory.As_Raw_Memory(ref value);

					for (int i = 0; i < (int)byteView.Size(); i += 2)
						(byteView[i], byteView[i + 1]) = (byteView[i + 1], byteView[i]);
				}

				return value;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check if number of malformed bytes in MOD pattern data exceeds
		/// some threshold
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ValidateModPatternData(FileReader file, uint32 threshold, bool extendedFormat)
		{
			ModPatternData patternData = new ModPatternData();

			if (!patternData.Read(file))
				return false;

			return CountMalformedModPatternData(patternData, extendedFormat) <= threshold;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ConvertModCommand(ModCommand m, uint8 command, uint8 param)//XX 18
		{
			m.Command = EffectCommand.None;
			m.Param = param;

			if ((command == 0x00) && (param == 0x00))
				m.Command = EffectCommand.None;
			else if ((command == 0x0f) && (param < 0x20))
			{
				// For a very long time (until OpenMPT 1.25.02.02), this code also imported 0x20 as CMD_SPEED for MOD files,
				// but this seems to contradict pretty much the majority of other MOD player out there.
				// 0x20 is Speed: Impulse Tracker, Scream Tracker, old ModPlug
				// 0x20 is Tempo: ProTracker, XMPlay, Imago Orpheus, Cubic Player, ChibiTracker, BeRoTracker, DigiTrakker, DigiTrekker, Disorder Tracker 2, DMP, Extreme's Tracker, ...
				m.Command = EffectCommand.Speed;
			}
			else if (command < effTrans.Length)
			{
				m.Command = effTrans[command];

				if (m.Command == EffectCommand.PatternBreak)
					m.Param = (ModCommandParam)(((m.Param >> 4) * 10) + (m.Param & 0x0f));
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert MOD sample header and validate
		/// </summary>
		/********************************************************************/
		public static uint32 ReadModSample(ModSampleHeader sampleHeader, ModSample sample, CharBuf sampleName, bool is4Chn)//XX 302
		{
			sampleHeader.ConvertToMpt(sample, is4Chn);
			sampleName.Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, sampleHeader.Name.ToArray()));

			// Get rid of weird characters in sample names
			for (c_int i = sampleName.Buf.Length - 1; i >= 0; i--)
			{
				uint8 c = sampleName.Buf[i];

				if ((c > 0) && (c < ' '))
					c = (uint8)' ';

				sampleName.Buf[i] = c;
			}

			// Check for invalid values
			return sampleHeader.GetInvalidByteScore();
		}



		/********************************************************************/
		/// <summary>
		/// Count malformed bytes in MOD pattern data
		/// </summary>
		/********************************************************************/
		public static uint32 CountMalformedModPatternData(ModPatternData patternData, bool extendedFormat)//XX 355
		{
			uint8 mask = (uint8)(extendedFormat ? 0xe0 : 0xf0);
			uint32 malformedBytes = 0;

			foreach (ModPatternData.ModPatternRowData row in patternData)
			{
				foreach (CPointer<uint8> data in row)
				{
					if ((data[0] & mask) != 0)
						malformedBytes++;

					if (!extendedFormat)
					{
						uint16 period = (uint16)(((data[0] & 0x0f) << 8) | data[1]);

						if ((period != 0) && (period != 0xfff))
						{
							// Allow periods to deviate by +/-1 as found in some files
							bool CompareFunc(uint16 l, uint16 r)
							{
								return l > (r + 1);
							}

							MptSpan<uint16> periodTable = new MptSpan<uint16>(Tables.ProTrackerPeriodTable).SubSpan(24, 36);

							if (!Algorithm.binary_search(periodTable.Begin(), periodTable.End(), period, CompareFunc))
								malformedBytes += 2;
						}
					}
				}
			}

			return malformedBytes;
		}



		/********************************************************************/
		/// <summary>
		/// Parse the order list to determine how many patterns are used in
		/// the file
		/// </summary>
		/********************************************************************/
		public static PatternIndex GetNumPatterns(FileReader file, CSoundFile sndFile, OrderIndex numOrders, SmpLength totalSampleLen, SmpLength wowSampleLen, bool validateHiddenPatterns)//XX 384
		{
			ModSequence order = sndFile.Order.Current;
			PatternIndex numPatterns = 0;			// Total number of patterns in file (determined by going through the whole order list) with pattern number < 128
			PatternIndex officialPatterns = 0;		// Number of patterns only found in the "official" part of the order list (i.e. order positions < claimed order length)
			PatternIndex numPatternsIllegal = 0;	// Total number of patterns in file, also counting in "invalid" pattern indexes >= 128

			for (OrderIndex ord = 0; ord < 128; ord++)
			{
				PatternIndex pat = order[ord];

				if ((pat < 128) && (numPatterns <= pat))
				{
					numPatterns = (PatternIndex)(pat + 1);

					if (ord < numOrders)
						officialPatterns = numPatterns;
				}

				if (pat >= numPatternsIllegal)
					numPatternsIllegal = (PatternIndex)(pat + 1);
			}

			// Remove the garbage patterns past the official order end now that we don't need them anymore
			order.resize(numOrders);

			size_t patternStartOffset = file.GetPosition();
			size_t sizeWithoutPatterns = totalSampleLen + patternStartOffset;
			size_t sizeWithOfficialPatterns = sizeWithoutPatterns + (officialPatterns * (size_t)sndFile.GetNumChannels() * 256);

			// There are some WOW files with an extra byte at the end, and also a MOD file (idntmind.mod, MD5 a3af5c3e1af269e32dfb6677c41c8453, SHA1 4884717c298575f9884b2211c762bb1725f73743)
			// where only the "official" patterns should be counted but the file also has an extra byte at the end.
			// Since MOD files can technically not have an odd file size, we just always round the actual file size down
			size_t fileSize = Numeric.Align_Down(file.GetLength(), (size_t)2);

			if ((wowSampleLen != 0) && (((wowSampleLen + patternStartOffset) + (numPatterns * 8U * 256U)) == fileSize))
			{
				// Check if this is a Mod's Grave WOW file... WOW files use the M.K. magic but are actually 8CHN files.
				// We do a simple pattern validation as well for regular MOD files that have non-module data attached at the end
				// (e.g. ponylips.mod, MD5 c039af363b1d99a492dafc5b5f9dd949, SHA1 1bee1941c47bc6f913735ce0cf1880b248b8fc93)
				file.Seek(patternStartOffset + (numPatterns * 4U * 256U));

				if (ValidateModPatternData(file, 16, true))
					sndFile.ChnSettings.resize(8);

				file.Seek(patternStartOffset);
			}
			else if ((numPatterns != officialPatterns) && (validateHiddenPatterns || (sizeWithOfficialPatterns == fileSize)))
			{
				// 15-sample SoundTracker specifics:
				// Fix SoundTracker modules where "hidden" patterns should be ignored.
				// razor-1911.mod (MD5 b75f0f471b0ae400185585ca05bf7fe8, SHA1 4de31af234229faec00f1e85e1e8f78f405d454b)
				// and captain_fizz.mod (MD5 55bd89fe5a8e345df65438dbfc2df94e, SHA1 9e0e8b7dc67939885435ea8d3ff4be7704207a43)
				// seem to have the "correct" file size when only taking the "official" patterns into account,
				// but they only play correctly when also loading the inofficial patterns.
				// On the other hand, the SoundTracker module
				// wolf1.mod (MD5 a4983d7a432d324ce8261b019257f4ed, SHA1 aa6b399d02546bcb6baf9ec56a8081730dea3f44),
				// wolf3.mod (MD5 af60840815aa9eef43820a7a04417fa6, SHA1 24d6c2e38894f78f6c5c6a4b693a016af8fa037b)
				// and jean_baudlot_-_bad_dudes_vs_dragonninja-dragonf.mod (MD5 fa48e0f805b36bdc1833f6b82d22d936, SHA1 39f2f8319f4847fe928b9d88eee19d79310b9f91)
				// only play correctly if we ignore the hidden patterns.
				// Hence, we have a peek at the first hidden pattern and check if it contains a lot of illegal data.
				// If that is the case, we assume it's part of the sample data and only consider the "official" patterns.
				//
				// 31-sample NoiseTracker / ProTracker specifics:
				// Interestingly, (broken) variants of the ProTracker modules
				// "killing butterfly" (MD5 bd676358b1dbb40d40f25435e845cf6b, SHA1 9df4ae21214ff753802756b616a0cafaeced8021),
				// "quartex" by Reflex (MD5 35526bef0fb21cb96394838d94c14bab, SHA1 116756c68c7b6598dcfbad75a043477fcc54c96c),
				// seem to have the "correct" file size when only taking the "official" patterns into account, but they only play
				// correctly when also loading the inofficial patterns.
				// On the other hand, "Shofixti Ditty.mod" from Star Control 2 (MD5 62b7b0819123400e4d5a7813eef7fc7d, SHA1 8330cd595c61f51c37a3b6f2a8559cf3fcaaa6e8)
				// doesn't sound correct when taking the second "inofficial" pattern into account
				file.Seek(patternStartOffset + (size_t)(officialPatterns * sndFile.GetNumChannels() * 256));

				if (!ValidateModPatternData(file, 64, true))
					numPatterns = officialPatterns;

				file.Seek(patternStartOffset);
			}

			if ((numPatternsIllegal > numPatterns) && ((sizeWithoutPatterns + (size_t)(numPatternsIllegal * sndFile.GetNumChannels() * 256)) == fileSize))
			{
				// Even those illegal pattern indexes (> 128) appear to be valid... What a weird file!
				// e.g. NIETNU.MOD, where the end of the order list is filled with FF rather than 00, and the file actually contains 256 patterns
				numPatterns = numPatternsIllegal;
			}
			else if (numPatternsIllegal >= 0xff)
			{
				// Patterns FE and FF are used with S3M semantics (e.g. some MODs written with old OpenMPT versions)
				order.Replace(0xfe, Snd_Def.PatternIndex_Skip);
				order.Replace(0xff, Snd_Def.PatternIndex_Invalid);
			}

			return numPatterns;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static (uint8, uint8) ReadModPatternEntry(FileReader file, ModCommand m)//XX 474
		{
			return ReadModPatternEntry(file.ReadArray<uint8>(4), m);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static (uint8, uint8) ReadModPatternEntry(array<uint8> data, ModCommand m)//XX 480
		{
			// Read Period
			uint16 period = (uint16)(((data[0] & 0x0f) << 8) | data[1]);
			size_t note = ModCommand.Note_None;

			if ((period > 0) && (period != 0xfff))
			{
				note = (size_t)Tables.ProTrackerPeriodTable.Length + 23 + ModCommand.Note_Min;

				for (size_t i = 0; i < (size_t)Tables.ProTrackerPeriodTable.Length; i++)
				{
					if (period >= Tables.ProTrackerPeriodTable[i])
					{
						if ((period != Tables.ProTrackerPeriodTable[i]) && (i != 0))
						{
							uint16 p1 = Tables.ProTrackerPeriodTable[i - 1];
							uint16 p2 = Tables.ProTrackerPeriodTable[i];

							if ((p1 - period) < (period - p2))
							{
								note = i + 23 + ModCommand.Note_Min;
								break;
							}
						}

						note = i + 24 + ModCommand.Note_Min;
						break;
					}
				}
			}

			m.Note = (ModCommandNote)note;

			// Read instrument
			m.Instr = (uint8)((data[2] >> 4) | (data[0] & 0x10));

			// Read effect
			m.Command = EffectCommand.None;
			uint8 command = (uint8)(data[2] & 0x0f), param = data[3];

			return (command, param);
		}
	}
}
