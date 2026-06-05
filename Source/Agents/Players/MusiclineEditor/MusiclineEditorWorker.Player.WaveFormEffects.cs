/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor
{
	/// <summary>
	/// Handle all the wave form effects
	/// </summary>
	internal partial class MusiclineEditorWorker
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MoveLoop(VoiceInfo voiceInfo, IChannel channel)
		{
			if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop))
			{
				SamplePointer sample;

				if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopStep))
				{
					voiceInfo.LoopDelay--;
					if (voiceInfo.LoopDelay >= 0)
					{
						sample = voiceInfo.LoopWaveSample!.Value;
						sample.StartOffset += voiceInfo.LoopCounterSave * 2U;

						voiceInfo.WaveSampleRepeatPointer = sample;
						voiceInfo.WaveSampleRepeatPointerOriginal = sample;
						return;
					}

					voiceInfo.LoopDelay = 0;

					if (voiceInfo.LoopWait != 0)
					{
						voiceInfo.LoopWaitCounter--;
						if (voiceInfo.LoopWaitCounter >= 0)
							return;

						voiceInfo.LoopWaitCounter = (short)voiceInfo.LoopWait;
					}

					voiceInfo.LoopCounterSave = voiceInfo.LoopCounter;
					LoopCounter(voiceInfo);
				}
				else
				{
					if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopInit))
					{
						if (voiceInfo.PartNote != 0)
						{
							voiceInfo.LoopCounterSave = voiceInfo.LoopCounter;
							LoopCounter(voiceInfo);
						}
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							voiceInfo.LoopCounterSave = voiceInfo.LoopCounter;

						LoopCounter(voiceInfo);
					}
				}

				ushort previousCounter = voiceInfo.LoopCounterSave;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.LoopStep))
				{
					voiceInfo.LoopDelay--;
					if (voiceInfo.LoopDelay < 0)
					{
						voiceInfo.LoopDelay = 0;

						if (voiceInfo.LoopWait != 0)
						{
							voiceInfo.LoopWaitCounter--;
							if (voiceInfo.LoopWaitCounter >= 0)
								goto NoStep;

							voiceInfo.LoopWaitCounter = (short)voiceInfo.LoopWait;
						}

						int step = voiceInfo.LoopStep;
						if (step < 0)
							step = -step;

						int counter = voiceInfo.LoopCounterSave;
						short speed = (sbyte)voiceInfo.LoopSpeed;

						if (speed < 0)
						{
							counter += speed * step;

							if (counter < 0)
								voiceInfo.LoopCounterSave = 0;
							else
								voiceInfo.LoopCounterSave = (ushort)counter;
						}
						else
						{
							counter += speed * step;

							if (counter > voiceInfo.LoopWaveSampleCounterMax)
								voiceInfo.LoopCounterSave = voiceInfo.LoopWaveSampleCounterMax;
							else
								voiceInfo.LoopCounterSave = (ushort)counter;
						}
					}
				}

				NoStep:
				sample = voiceInfo.LoopWaveSample!.Value;
				sample.StartOffset += previousCounter * 2U;

				voiceInfo.WaveSampleRepeatPointer = sample;
				voiceInfo.WaveSampleRepeatPointerOriginal = sample;

				if ((voiceInfo.LoopTurns >= 0) || !voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.LoopStop))
					return;

				voiceInfo.Effects1.Effect &= ~InstrumentEffect1.Loop;

				channel.SetSample(Tables.ZeroSample, 0, 2);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LoopCounter(VoiceInfo voiceInfo)
		{
			if (voiceInfo.LoopTurns < 0)
				return;

			int counter = voiceInfo.LoopCounter;

			if (voiceInfo.LoopRepeat < voiceInfo.LoopRepeatEnd)
			{
				if (voiceInfo.LoopStep < 0)
				{
					counter += voiceInfo.LoopStep;
					if (counter >= voiceInfo.LoopRepeat)
					{
						voiceInfo.LoopCounter = (ushort)counter;
						return;
					}
				}
				else
				{
					counter += voiceInfo.LoopStep;
					if (counter <= voiceInfo.LoopRepeatEnd)
					{
						voiceInfo.LoopCounter = (ushort)counter;
						return;
					}
				}
			}
			else
			{
				if (voiceInfo.LoopStep < 0)
				{
					counter += voiceInfo.LoopStep;
					if (counter >= voiceInfo.LoopRepeatEnd)
					{
						voiceInfo.LoopCounter = (ushort)counter;
						return;
					}
				}
				else
				{
					counter += voiceInfo.LoopStep;
					if (counter <= voiceInfo.LoopRepeat)
					{
						voiceInfo.LoopCounter = (ushort)counter;
						return;
					}
				}
			}

			if (voiceInfo.LoopTurns != 0)
			{
				voiceInfo.LoopTurns--;
				if (voiceInfo.LoopTurns == 0)
					voiceInfo.LoopTurns = -1;
			}

			counter -= voiceInfo.LoopStep;

			voiceInfo.LoopStep = -voiceInfo.LoopStep;
			voiceInfo.LoopCounter = (ushort)counter;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PhasePlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Phase))
			{
				CounterInfo counterInfo = voiceInfo.PhaseCounter;

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseStep))
				{
					if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseInit))
					{
						if (voiceInfo.PartNote == 0)
							goto NoCount;

						counterInfo.SaveCounter = counterInfo.Counter;
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							counterInfo.SaveCounter = counterInfo.Counter;
					}
				}
				else
					counterInfo.SaveCounter = counterInfo.Counter;

				Counter(counterInfo);

				NoCount:
				ushort previousCounter = counterInfo.SaveCounter;

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseStep))
				{
					if (counterInfo.Delay != 0)
						counterInfo.Delay--;
					else
					{
						short speed = (sbyte)voiceInfo.PhaseSpeed;
						if (speed < 0)
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter - speed);
							if ((short)counterInfo.SaveCounter > 512)
								counterInfo.SaveCounter = 512;
						}
						else
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter - speed);
							if ((short)counterInfo.SaveCounter < 2)
								counterInfo.SaveCounter = 2;
						}
					}
				}

				uint repeatLength = voiceInfo.WaveSampleRepeatLength * 2U;
				byte[] sizerTable;
				ushort[] sizerOffsetTable;

				if (repeatLength > 128)
				{
					previousCounter++;
					previousCounter >>= 1;
					sizerTable = Tables.SizerTable256;
					sizerOffsetTable = Tables.SizerOffset256;
				}
				else if (repeatLength > 64)
				{
					previousCounter += 3;
					previousCounter >>= 2;
					sizerTable = Tables.SizerTable128;
					sizerOffsetTable = Tables.SizerOffset128;
				}
				else if (repeatLength > 32)
				{
					previousCounter += 7;
					previousCounter >>= 3;
					sizerTable = Tables.SizerTable64;
					sizerOffsetTable = Tables.SizerOffset64;
				}
				else if (repeatLength > 16)
				{
					previousCounter += 15;
					previousCounter >>= 4;
					sizerTable = Tables.SizerTable32;
					sizerOffsetTable = Tables.SizerOffset32;
				}
				else
				{
					previousCounter += 31;
					previousCounter >>= 5;
					sizerTable = Tables.SizerTable16;
					sizerOffsetTable = Tables.SizerOffset16;
				}

				SamplePointer repeatBuffer = voiceInfo.WaveSampleRepeatPointer!.Value;
				SamplePointer phaseBuffer = new SamplePointer(voiceInfo.PhaseWaveBuffer);

				voiceInfo.WaveSampleRepeatPointer = phaseBuffer;

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop) || (voiceInfo.WaveOrSample != SampleType.Sample))
					voiceInfo.WaveSamplePointer = phaseBuffer;

				if ((previousCounter >= repeatLength) || (previousCounter == 0))
				{
					Phase_Mova(repeatBuffer, phaseBuffer, repeatLength);
					return;
				}

				uint todo = previousCounter;
				uint leftTodo = repeatLength - previousCounter;

				ushort sizerOffset = sizerOffsetTable[previousCounter - 1];

				switch (voiceInfo.PhaseType)
				{
					case PhaseType.Low:
					{
						Phase_Low(voiceInfo, sizerTable, sizerOffset, todo, leftTodo, previousCounter, repeatBuffer, phaseBuffer);
						break;
					}

					case PhaseType.High:
					{
						Phase_High(voiceInfo, sizerTable, sizerOffset, todo, leftTodo, previousCounter, repeatBuffer, phaseBuffer);
						break;
					}

					case PhaseType.Med:
					{
						Phase_Med(voiceInfo, sizerTable, sizerOffset, todo, leftTodo, previousCounter, repeatBuffer, phaseBuffer);
						break;
					}

					default:
					{
						Phase_Quick(voiceInfo, sizerTable, sizerOffset, todo, leftTodo, repeatBuffer, phaseBuffer);
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Phase_Quick(VoiceInfo voiceInfo, byte[] sizerTable, ushort sizerOffset, uint todo, uint leftTodo, SamplePointer repeatBuffer, SamplePointer phaseBuffer)
		{
			SamplePointer phaseCopy = phaseBuffer;
			byte index = 0;

			for (uint i = 0; i < todo; i++)
			{
				index = sizerTable[sizerOffset++];
				phaseBuffer.SampleData[phaseBuffer.StartOffset++] = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
			}

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseFill))
			{
				for (uint i = 0; i < leftTodo; i++)
					phaseBuffer.SampleData[phaseBuffer.StartOffset++] = phaseCopy.SampleData[phaseCopy.StartOffset++];
			}
			else
			{
				if (leftTodo > 0)
				{
					sbyte sample = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];

					for (uint i = 0; i < leftTodo; i++)
						phaseBuffer.SampleData[phaseBuffer.StartOffset++] = sample;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Phase_High(VoiceInfo voiceInfo, byte[] sizerTable, ushort sizerOffset, uint todo, uint leftTodo, ushort previousCounter, SamplePointer repeatBuffer, SamplePointer phaseBuffer)
		{
			ushort sizerOffsetCopy = sizerOffset;
			SamplePointer repeatCopy = repeatBuffer;
			byte index = 0;

			for (uint i = 0; i < todo; i++)
			{
				index = sizerTable[sizerOffset++];

				short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
				short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
				sbyte output = (sbyte)(((sample1 + sample2) + (sample1 * 2)) >> 2);

				phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
			}

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseFill))
			{
				if (leftTodo > 0)
				{
					sizerOffset = sizerOffsetCopy;

					do
					{
						uint counter = Math.Min(previousCounter, leftTodo);

						for (uint i = 0; i < counter; i++)
						{
							index = sizerTable[sizerOffset++];

							short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
							short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
							sbyte output = (sbyte)(((sample1 + sample2) + (sample1 * 2)) >> 2);

							phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
						}

						leftTodo -= counter;
					}
					while (leftTodo > 0);
				}
			}
			else
			{
				if (leftTodo > 0)
				{
					short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
					sample1 += (short)(sample1 * 2);

					for (uint i = 0; i < leftTodo; i++)
					{
						short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
						sbyte output = (sbyte)((sample1 + sample2) >> 2);

						phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Phase_Med(VoiceInfo voiceInfo, byte[] sizerTable, ushort sizerOffset, uint todo, uint leftTodo, ushort previousCounter, SamplePointer repeatBuffer, SamplePointer phaseBuffer)
		{
			ushort sizerOffsetCopy = sizerOffset;
			SamplePointer repeatCopy = repeatBuffer;
			byte index = 0;

			for (uint i = 0; i < todo; i++)
			{
				index = sizerTable[sizerOffset++];

				short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
				short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
				sbyte output = (sbyte)((sample1 + sample2) >> 1);

				phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
			}

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseFill))
			{
				if (leftTodo > 0)
				{
					do
					{
						uint counter = Math.Min(previousCounter, leftTodo);
						sizerOffset = sizerOffsetCopy;

						for (uint i = 0; i < counter; i++)
						{
							index = sizerTable[sizerOffset++];

							short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
							short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
							sbyte output = (sbyte)((sample1 + sample2) >> 1);

							phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
						}

						leftTodo -= counter;
					}
					while (leftTodo > 0);
				}
			}
			else
			{
				if (leftTodo > 0)
				{
					short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];

					for (uint i = 0; i < leftTodo; i++)
					{
						short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
						sbyte output = (sbyte)((sample1 + sample2) >> 1);

						phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Phase_Low(VoiceInfo voiceInfo, byte[] sizerTable, ushort sizerOffset, uint todo, uint leftTodo, ushort previousCounter, SamplePointer repeatBuffer, SamplePointer phaseBuffer)
		{
			ushort sizerOffsetCopy = sizerOffset;
			SamplePointer repeatCopy = repeatBuffer;
			byte index = 0;

			for (uint i = 0; i < todo; i++)
			{
				index = sizerTable[sizerOffset++];

				short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
				short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
				sbyte output = (sbyte)(((sample1 + sample2) + (sample2 * 2)) >> 2);

				phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
			}

			if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.PhaseFill))
			{
				if (leftTodo > 0)
				{
					sizerOffset = sizerOffsetCopy;

					do
					{
						uint counter = Math.Min(previousCounter, leftTodo);

						for (uint i = 0; i < counter; i++)
						{
							index = sizerTable[sizerOffset++];

							short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];
							short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
							sbyte output = (sbyte)(((sample1 + sample2) + (sample2 * 2)) >> 2);

							phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
						}

						leftTodo -= counter;
					}
					while (leftTodo > 0);
				}
			}
			else
			{
				if (leftTodo > 0)
				{
					short sample1 = repeatBuffer.SampleData[repeatBuffer.StartOffset + index];

					for (uint i = 0; i < leftTodo; i++)
					{
						short sample2 = repeatCopy.SampleData[repeatCopy.StartOffset++];
						sbyte output = (sbyte)(((sample1 + sample2) + (sample2 * 2)) >> 2);

						phaseBuffer.SampleData[phaseBuffer.StartOffset++] = output;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Phase_Mova(SamplePointer repeatBuffer, SamplePointer phaseBuffer, uint todo)
		{
			for (uint i = 0; i < todo; i++)
				phaseBuffer.SampleData[phaseBuffer.StartOffset++] = repeatBuffer.SampleData[repeatBuffer.StartOffset++];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void MixPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Mix))
			{
				CounterInfo counterInfo = voiceInfo.MixCounter;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixStep))
				{
					if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixInit))
					{
						if (voiceInfo.PartNote == 0)
							goto NoCount;

						counterInfo.SaveCounter = counterInfo.Counter;
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							counterInfo.SaveCounter = counterInfo.Counter;
					}
				}
				else
					counterInfo.SaveCounter = counterInfo.Counter;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixCounter))
					OneWayCounter(counterInfo);
				else
					Counter(counterInfo);

				NoCount:
				ushort previousCounter = counterInfo.SaveCounter;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixStep))
				{
					if (counterInfo.Delay != 0)
						counterInfo.Delay--;
					else
					{
						short speed = (sbyte)voiceInfo.MixSpeed;
						if (speed < 0)
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if ((short)counterInfo.SaveCounter < 0)
								counterInfo.SaveCounter = 0;
						}
						else
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if (counterInfo.SaveCounter > 510)
								counterInfo.SaveCounter = 510;
						}
					}
				}

				uint repeatLength = voiceInfo.WaveSampleRepeatLength * 2U;
				ushort length;

				if (repeatLength > 128)
				{
					previousCounter >>= 1;
					length = 0;
				}
				else if (repeatLength > 64)
				{
					previousCounter >>= 2;
					length = 256;
				}
				else if (repeatLength > 32)
				{
					previousCounter >>= 3;
					length = 256 + 128;
				}
				else if (repeatLength > 16)
				{
					previousCounter >>= 4;
					length = 256 + 128 + 64;
				}
				else
				{
					previousCounter >>= 5;
					length = 256 + 128 + 64 + 32;
				}

				SamplePointer repeatBuffer = voiceInfo.WaveSampleRepeatPointer!.Value;
				SamplePointer mixBuffer = new SamplePointer(voiceInfo.MixWaveBuffer);

				if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.MixBuff))
				{
					mixBuffer = repeatBuffer;

					if (voiceInfo.MixWaveNumber != 0)
					{
						mixBuffer = samples[voiceInfo.MixWaveNumber].SampleRepeatPointer!.Value;
						mixBuffer.StartOffset += length;
					}
				}

				SamplePointer repeat2Buffer = mixBuffer;
				SamplePointer destBuffer = new SamplePointer(voiceInfo.MixWaveBuffer);
				voiceInfo.WaveSampleRepeatPointer = destBuffer;

				if (voiceInfo.WaveOrSample != SampleType.Sample)
					voiceInfo.WaveSamplePointer = destBuffer;

				mixBuffer.StartOffset += previousCounter;
				repeatLength -= previousCounter;

				int boost = voiceInfo.MixResFilBoost.HasFlag(BoostFlag.Mix) ? 0 : 1;

				void Loop(sbyte[] source1, uint source1Offset, sbyte[] source2, uint source2Offset, sbyte[] destination, uint destinationOffset, uint count)
				{
					for (int i = 0; i < count; i++)
					{
						short val = (short)((source1[source1Offset++] + source2[source2Offset++]) >> boost);
						destination[destinationOffset++] = (sbyte)val;
					}
				}

				Loop(repeatBuffer.SampleData, repeatBuffer.StartOffset, mixBuffer.SampleData, mixBuffer.StartOffset, destBuffer.SampleData, destBuffer.StartOffset, repeatLength);

				if (previousCounter != 0)
					Loop(repeatBuffer.SampleData, repeatBuffer.StartOffset + repeatLength, repeat2Buffer.SampleData, repeat2Buffer.StartOffset, destBuffer.SampleData, destBuffer.StartOffset + repeatLength, previousCounter);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ResonancePlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Resonance))
			{
				CounterInfo counterInfo = voiceInfo.ResonanceCounter;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.ResonanceStep))
				{
					if (!voiceInfo.Effects2.Argument.HasFlag(MixFlag.ResonanceInit))
					{
						if (voiceInfo.PartNote == 0)
							goto NoCount;

						counterInfo.SaveCounter = counterInfo.Counter;
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							counterInfo.SaveCounter = counterInfo.Counter;
					}
				}
				else
					counterInfo.SaveCounter = counterInfo.Counter;

				Counter(counterInfo);

				NoCount:
				ushort previousCounter = counterInfo.SaveCounter;

				if (voiceInfo.Effects2.Argument.HasFlag(MixFlag.ResonanceStep))
				{
					if (counterInfo.Delay != 0)
						counterInfo.Delay--;
					else
					{
						short speed = (sbyte)voiceInfo.ResonanceSpeed;
						if (speed < 0)
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if ((short)counterInfo.SaveCounter < 0)
								counterInfo.SaveCounter = 0;
						}
						else
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if (counterInfo.SaveCounter > 510)
								counterInfo.SaveCounter = 510;
						}
					}
				}

				SamplePointer repeatBuffer = voiceInfo.WaveSampleRepeatPointer!.Value;
				SamplePointer resonanceBuffer = new SamplePointer(voiceInfo.ResonanceWaveBuffer);

				voiceInfo.WaveSampleRepeatPointer = resonanceBuffer;

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop) || (voiceInfo.WaveOrSample != SampleType.Sample))
					voiceInfo.WaveSamplePointer = resonanceBuffer;

				uint repeatLength = voiceInfo.WaveSampleRepeatLength * 2U;
				sbyte lastSample = voiceInfo.ResonanceLastSample;

				if (voiceInfo.ResonanceLastInit)
				{
					voiceInfo.ResonanceLastInit = false;
					lastSample = (sbyte)(repeatBuffer.SampleData[repeatBuffer.StartOffset + repeatLength - 1] >> 2);
				}

				short sampleState = (short)(lastSample << 7);
				previousCounter &= 0xfffe;
				ushort dampingCoef = 0x8000;
				short velocity = 0;
				ushort cutoffPeriod = Tables.ResonanceList[previousCounter / 2];
				sbyte output = lastSample;

				dampingCoef -= Tables.ResampleList[voiceInfo.ResonanceAmp];
				dampingCoef = (ushort)((dampingCoef * 0xe666) >> 16);

				int boost = voiceInfo.MixResFilBoost.HasFlag(BoostFlag.Resonance) ? 6 : 7;

				for (int i = 0; i < repeatLength; i++)
				{
					int acceleration = (((repeatBuffer.SampleData[repeatBuffer.StartOffset++] << 5) - sampleState) << 7) / cutoffPeriod;
					velocity = (short)(velocity + acceleration);
					sampleState = (short)(sampleState + velocity);
					output = (sbyte)(sampleState >> boost);

					resonanceBuffer.SampleData[resonanceBuffer.StartOffset++] = output;

					velocity = (short)((velocity * (short)dampingCoef) >> 15);
				}

				voiceInfo.ResonanceLastSample = output;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FilterPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Filter))
			{
				CounterInfo counterInfo = voiceInfo.FilterCounter;

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterStep))
				{
					if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterInit))
					{
						if (voiceInfo.PartNote == 0)
							goto NoCount;

						counterInfo.SaveCounter = counterInfo.Counter;
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							counterInfo.SaveCounter = counterInfo.Counter;
					}
				}
				else
					counterInfo.SaveCounter = counterInfo.Counter;

				Counter(counterInfo);

				NoCount:
				ushort previousCounter = counterInfo.SaveCounter;

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.FilterStep))
				{
					if (counterInfo.Delay != 0)
						counterInfo.Delay--;
					else
					{
						short speed = (sbyte)voiceInfo.FilterSpeed;
						if (speed < 0)
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if ((short)counterInfo.SaveCounter < 0)
								counterInfo.SaveCounter = 0;
						}
						else
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if (counterInfo.SaveCounter > 510)
								counterInfo.SaveCounter = 510;
						}
					}
				}

				SamplePointer repeatBuffer = voiceInfo.WaveSampleRepeatPointer!.Value;
				SamplePointer filterBuffer = new SamplePointer(voiceInfo.FilterWaveBuffer);

				voiceInfo.WaveSampleRepeatPointer = filterBuffer;

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop) || (voiceInfo.WaveOrSample != SampleType.Sample))
					voiceInfo.WaveSamplePointer = filterBuffer;

				uint repeatLength = voiceInfo.WaveSampleRepeatLength * 2U;
				sbyte lastSample = voiceInfo.FilterLastSample;

				if (voiceInfo.FilterType == FilterType.Normal)
				{
					if (voiceInfo.FilterLastInit)
					{
						voiceInfo.FilterLastInit = false;
						lastSample = repeatBuffer.SampleData[repeatBuffer.StartOffset + repeatLength - 1];
					}

					short sampleState = (short)(lastSample << 7);
					previousCounter &= 0xfffe;
					ushort cutoffCoef = Tables.FilterList[previousCounter / 2];
					ushort dampingCoef = (ushort)(0x8000 - cutoffCoef);
					cutoffCoef >>= 1;
					dampingCoef = (ushort)(((short)dampingCoef * unchecked((short)0xf000)) >> 16);
					short velocity = 0;
					sbyte output = lastSample;

					int boost = voiceInfo.MixResFilBoost.HasFlag(BoostFlag.Filter) ? 6 : 7;

					for (int i = 0; i < repeatLength; i++)
					{
						int acceleration = (((repeatBuffer.SampleData[repeatBuffer.StartOffset++] << 7) - sampleState) * cutoffCoef) >> 14;
						velocity = (short)(velocity + acceleration);
						sampleState = (short)(sampleState + velocity);
						output = (sbyte)(sampleState >> boost);

						filterBuffer.SampleData[filterBuffer.StartOffset++] = output;

						velocity = (short)((velocity * (short)dampingCoef) >> 15);
					}

					voiceInfo.FilterLastSample = output;
				}
				else
				{
					if (voiceInfo.FilterLastInit)
					{
						voiceInfo.FilterLastInit = false;
						lastSample = (sbyte)(repeatBuffer.SampleData[repeatBuffer.StartOffset + repeatLength - 1] >> 1);
					}

					short sampleState = (short)(lastSample << 7);
					previousCounter &= 0xfffe;
					ushort cutoffCoef = Tables.ResonanceFilterList[previousCounter / 2];
					ushort dampingCoef = (ushort)(0x8000 - cutoffCoef);
					cutoffCoef >>= 1;
					dampingCoef = (ushort)((dampingCoef * 0xe666) >> 16);
					short velocity = 0;
					sbyte output = lastSample;

					int boost = voiceInfo.MixResFilBoost.HasFlag(BoostFlag.Filter) ? 6 : 7;

					for (int i = 0; i < repeatLength; i++)
					{
						int acceleration = (((repeatBuffer.SampleData[repeatBuffer.StartOffset++] << 6) - sampleState) * cutoffCoef) >> 14;
						velocity = (short)(velocity + acceleration);
						sampleState = (short)(sampleState + velocity);
						output = (sbyte)(sampleState >> boost);

						filterBuffer.SampleData[filterBuffer.StartOffset++] = output;

						velocity = (short)((velocity * (short)dampingCoef) >> 15);
					}

					voiceInfo.FilterLastSample = output;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TransformPlay(VoiceInfo voiceInfo)
		{
			if (voiceInfo.Effects2.Effect.HasFlag(InstrumentEffect2.Transform))
			{
				CounterInfo counterInfo = voiceInfo.TransformCounter;

				uint repeatLength = voiceInfo.WaveSampleRepeatLength * 2U;
				ushort length = 0;

				if (repeatLength != 256)
				{
					length = 256;
					if (repeatLength != 128)
					{
						length = 256 + 128;
						if (repeatLength != 64)
						{
							length = 256 + 128 + 64;
							if (repeatLength != 32)
								length = 256 + 128 + 64 + 32;
						}
					}
				}

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformStep))
				{
					if (!voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformInit))
					{
						if (voiceInfo.PartNote == 0)
							goto NoCount;

						counterInfo.SaveCounter = counterInfo.Counter;
					}
					else
					{
						if (voiceInfo.PartNote != 0)
							counterInfo.SaveCounter = counterInfo.Counter;
					}
				}
				else
					counterInfo.SaveCounter = counterInfo.Counter;

				Counter(counterInfo);

				NoCount:
				ushort previousCounter = counterInfo.SaveCounter;

				if (voiceInfo.Effects1.Argument.HasFlag(InstrumentFlag.TransformStep))
				{
					if (counterInfo.Delay != 0)
						counterInfo.Delay--;
					else
					{
						short speed = (sbyte)voiceInfo.TransformSpeed;
						if (speed < 0)
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if ((short)counterInfo.SaveCounter < 0)
								counterInfo.SaveCounter = 0;
						}
						else
						{
							counterInfo.SaveCounter = (ushort)(counterInfo.SaveCounter + speed);
							if (counterInfo.SaveCounter > 510)
								counterInfo.SaveCounter = 510;
						}
					}
				}

				previousCounter >>= 1;
				short octaveSize = 256;

				short remainder = (short)previousCounter;
				ushort octaveIndex = 0;

				remainder -= octaveSize;
				if (remainder > 0)
				{
					octaveIndex++;
					previousCounter = (ushort)remainder;

					remainder -= octaveSize;
					if (remainder > 0)
					{
						octaveIndex++;
						previousCounter = (ushort)remainder;

						remainder -= octaveSize;
						if (remainder > 0)
						{
							octaveIndex++;
							previousCounter = (ushort)remainder;

							remainder -= octaveSize;
							if (remainder > 0)
							{
								octaveIndex++;
								previousCounter = (ushort)remainder;

								remainder -= octaveSize;
								if (remainder > 0)
								{
									octaveIndex++;
									previousCounter = (ushort)remainder;
								}
							}
						}
					}
				}

				int interpolationFactor = previousCounter;

				int sampleNumberIndex = octaveIndex;
				byte sampleNumber = voiceInfo.TransformWaveSampleNumbers[sampleNumberIndex++];
				if (sampleNumber == 0)
					return;

				SamplePointer sourceBuffer1 = samples[sampleNumber].SampleRepeatPointer!.Value;
				sourceBuffer1.StartOffset += length;

				if (octaveIndex == 0)
					sourceBuffer1 = voiceInfo.WaveSampleRepeatPointer!.Value;

				sampleNumber = voiceInfo.TransformWaveSampleNumbers[sampleNumberIndex];
				if ((sampleNumber == 0) || (sampleNumber >= numberOfSamples))	// TNE: Added last check for Fronky
					return;

				SamplePointer sourceBuffer2 = samples[sampleNumber].SampleRepeatPointer!.Value;
				sourceBuffer2.StartOffset += length;

				// TNE: Added check to prevent index of range exception. Needed for Preyravenous
				if (sourceBuffer2.StartOffset >= sourceBuffer2.SampleData.Length)
					return;

				SamplePointer transformBuffer = new SamplePointer(voiceInfo.TransformWaveBuffer);

				voiceInfo.WaveSampleRepeatPointer = transformBuffer;

				if (voiceInfo.Effects1.Effect.HasFlag(InstrumentEffect1.Loop) || (voiceInfo.WaveOrSample != SampleType.Sample))
					voiceInfo.WaveSamplePointer = transformBuffer;

				for (int i = 0; i < repeatLength; i++)
				{
					int sample1 = sourceBuffer1.SampleData[sourceBuffer1.StartOffset++];
					int sample2 = sourceBuffer2.SampleData[sourceBuffer2.StartOffset++];

					sbyte destSample = (sbyte)((((sample2 - sample1) * interpolationFactor) >> 8) + sample1);
					transformBuffer.SampleData[transformBuffer.StartOffset++] = destSample;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void OneWayCounter(CounterInfo counterInfo)
		{
			ushort counter = counterInfo.Counter;

			if (counterInfo.Step || (counterInfo.Delay == 0))
			{
				counter = (ushort)(counter + counterInfo.Speed);
				counter &= 0x1ff;
				counterInfo.Counter = counter;
			}
			else
				counterInfo.Delay--;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Counter(CounterInfo counterInfo)
		{
			short counter = (short)counterInfo.Counter;

			if (counterInfo.Step || (counterInfo.Delay == 0))
			{
				if (counterInfo.Turns < 0)
					return;

				if (counterInfo.Repeat < counterInfo.RepeatEnd)
				{
					if (counterInfo.Speed < 0)
					{
						counter += counterInfo.Speed;
						if (counter >= counterInfo.Repeat)
						{
							counterInfo.Counter = (ushort)counter;
							return;
						}
					}
					else
					{
						counter += counterInfo.Speed;
						if (counter <= counterInfo.RepeatEnd)
						{
							counterInfo.Counter = (ushort)counter;
							return;
						}
					}
				}
				else
				{
					if (counterInfo.Speed < 0)
					{
						counter += counterInfo.Speed;
						if (counter >= counterInfo.RepeatEnd)
						{
							counterInfo.Counter = (ushort)counter;
							return;
						}
					}
					else
					{
						counter += counterInfo.Speed;
						if (counter <= counterInfo.Repeat)
						{
							counterInfo.Counter = (ushort)counter;
							return;
						}
					}
				}

				if (counterInfo.Turns != 0)
				{
					counterInfo.Turns--;
					if (counterInfo.Turns == 0)
						counterInfo.Turns = -1;
				}

				counter -= counterInfo.Speed;
				counterInfo.Speed = (short)-counterInfo.Speed;

				counterInfo.Counter = (ushort)counter;
			}
			else
				counterInfo.Delay--;
		}
	}
}
