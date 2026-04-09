/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// 
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessModulation(VoiceVars voice)
		{
			if (voice.EffectsMode > 0)
			{
				AddBegin(voice);
				Sid(voice);
				Vibrato(voice);
				Portamento(voice);
				Envelope(voice);
			}
			else if (voice.EffectsMode == 0)
				voice.EffectsMode = 1;

			RandomPlay(voice);
			FadeApply(voice);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AddBegin(VoiceVars voice)
		{
			if (variant.NoAddBeginCount || (voice.AddBeginCount == 0))
				return;

			voice.Sample.Start = (uint)(voice.Sample.Start + voice.AddBeginOffset);

			if (voice.Sample.Start < offsets.SampleData)
			{
				// Found only a single file that underflows by 0x10
				voice.Sample.Start = offsets.SampleData;
			}

			if (voice.Sid.TargetLength != 0)
				voice.Sid.SourceOffset = voice.Sample.Start;
			else
				ToPaulaStart(voice, voice.Sample.Start);

			if (--voice.AddBeginCount == 0)
			{
				voice.AddBeginCount = voice.AddBeginArg;
				voice.AddBeginOffset *= -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Envelope(VoiceVars voice)
		{
			if (voice.Envelope.Flag == 0)
				return;

			if (voice.Envelope.Count > 0)
			{
				voice.Envelope.Count--;
				return;
			}

			voice.Envelope.Count = voice.Envelope.Flag;

			if (voice.Volume < voice.Envelope.Target)	// Up
			{
				voice.Volume = (sbyte)(voice.Volume + voice.Envelope.Speed);

				if (voice.Volume >= voice.Envelope.Target)
				{
					voice.Volume = (sbyte)voice.Envelope.Target;
					voice.Envelope.Flag = 0;
				}
			}
			else	// Down
			{
				voice.Volume = (sbyte)(voice.Volume - voice.Envelope.Speed);

				if (voice.Volume <= voice.Envelope.Target)
				{
					voice.Volume = (sbyte)voice.Envelope.Target;
					voice.Envelope.Flag = 0;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Vibrato with relative adjustment depending on period
		/// </summary>
		/********************************************************************/
		private void Vibrato(VoiceVars voice)
		{
			if (voice.Vibrato.Time == 0)
				return;

			voice.Vibrato.Delta += voice.Vibrato.Intensity;
			uword p;

			if (variant.VibratoUnscaled)
				p = (uword)(voice.Period + voice.Vibrato.Delta);
			else
				p = (uword)(((0x800 + voice.Vibrato.Delta) * voice.Period) >> 11);

			if (voice.Portamento.Speed == 0)
				voice.OutputPeriod = p;

			if (--voice.Vibrato.Count == 0)
			{
				voice.Vibrato.Count = voice.Vibrato.Time;
				voice.Vibrato.Intensity *= -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Portamento with relative adjustment depending on period
		/// </summary>
		/********************************************************************/
		private void Portamento(VoiceVars voice)
		{
			if ((voice.Portamento.Speed == 0) || (--voice.Portamento.Wait != 0))
				return;

			voice.Portamento.Wait = voice.Portamento.Count;

			uword current = voice.Portamento.Period;
			uword target = voice.Period;		// Target period

			if (current == target)
				goto End;
			else if (current < target)
			{
				// Down, increase period
				if (variant.PortaUnscaled)
					current += voice.Portamento.Speed;
				else
					current = (uword)(((0x100 + voice.Portamento.Speed) * current) >> 8);

				if (current < target)
					goto Set;
				else
					goto End;
			}
			else
			{
				// Up, decrease period
				if (variant.PortaUnscaled)
					current -= voice.Portamento.Speed;

				current = (uword)(((0x100 - voice.Portamento.Speed) * current) >> 8);

				if (current > target)
					goto Set;
				else
					goto End;
			}

//			return;

			End:
			voice.Portamento.Speed = 0;
			current = target;

			Set:
			current &= 0x7ff;
			voice.Portamento.Period = current;
			voice.OutputPeriod = current;
		}



		/********************************************************************/
		/// <summary>
		/// Real-time synthesizer based on interpolating or sampling values
		/// from a varying source sample area
		/// </summary>
		/********************************************************************/
		private void Sid(VoiceVars voice)
		{
			if (voice.Sid.TargetLength == 0)
				return;

			CPointer<ubyte> pTarget = pBuf + offsets.SampleData + voice.Sid.TargetOffset;
			udword currSourceOffset = 0;
			udword x = voice.Sid.Op3.Offset;
			ubyte d = (ubyte)(voice.Sid.Op1.InterDelta >> 8);
			ubyte dx = 0;
			sword cur = voice.Sid.LastSample;

			for (c_int i = voice.Sid.TargetLength; i >= 0; i--)
			{
				x += voice.Sid.Op2.Offset;
				currSourceOffset += x;

				// Target sample
				sword s = (sbyte)pBuf[voice.Sid.SourceOffset + ((currSourceOffset >> 16) & voice.Sid.SourceLength)];

				if ((d == 0) || (s == cur))		// Direct sampling mode || target reached
					pTarget[0, 1] = (ubyte)s;
				else if (s > cur)				// Target higher
				{
					cur += (sword)(d + dx);
					dx = (ubyte)(cur > 0x7f ? 1 : 0);

					if ((cur > 0x7f) || (cur >= s))	// Overflow or reached
					{
						cur = s;
						pTarget[0, 1] = (ubyte)s;
					}
					else	// s > cur
						pTarget[0, 1] = (ubyte)cur;
				}
				else if (s < cur)				// Target lower
				{
					cur -= (sword)(d + dx);
					dx = (ubyte)(cur < -128 ? 1 : 0);

					if ((cur < -128) || (cur <= s))	// Underflow or reached
					{
						cur = s;
						pTarget[0, 1] = (ubyte)s;
					}
					else	// cur > s
						pTarget[0, 1] = (ubyte)cur;

				}
			}

			voice.Sid.LastSample = (sbyte)cur;

			if (d != 0)
			{
				voice.Sid.Op1.InterDelta = (uword)(voice.Sid.Op1.InterDelta + voice.Sid.Op1.InterMod);

				if (--voice.Sid.Op1.Count == 0)
				{
					voice.Sid.Op1.Count = voice.Sid.Op1.Speed;
					voice.Sid.Op1.InterMod *= -1;
				}
			}

			voice.Sid.Op3.Offset = (uint)(voice.Sid.Op3.Offset + voice.Sid.Op3.Delta);

			if (--voice.Sid.Op3.Count == 0)
			{
				voice.Sid.Op3.Count = voice.Sid.Op3.Speed;
				if (voice.Sid.Op3.Count != 0)
					voice.Sid.Op3.Delta *= -1;
			}

			voice.Sid.Op2.Offset = (uint)(voice.Sid.Op2.Offset + voice.Sid.Op2.Delta);

			if (--voice.Sid.Op2.Count == 0)
			{
				voice.Sid.Op2.Count = voice.Sid.Op2.Speed;
				if (voice.Sid.Op2.Count != 0)
					voice.Sid.Op2.Delta *= -1;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Master volume with target modes "set, up, down"
		/// </summary>
		/********************************************************************/
		private void FadeInit(ubyte target, ubyte speed)
		{
			if (playerInfo.Fade.Active)
				return;

			playerInfo.Fade.Active = true;
			playerInfo.Fade.Target = target;
			playerInfo.Fade.Count = playerInfo.Fade.Speed = speed;

			// With speed=0 target volume can be set directly
			if ((playerInfo.Fade.Speed == 0) || (playerInfo.Fade.Volume == playerInfo.Fade.Target))
			{
				playerInfo.Fade.Volume = playerInfo.Fade.Target;
				playerInfo.Fade.Delta = 0;
				playerInfo.Fade.Active = false;
				return;
			}

			if (playerInfo.Fade.Volume < playerInfo.Fade.Target)
				playerInfo.Fade.Delta = 1;
			else
				playerInfo.Fade.Delta = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FadeApply(VoiceVars voice)
		{
			PaulaVoice paulaVoice = paulaVoices[voice.VoiceNum];

			if ((playerInfo.Fade.Delta != 0) && (--playerInfo.Fade.Count == 0))
			{
				playerInfo.Fade.Count = playerInfo.Fade.Speed;
				playerInfo.Fade.Volume = (ubyte)(playerInfo.Fade.Volume + playerInfo.Fade.Delta);

				if (playerInfo.Fade.Volume == playerInfo.Fade.Target)
				{
					playerInfo.Fade.Delta = 0;
					playerInfo.Fade.Active = false;
				}
			}

			sbyte vol = voice.Volume;

			if (playerInfo.Fade.Volume < 64)
				vol = (sbyte)((4 * voice.Volume * playerInfo.Fade.Volume) / (4 * 0x40));

			paulaVoice.Paula.Volume = (uword)vol;
		}



		/********************************************************************/
		/// <summary>
		/// Runtime effect support for two very rarely used macros commands.
		/// $1b RandomPlay + $1e RandomMask resp. RandomLimit.
		///
		/// There are multiple implementations, which are incompatible with
		/// eachother. They are not needed, since no module uses them
		/// </summary>
		/********************************************************************/
		private void RandomPlay(VoiceVars voice)
		{
			if (voice.Rnd.Flag == 0)
				return;
			else if (voice.Rnd.Flag > 0)
			{
				// The specified macro is abused as arpeggiator input array
				voice.Rnd.Arp.Offset = GetMacroOffset(voice.Rnd.Macro);
				voice.Rnd.Arp.Pos = 0;
				voice.Rnd.Flag = -1;

				if ((voice.Rnd.Mode & 1) != 0)
				{
					// Mode bit 0 set
					RandomPlayMask(voice);
				}
			}

			if (--voice.Rnd.Count == 0)
			{
				voice.Rnd.Count = voice.Rnd.Speed;

				do
				{
					ubyte aa = pBuf[voice.Rnd.Arp.Offset + voice.Rnd.Arp.Pos];
					playerInfo.Cmd.Aa = aa;

					if (aa != 0)
						break;

					if (voice.Rnd.Arp.Pos == 0)
						return;

					voice.Rnd.Arp.Pos = 0;
				}
				while (true);
			}
			else
			{
				RandomPlayReverb(voice);
				return;
			}

			uword n = (uword)(((sbyte)playerInfo.Cmd.Aa + voice.Note) & 0x3f);
			if (n == 0)
			{
				RandomPlayMask(voice);
				return;
			}

			uword p = NoteToPeriod(n);
			sword finetune = voice.Detune;

			if (finetune != 0)
				p = (uword)(((finetune + 0x100) * p) >> 8);

			if ((voice.Rnd.Mode & 1) == 0)	// Mode bit 0
			{
				voice.Period = p;

				if (voice.Portamento.Speed != 0)
					return;

				voice.OutputPeriod = p;

				RandomPlayCheckWait(voice);

				voice.Rnd.Arp.Pos++;
				return;
			}

			// Mode bit 0 set
			Randomize();

			if (((voice.Rnd.Mode & 4) != 0) || ((voice.Rnd.Arp.Pos & 3) != 0) || ((playerInfo.Admin.RandomWord & 0xff) > 16))
			{
				// Mode bit 2 set
				RandomPlayCheckWait(voice);

				voice.Period = p;

				if (voice.Portamento.Speed == 0)
					voice.OutputPeriod = p;
			}

			voice.Rnd.Arp.Pos++;

			if ((playerInfo.Cmd.Aa & 0x40) == 0)
				return;

			Randomize();

			if ((playerInfo.Admin.RandomWord >> 8) > 6)
			{
				RandomPlayMask(voice);
				return;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RandomPlayReverb(VoiceVars voice)
		{
			if (((voice.Rnd.Mode & 2) == 0) || (((voice.Rnd.Speed * 3) / 8) != voice.Rnd.Count))
				return;

			// Next voice, and loop from 3 to 0
			VoiceVars voice2 = voiceVars[(voice.VoiceNum + 1) & 3];
			voice2.Volume = (sbyte)((voice.Volume * 5) / 8);

			if (voice.Period != voice2.Period)
			{
				voice2.Period = voice.Period;
				voice2.OutputPeriod = voice.Period;

				RandomPlayCheckWait(voice);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RandomPlayMask(VoiceVars voice)
		{
			Randomize();

			voice.Rnd.Arp.Pos = (uword)((playerInfo.Admin.RandomWord & 0xff) & voice.Rnd.Mask);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void RandomPlayCheckWait(VoiceVars voice)
		{
			if ((playerInfo.Cmd.Aa & 0x80) != 0)
				voice.Rnd.BlockWait = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Randomize()
		{
			// The RNG implementation varies in publicly released TFMX player
			// object code such as TFMX Professional. Usually it retrieves
			// the vertical raster position from DFF006/VHPOSR and modifies
			// it in varying ways
			playerInfo.Admin.RandomWord = (ushort)((playerInfo.Admin.RandomWord ^ RandomGenerator.GetRandomNumber()) + 0x57294335);
		}
	}
}
