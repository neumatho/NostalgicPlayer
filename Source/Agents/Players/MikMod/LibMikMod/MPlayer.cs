/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Agent.Player.MikMod.Containers;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod;
using Polycode.NostalgicPlayer.Agent.Shared.MikMod.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.MikMod.LibMikMod
{
	/// <summary>
	/// Main player routine
	/// </summary>
	internal class MPlayer
	{
		private readonly Module pf;
		private readonly IDriver driver;

		private readonly MUniTrk uniTrk;

		private readonly Random rnd;

		/// <summary></summary>
		public readonly byte mdSngChn;

		/// <summary></summary>
		public ushort mdBpm;

		/// <summary>
		/// Indicate if module end has been reached
		/// </summary>
		public bool endReached;

		// Effect method call lookup table
		private delegate int EffectFunc(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel);

		private readonly EffectFunc[] effects;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MPlayer(Module of, IDriver driver)
		{
			pf = of;
			this.driver = driver;

			uniTrk = new MUniTrk();

			rnd = new Random();

			// Initialize mixer stuff
			mdSngChn = Math.Max(of.NumChn, of.NumVoices);

			effects = new EffectFunc[(int)Command.UniLast]
			{
				DoNothing,			// 0
				DoNothing,			// UNI_NOTE
				DoNothing,			// UNI_INSTRUMENT
				DoPtEffect0,		// UNI_PTEFFECT0
				DoPtEffect1,		// UNI_PTEFFECT1
				DoPtEffect2,		// UNI_PTEFFECT2
				DoPtEffect3,		// UNI_PTEFFECT3
				DoPtEffect4,		// UNI_PTEFFECT4
				DoPtEffect5,		// UNI_PTEFFECT5
				DoPtEffect6,		// UNI_PTEFFECT6
				DoPtEffect7,		// UNI_PTEFFECT7
				DoPtEffect8,		// UNI_PTEFFECT8
				DoPtEffect9,		// UNI_PTEFFECT9
				DoPtEffectA,		// UNI_PTEFFECTA
				DoPtEffectB,		// UNI_PTEFFECTB
				DoPtEffectC,		// UNI_PTEFFECTC
				DoPtEffectD,		// UNI_PTEFFECTD
				DoPtEffectE,		// UNI_PTEFFECTE
				DoPtEffectF,		// UNI_PTEFFECTF
				DoS3MEffectA,		// UNI_S3MEFFECTA
				DoS3MEffectD,		// UNI_S3MEFFECTD
				DoS3MEffectE,		// UNI_S3MEFFECTE
				DoS3MEffectF,		// UNI_S3MEFFECTF
				DoS3MEffectI,		// UNI_S3MEFFECTI
				DoS3MEffectQ,		// UNI_S3MEFFECTQ
				DoS3MEffectR,		// UNI_S3MEFFECTR
				DoS3MEffectT,		// UNI_S3MEFFECTT
				DoS3MEffectU,		// UNI_S3MEFFECTU
				DoKeyOff,			// UNI_KEYOFF
				DoKeyFade,			// UNI_KEYFADE
				DoVolEffects,		// UNI_VOLEFFECTS
				DoPtEffect4Fix,		// UNI_XMEFFECT4
				DoXmEffect6,		// UNI_XMEFFECT6
				DoXmEffectA,		// UNI_XMEFFECTA
				DoXmEffectE1,		// UNI_XMEFFECTE1
				DoXmEffectE2,		// UNI_XMEFFECTE2
				DoXmEffectEA,		// UNI_XMEFFECTEA
				DoXmEffectEB,		// UNI_XMEFFECTEB
				DoXmEffectG,		// UNI_XMEFFECTG
				DoXmEffectH,		// UNI_XMEFFECTH
				DoXmEffectL,		// UNI_XMEFFECTL
				DoXmEffectP,		// UNI_XMEFFECTP
				DoXmEffectX1,		// UNI_XMEFFECTX1
				DoXmEffectX2,		// UNI_XMEFFECTX2
				DoItEffectG,		// UNI_ITEFFECTG
				DoItEffectH,		// UNI_ITEFFECTH
				DoItEffectI,		// UNI_ITEFFECTI
				DoItEffectM,		// UNI_ITEFFECTM
				DoItEffectN,		// UNI_ITEFFECTN
				DoItEffectP,		// UNI_ITEFFECTP
				DoItEffectT,		// UNI_ITEFFECTT
				DoItEffectU,		// UNI_ITEFFECTU
				DoItEffectW,		// UNI_ITEFFECTW
				DoItEffectY,		// UNI_ITEFFECTY
				DoNothing,			// UNI_ITEFFECTZ
				DoItEffectS0,		// UNI_ITEFFECTS0
				DoUltEffect9,		// UNI_ULTEFFECT9
				DoMedSpeed,			// UNI_MEDSPEED
				DoMedEffectF1,		// UNI_MEDEFFECTF1
				DoMedEffectF2,		// UNI_MEDEFFECTF2
				DoMedEffectF3,		// UNI_MEDEFFECTF3
				DoOktArp,			// UNI_OKTARP
				DoNothing,			// Unused
				DoPtEffect4Fix,		// UNI_S3MEFFECTH
				DoItEffectH_Old,	// UNI_ITEFFECTH_OLD
				DoItEffectU_Old,	// UNI_ITEFFECTU_OLD
				DoPtEffect4Fix,		// UNI_GDMEFFECT4
				DoPtEffect7Fix,		// UNI_GDMEFFECT7
				DoS3MEffectU,		// UNI_GDMEFFECT14
				DoMedEffectVib,		// UNI_MEDEFFECT_VIB
				DoMedEffectFD,		// UNI_MEDEFFECT_FD
				DoMedEffect16,		// UNI_MEDEFFECT_16
				DoMedEffect18,		// UNI_MEDEFFECT_18
				DoMedEffect1E,		// UNI_MEDEFFECT_1E
				DoMedEffect1F,		// UNI_MEDEFFECT_1F
				DoFarEffect1,		// UNI_FAREFFECT1
				DoFarEffect2,		// UNI_FAREFFECT2
				DoFarEffect3,		// UNI_FAREFFECT3
				DoFarEffect4,		// UNI_FAREFFECT4
				DoFarEffect6,		// UNI_FAREFFECT6
				DoFarEffectD,		// UNI_FAREFFECTD
				DoFarEffectE,		// UNI_FAREFFECTE
				DoFarEffectF		// UNI_FAREFFECTF
			};
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the player
		/// </summary>
		/********************************************************************/
		public void Init(Module mod, short startPos)
		{
			mod.ExtSpd = true;
			mod.PanFlag = true;
			mod.Wrap = false;
			mod.Loop = true;
			mod.FadeOut = false;

			mod.RelSpd = 0;

			// Make sure that the player doesn't start with garbage
			mod.Control = Helpers.InitializeArray<Mp_Control>(mod.NumChn);
			mod.Voice = Helpers.InitializeArray<Mp_Voice>(mdSngChn);

			mod.NumVoices = mdSngChn;

			InitInternal(mod, startPos);
		}



		/********************************************************************/
		/// <summary>
		/// The main player function
		/// </summary>
		/********************************************************************/
		public void HandleTick()
		{
			endReached = false;

			if ((pf == null) || (pf.SngPos >= pf.NumPos))
				return;

			// Update time counter (sngTime is in milliseconds (in fact 2^-10))
			pf.SngRemainder += (1 << 9) * 5;		// Thus 2.5 * (1 << 10), since fps=0.4 * tempo
			pf.SngTime += (uint)pf.SngRemainder / pf.Bpm;
			pf.SngRemainder %= pf.Bpm;

			if (++pf.VbTick >= pf.SngSpd)
			{
				if (pf.Pat_RepCrazy)
					pf.Pat_RepCrazy = false;		// Play 2 times row 0
				else
					pf.PatPos++;

				pf.VbTick = 0;

				// Process pattern-delay. pf.PatDly2 is the counter and pf.PatDly is
				// the command memory
				if (pf.PatDly != 0)
				{
					pf.PatDly2 = pf.PatDly;
					pf.PatDly = 0;
				}

				if (pf.PatDly2 != 0)
				{
					// Pattern delay active
					if (--pf.PatDly2 != 0)
					{
						// So turn back pf.PatPos by 1
						if (pf.PatPos != 0)
							pf.PatPos--;
					}
				}

				// Do we have to get a new pattern pointer? (when pf.PatPos reaches the
				// pattern size, or when a pattern break is active)
				if ((pf.PatPos >= pf.NumRow) && (pf.PosJmp == 0))
					pf.PosJmp = 3;

				if (pf.PosJmp != 0)
				{
					pf.PatPos = (ushort)(pf.NumRow != 0 ? (pf.PatBrk % pf.NumRow) : 0);
					pf.Pat_RepCrazy = false;
					pf.SngPos += (short)(pf.PosJmp - 2);

					for (short channel = 0; channel < pf.NumChn; channel++)
						pf.Control[channel].Pat_RepPos = -1;

					pf.PatBrk = 0;
					pf.PosJmp = 0;

					if (pf.SngPos < 0)
						pf.SngPos = (short)(pf.NumPos - 1);

					// Handle the "---" (end of song) pattern since it can occur
					// *inside* the module in some formats
					if ((pf.SngPos >= pf.NumPos) || (pf.Positions[pf.SngPos] == SharedConstant.Last_Pattern))
					{
						if (!pf.Wrap)
							return;

						pf.SngPos = (short)pf.RepPos;
						if (pf.SngPos == 0)
						{
							pf.Volume = pf.InitVolume > 128 ? (short)128 : pf.InitVolume;

							if ((pf.Flags & ModuleFlag.FarTempo) != 0)
							{
								pf.FarCurTempo = pf.InitSpeed;
								pf.FarTempoBend = 0;
								SetFarTempo(pf);
							}
							else
							{
								if (pf.InitSpeed != 0)
									pf.SngSpd = pf.InitSpeed < pf.BpmLimit ? pf.InitSpeed : pf.BpmLimit;
								else
									pf.SngSpd = 6;

								pf.Bpm = pf.InitTempo < pf.BpmLimit ? pf.BpmLimit : pf.InitTempo;
							}
						}

						// Tell NostalgicPlayer we has restarted
						endReached = true;
					}
				}

				if (pf.PatDly2 == 0)
					Pt_Notes(pf);
			}

			// Fade global volume if enabled and we're playing the last pattern
			int maxVolume;

			if (((pf.SngPos == (pf.NumPos - 1)) || (pf.Positions[pf.SngPos + 1] == SharedConstant.Last_Pattern)) && pf.FadeOut)
				maxVolume = pf.NumRow != 0 ? ((pf.NumRow - pf.PatPos) * 128) / pf.NumRow : 0;
			else
				maxVolume = 128;

			Pt_EffectsPass1(pf);

			if ((pf.Flags & ModuleFlag.Nna) != 0)
				Pt_Nna(pf);

			Pt_SetupVoices(pf);
			Pt_EffectsPass2(pf);

			// Now set up the actual hardware channel playback information
			Pt_UpdateVoices(pf, maxVolume);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the note period and return it
		/// </summary>
		/********************************************************************/
		public ushort GetPeriod(ModuleFlag flags, ushort note, uint speed)
		{
			if ((flags & ModuleFlag.XmPeriods) != 0)
			{
				if ((flags & ModuleFlag.Linear) != 0)
					return GetLinearPeriod(note, speed);
				else
					return GetLogPeriod(note, speed);
			}
			else
				return GetOldPeriod(note, speed);
		}



		/********************************************************************/
		/// <summary>
		/// XM linear period to frequency conversion
		/// </summary>
		/********************************************************************/
		public uint GetFrequency(ModuleFlag flags, uint period)
		{
			if ((flags & ModuleFlag.Linear) != 0)
			{
				int shift = ((int)period / 768) - Constant.HighOctave;

				if (shift >= 0)
					return LookupTables.LinTab[period % 768] >> shift;
				else
					return LookupTables.LinTab[period % 768] << (-shift);
			}
			else
				return (uint)((8363L * 1712L) / (period != 0 ? period : 1));
		}



		/********************************************************************/
		/// <summary>
		/// NUMVOICES macro made as an inline method
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte NumVoices(Module mod)
		{
			return mdSngChn < mod.NumVoices ? mdSngChn : mod.NumVoices;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// More initializing of the player
		/// </summary>
		/********************************************************************/
		private void InitInternal(Module mod, short startPos)
		{
			for (int t = 0; t < mod.NumChn; t++)
			{
				mod.Control[t].Main.ChanVol = mod.ChanVol[t];
				mod.Control[t].Main.Panning = (short)mod.Panning[t];
			}

			mod.SngTime = 0;
			mod.SngRemainder = 0;

			mod.Pat_RepCrazy = false;
			mod.SngPos = startPos;

			if ((mod.Flags & ModuleFlag.FarTempo) != 0)
			{
				mod.FarCurTempo = mod.InitSpeed;
				mod.FarTempoBend = 0;
				SetFarTempo(mod);
			}
			else
			{
				if (mod.InitSpeed != 0)
					mod.SngSpd = mod.InitSpeed < mod.BpmLimit ? mod.InitSpeed : mod.BpmLimit;
				else
					mod.SngSpd = 6;

				mod.Bpm = mod.InitTempo < mod.BpmLimit ? mod.BpmLimit : mod.InitTempo;
			}

			mod.Volume = (short)(mod.InitVolume > 128 ? 128 : mod.InitVolume);

			mod.VbTick = mod.SngSpd;
			mod.PatDly = 0;
			mod.PatDly2 = 0;
			mod.RealChn = 0;

			mod.PatPos = 0;
			mod.PosJmp = 2;				// Make sure the player fetches the first note
			mod.NumRow = 0xffff;
			mod.PatBrk = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Handles new notes or instruments
		/// </summary>
		/********************************************************************/
		private void Pt_Notes(Module mod)
		{
			for (short channel = 0; channel < mod.NumChn; channel++)
			{
				int tr;

				Mp_Control a = mod.Control[channel];

				if (mod.SngPos >= mod.NumPos)
				{
					tr = mod.NumTrk;
					mod.NumRow = 0;
				}
				else
				{
					tr = mod.Patterns[(mod.Positions[mod.SngPos] * mod.NumChn) + channel];
					mod.NumRow = mod.PattRows[mod.Positions[mod.SngPos]];
				}

				a.Row = (tr < mod.NumTrk) ? mod.Tracks[tr] : null;
				if (a.Row != null)
					a.RowOffset = uniTrk.UniFindRow(a.Row, mod.PatPos);

				a.NewSamp = 0;

				if (mod.VbTick == 0)
					a.Main.NoteDelay = 0;

				if ((a.Row == null) || (mod.NumRow == 0))
					continue;

				uniTrk.UniSetRow(a.Row, a.RowOffset);
				int funky = 0;

				byte c;
				while ((c = uniTrk.UniGetByte()) != 0)
				{
					switch ((Command)c)
					{
						case Command.UniNote:
						{
							funky |= 1;

							a.OldNote = a.ANote;
							a.ANote = uniTrk.UniGetByte();
							a.Main.Kick = Kick.Note;
							a.Main.Start = -1;
							a.Sliding = 0;
							a.FarTonePortaRunning = false;

							// Retrig tremolo and vibrato waves?
							if ((a.WaveControl & 0x40) == 0)
								a.TrmPos = 0;

							if ((a.WaveControl & 0x04) == 0)
								a.VibPos = 0;

							a.PanbPos = 0;
							break;
						}

						case Command.UniInstrument:
						{
							byte inst = uniTrk.UniGetByte();

							if (inst >= mod.NumIns)
								break;					// Safety valve

							funky |= 2;

							a.Main.I = (mod.Flags & ModuleFlag.Inst) != 0 ? mod.Instruments[inst] : null;
							a.Retrig = 0;
							a.S3MTremor = 0;
							a.UltOffset = 0;
							a.Main.Sample = inst;
							break;
						}

						default:
						{
							uniTrk.UniSkipOpcode();
							break;
						}
					}
				}

				if (funky != 0)
				{
					Instrument i;
					Sample s;

					if ((i = a.Main.I) != null)
					{
						if (i.SampleNumber[a.ANote] >= mod.NumSmp)
							continue;

						s = mod.Samples[i.SampleNumber[a.ANote]];
						a.Main.Note = i.SampleNote[a.ANote];
					}
					else
					{
						a.Main.Note = a.ANote;
						s = mod.Samples[a.Main.Sample];
					}

					if (a.Main.S != s)
					{
						a.Main.S = s;
						a.NewSamp = a.Main.Period;
					}

					// Channel or instrument determined panning?
					a.Main.Panning = (short)mod.Panning[channel];

					if ((s.Flags & SampleFlag.OwnPan) != 0)
						a.Main.Panning = s.Panning;
					else
					{
						if ((i != null) && ((i.Flags & InstrumentFlag.OwnPan) != 0))
							a.Main.Panning = i.Panning;
					}

					a.Main.Handle = s.Handle;
					a.Speed = s.Speed;

					if (i != null)
					{
						if (mod.PanFlag && ((i.Flags & InstrumentFlag.PitchPan) != 0) && (a.Main.Panning != SharedConstant.Pan_Surround))
						{
							a.Main.Panning += (short)(((a.ANote - i.PitPanCenter) * i.PitPanSep) / 8);

							if (a.Main.Panning < SharedConstant.Pan_Left)
								a.Main.Panning = SharedConstant.Pan_Left;
							else
							{
								if (a.Main.Panning > SharedConstant.Pan_Right)
									a.Main.Panning = SharedConstant.Pan_Right;
							}
						}

						a.Main.PitFlg = i.PitFlg;
						a.Main.VolFlg = i.VolFlg;
						a.Main.PanFlg = i.PanFlg;
						a.Main.Nna = i.NnaType;
						a.Dca = i.Dca;
						a.Dct = i.Dct;
					}
					else
					{
						a.Main.PitFlg = a.Main.VolFlg = a.Main.PanFlg = 0;
						a.Main.Nna = 0;
						a.Dca = 0;
						a.Dct = Dct.Off;
					}

					if ((funky & 2) != 0)		// Instrument change
					{
						// IT random volume variations: 0:8 bit fixed, and one bit for sign
						a.Volume = a.TmpVolume = s.Volume;

						if ((s != null) && (i != null))
						{
							if (i.RVolVar != 0)
							{
								a.Volume = a.TmpVolume = (short)(s.Volume + ((s.Volume * (i.RVolVar * GetRandom(512))) / 25600));

								if (a.Volume < 0)
									a.Volume = a.TmpVolume = 0;
								else
								{
									if (a.Volume > 64)
										a.Volume = a.TmpVolume = 64;
								}
							}

							if (mod.PanFlag && (a.Main.Panning != SharedConstant.Pan_Surround))
							{
								a.Main.Panning += (short)((a.Main.Panning * (i.RPanVar * GetRandom(512))) / 25600);

								if (a.Main.Panning < SharedConstant.Pan_Left)
									a.Main.Panning = SharedConstant.Pan_Left;
								else
								{
									if (a.Main.Panning > SharedConstant.Pan_Right)
										a.Main.Panning = SharedConstant.Pan_Right;
								}
							}
						}
					}

					a.WantedPeriod = a.TmpPeriod = GetPeriod(mod.Flags, (ushort)(a.Main.Note << 1), a.Speed);
					a.Main.KeyOff = KeyFlag.Kick;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Changes the voices, e.g. create the envelopes
		/// </summary>
		/********************************************************************/
		private void Pt_UpdateVoices(Module mod, int maxVolume)
		{
			mod.TotalChn = mod.RealChn = 0;

			for (short channel = 0; channel < NumVoices(mod); channel++)
			{
				Mp_Voice aOut = mod.Voice[channel];
				Instrument i = aOut.Main.I;
				Sample s = aOut.Main.S;

				if ((s == null) || (s.Length == 0))
					continue;

				if (aOut.Main.Period < 40)
					aOut.Main.Period = 40;
				else
				{
					if (aOut.Main.Period > 50000)
						aOut.Main.Period = 50000;
				}

				if ((aOut.Main.Kick == Kick.Note) || (aOut.Main.Kick == Kick.KeyOff))
				{
					driver.VoicePlayInternal((sbyte)channel, s, (aOut.Main.Start == -1) ? (((s.Flags & SampleFlag.UstLoop) != 0) ? s.LoopStart : 0) : (uint)aOut.Main.Start);

					aOut.Main.FadeVol = 32768;
					aOut.ASwpPos = 0;
				}

				short envVol = 256;
				short envPan = SharedConstant.Pan_Center;
				short envPit = 32;

				if ((i != null) && ((aOut.Main.Kick == Kick.Note) || (aOut.Main.Kick == Kick.Env)))
				{
					if ((aOut.Main.VolFlg & EnvelopeFlag.On) != 0)
						envVol = StartEnvelope(ref aOut.VEnv, aOut.Main.VolFlg, i.VolPts, i.VolSusBeg, i.VolSusEnd, i.VolBeg, i.VolEnd, i.VolEnv, aOut.Main.KeyOff);

					if ((aOut.Main.PanFlg & EnvelopeFlag.On) != 0)
						envPan = StartEnvelope(ref aOut.PEnv, aOut.Main.PanFlg, i.PanPts, i.PanSusBeg, i.PanSusEnd, i.PanBeg, i.PanEnd, i.PanEnv, aOut.Main.KeyOff);

					if ((aOut.Main.PitFlg & EnvelopeFlag.On) != 0)
						envPit = StartEnvelope(ref aOut.CEnv, aOut.Main.PitFlg, i.PitPts, i.PitSusBeg, i.PitSusEnd, i.PitBeg, i.PitEnd, i.PitEnv, aOut.Main.KeyOff);

					if ((aOut.CEnv.Flg & EnvelopeFlag.On) != 0)
						aOut.MasterPeriod = GetPeriod(mod.Flags, (ushort)(aOut.Main.Note << 1), aOut.Master.Speed);
				}
				else
				{
					if ((aOut.Main.VolFlg & EnvelopeFlag.On) != 0)
						envVol = ProcessEnvelope(aOut, ref aOut.VEnv, 256);

					if ((aOut.Main.PanFlg & EnvelopeFlag.On) != 0)
						envPan = ProcessEnvelope(aOut, ref aOut.PEnv, SharedConstant.Pan_Center);

					if ((aOut.Main.PitFlg & EnvelopeFlag.On) != 0)
						envPit = ProcessEnvelope(aOut, ref aOut.CEnv, 32);
				}

				if (aOut.Main.Kick == Kick.Note)
					aOut.Main.Kick_Flag = true;

				aOut.Main.Kick = Kick.Absent;

				uint tmpVol = aOut.Main.FadeVol;			// Max 32768
				tmpVol *= aOut.Main.ChanVol;				// * max 64
				tmpVol *= (uint)aOut.Main.OutVolume;		// * max 256
				tmpVol /= (256 * 64);						// tmpVol is max 32768 again

				aOut.TotalVol = tmpVol >> 2;				// Used to determine sample volume

				tmpVol *= (uint)envVol;						// * max 256
				tmpVol *= (uint)mod.Volume;					// * max 128
				tmpVol /= (128 * 256 * 128);

				// Fade out
				if (mod.SngPos >= mod.NumPos)
					tmpVol = 0;
				else
					tmpVol = (tmpVol * (uint)maxVolume) / 128;

				if ((aOut.MasterChn != -1) && mod.Control[aOut.MasterChn].Muted)
					driver.VoiceSetVolumeInternal((sbyte)channel, 0);
				else
				{
					driver.VoiceSetVolumeInternal((sbyte)channel, (ushort)tmpVol);

					if ((tmpVol != 0) && (aOut.Master != null) && (aOut.Master.Slave == aOut))
						mod.RealChn++;

					mod.TotalChn++;
				}

				if (aOut.Main.Panning == SharedConstant.Pan_Surround)
					driver.VoiceSetPanningInternal((sbyte)channel, SharedConstant.Pan_Surround);
				else
				{
					if (mod.PanFlag && ((aOut.PEnv.Flg & EnvelopeFlag.On) != 0))
						driver.VoiceSetPanningInternal((sbyte)channel, (uint)DoPan(envPan, aOut.Main.Panning));
					else
						driver.VoiceSetPanningInternal((sbyte)channel, (uint)aOut.Main.Panning);
				}

				int vibVal;

				if ((aOut.Main.Period != 0) && (s.VibDepth != 0))
				{
					if ((s.VibFlags & VibratoFlag.It) != 0)
					{
						// IT auto-vibrato uses regular waveforms
						vibVal = LfoVibratoIt((sbyte)aOut.AVibPos, s.VibType);
					}
					else
					{
						// XM auto-vibrato uses its own set of waveforms.
						// Also, uses LFO amplitudes on [-63,63], possibly to compensate
						// for depth being multiplied by 4 in the loader(?)
						vibVal = LfoAutoVibratoXm((sbyte)aOut.AVibPos, s.VibType) >> 2;
					}
				}
				else
					vibVal = 0;

				int vibDpt;

				if ((s.VibFlags & VibratoFlag.It) != 0)
				{
					if ((aOut.ASwpPos >> 8) < s.VibDepth)
					{
						aOut.ASwpPos += s.VibDepth;
						vibDpt = aOut.ASwpPos;
					}
					else
						vibDpt = s.VibDepth << 8;

					vibVal = (vibVal * vibDpt) >> 16;

					if (aOut.MFlag)
					{
						// This vibrato value is the correct value in fine linear slide
						// steps, but MikMod linear periods are halved, so the final
						// value also needs to be halved in linear mode
						if ((mod.Flags & ModuleFlag.Linear) != 0)
							vibVal >>= 1;

						aOut.Main.Period -= (ushort)vibVal;
					}
				}
				else
				{
					// Do XM style auto-vibrato
					if ((aOut.Main.KeyOff & KeyFlag.Off) == 0)
					{
						if (aOut.ASwpPos < s.VibSweep)
						{
							vibDpt = (aOut.ASwpPos * s.VibDepth) / s.VibSweep;
							aOut.ASwpPos++;
						}
						else
							vibDpt = s.VibDepth;
					}
					else
					{
						// Keyoff -> depth becomes 0 if final depth wasn't reached or
						// stays at final level if depth WAS reached
						if (aOut.ASwpPos >= s.VibSweep)
							vibDpt = s.VibDepth;
						else
							vibDpt = 0;
					}

					vibVal = (vibVal * vibDpt) >> 8;
					aOut.Main.Period -= (ushort)vibVal;
				}

				// Update vibrato position
				aOut.AVibPos = (ushort)((aOut.AVibPos + s.VibRate) & 0xff);

				// Process pitch envelope
				ushort playPeriod = aOut.Main.Period;

				if (((aOut.Main.PitFlg & EnvelopeFlag.On) != 0) && (envPit != 32))
				{
					envPit -= 32;
					if (((aOut.Main.Note << 1) + envPit) <= 0)
						envPit = (short)-(aOut.Main.Note << 1);

					int p1 = GetPeriod(mod.Flags, (ushort)((aOut.Main.Note << 1) + envPit), aOut.Master.Speed) - aOut.MasterPeriod;

					if (p1 > 0)
					{
						if ((ushort)(playPeriod + p1) <= playPeriod)
						{
							p1 = 0;
							aOut.Main.KeyOff |= KeyFlag.Off;
						}
					}
					else
					{
						if (p1 < 0)
						{
							if ((ushort)(playPeriod + p1) >= playPeriod)
							{
								p1 = 0;
								aOut.Main.KeyOff |= KeyFlag.Off;
							}
						}
					}

					playPeriod += (ushort)p1;
				}

				if (aOut.Main.FadeVol == 0)			// Check for a dead note (fadeVol = 0)
				{
					driver.VoiceStopInternal((sbyte)channel);
					mod.TotalChn--;

					if ((tmpVol != 0) && (aOut.Master != null) && (aOut.Master.Slave == aOut))
						mod.RealChn--;
				}
				else
				{
					driver.VoiceSetFrequencyInternal((sbyte)channel, GetFrequency(mod.Flags, playPeriod));

					// If keyFade, start subtracting FadeOutSpeed from fadeVol:
					if ((i != null) && ((aOut.Main.KeyOff & KeyFlag.Fade) != 0))
					{
						if (aOut.Main.FadeVol >= i.VolFade)
							aOut.Main.FadeVol -= i.VolFade;
						else
							aOut.Main.FadeVol = 0;
					}
				}
			}

			mdBpm = (ushort)(mod.Bpm + mod.RelSpd);
			if (mdBpm < mod.BpmLimit)
				mdBpm = mod.BpmLimit;
			else
			{
				if (((mod.Flags & ModuleFlag.HighBpm) == 0) && (mdBpm > 255))
					mdBpm = 255;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Setup module and NNA voices
		/// </summary>
		/********************************************************************/
		private void Pt_SetupVoices(Module mod)
		{
			for (short channel = 0; channel < mod.NumChn; channel++)
			{
				Mp_Control a = mod.Control[channel];

				if (a.Main.NoteDelay != 0)
					continue;

				Mp_Voice aOut;

				if (a.Main.Kick == Kick.Note)
				{
					// If no channel was cut above, find an empty or quiet channel here
					if ((mod.Flags & ModuleFlag.Nna) != 0)
					{
						if (a.Slave == null)
						{
							int newChn;
							if ((newChn = Mp_FindEmptyChannel(mod)) != -1)
								a.Slave = mod.Voice[a.SlaveChn = (byte)newChn];
						}
					}
					else
						a.Slave = mod.Voice[a.SlaveChn = (byte)channel];

					// Assign parts of MP_VOICE only done for a KICK_NOTE
					if ((aOut = a.Slave) != null)
					{
						if (aOut.MFlag && (aOut.Master != null))
							aOut.Master.Slave = null;

						aOut.Master = a;
						a.Slave = aOut;
						aOut.MasterChn = channel;
						aOut.MFlag = true;
					}
				}
				else
					aOut = a.Slave;

				if (aOut != null)
					aOut.Main = a.Main;

				a.Main.Kick = Kick.Absent;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Manages the NNA
		/// </summary>
		/********************************************************************/
		private void Pt_Nna(Module mod)
		{
			for (short channel = 0; channel < mod.NumChn; channel++)
			{
				Mp_Control a = mod.Control[channel];

				if (a.Main.Kick == Kick.Note)
				{
					bool kill = false;

					if (a.Slave != null)
					{
						Mp_Voice aOut = a.Slave;

						if ((aOut.Main.Nna & Nna.Mask) != 0)
						{
							// Make sure the old MP_VOICE channel knows it has no master now!
							a.Slave = null;

							// Assume the channel is taken by NNA
							aOut.MFlag = false;

							switch (aOut.Main.Nna)
							{
								case Nna.Continue:		// Continue note, do nothing
									break;

								case Nna.Off:			// Note off
								{
									aOut.Main.KeyOff |= KeyFlag.Off;

									if (((aOut.Main.VolFlg & EnvelopeFlag.On) == 0) || ((aOut.Main.VolFlg & EnvelopeFlag.Loop) != 0))
										aOut.Main.KeyOff = KeyFlag.Kill;

									break;
								}

								case Nna.Fade:
								{
									aOut.Main.KeyOff |= KeyFlag.Fade;
									break;
								}
							}
						}
					}

					if (a.Dct != Dct.Off)
					{
						for (int t = 0; t < NumVoices(mod); t++)
						{
							if (!driver.VoiceStoppedInternal((sbyte)t) && (mod.Voice[t].MasterChn == channel) && (a.Main.Sample == mod.Voice[t].Main.Sample))
							{
								kill = false;

								switch (a.Dct)
								{
									case Dct.Note:
									{
										if (a.Main.Note == mod.Voice[t].Main.Note)
											kill = true;

										break;
									}

									case Dct.Sample:
									{
										if (a.Main.Handle == mod.Voice[t].Main.Handle)
											kill = true;

										break;
									}

									case Dct.Inst:
									{
										kill = true;
										break;
									}
								}

								if (kill)
								{
									switch (a.Dca)
									{
										case Dca.Cut:
										{
											mod.Voice[t].Main.FadeVol = 0;
											break;
										}

										case Dca.Off:
										{
											mod.Voice[t].Main.KeyOff |= KeyFlag.Off;

											if (((mod.Voice[t].Main.VolFlg & EnvelopeFlag.On) == 0) || ((mod.Voice[t].Main.VolFlg & EnvelopeFlag.Loop) != 0))
												mod.Voice[t].Main.KeyOff = KeyFlag.Kill;

											break;
										}

										case Dca.Fade:
										{
											mod.Voice[t].Main.KeyOff |= KeyFlag.Fade;
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handles effects
		/// </summary>
		/********************************************************************/
		private void Pt_EffectsPass1(Module mod)
		{
			for (short channel = 0; channel < mod.NumChn; channel++)
			{
				Mp_Control a = mod.Control[channel];

				Mp_Voice aOut;
				if ((aOut = a.Slave) != null)
				{
					a.Main.FadeVol = aOut.Main.FadeVol;
					a.Main.Period = aOut.Main.Period;

					if (a.Main.Kick == Kick.KeyOff)
						a.Main.KeyOff = aOut.Main.KeyOff;
				}

				if (a.Row == null)
					continue;

				uniTrk.UniSetRow(a.Row, a.RowOffset);

				a.OwnPer = a.OwnVol = false;
				int explicitSlides = Pt_PlayEffects(mod, channel, a);

				// Continue volume slide if necessary for XM and IT
				if ((mod.Flags & ModuleFlag.BgSlides) != 0)
				{
					if ((explicitSlides == 0) && (a.Sliding != 0))
						DoS3MVolSlide(mod.VbTick, mod.Flags, a, 0);
					else
					{
						if (a.TmpVolume != 0)
							a.Sliding = (sbyte)explicitSlides;
					}
				}

				// Keep running Farandole tone porta
				if (a.FarTonePortaRunning)
					DoFarTonePorta(a);

				if (!a.OwnPer)
					a.Main.Period = a.TmpPeriod;

				if (!a.OwnVol)
					a.Volume = a.TmpVolume;

				if (a.Main.S != null)
				{
					if (a.Main.I != null)
						a.Main.OutVolume = (short)((a.Volume * a.Main.S.GlobVol * a.Main.I.GlobVol) >> 10);
					else
						a.Main.OutVolume = (short)((a.Volume * a.Main.S.GlobVol) >> 4);

					if (a.Main.OutVolume > 256)
						a.Main.OutVolume = 256;
					else
					{
						if (a.Main.OutVolume < 0)
							a.Main.OutVolume = 0;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Second effect pass
		/// </summary>
		/********************************************************************/
		private void Pt_EffectsPass2(Module mod)
		{
			for (short channel = 0; channel < mod.NumChn; channel++)
			{
				Mp_Control a = mod.Control[channel];

				if (a.Row == null)
					continue;

				uniTrk.UniSetRow(a.Row, a.RowOffset);

				byte c;
				while ((c = uniTrk.UniGetByte()) != 0)
				{
					if (((Command)c) == Command.UniItEffectS0)
					{
						c = uniTrk.UniGetByte();
						if ((SsCommand)(c >> 4) == SsCommand.S7Effects)
							DoNnaEffects(mod, a, (byte)(c & 0xf));
					}
					else
						uniTrk.UniSkipOpcode();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Returns a random value between 0 and ceil - 1, ceil must be a
		/// power of two
		/// </summary>
		/********************************************************************/
		private int GetRandom(int ceilVal)
		{
			return rnd.Next(ceilVal);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the note period and return it
		/// </summary>
		/********************************************************************/
		private ushort GetOldPeriod(ushort note, uint speed)
		{
			// This happens sometimes on badly converted AMF, and old MOD
			if (speed == 0)
				return 4242;		// Prevent divide overflow.. (42 hehe)

			ushort n = (ushort)(note % (2 * SharedConstant.Octave));
			ushort o = (ushort)(note / (2 * SharedConstant.Octave));

			return (ushort)(((8363L * LookupTables.OldPeriods[n]) >> o) / speed);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the note period and return it
		/// </summary>
		/********************************************************************/
		private ushort GetLinearPeriod(ushort note, uint fine)
		{
			ushort t = (ushort)(((20L + 2 * Constant.HighOctave) * SharedConstant.Octave + 2 - note) * 32L - (fine >> 1));

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the note period and return it
		/// </summary>
		/********************************************************************/
		private ushort GetLogPeriod(ushort note, uint fine)
		{
			ushort n = (ushort)(note % (2 * SharedConstant.Octave));
			ushort o = (ushort)(note / (2 * SharedConstant.Octave));
			uint i = (uint)(n << 2) + (fine >> 4);		// n * 8 + fine / 16

			ushort p1 = LookupTables.LogTab[i];
			ushort p2 = LookupTables.LogTab[i + 1];

			return (ushort)(Interpolate((short)(fine >> 4), 0, 15, (short)p1, (short)p2) >> o);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private short Interpolate(short p, short p1, short p2, short v1, short v2)
		{
			if ((p1 == p2) || (p == p1))
				return v1;

			return (short)(v1 + (((p - p1) * (v2 - v1)) / (p2 - p1)));
		}



		/********************************************************************/
		/// <summary>
		/// Interpolates the envelope
		/// </summary>
		/********************************************************************/
		private short InterpolateEnv(short p, ref EnvPt a, ref EnvPt b)
		{
			return Interpolate(p, a.Pos, b.Pos, a.Val, b.Val);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the panning value
		/// </summary>
		/********************************************************************/
		private short DoPan(short envPan, short pan)
		{
			int newPan = pan + (((envPan - SharedConstant.Pan_Center) * (128 - Math.Abs(pan - SharedConstant.Pan_Center))) / 128);

			return (short)(newPan < SharedConstant.Pan_Left ? SharedConstant.Pan_Left : (newPan > SharedConstant.Pan_Right ? SharedConstant.Pan_Right : newPan));
		}



		/********************************************************************/
		/// <summary>
		/// Initialize and start the envelope
		/// </summary>
		/********************************************************************/
		private short StartEnvelope(ref EnvPr t, EnvelopeFlag flg, byte pts, byte susBeg, byte susEnd, byte beg, byte end, EnvPt[] p, KeyFlag keyOff)
		{
			t.Flg = flg;
			t.Pts = pts;
			t.SusBeg = susBeg;
			t.SusEnd = susEnd;
			t.Beg = beg;
			t.End = end;
			t.Env = p;
			t.P = 0;
			t.A = 0;
			t.B = (((t.Flg & EnvelopeFlag.Sustain) != 0) && ((keyOff & KeyFlag.Off) == 0)) ? (ushort)0 : (ushort)1;

			if (t.Pts == 0)			// FIXME: bad/crafted file. better/more general solution?
			{
				t.B = 0;
				return t.Env[0].Val;
			}

			// Imago Orpheus sometimes stores an extra initial point in the envelope
			if ((t.Pts >= 2) && (t.Env[0].Pos == t.Env[1].Pos))
			{
				t.A++;
				t.B++;
			}

			// Fit in the envelope, still
			if (t.A >= t.Pts)
				t.A = (ushort)(t.Pts - 1);

			if (t.B >= t.Pts)
				t.B = (ushort)(t.Pts - 1);

			return t.Env[t.A].Val;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the next envelope value
		///
		/// This procedure processes all envelope types, include volume,
		/// pitch, and panning. Envelopes are defined by a set of points,
		/// each with a magnitude [relating either to volume, panning
		/// position, or pitch modifier] and a tick position.
		///
		/// Envelopes work in the following manner:
		///
		/// (a) Each tick the envelope is moved a point further in its
		///     progression. For an accurate progression, magnitudes
		///     between two envelope points are interpolated.
		///
		/// (b) When progression reaches a defined point on the envelope,
		///     values are shifted to interpolate between this point and the
		///     next, and checks for loops or envelope end are done.
		///
		/// Misc:
		///     Sustain loops are loops that are only active as long as the
		///     keyoff flag is clear. When a volume envelope terminates,
		///     so does the current fadeout.
		/// </summary>
		/********************************************************************/
		private short ProcessEnvelope(Mp_Voice aOut, ref EnvPr t, short v)
		{
			if ((t.Flg & EnvelopeFlag.On) != 0)
			{
				if (t.Pts == 0)			// FIXME: bad/crafted file. better/more general solution?
				{
					t.B = 0;
					return t.Env[0].Val;
				}

				byte a, b;				// Actual points in the envelope
				ushort p;				// The 'tick counter' - real point being played

				a = (byte)t.A;
				b = (byte)t.B;
				p = (ushort)t.P;

				// Sustain loop on one point (XM type).
				// Not processed if KEYOFF.
				// Don't move and don't interpolate when the point is reached
				if (((t.Flg & EnvelopeFlag.Sustain) != 0) && (t.SusBeg == t.SusEnd) && (((aOut.Main.KeyOff & KeyFlag.Off) == 0) && (p == t.Env[t.SusBeg].Pos)))
					v = t.Env[t.SusBeg].Val;
				else
				{
					// All following situations will require interpolation between
					// two envelope points
					//
					// Sustain loop between two points (IT type).
					// Not processed if KEYOFF.
					//
					// If we were on a loop point, loop now
					if (((t.Flg & EnvelopeFlag.Sustain) != 0) && ((aOut.Main.KeyOff & KeyFlag.Off) == 0) && (a >= t.SusEnd))
					{
						a = t.SusBeg;
						b = (t.SusBeg == t.SusEnd) ? a : (byte)(a + 1);
						p = (ushort)t.Env[a].Pos;
						v = t.Env[a].Val;
					}
					else
					{
						// Regular loop
						// Be sure to correctly handle single point loops
						if (((t.Flg & EnvelopeFlag.Loop) != 0) && (a >= t.End))
						{
							a = t.Beg;
							b = (t.Beg == t.End) ? a : (byte)(a + 1);
							p = (ushort)t.Env[a].Pos;
							v = t.Env[a].Val;
						}
						else
						{
							// Non looping situations
							if (a != b)
								v = InterpolateEnv((short)p, ref t.Env[a], ref t.Env[b]);
							else
								v = t.Env[a].Val;
						}
					}

					// Start to fade if the volume envelope is finished
					if (p >= t.Env[t.Pts - 1].Pos)
					{
						if ((t.Flg & EnvelopeFlag.VolEnv) != 0)
						{
							aOut.Main.KeyOff |= KeyFlag.Fade;

							if (v == 0)
								aOut.Main.FadeVol = 0;
						}
					}
					else
					{
						p++;

						// Did pointer reach point b?
						if (p >= t.Env[b].Pos)
							a = b++;			// Shift points a and b
					}

					t.A = a;
					t.B = b;
					t.P = (short)p;
				}
			}

			return v;
		}



		/********************************************************************/
		/// <summary>
		/// Returns MP_CONTROL index of free channel.
		///
		/// New note action scoring system:
		/// -------------------------------
		///  1) Total-volume (fadeVol, chanVol, volume) is the main scorer
		///  2) A looping sample is a bonus x2
		///  3) A foreground channel is a bonus x4
		///  4) An active envelope with keyoff is a handicap -x2
		/// </summary>
		/********************************************************************/
		private int Mp_FindEmptyChannel(Module mod)
		{
			int t;

			for (t = 0; t < NumVoices(mod); t++)
			{
				if (((mod.Voice[t].Main.Kick == Kick.Absent) || (mod.Voice[t].Main.Kick == Kick.Env)) && driver.VoiceStoppedInternal((sbyte)t))
					return t;
			}

			uint tVol = 0xffffff;
			t = -1;

			for (uint k = 0; k < NumVoices(mod); k++)
			{
				Mp_Voice a = mod.Voice[k];

				// Allow us to take over a non-existing sample
				if (a.Main.S == null)
					return (int)k;

				if ((a.Main.Kick == Kick.Absent) || (a.Main.Kick == Kick.Env))
				{
					uint pp = a.TotalVol << (((a.Main.S.Flags & SampleFlag.Loop) != 0) ? 1 : 0);
					if ((a.Master != null) && (a == a.Master.Slave))
						pp <<= 2;

					if (pp < tVol)
					{
						tVol = pp;
						t = (int)k;
					}
				}
			}

			if (tVol > 8000 * 7)
				return -1;

			return t;
		}



		/********************************************************************/
		/// <summary>
		/// Parses the effects
		/// </summary>
		/********************************************************************/
		private int Pt_PlayEffects(Module mod, short channel, Mp_Control a)
		{
			ushort tick = mod.VbTick;
			ModuleFlag flags = mod.Flags;
			int explicitSlides = 0;

			byte c;
			while ((c = uniTrk.UniGetByte()) != 0)
			{
				EffectFunc f = effects[c];

				if (f != DoNothing)
					a.Sliding = 0;

				explicitSlides |= f(tick, flags, a, mod, channel);
			}

			return explicitSlides;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate vibrato value based on given waveform
		/// </summary>
		/********************************************************************/
		private short LfoVibrato(sbyte position, byte waveform)
		{
			switch (waveform)
			{
				// Sine
				case 0:
				{
					short amp = LookupTables.VibratoTable[position & 0x7f];
					return (short)((position >= 0) ? amp : -amp);
				}

				// Ramp down - ramps up because MOD/S3M apply these to period
				case 1:
					return (short)(((byte)position << 1) - 255);

				// Square wave
				case 2:
					return (short)((position >= 0) ? 255 : -255);

				// Random wave
				case 3:
					return (short)(GetRandom(512) - 256);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate tremolo value based on given waveform
		/// </summary>
		/********************************************************************/
		private short LfoTremolo(sbyte position, byte waveform)
		{
			switch (waveform)
			{
				// Ramp down - tremolo ramp down actually ramps down
				case 1:
					return (short)(255 - ((byte)position << 1));
			}

			return LfoVibrato(position, waveform);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate vibrato value based on given waveform for IT modules
		/// </summary>
		/********************************************************************/
		private short LfoVibratoIt(sbyte position, byte waveform)
		{
			switch (waveform)
			{
				// Ramp down - IT ramp down actually ramps down
				case 1:
					return (short)(255 - ((byte)position << 1));

				// Square wave - IT square wave oscillates between 0 and 255
				case 2:
					return (short)((position >= 0) ? 255 : 0);
			}

			return LfoVibrato(position, waveform);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate panbrello value based on given waveform
		/// </summary>
		/********************************************************************/
		private short LfoPanbrello(sbyte position, byte waveform)
		{
			switch (waveform)
			{
				// Sine
				case 0:
					return LookupTables.PanbrelloTable[position];

				// Ramp down
				case 1:
					return (short)(64 - ((byte)position >> 1));

				// Square wave
				case 2:
					return (short)((position >= 0) ? 64 : 0);

				// Random
				case 3:
					return (short)(GetRandom(128) - 64);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Calculate auto vibrato value based on given waveform for XM
		/// modules
		/// </summary>
		/********************************************************************/
		private short LfoAutoVibratoXm(sbyte position, byte waveform)
		{
			// XM auto vibrato uses a different set of waveforms than vibrato/tremolo
			switch (waveform)
			{
				// Sine
				case 0:
				{
					short amp = LookupTables.VibratoTable[position & 0x7f];
					return (short)((position >= 0) ? amp : -amp);
				}

				// Square wave
				case 1:
					return (short)((position >= 0) ? 255 : -255);

				// Ramp down
				case 2:
					return (short)-(position << 1);

				// Ramp up
				case 3:
					return (short)(position << 1);
			}

			return 0;
		}

		#region ProTracker effect helpers
		/********************************************************************/
		/// <summary>
		/// Parses the ProTracker extra effects
		/// </summary>
		/********************************************************************/
		private void DoEEffects(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel, byte dat)
		{
			byte nib = (byte)(dat & 0xf);

			switch (dat >> 4)
			{
				// Hardware filter toggle, not supported
				case 0x0:
					break;

				// Fine slide up
				case 0x1:
				{
					if (a.Main.Period != 0)
					{
						if (tick == 0)
							a.TmpPeriod -= (ushort)(nib << 2);
					}
					break;
				}

				// Fine slide down
				case 0x2:
				{
					if (a.Main.Period != 0)
					{
						if (tick == 0)
							a.TmpPeriod += (ushort)(nib << 2);
					}
					break;
				}

				// Glissando control
				case 0x3:
				{
					a.Glissando = nib;
					break;
				}

				// Set vibrato wave form
				case 0x4:
				{
					a.WaveControl &= 0xf0;
					a.WaveControl |= nib;
					break;
				}

				// Set fine tune
				case 0x5:
				{
					if (a.Main.Period != 0)
					{
						if ((flags & ModuleFlag.XmPeriods) != 0)
							a.Speed = (uint)(nib + 128);
						else
							a.Speed = SharedLookupTables.FineTune[nib];

						a.TmpPeriod = GetPeriod(flags, (ushort)(a.Main.Note << 1), a.Speed);
					}
					break;
				}

				// Set pattern loop
				case 0x6:
				{
					DoLoop(tick, flags, a, mod, nib);
					break;
				}

				// Set tremolo wave form
				case 0x7:
				{
					a.WaveControl &= 0x0f;
					a.WaveControl |= (byte)(nib << 4);
					break;
				}

				// Set panning
				case 0x8:
				{
					if (mod.PanFlag)
					{
						if (nib <= 8)
							nib <<= 4;
						else
							nib *= 17;

						a.Main.Panning = nib;
						mod.Panning[channel] = nib;
					}
					break;
				}

				// Retrig note
				case 0x9:
				{
					// ProTracker: Retriggers on tick 0 first, does nothing when nib=0.
					// FastTracker 2: Retriggers on tick nib first, including nib=0
					if (tick == 0)
					{
						if ((flags & ModuleFlag.Ft2Quirks) != 0)
							a.Retrig = (sbyte)nib;
						else if (nib != 0)
							a.Retrig = 0;
						else
							break;
					}

					// Only retrigger if data nibble > 0, or if tick 0 (FT2 compat)
					if ((nib != 0) || (tick == 0))
					{
						if (a.Retrig == 0)
						{
							// When retrig counter reaches 0,
							// reset counter and restart the sample
							if (a.Main.Period != 0)
								a.Main.Kick = Kick.Note;

							a.Retrig = (sbyte)nib;
						}

						a.Retrig--;			// Count down
					}
					break;
				}

				// Fine volume slide up
				case 0xa:
				{
					if (tick != 0)
						break;

					a.TmpVolume += nib;

					if (a.TmpVolume > 64)
						a.TmpVolume = 64;

					break;
				}

				// Fine volume slide down
				case 0xb:
				{
					if (tick != 0)
						break;

					a.TmpVolume -= nib;

					if (a.TmpVolume < 0)
						a.TmpVolume = 0;

					break;
				}

				// Cut note
				case 0xc:
				{
					// When tick reaches the cut-note value,
					// turn the volume to zero (just like on the Amiga)
					if (tick >= nib)
						a.TmpVolume = 0;		// Just turn the volume down

					break;
				}

				// Note delay
				case 0xd:
				{
					// Delay the start of the sample until tick == nib
					if (tick == 0)
						a.Main.NoteDelay = nib;
					else
					{
						if (a.Main.NoteDelay != 0)
							a.Main.NoteDelay--;
					}
					break;
				}

				// Pattern delay
				case 0xe:
				{
					if (tick == 0)
					{
						if (mod.PatDly2 == 0)
							mod.PatDly = (byte)(nib + 1);	// Only once, when tick = 0
					}
					break;
				}

				// Invert loop, not supported
				case 0xf:
					break;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Arpeggio helper method
		/// </summary>
		/********************************************************************/
		private void DoArpeggio(ushort tick, ModuleFlag flags, Mp_Control a, byte style)
		{
			byte note = a.Main.Note;

			if (a.ArpMem != 0)
			{
				switch (style)
				{
					// Mod style: N, N+x, N+y
					case 0:
					{
						switch (tick % 3)
						{
							// Case 0: unchanged
							case 1:
							{
								note += (byte)(a.ArpMem >> 4);
								break;
							}

							case 2:
							{
								note += (byte)(a.ArpMem & 0xf);
								break;
							}
						}
						break;
					}

					// Okt arpeggio 3: N-x, N, N+y
					case 3:
					{
						switch (tick % 3)
						{
							case 0:
							{
								note -= (byte)(a.ArpMem >> 4);
								break;
							}

							// Case 1: unchanged
							case 2:
							{
								note += (byte)(a.ArpMem & 0xf);
								break;
							}
						}
						break;
					}

					// Okt arpeggio 4: N, N+y, N, N-x
					case 4:
					{
						switch (tick % 4)
						{
							// Case 0, case 2: unchanged
							case 1:
							{
								note += (byte)(a.ArpMem & 0xf);
								break;
							}

							case 3:
							{
								note -= (byte)(a.ArpMem >> 4);
								break;
							}
						}
						break;
					}

					// Okt arpeggio 5: N-x, N+y, N and nothing at tick 0
					case 5:
					{
						if (tick == 0)
							break;

						switch (tick % 3)
						{
							// Case 0: unchanged
							case 1:
							{
								note -= (byte)(a.ArpMem >> 4);
								break;
							}

							case 2:
							{
								note += (byte)(a.ArpMem & 0xf);
								break;
							}
						}
						break;
					}
				}

				a.Main.Period = GetPeriod(flags, (ushort)(note << 1), a.Speed);
				a.OwnPer = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Tone slide helper method
		/// </summary>
		/********************************************************************/
		private void DoToneSlide(ushort tick, Mp_Control a)
		{
			if (a.Main.FadeVol == 0)
				a.Main.Kick = (a.Main.Kick == Kick.Note) ? Kick.Note : Kick.KeyOff;
			else
				a.Main.Kick = (a.Main.Kick == Kick.Note) ? Kick.Env : Kick.Absent;

			if (tick != 0)
			{
				// We have to slide a.Main.Period toward a.WantedPeriod,
				// compute the difference between those two values
				int dist = a.Main.Period - a.WantedPeriod;

				// If they are equal or if portamento speed is too big...
				if ((dist == 0) || (a.PortSpeed > Math.Abs(dist)))
				{
					// ...make TmpPeriod equal tPeriod
					a.TmpPeriod = a.Main.Period = a.WantedPeriod;
				}
				else
				{
					if (dist > 0)
					{
						a.TmpPeriod -= a.PortSpeed;
						a.Main.Period -= a.PortSpeed;			// Dist > 0, slide up
					}
					else
					{
						a.TmpPeriod += a.PortSpeed;
						a.Main.Period += a.PortSpeed;			// Dist < 0, slide down
					}
				}
			}
			else
				a.TmpPeriod = a.Main.Period;

			a.OwnPer = true;
		}



		/********************************************************************/
		/// <summary>
		/// Vibrato helper method
		/// </summary>
		/********************************************************************/
		private void DoVibrato(ushort tick, Mp_Control a, VibratoFlags flags)
		{
			if ((tick == 0) && ((flags & VibratoFlags.PtBugs) != 0))
				return;

			short temp = LfoVibrato(a.VibPos, (byte)(a.WaveControl & 3));

			temp *= a.VibDepth;
			temp >>= 7;
			temp <<= 2;

			a.Main.Period = (ushort)(a.TmpPeriod + temp);
			a.OwnPer = true;

			if ((tick != 0) || ((flags & VibratoFlags.Tick0) != 0))
				a.VibPos += (sbyte)a.VibSpd;
		}



		/********************************************************************/
		/// <summary>
		/// Tremolo helper method
		/// </summary>
		/********************************************************************/
		private void DoTremolo(ushort tick, Mp_Control a, VibratoFlags flags)
		{
			if ((tick == 0) && ((flags & VibratoFlags.PtBugs) != 0))
				return;

			short temp = LfoTremolo(a.TrmPos, (byte)((a.WaveControl >> 4) & 3));

			temp *= a.TrmDepth;
			temp >>= 6;

			a.Volume = (short)(a.TmpVolume + temp);

			if (a.Volume > 64)
				a.Volume = 64;

			if (a.Volume < 0)
				a.Volume = 0;

			a.OwnVol = true;

			if ((tick != 0) || ((flags & VibratoFlags.Tick0) != 0))
				a.TrmPos += (sbyte)a.TrmSpd;
		}



		/********************************************************************/
		/// <summary>
		/// Volume slide helper method
		/// </summary>
		/********************************************************************/
		private void DoVolSlide(Mp_Control a, byte dat)
		{
			if ((dat & 0xf) != 0)
			{
				a.TmpVolume -= (short)(dat & 0x0f);

				if (a.TmpVolume < 0)
					a.TmpVolume = 0;
			}
			else
			{
				a.TmpVolume += (short)(dat >> 4);

				if (a.TmpVolume > 64)
					a.TmpVolume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Handle pattern loop
		/// </summary>
		/********************************************************************/
		private void DoLoop(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, byte param)
		{
			if (tick != 0)
				return;

			if (param != 0)		// Set RepPos or RepCnt?
			{
				// Set RepCnt, so check if RepCnt already is set, which means we
				// are already looping
				if (a.Pat_RepCnt != 0)
					a.Pat_RepCnt--;			// Already looping, decrease counter
				else
					a.Pat_RepCnt = param;	// Not yet looping, so set RepCnt

				if (a.Pat_RepCnt != 0)		// Jump to RepPos if RepCnt > 0
				{
					if (a.Pat_RepPos == Constant.Pos_None)
						a.Pat_RepPos = (short)(mod.PatPos - 1);

					if (a.Pat_RepPos == 1)
					{
						mod.Pat_RepCrazy = true;
						mod.PatPos = 0;
					}
					else
						mod.PatPos = (ushort)a.Pat_RepPos;
				}
				else
					a.Pat_RepPos = Constant.Pos_None;
			}
			else
			{
				a.Pat_RepPos = (short)(mod.PatPos - 1);	// Set reppos - can be (-1)

				// Emulate the FT2 pattern loop (E60) bug:
				// http://milkytracker.org/docs/MilkyTracker.html#fxE6x
				// roadblas.xm plays correctly with this
				if ((flags & ModuleFlag.Ft2Quirks) != 0)
					mod.PatBrk = mod.PatPos;
			}
		}
		#endregion

		#region ScreamTracker 3 effect helpers
		/********************************************************************/
		/// <summary>
		/// Volume slide helper method
		/// </summary>
		/********************************************************************/
		private void DoS3MVolSlide(ushort tick, ModuleFlag flags, Mp_Control a, byte inf)
		{
			if (inf != 0)
				a.S3MVolSlide = inf;
			else
				inf = a.S3MVolSlide;

			byte lo = (byte)(inf & 0xf);
			byte hi = (byte)(inf >> 4);

			if (lo == 0)
			{
				if ((tick != 0) || ((flags & ModuleFlag.S3MSlides) != 0))
					a.TmpVolume += hi;
			}
			else
			{
				if (hi == 0)
				{
					if ((tick != 0) || ((flags & ModuleFlag.S3MSlides) != 0))
						a.TmpVolume -= lo;
				}
				else
				{
					if (lo == 0xf)
					{
						if (tick == 0)
							a.TmpVolume += (short)(hi != 0 ? hi : 0xf);
					}
					else
					{
						if (hi == 0xf)
						{
							if (tick == 0)
								a.TmpVolume -= (short)(lo != 0 ? lo : 0xf);
						}
						else
							return;
					}
				}
			}

			if (a.TmpVolume < 0)
				a.TmpVolume = 0;
			else
			{
				if (a.TmpVolume > 64)
					a.TmpVolume = 64;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Slide down helper method
		/// </summary>
		/********************************************************************/
		private void DoS3MSlideDn(ushort tick, Mp_Control a, byte inf)
		{
			if (inf != 0)
				a.SlideSpeed = inf;
			else
				inf = (byte)a.SlideSpeed;

			byte hi = (byte)(inf >> 4);
			byte lo = (byte)(inf & 0xf);

			if (hi == 0xf)
			{
				if (tick == 0)
					a.TmpPeriod += (ushort)(lo << 2);
			}
			else
			{
				if (hi == 0xe)
				{
					if (tick == 0)
						a.TmpPeriod += lo;
				}
				else
				{
					if (tick != 0)
						a.TmpPeriod += (ushort)(inf << 2);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Slide up helper method
		/// </summary>
		/********************************************************************/
		private void DoS3MSlideUp(ushort tick, Mp_Control a, byte inf)
		{
			if (inf != 0)
				a.SlideSpeed = inf;
			else
				inf = (byte)a.SlideSpeed;

			byte hi = (byte)(inf >> 4);
			byte lo = (byte)(inf & 0xf);

			if (hi == 0xf)
			{
				if (tick == 0)
					a.TmpPeriod -= (ushort)(lo << 2);
			}
			else
			{
				if (hi == 0xe)
				{
					if (tick == 0)
						a.TmpPeriod -= lo;
				}
				else
				{
					if (tick != 0)
						a.TmpPeriod -= (ushort)(inf << 2);
				}
			}
		}
		#endregion

		#region FastTracker II effect helpers
		/********************************************************************/
		/// <summary>
		/// Set the envelope tick to the position given
		/// </summary>
		/********************************************************************/
		private void SetEnvelopePosition(ref EnvPr t, EnvPt[] p, short pos)
		{
			if (t.Pts > 0)
			{
				bool found = false;

				for (ushort i = 0; i < t.Pts - 1; i++)
				{
					if ((pos >= p[i].Pos) && (pos < p[i + 1].Pos))
					{
						t.A = i;
						t.B = (ushort)(i + 1);
						t.P = pos;
						found = true;
						break;
					}
				}

				// If position is after the last envelope point, just set
				// it to the last one
				if (!found)
				{
					t.A = (ushort)(t.Pts - 1);
					t.B = t.Pts;
					t.P = p[t.A].Pos;
				}
			}
		}
		#endregion

		#region ImpulseTracker effect helpers
		/********************************************************************/
		/// <summary>
		/// Tone slide helper method
		/// </summary>
		/********************************************************************/
		private void DoItToneSlide(ushort tick, Mp_Control a, byte dat)
		{
			if (dat != 0)
				a.PortSpeed = dat;

			// If we don't come from another note, ignore the slide and play
			// the note as is
			if ((a.OldNote == 0) || (a.Main.Period == 0))
				return;

			if ((tick == 0) && (a.NewSamp != 0))
			{
				a.Main.Kick = Kick.Note;
				a.Main.Start = -1;
			}
			else
				a.Main.Kick = (a.Main.Kick == Kick.Note) ? Kick.Env : Kick.Absent;

			if (tick != 0)
			{
				// We have to slide a.Main.Period toward a.WantedPeriod,
				// compute the difference between those two values
				int dist = a.Main.Period - a.WantedPeriod;

				// If they are equal or if portamento speed is too big...
				if ((dist == 0) || ((a.PortSpeed << 2) > Math.Abs(dist)))
				{
					// ...make TmpPeriod equal tPeriod
					a.TmpPeriod = a.Main.Period = a.WantedPeriod;
				}
				else
				{
					if (dist > 0)
					{
						a.TmpPeriod -= (ushort)(a.PortSpeed << 2);
						a.Main.Period -= (ushort)(a.PortSpeed << 2);	// Dist > 0, slide up
					}
					else
					{
						a.TmpPeriod += (ushort)(a.PortSpeed << 2);
						a.Main.Period += (ushort)(a.PortSpeed << 2);	// Dist < 0, slide down
					}
				}
			}
			else
				a.TmpPeriod = a.Main.Period;

			a.OwnPer = true;
		}



		/********************************************************************/
		/// <summary>
		/// Vibrato helper method
		/// </summary>
		/********************************************************************/
		private void DoItVibrato(ushort tick, Mp_Control a, byte dat, ItVibratoFlags flags)
		{
			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.VibDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.VibSpd = (byte)((dat & 0xf0) >> 2);
			}

			if (a.Main.Period == 0)
				return;

			short temp = LfoVibratoIt(a.VibPos, (byte)(a.WaveControl & 3));

			temp *= a.VibDepth;

			if ((flags & ItVibratoFlags.Old) == 0)
			{
				temp >>= 8;

				if ((flags & ItVibratoFlags.Fine) == 0)
					temp <<= 2;

				// Subtract vibrato from period so positive vibrato translates to increase in pitch
				a.Main.Period = (ushort)(a.TmpPeriod - temp);
				a.OwnPer = true;
			}
			else
			{
				// Old IT vibrato is twice as deep
				temp >>= 7;

				if ((flags & ItVibratoFlags.Fine) == 0)
					temp <<= 2;

				// Old IT vibrato uses the same waveforms but they are applied reversed
				a.Main.Period = (ushort)(a.TmpPeriod + temp);
				a.OwnPer = true;

				// Old IT vibrato does not update on the first tick
				if (tick == 0)
					return;
			}

			a.VibPos += (sbyte)a.VibSpd;
		}



		/********************************************************************/
		/// <summary>
		/// Effect S7: NNA effects
		/// </summary>
		/********************************************************************/
		private void DoNnaEffects(Module mod, Mp_Control a, byte dat)
		{
			dat &= 0xf;
			Mp_Voice aOut = (a.Slave != null) ? a.Slave : null;

			switch (dat)
			{
				// Past note cut
				case 0x0:
				{
					for (int t = 0; t < NumVoices(mod); t++)
					{
						if (mod.Voice[t].Master == a)
							mod.Voice[t].Main.FadeVol = 0;
					}
					break;
				}

				// Past note off
				case 0x1:
				{
					for (int t = 0; t < NumVoices(mod); t++)
					{
						if (mod.Voice[t].Master == a)
						{
							mod.Voice[t].Main.KeyOff |= KeyFlag.Off;

							if (((mod.Voice[t].VEnv.Flg & EnvelopeFlag.On) == 0) || ((mod.Voice[t].VEnv.Flg & EnvelopeFlag.Loop) != 0))
								mod.Voice[t].Main.KeyOff = KeyFlag.Kill;
						}
					}
					break;
				}

				// Past note fade
				case 0x2:
				{
					for (int t = 0; t < NumVoices(mod); t++)
					{
						if (mod.Voice[t].Master == a)
							mod.Voice[t].Main.KeyOff |= KeyFlag.Fade;
					}
					break;
				}

				// Set NNA note cut
				case 0x3:
				{
					a.Main.Nna = (a.Main.Nna & ~Nna.Mask) | Nna.Cut;
					break;
				}

				// Set NNA note continue
				case 0x4:
				{
					a.Main.Nna = (a.Main.Nna & ~Nna.Mask) | Nna.Continue;
					break;
				}

				// Set NNA note off
				case 0x5:
				{
					a.Main.Nna = (a.Main.Nna & ~Nna.Mask) | Nna.Off;
					break;
				}

				// Set NNA note fade
				case 0x6:
				{
					a.Main.Nna = (a.Main.Nna & ~Nna.Mask) | Nna.Fade;
					break;
				}

				// Disable volume envelope
				case 0x7:
				{
					if (aOut != null)
						aOut.Main.VolFlg &= ~EnvelopeFlag.On;

					break;
				}

				// Enable volume envelope
				case 0x8:
				{
					if (aOut != null)
						aOut.Main.VolFlg |= EnvelopeFlag.On;

					break;
				}

				// Disable panning envelope
				case 0x9:
				{
					if (aOut != null)
						aOut.Main.PanFlg &= ~EnvelopeFlag.On;

					break;
				}

				// Enable panning envelope
				case 0xa:
				{
					if (aOut != null)
						aOut.Main.PanFlg |= EnvelopeFlag.On;

					break;
				}

				// Disable pitch envelope
				case 0xb:
				{
					if (aOut != null)
						aOut.Main.PitFlg &= ~EnvelopeFlag.On;

					break;
				}

				// Enable pitch envelope
				case 0xc:
				{
					if (aOut != null)
						aOut.Main.PitFlg |= EnvelopeFlag.On;

					break;
				}
			}
		}
		#endregion

		#region Farandole effect helpers
		/********************************************************************/
		/// <summary>
		/// Tone portamento helper method
		/// </summary>
		/********************************************************************/
		private void DoFarTonePorta(Mp_Control a)
		{
			if (a.Main.FadeVol == 0)
				a.Main.Kick = (a.Main.Kick == Kick.Note) ? Kick.Note : Kick.KeyOff;
			else
				a.Main.Kick = (a.Main.Kick == Kick.Note) ? Kick.Env : Kick.Absent;

			a.FarCurrentValue += a.FarTonePortaSpeed;

			// Have we reached our note
			bool reachedNote = (a.FarTonePortaSpeed > 0) ? a.FarCurrentValue >= a.WantedPeriod : a.FarCurrentValue <= a.WantedPeriod;
			if (reachedNote)
			{
				// Stop the porta and set the periods to the reached note
				a.TmpPeriod = a.Main.Period = a.WantedPeriod;
				a.FarTonePortaRunning = false;
			}
			else
			{
				// Do the porta
				a.TmpPeriod = a.Main.Period = (ushort)a.FarCurrentValue;
			}

			a.OwnPer = true;
		}



		/********************************************************************/
		/// <summary>
		/// Find tempo factor
		/// </summary>
		/********************************************************************/
		internal int GetFarTempoFactor(Module mod)
		{
			return mod.FarCurTempo == 0 ? 256 : (128 / mod.FarCurTempo);
		}



		/********************************************************************/
		/// <summary>
		/// Set the right speed and BPM for Farandole modules
		/// </summary>
		/********************************************************************/
		internal void SetFarTempo(Module mod)
		{
			/* According to the Farandole document, the tempo of the song is 32/tempo notes per second.
			   So if we set tempo to 1, we will get 32 notes per second. We then need to translate this
			   to BPM, since this is what we're using as tempo.

			   We know 125 BPM is at 50 hz speed (see https://modarchive.org/forums/index.php?topic=2709.0
			   for more information). So the factor is 125/50 = 2.5. So we take the 32 notes per second
			   above and multiply with 2.5: 32 * 2.5 = 80 BPM.

			   So we now know, at speed 1, we need to run at 80 BPM and number of ticks (speed counter) is 1.

			   Farandole however, uses another approach to calculate the tempo. It takes the speed
			   argument and calculate a ticks per second as 128/arg. It also set the tick counter to 3 most
			   of the times. It calculate a value (I guess it is how often GUS need to call the player) by
			   1197255 / (128 / arg). So if we set speed to 1, we will get 1197255 / 128 = 9353.

			   Ok, we know if we set the speed counter to 1, we need to run at 80 BPM, but now Farandole
			   set the tick counter to 3, so we need to calculate the BPM to use for the difference. This
			   is easy enough, just say 80 * 3 = 240 BPM.

			   So with all this information, we will try to calculate the right BPM for the speed set.

			   For argument 1:

			   tps = 128 / 1 = 128
			   gus = 1197255 / tps = 1197255 / 128 = 9353
			   counter = 3
			   factor = gus / 9353 = 9353 / 9353 = 1
			   bpm = (80 * counter) / factor = (80 * 3) / 1 = 240

			   For argument 4, which is the default speed

			   tps = 128 / 4 = 32
			   gus = 1197255 / tps = 1197255 / 32 = 37414
			   counter = 3
			   factor = gus / 9353 = 37414 / 9353 = 4
			   bpm = (80 * counter) / factor = (80 * 3) / 4 = 60

			   For argument 15, which is the slowest speed

			   tps = 128 / 15 = 8
			   gus = 1197255 / tps = 1197255 / 8 = 149656
			   counter = 6 (see code below for why)
			   factor = gus / 9353 = 149656 / 9353 = 16
			   bpm = (80 * counter) / factor = (80 * 6) / 16 = 30

			   You can make yourself a little exercise to prove that the above is correct :-) */

			short realTempo = (short)(mod.FarTempoBend + GetFarTempoFactor(mod));

			int gus = 1197255 / realTempo;

			int eax = gus;
			byte cx = 0, di = 0;

			while (eax > 0xffff)
			{
				eax >>= 1;
				di++;
				cx++;
			}

			if (cx >= 2)
				di++;

			mod.SngSpd = (byte)(di + 3);

			int factor = (int)Math.Round(gus / 9353.0f, MidpointRounding.AwayFromZero);
			mod.Bpm = (ushort)((80 * mod.SngSpd) / factor);
		}
		#endregion

		#region Special specific effects
		/********************************************************************/
		/// <summary>
		/// Do nothing effect
		/// </summary>
		/********************************************************************/
		private int DoNothing(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			uniTrk.UniSkipOpcode();

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// KeyOff effect
		/// </summary>
		/********************************************************************/
		private int DoKeyOff(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			a.Main.KeyOff |= KeyFlag.Off;

			if (((a.Main.VolFlg & EnvelopeFlag.On) == 0) || ((a.Main.VolFlg & EnvelopeFlag.Loop) != 0))
				a.Main.KeyOff = KeyFlag.Kill;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// KeyFade effect
		/// </summary>
		/********************************************************************/
		private int DoKeyFade(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick >= dat) || (tick == mod.SngSpd - 1))
			{
				a.Main.KeyOff = KeyFlag.Kill;

				if ((a.Main.VolFlg & EnvelopeFlag.On) == 0)
					a.Main.FadeVol = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// ImpulseTracker volume/panning column effects
		///
		/// All volume/pan column effects share the same memory space
		/// </summary>
		/********************************************************************/
		private int DoVolEffects(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			VolEffect c = (VolEffect)uniTrk.UniGetByte();
			byte inf = uniTrk.UniGetByte();

			if ((c == 0) && (inf == 0))
			{
				c = a.VolEffect;
				inf = a.VolData;
			}
			else
			{
				a.VolEffect = c;
				a.VolData = inf;
			}

			if (c != 0)
			{
				switch (c)
				{
					case VolEffect.Volume:
					{
						if (tick != 0)
							break;

						if (inf > 64)
							inf = 64;

						a.TmpVolume = inf;
						break;
					}

					case VolEffect.Panning:
					{
						if (mod.PanFlag)
							a.Main.Panning = inf;

						break;
					}

					case VolEffect.VolSlide:
					{
						DoS3MVolSlide(tick, flags, a, inf);
						return 1;
					}

					case VolEffect.PitchSlideDn:
					{
						if (a.Main.Period != 0)
							DoS3MSlideDn(tick, a, inf);

						break;
					}

					case VolEffect.PitchSlideUp:
					{
						if (a.Main.Period != 0)
							DoS3MSlideUp(tick, a, inf);

						break;
					}

					case VolEffect.Portamento:
					{
						DoItToneSlide(tick, a, inf);
						break;
					}

					case VolEffect.Vibrato:
					{
						DoItVibrato(tick, a, inf, ItVibratoFlags.None);
						break;
					}
				}
			}

			return 0;
		}
		#endregion

		#region ProTracker specific effects
		/********************************************************************/
		/// <summary>
		/// Effect 0: Arpeggio
		/// </summary>
		/********************************************************************/
		private int DoPtEffect0(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat == 0) && ((flags & ModuleFlag.ArpMem) != 0))
					dat = a.ArpMem;
				else
					a.ArpMem = dat;
			}

			if (a.Main.Period != 0)
				DoArpeggio(tick, flags, a, 0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 1: Portamento up
		/// </summary>
		/********************************************************************/
		private int DoPtEffect1(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick == 0) && (dat != 0))
				a.SlideSpeed = (ushort)(dat << 2);

			if (a.Main.Period != 0)
			{
				if (tick != 0)
					a.TmpPeriod -= a.SlideSpeed;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 2: Portamento down
		/// </summary>
		/********************************************************************/
		private int DoPtEffect2(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick == 0) && (dat != 0))
				a.SlideSpeed = (ushort)(dat << 2);

			if (a.Main.Period != 0)
			{
				if (tick != 0)
					a.TmpPeriod += a.SlideSpeed;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 3: Tone portamento
		/// </summary>
		/********************************************************************/
		private int DoPtEffect3(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick == 0) && (dat != 0))
				a.PortSpeed = (ushort)(dat << 2);

			if (a.Main.Period != 0)
				DoToneSlide(tick, a);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 4: Vibrato
		/// </summary>
		/********************************************************************/
		private int DoPtEffect4(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.VibDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.VibSpd = (byte)((dat & 0xf0) >> 2);
			}

			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.PtBugs);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 4: Vibrato (fix)
		/// </summary>
		/********************************************************************/
		private int DoPtEffect4Fix(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// PT equivalent vibrato but without the tick 0 bug
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.VibDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.VibSpd = (byte)((dat & 0xf0) >> 2);
			}

			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.None);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 5: Tone + volume slide
		/// </summary>
		/********************************************************************/
		private int DoPtEffect5(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (a.Main.Period != 0)
				DoToneSlide(tick, a);

			if (tick != 0)
				DoVolSlide(a, dat);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 6: Vibrato + volume slide
		/// </summary>
		/********************************************************************/
		private int DoPtEffect6(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.PtBugs);

			DoPtEffectA(tick, flags, a, mod, channel);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 7: Tremolo
		/// </summary>
		/********************************************************************/
		private int DoPtEffect7(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.TrmDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.TrmSpd = (byte)((dat & 0xf0) >> 2);
			}

			// TODO: PT should have the same tick 0 bug here that vibrato does. Several other
			// formats use this effect and rely on it not being broken, so don't right now
			if (a.Main.Period != 0)
				DoTremolo(tick, a, VibratoFlags.None);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 7: Tremolo (fix)
		/// </summary>
		/********************************************************************/
		private int DoPtEffect7Fix(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// PT equivalent vibrato but without the tick 0 bug
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.TrmDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.TrmSpd = (byte)((dat & 0xf0) >> 2);
			}

			if (a.Main.Period != 0)
				DoTremolo(tick, a, VibratoFlags.None);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 8: Panning
		/// </summary>
		/********************************************************************/
		private int DoPtEffect8(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (mod.PanFlag)
			{
				a.Main.Panning = dat;
				mod.Panning[channel] = dat;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 9: Sample offset
		/// </summary>
		/********************************************************************/
		private int DoPtEffect9(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if (dat != 0)
					a.SOffset = (ushort)(dat << 8);

				a.Main.Start = (int)(a.HiOffset | a.SOffset);

				if ((a.Main.S != null) && (a.Main.Start > a.Main.S.Length))
					a.Main.Start = (int)((a.Main.S.Flags & (SampleFlag.Loop | SampleFlag.Bidi)) != 0 ? a.Main.S.LoopStart : a.Main.S.Length);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect A: Volume slide
		/// </summary>
		/********************************************************************/
		private int DoPtEffectA(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick != 0)
				DoVolSlide(a, dat);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect B: Position jump
		/// </summary>
		/********************************************************************/
		private int DoPtEffectB(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick != 0) || (mod.PatDly2 != 0))
				return 0;

			if (dat >= mod.NumPos)		// Crafted file?
				dat = (byte)(mod.NumPos - 1);

			// Vincent Voois uses a nasty trick in "Universal Bolero"
			if ((dat == mod.SngPos) && (mod.PatBrk == mod.PatPos))
				return 0;

			if (!mod.Loop && (mod.PatBrk == 0) && ((dat < mod.SngPos) || ((mod.SngPos == (mod.NumPos - 1)) && (mod.PatBrk == 0)) || ((dat == mod.SngPos) && ((flags & ModuleFlag.NoWrap) != 0))))
			{
				// If we don't loop, better not to skip the end of the pattern, after all... so:
//				mod.PatBrk = 0;
				mod.PosJmp = 3;
			}
			else
			{
				// If we were fading, adjust...
				if (mod.SngPos == (mod.NumPos - 1))
					mod.Volume = mod.InitVolume > 128 ? (short)128 : mod.InitVolume;

				if (dat <= mod.SngPos)
				{
					// Tell NostalgicPlayer the module has ended
					endReached = true;
				}
				else
				{
					// If more than one Bxx effect on the same line,
					// cancel the "module end"
					endReached = false;
				}

				mod.SngPos = dat;
				mod.PosJmp = 2;
				mod.PatPos = 0;

				// Cancel the FT2 pattern loop (E60) bug.
				// Also see DoEEffects() for it
				if ((flags & ModuleFlag.Ft2Quirks) != 0)
					mod.PatBrk = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect C: Volume change
		/// </summary>
		/********************************************************************/
		private int DoPtEffectC(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick != 0)
				return 0;

			if (dat == 0xff)
				a.ANote = dat = 0;		// Note cut
			else
			{
				if (dat > 64)
					dat = 64;
			}

			a.TmpVolume = dat;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect D: Pattern break
		/// </summary>
		/********************************************************************/
		private int DoPtEffectD(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick != 0) || (mod.PatDly2 != 0))
				return 0;

			if ((dat != 0) && (dat >= mod.NumRow))		// Crafted file?
				dat = 0;

			if ((mod.Positions[mod.SngPos] != SharedConstant.Last_Pattern) && (dat > mod.PattRows[mod.Positions[mod.SngPos]]))
				dat = (byte)mod.PattRows[mod.Positions[mod.SngPos]];

			mod.PatBrk = dat;

			if (mod.PosJmp == 0)
			{
				// Don't ask me to explain this code - it makes
				// backwards.s3m and children.xm (heretic's version) play
				// correctly, among others. Take that for granted, or write
				// the page of comments yourself... you might need some
				// aspirin - Miod
				if ((mod.SngPos == mod.NumPos - 1) && (dat != 0) && (mod.Loop || ((mod.Positions[mod.SngPos] == (mod.NumPat - 1)) && ((flags & ModuleFlag.NoWrap) == 0))))
				{
					mod.SngPos = 0;
					mod.PosJmp = 2;
					endReached = true;
				}
				else
					mod.PosJmp = 3;
			}
			else
			{
				if ((mod.PatBrk != 0) && (mod.PosJmp == 2))
					endReached = false;				// This is done to make Enantiodromia.xm to play at all!
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E: Extra effects
		/// </summary>
		/********************************************************************/
		private int DoPtEffectE(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoEEffects(tick, flags, a, mod, channel, uniTrk.UniGetByte());

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F: Set speed
		/// </summary>
		/********************************************************************/
		private int DoPtEffectF(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick != 0) || (mod.PatDly2 != 0))
				return 0;

			if (mod.ExtSpd && (dat >= mod.BpmLimit))
				mod.Bpm = dat;
			else
			{
				if (dat != 0)
				{
					mod.SngSpd = (ushort)((dat >= mod.BpmLimit) ? mod.BpmLimit - 1 : dat);
					mod.VbTick = 0;
				}
			}

			return 0;
		}
		#endregion

		#region ScreamTracker 3 specific effects
		/********************************************************************/
		/// <summary>
		/// Effect A: Set speed
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectA(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte speed = uniTrk.UniGetByte();

			if ((tick != 0) || (mod.PatDly2 != 0))
				return 0;

			if (speed >= 128)
				speed -= 128;

			if (speed != 0)
			{
				mod.SngSpd = speed;
				mod.VbTick = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect D: Volume slide / fine volume slide
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectD(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoS3MVolSlide(tick, flags, a, uniTrk.UniGetByte());

			return 1;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E: Slide / fine slide / extra fine slide down
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectE(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (a.Main.Period != 0)
				DoS3MSlideDn(tick, a, dat);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F: Slide / fine slide / extra fine slide up
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectF(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (a.Main.Period != 0)
				DoS3MSlideUp(tick, a, dat);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect I: Tremor
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectI(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				a.S3MTrOnOf = inf;
			else
			{
				inf = a.S3MTrOnOf;
				if (inf == 0)
					return 0;
			}

			if (tick == 0)
				return 0;

			byte on = (byte)((inf >> 4) + 1);
			byte off = (byte)((inf & 0xf) + 1);

			a.S3MTremor %= (byte)(on + off);
			a.Volume = (a.S3MTremor < on) ? a.TmpVolume : (short)0;
			a.OwnVol = true;
			a.S3MTremor++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Q: Retrig + volume slide
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectQ(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (a.Main.Period != 0)
			{
				if (inf != 0)
				{
					a.S3MRtgSlide = (byte)(inf >> 4);
					a.S3MRtgSpeed = (byte)(inf & 0xf);
				}

				// Only retrigger if low nibble > 0
				if (a.S3MRtgSpeed > 0)
				{
					if (a.Retrig == 0)
					{
						// When retrig counter reaches 0,
						// reset counter and restart the sample
						if (a.Main.Kick != Kick.Note)
							a.Main.Kick = Kick.KeyOff;

						a.Retrig = (sbyte)a.S3MRtgSpeed;

						if ((tick != 0) || ((flags & ModuleFlag.S3MSlides) != 0))
						{
							switch (a.S3MRtgSlide)
							{
								case 1:
								case 2:
								case 3:
								case 4:
								case 5:
								{
									a.TmpVolume -= (short)(1 << (a.S3MRtgSlide - 1));
									break;
								}

								case 6:
								{
									a.TmpVolume = (short)((2 * a.TmpVolume) / 3);
									break;
								}

								case 7:
								{
									a.TmpVolume >>= 1;
									break;
								}

								case 9:
								case 0xa:
								case 0xb:
								case 0xc:
								case 0xd:
								{
									a.TmpVolume += (short)(1 << (a.S3MRtgSlide - 9));
									break;
								}

								case 0xe:
								{
									a.TmpVolume = (short)((3 * a.TmpVolume) >> 1);
									break;
								}

								case 0xf:
								{
									a.TmpVolume <<= 1;
									break;
								}
							}

							if (a.TmpVolume < 0)
								a.TmpVolume = 0;
							else
							{
								if (a.TmpVolume > 64)
									a.TmpVolume = 64;
							}
						}
					}

					a.Retrig--;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect R: Tremolo
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectR(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.TrmDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.TrmSpd = (byte)((dat & 0xf0) >> 2);
			}

			short temp = LfoTremolo(a.TrmPos, (byte)((a.WaveControl >> 4) & 3));

			temp *= a.TrmDepth;
			temp >>= 7;

			a.Volume = (short)(a.TmpVolume + temp);

			if (a.Volume > 64)
				a.Volume = 64;

			if (a.Volume < 0)
				a.Volume = 0;

			a.OwnVol = true;

			if (tick != 0)
				a.TrmPos += (sbyte)a.TrmSpd;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect T: Tempo
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectT(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte tempo = uniTrk.UniGetByte();

			if ((tick != 0) || (mod.PatDly2 != 0))
				return 0;

			mod.Bpm = (tempo < 32) ? (ushort)32 : tempo;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect U: Fine vibrato
		/// </summary>
		/********************************************************************/
		private int DoS3MEffectU(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.VibDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.VibSpd = (byte)((dat & 0xf0) >> 2);
			}

			if (a.Main.Period != 0)
			{
				short temp = LfoVibrato(a.VibPos, (byte)(a.WaveControl & 3));

				temp *= a.VibDepth;
				temp >>= 7;

				a.Main.Period = (ushort)(a.TmpPeriod + temp);
				a.OwnPer = true;

				if (tick != 0)
					a.VibPos += (sbyte)a.VibSpd;
			}

			return 0;
		}
		#endregion

		#region FastTracker II specific effects
		/********************************************************************/
		/// <summary>
		/// Effect 6: Vibrato + volume slide
		/// </summary>
		/********************************************************************/
		private int DoXmEffect6(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.None);

			return DoXmEffectA(tick, flags, a, mod, channel);
		}



		/********************************************************************/
		/// <summary>
		/// Effect A: Volume slide
		/// </summary>
		/********************************************************************/
		private int DoXmEffectA(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				a.S3MVolSlide = inf;
			else
				inf = a.S3MVolSlide;

			if (tick != 0)
			{
				byte lo = (byte)(inf & 0xf);
				byte hi = (byte)(inf >> 4);

				if (hi == 0)
				{
					a.TmpVolume -= lo;

					if (a.TmpVolume < 0)
						a.TmpVolume = 0;
				}
				else
				{
					a.TmpVolume += hi;

					if (a.TmpVolume > 64)
						a.TmpVolume = 64;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E1: Fine portamento slide up
		/// </summary>
		/********************************************************************/
		private int DoXmEffectE1(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if (dat != 0)
					a.FPortUpSpd = dat;

				if (a.Main.Period != 0)
					a.TmpPeriod -= (ushort)(a.FPortUpSpd << 2);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E2: Fine portamento slide down
		/// </summary>
		/********************************************************************/
		private int DoXmEffectE2(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if (dat != 0)
					a.FPortDnSpd = dat;

				if (a.Main.Period != 0)
					a.TmpPeriod += (ushort)(a.FPortDnSpd << 2);
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect EA: Fine volume slide up
		/// </summary>
		/********************************************************************/
		private int DoXmEffectEA(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if (dat != 0)
					a.FSlideUpSpd = dat;

				a.TmpVolume += a.FSlideUpSpd;

				if (a.TmpVolume > 64)
					a.TmpVolume = 64;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect EB: Fine volume slide down
		/// </summary>
		/********************************************************************/
		private int DoXmEffectEB(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if (dat != 0)
					a.FSlideDnSpd = dat;

				a.TmpVolume -= a.FSlideDnSpd;

				if (a.TmpVolume < 0)
					a.TmpVolume = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect G: Set global volume
		/// </summary>
		/********************************************************************/
		private int DoXmEffectG(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			mod.Volume = (short)(uniTrk.UniGetByte() << 1);

			if (mod.Volume > 128)
				mod.Volume = 128;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect H: Global slide
		/// </summary>
		/********************************************************************/
		private int DoXmEffectH(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (tick != 0)
			{
				if (inf != 0)
					mod.GlobalSlide = inf;
				else
					inf = mod.GlobalSlide;

				if ((inf & 0xf0) != 0)
					inf &= 0xf0;

				mod.Volume = (short)(mod.Volume + ((inf >> 4) - (inf & 0xf)) * 2);

				if (mod.Volume < 0)
					mod.Volume = 0;
				else
				{
					if (mod.Volume > 128)
						mod.Volume = 128;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect L: Set envelope position
		/// </summary>
		/********************************************************************/
		private int DoXmEffectL(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if ((tick == 0) && (a.Main.I != null))
			{
				Instrument i = a.Main.I;

				Mp_Voice aOut;
				if ((aOut = a.Slave) != null)
				{
					if (aOut.VEnv.Env != null)
						SetEnvelopePosition(ref aOut.VEnv, i.VolEnv, dat);

					if (aOut.PEnv.Env != null)
					{
						// Because of a bug in FastTracker II, only the panning envelope
						// position is set if the volume sustain flag is set. Other players
						// may set the panning all the time
						if (((mod.Flags & ModuleFlag.Ft2Quirks) == 0) || ((i.VolFlg & EnvelopeFlag.Sustain) != 0))
							SetEnvelopePosition(ref aOut.PEnv, i.PanEnv, dat);
					}
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect P: Panning slide
		/// </summary>
		/********************************************************************/
		private int DoXmEffectP(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (!mod.PanFlag)
				return 0;

			if (inf != 0)
				a.PanSSpd = inf;
			else
				inf = a.PanSSpd;

			if (tick != 0)
			{
				byte lo = (byte)(inf & 0xf);
				byte hi = (byte)(inf >> 4);

				// Slide right has absolute priority
				if (hi != 0)
					lo = 0;

				short pan = (short)(((a.Main.Panning == SharedConstant.Pan_Surround) ? SharedConstant.Pan_Center : a.Main.Panning) + hi - lo);
				a.Main.Panning = (short)((pan < SharedConstant.Pan_Left) ? SharedConstant.Pan_Left : (pan > SharedConstant.Pan_Right ? SharedConstant.Pan_Right : pan));
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect X1: Extra fine portamento up
		/// </summary>
		/********************************************************************/
		private int DoXmEffectX1(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (dat != 0)
				a.FFPortUpSpd = dat;
			else
				dat = a.FFPortUpSpd;

			if (a.Main.Period != 0)
			{
				if (tick == 0)
				{
					a.Main.Period -= dat;
					a.TmpPeriod -= dat;
					a.OwnPer = true;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect X2: Extra fine portamento down
		/// </summary>
		/********************************************************************/
		private int DoXmEffectX2(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (dat != 0)
				a.FFPortDnSpd = dat;
			else
				dat = a.FFPortDnSpd;

			if (a.Main.Period != 0)
			{
				if (tick == 0)
				{
					a.Main.Period += dat;
					a.TmpPeriod += dat;
					a.OwnPer = true;
				}
			}

			return 0;
		}
		#endregion

		#region ImpulseTracker specific effects
		/********************************************************************/
		/// <summary>
		/// Effect G: Tone portamento
		/// </summary>
		/********************************************************************/
		private int DoItEffectG(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoItToneSlide(tick, a, uniTrk.UniGetByte());

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect H: Vibrato
		/// </summary>
		/********************************************************************/
		private int DoItEffectH(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoItVibrato(tick, a, uniTrk.UniGetByte(), ItVibratoFlags.None);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect H: Vibrato (old version)
		/// </summary>
		/********************************************************************/
		private int DoItEffectH_Old(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoItVibrato(tick, a, uniTrk.UniGetByte(), ItVibratoFlags.Old);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect I: Tremor
		/// </summary>
		/********************************************************************/
		private int DoItEffectI(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				a.S3MTrOnOf = inf;
			else
			{
				inf = a.S3MTrOnOf;
				if (inf == 0)
					return 0;
			}

			byte on = (byte)(inf >> 4);
			byte off = (byte)(inf & 0xf);

			a.S3MTremor %= (byte)(on + off);
			a.Volume = (a.S3MTremor < on) ? a.TmpVolume : (short)0;
			a.OwnVol = true;
			a.S3MTremor++;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect M: Set channel volume
		/// </summary>
		/********************************************************************/
		private int DoItEffectM(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			a.Main.ChanVol = uniTrk.UniGetByte();

			if (a.Main.ChanVol > 64)
				a.Main.ChanVol = 64;
			else
			{
				if (a.Main.ChanVol < 0)
					a.Main.ChanVol = 0;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect N: Slide / fine slide channel volume
		/// </summary>
		/********************************************************************/
		private int DoItEffectN(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				a.ChanVolSlide = inf;
			else
				inf = a.ChanVolSlide;

			byte lo = (byte)(inf & 0xf);
			byte hi = (byte)(inf >> 4);

			if (hi == 0)
				a.Main.ChanVol -= lo;
			else
			{
				if (lo == 0)
					a.Main.ChanVol += hi;
				else
				{
					if (hi == 0xf)
					{
						if (tick == 0)
							a.Main.ChanVol -= lo;
					}
					else
					{
						if (lo == 0xf)
						{
							if (tick == 0)
								a.Main.ChanVol += hi;
						}
					}
				}
			}

			if (a.Main.ChanVol < 0)
				a.Main.ChanVol = 0;
			else
			{
				if (a.Main.ChanVol > 64)
					a.Main.ChanVol = 64;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect P: Slide / fine slide channel panning
		/// </summary>
		/********************************************************************/
		private int DoItEffectP(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				a.PanSSpd = inf;
			else
				inf = a.PanSSpd;

			if (!mod.PanFlag)
				return 0;

			byte lo = (byte)(inf & 0xf);
			byte hi = (byte)(inf >> 4);

			short pan = (a.Main.Panning == SharedConstant.Pan_Surround) ? (short)SharedConstant.Pan_Center : a.Main.Panning;

			if (hi == 0)
				pan += (short)(lo << 2);
			else
			{
				if (lo == 0)
					pan -= (short)(hi << 2);
				else
				{
					if (hi == 0xf)
					{
						if (tick == 0)
							pan += (short)(lo << 2);
					}
					else
					{
						if (lo == 0xf)
						{
							if (tick == 0)
								pan -= (short)(hi << 2);
						}
					}
				}
			}

			a.Main.Panning = (short)((pan < SharedConstant.Pan_Left) ? SharedConstant.Pan_Left : (pan > SharedConstant.Pan_Right ? SharedConstant.Pan_Right : pan));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect S0: Special
		///
		/// Impulse/ScreamTracker Sxx effects. All effects share the same
		/// memory space
		/// </summary>
		/********************************************************************/
		private int DoItEffectS0(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			byte inf = (byte)(dat & 0xf);
			SsCommand c = (SsCommand)(dat >> 4);

			if (dat == 0)
			{
				c = a.SsEffect;
				inf = a.SsData;
			}
			else
			{
				a.SsEffect = c;
				a.SsData = inf;
			}

			switch (c)
			{
				// S1x: Set glissando voice
				case SsCommand.Glissando:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x30 | inf));
					break;
				}

				// S2x: Set finetune
				case SsCommand.Finetune:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x50 | inf));
					break;
				}

				// S3x: Set vibrato waveform
				case SsCommand.VibWave:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x40 | inf));
					break;
				}

				// S4x: Set tremolo waveform
				case SsCommand.TremWave:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x70 | inf));
					break;
				}

				// S5x: Set panbrello waveform
				case SsCommand.PanWave:
				{
					a.PanbWave = inf;
					break;
				}

				// S6x: Delay x number of frames (patDly)
				case SsCommand.FrameDelay:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0xe0 | inf));
					break;
				}

				// S7x: Instrument / NNA commands
				case SsCommand.S7Effects:
				{
					DoNnaEffects(mod, a, inf);
					break;
				}

				// S8x: Set panning position
				case SsCommand.Panning:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x80 | inf));
					break;
				}

				// S9x: Set surround sound
				case SsCommand.Surround:
				{
					if (mod.PanFlag)
					{
						a.Main.Panning = SharedConstant.Pan_Surround;
						mod.Panning[channel] = SharedConstant.Pan_Surround;
					}
					break;
				}

				// SAy: Set high order sample offset (yxx00h)
				case SsCommand.HiOffset:
				{
					if (tick == 0)
					{
						a.HiOffset = (uint)inf << 16;
						a.Main.Start = (int)(a.HiOffset | a.SOffset);

						if ((a.Main.S != null) && (a.Main.Start > a.Main.S.Length))
							a.Main.Start = (int)((a.Main.S.Flags & (SampleFlag.Loop | SampleFlag.Bidi)) != 0 ? a.Main.S.LoopStart : a.Main.S.Length);
					}
					break;
				}

				// SBx: Pattern loop
				case SsCommand.PatLoop:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0x60 | inf));
					break;
				}

				// SCx: Note cut
				case SsCommand.NoteCut:
				{
					if (inf == 0)
						inf = 1;

					DoEEffects(tick, flags, a, mod, channel, (byte)(0xc0 | inf));
					break;
				}

				// SDx: Note delay
				case SsCommand.NoteDelay:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0xd0 | inf));
					break;
				}

				// SEx: Pattern delay
				case SsCommand.PatDelay:
				{
					DoEEffects(tick, flags, a, mod, channel, (byte)(0xe0 | inf));
					break;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect T: Set tempo
		/// </summary>
		/********************************************************************/
		private int DoItEffectT(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte tempo = uniTrk.UniGetByte();

			if (mod.PatDly2 != 0)
				return 0;

			short temp = (short)mod.Bpm;

			if ((tempo & 0x10) != 0)
				temp += (short)(tempo & 0x0f);
			else
				temp -= tempo;

			mod.Bpm = (ushort)((temp > 255) ? 255 : (temp < 1 ? 1 : temp));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect U: Fine vibrato
		/// </summary>
		/********************************************************************/
		private int DoItEffectU(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoItVibrato(tick, a, uniTrk.UniGetByte(), ItVibratoFlags.Fine);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect U: Fine vibrato (old version)
		/// </summary>
		/********************************************************************/
		private int DoItEffectU_Old(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			DoItVibrato(tick, a, uniTrk.UniGetByte(), ItVibratoFlags.Fine | ItVibratoFlags.Old);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect W: Slide / fine slide global volume
		/// </summary>
		/********************************************************************/
		private int DoItEffectW(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte inf = uniTrk.UniGetByte();

			if (inf != 0)
				mod.GlobalSlide = inf;
			else
				inf = mod.GlobalSlide;

			byte lo = (byte)(inf & 0xf);
			byte hi = (byte)(inf >> 4);

			if (lo == 0)
			{
				if (tick != 0)
					mod.Volume += hi;
			}
			else
			{
				if (hi == 0)
				{
					if (tick != 0)
						mod.Volume -= lo;
				}
				else
				{
					if (lo == 0xf)
					{
						if (tick == 0)
							mod.Volume += hi;
					}
					else
					{
						if (hi == 0xf)
						{
							if (tick == 0)
								mod.Volume -= lo;
						}
					}
				}
			}

			if (mod.Volume < 0)
				mod.Volume = 0;
			else
			{
				if (mod.Volume > 128)
					mod.Volume = 128;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect Y: Panbrello
		/// </summary>
		/********************************************************************/
		private int DoItEffectY(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.PanbDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.PanbSpd = (sbyte)((dat & 0xf0) >> 4);
			}

			if (mod.PanFlag)
			{
				// TODO: When wave is random, each random value persists for a number of
				// ticks equal to the speed nibble. This behaviour is unique to panbrello
				short temp = LfoPanbrello((sbyte)a.PanbPos, a.PanbWave);

				temp *= a.PanbDepth;
				temp = (short)((temp / 8) + mod.Panning[channel]);

				a.Main.Panning = (short)((temp < SharedConstant.Pan_Left) ? SharedConstant.Pan_Left : (temp > SharedConstant.Pan_Right) ? SharedConstant.Pan_Right : temp);
				a.PanbPos += (byte)a.PanbSpd;
			}

			return 0;
		}
		#endregion

		#region UltraTracker specific effects
		/********************************************************************/
		/// <summary>
		/// Effect 9: Sample offset
		/// </summary>
		/********************************************************************/
		private int DoUltEffect9(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			ushort offset = uniTrk.UniGetWord();

			if (offset != 0)
				a.UltOffset = offset;

			a.Main.Start = a.UltOffset << 2;

			if ((a.Main.S != null) && (a.Main.Start > a.Main.S.Length))
				a.Main.Start = (int)((a.Main.S.Flags & (SampleFlag.Loop | SampleFlag.Bidi)) != 0 ? a.Main.S.LoopStart : a.Main.S.Length);

			return 0;
		}
		#endregion

		#region OctaMED specific effects
		/********************************************************************/
		/// <summary>
		/// Speed effect
		/// </summary>
		/********************************************************************/
		private int DoMedSpeed(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			ushort speed = uniTrk.UniGetWord();

			mod.Bpm = speed;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Vibrato effect
		/// </summary>
		/********************************************************************/
		private int DoMedEffectVib(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// MED vibrato (larger speed/depth range than PT vibrato)
			byte rate = uniTrk.UniGetByte();
			byte depth = uniTrk.UniGetByte();

			if (tick == 0)
			{
				a.VibSpd = rate;
				a.VibDepth = depth;
			}

			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.Tick0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F1: Play note twice
		/// </summary>
		/********************************************************************/
		private int DoMedEffectF1(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// "Play twice". Despite the documentation, this only retriggers exactly one time
			// on the third tick (i.e. it is not equivalent to PT E93)
			if (tick == 3)
			{
				if (a.Main.Period != 0)
					a.Main.Kick = Kick.Note;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F2: Delay note
		/// </summary>
		/********************************************************************/
		private int DoMedEffectF2(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Delay for 3 ticks before playing
			DoEEffects(tick, flags, a, mod, channel, 0xd3);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F3: Play note tree times
		/// </summary>
		/********************************************************************/
		private int DoMedEffectF3(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// "Play three times". Actually, it's just a regular retrigger every 2 ticks,
			// starting from tick 2
			if (tick == 0)
				a.Retrig = 2;

			if (a.Retrig == 0)
			{
				if (a.Main.Period != 0)
					a.Main.Kick = Kick.Note;

				a.Retrig = 2;
			}

			a.Retrig--;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect FD: Set pitch
		/// </summary>
		/********************************************************************/
		private int DoMedEffectFD(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Set pitch without triggering a new note
			a.Main.Kick = Kick.Absent;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 16: Pattern loop
		/// </summary>
		/********************************************************************/
		private int DoMedEffect16(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Loop (similar to PT E6x but with an extended range).
			// TODO: Currently doesn't support the loop point persisting between patterns.
			// It's not clear if anything actually relies on that
			byte param = uniTrk.UniGetByte();

			DoLoop(tick, flags, a, mod, param);

			// OctaMED repeat position is global so set it for every channel...
			// This fixes a playback bug found in "(brooker) #1.med", which sets
			// the jump position in track 2 but jumps in track 1
			int repPos = a.Pat_RepPos;

			for (int i = 0; i < pf.NumChn; i++)
				pf.Control[i].Pat_RepPos = (short)repPos;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 18: Cut note
		/// </summary>
		/********************************************************************/
		private int DoMedEffect18(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Cut note (same as PT ECx but with an extended range)
			byte param = uniTrk.UniGetByte();
			if (tick >= param)
				a.TmpVolume = 0;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 1E: Pattern delay
		/// </summary>
		/********************************************************************/
		private int DoMedEffect1E(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Pattern delay (same as PT EEx but with an extended range)
			byte param = uniTrk.UniGetByte();
			if ((tick == 0) && (mod.PatDly2 == 0))
				mod.PatDly = (byte)((param < 255) ? param + 1 : 255);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 1F: Note delay + retrigger
		/// </summary>
		/********************************************************************/
		private int DoMedEffect1F(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			// Combined note delay and retrigger (same as PT E9x and EDx but can be combinded)
			// The high nibble is delay and the low nibble is retrigger
			byte param = uniTrk.UniGetByte();
			byte retrig = (byte)(param & 0xf);

			if (tick == 0)
			{
				a.Main.NoteDelay = (byte)((param & 0xf0) >> 4);
				a.Retrig = (sbyte)retrig;
			}
			else
			{
				if (a.Main.NoteDelay != 0)
					a.Main.NoteDelay--;
			}

			if (a.Main.NoteDelay == 0)
			{
				if ((retrig != 0) && (a.Retrig == 0))
				{
					if (a.Main.Period != 0)
						a.Main.Kick = Kick.Note;

					a.Retrig = (sbyte)retrig;
				}

				a.Retrig--;
			}

			return 0;
		}
		#endregion

		#region Octalyzer specific effects
		/********************************************************************/
		/// <summary>
		/// Arpeggio effect
		/// </summary>
		/********************************************************************/
		private int DoOktArp(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat2 = uniTrk.UniGetByte();		// Arpeggio style
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat == 0) && ((flags & ModuleFlag.ArpMem) != 0))
					dat = a.ArpMem;
				else
					a.ArpMem = dat;
			}

			if (a.Main.Period != 0)
				DoArpeggio(tick, flags, a, dat2);

			return 0;
		}
		#endregion

		#region Farandole specific effects
		/********************************************************************/
		/// <summary>
		/// Effect 1: Portamento up
		/// </summary>
		/********************************************************************/
		private int DoFarEffect1(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				a.SlideSpeed = (ushort)(dat << 2);

				if (a.Main.Period != 0)
					a.TmpPeriod -= a.SlideSpeed;

				a.FarTonePortaRunning = false;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 2: Portamento down
		/// </summary>
		/********************************************************************/
		private int DoFarEffect2(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				a.SlideSpeed = (ushort)(dat << 2);

				if (a.Main.Period != 0)
					a.TmpPeriod += a.SlideSpeed;

				a.FarTonePortaRunning = false;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 3: Porta to note
		/// </summary>
		/********************************************************************/
		private int DoFarEffect3(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				// We have to slide a.Main.Period toward a.WantedPeriod,
				// compute the difference between those two values
				float dist = a.WantedPeriod - a.Main.Period;

				// Adjust effect argument
				if (dat == 0)
					dat = 1;

				// Unlike other players, the data is how many rows the port
				// should take and not a speed
				a.FarTonePortaSpeed = dist / (mod.SngSpd * dat);
				a.FarCurrentValue = a.Main.Period;
				a.FarTonePortaRunning = true;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 4: Retrigger
		/// </summary>
		/********************************************************************/
		private int DoFarEffect4(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			// Here the argument is the number of retrigs to play evenly
			// spaced in the current row
			if (tick == 0)
			{
				if (dat != 0)
				{
					a.FarRetrigCount = dat;
					a.Retrig = 0;
				}
			}

			if (dat != 0)
			{
				if (a.Retrig == 0)
				{
					if (a.FarRetrigCount > 0)
					{
						// When retrig counter reaches 0,
						// reset counter and restart the sample
						if (a.Main.Period != 0)
							a.Main.Kick = Kick.Note;

						a.FarRetrigCount--;
						if (a.FarRetrigCount > 0)
							a.Retrig = (sbyte)(((mod.FarTempoBend + GetFarTempoFactor(mod)) / dat / 8) - 1);
					}
				}
				else
					a.Retrig--;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect 6: Vibrato
		/// </summary>
		/********************************************************************/
		private int DoFarEffect6(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				if ((dat & 0x0f) != 0)
					a.VibDepth = (byte)(dat & 0x0f);

				if ((dat & 0xf0) != 0)
					a.VibSpd = (byte)((dat & 0xf0) * 6);
			}

			if (a.Main.Period != 0)
				DoVibrato(tick, a, VibratoFlags.Tick0);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect D: Fine tempo down
		/// </summary>
		/********************************************************************/
		private int DoFarEffectD(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (dat != 0)
			{
				mod.FarTempoBend -= dat;

				if ((mod.FarTempoBend + GetFarTempoFactor(mod)) <= 0)
					mod.FarTempoBend = 0;
			}
			else
				mod.FarTempoBend = 0;

			SetFarTempo(mod);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect E: Fine tempo up
		/// </summary>
		/********************************************************************/
		private int DoFarEffectE(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (dat != 0)
			{
				mod.FarTempoBend += dat;

				if ((mod.FarTempoBend + GetFarTempoFactor(mod)) >= 100)
					mod.FarTempoBend = 100;
			}
			else
				mod.FarTempoBend = 0;

			SetFarTempo(mod);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Effect F: Set tempo
		/// </summary>
		/********************************************************************/
		private int DoFarEffectF(ushort tick, ModuleFlag flags, Mp_Control a, Module mod, short channel)
		{
			byte dat = uniTrk.UniGetByte();

			if (tick == 0)
			{
				mod.FarCurTempo = dat;
				mod.VbTick = 0;

				SetFarTempo(mod);
			}

			return 0;
		}
		#endregion

		#endregion
	}
}
