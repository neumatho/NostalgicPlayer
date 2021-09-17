/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSid
{
	/// <summary>
	/// Envelope generator
	/// </summary>
	internal class EnvelopeGenerator
	{
		private enum State
		{
			Attack,
			DecaySustain,
			Release
		}

		private uint rateCounter;
		private uint ratePeriod;
		private uint exponentialCounter;
		private uint exponentialCounterPeriod;
		private uint envelopeCounter;
		private bool holdZero;

		private uint attack;
		private uint decay;
		private uint sustain;
		private uint release;

		private uint gate;

		private State state;

		// Rate counter periods are calculated from the Envelope Rates table in
		// the Programmer's Reference Guide. The rate counter period is the number of
		// cycles between each increment of the envelope counter.
		// The rates have been verified by sampling ENV3. 
		//
		// The rate counter is a 16 bit register which is incremented each cycle.
		// When the counter reaches a specific comparison value, the envelope counter
		// is incremented (attack) or decremented (decay/release) and the
		// counter is zeroed.
		//
		// NB! Sampling ENV3 shows that the calculated values are not exact.
		// It may seem like most calculated values have been rounded (.5 is rounded
		// down) and 1 has been added to the result. A possible explanation for this
		// is that the SID designers have used the calculated values directly
		// as rate counter comparison values, not considering a one cycle delay to
		// zero the counter. This would yield an actual period of comparison value + 1.
		//
		// The time of the first envelope count can not be exactly controlled, except
		// possibly by resetting the chip. Because of this we cannot do cycle exact
		// sampling and must devise another method to calculate the rate counter
		// periods.
		//
		// The exact rate counter periods can be determined e.g. by counting the number
		// of cycles from envelope level 1 to envelope level 129, and dividing the
		// number of cycles by 128. CIA1 timer A and B in linked mode can perform
		// the cycle count. This is the method used to find the rates below.
		//
		// To avoid the ADSR delay bug, sampling of ENV3 should be done using
		// sustain = release = 0. This ensures that the attack state will not lower
		// the current rate counter period.
		//
		// The ENV3 sampling code below yields a maximum timing error of 14 cycles.
		//     lda #$01
		// l1: cmp $d41c
		//     bne l1
		//     ...
		//     lda #$ff
		// l2: cmp $d41c
		//     bne l2
		//
		// This yields a maximum error for the calculated rate period of 14/128 cycles.
		// The described method is thus sufficient for exact calculation of the rate
		// periods
		//
		private static readonly uint[] rateCounterPeriod =
		{
			    9,  //   2ms*1.0MHz/256 =     7.81
			   32,  //   8ms*1.0MHz/256 =    31.25
			   63,  //  16ms*1.0MHz/256 =    62.50
			   95,  //  24ms*1.0MHz/256 =    93.75
			  149,  //  38ms*1.0MHz/256 =   148.44
			  220,  //  56ms*1.0MHz/256 =   218.75
			  267,  //  68ms*1.0MHz/256 =   265.63
			  313,  //  80ms*1.0MHz/256 =   312.50
			  392,  // 100ms*1.0MHz/256 =   390.63
			  977,  // 250ms*1.0MHz/256 =   976.56
			 1954,  // 500ms*1.0MHz/256 =  1953.13
			 3126,  // 800ms*1.0MHz/256 =  3125.00
			 3907,  //   1 s*1.0MHz/256 =  3906.25
			11720,  //   3 s*1.0MHz/256 = 11718.75
			19532,  //   5 s*1.0MHz/256 = 19531.25
			31251   //   8 s*1.0MHz/256 = 31250.00
		};

		// For decay and release, the clock to the envelope counter is sequentially
		// divided by 1, 2, 4, 8, 16, 30, 1 to create a piece-wise linear approximation
		// of an exponential. The exponential counter period is loaded at the envelope
		// counter values 255, 93, 54, 26, 14, 6, 0. The period can be different for the
		// same envelope counter value, depending on whether the envelope has been
		// rising (attack -> release) or sinking (decay/release).
		//
		// Since it is not possible to reset the rate counter (the test bit has no
		// influence on the envelope generator whatsoever) a method must be devised to
		// do cycle exact sampling of ENV3 to do the investigation. This is possible
		// with knowledge of the rate period for A=0, found above.
		//
		// The CPU can be synchronized with ENV3 by first synchronizing with the rate
		// counter by setting A=0 and wait in a carefully timed loop for the envelope
		// counter _not_ to change for 9 cycles. We can then wait for a specific value
		// of ENV3 with another timed loop to fully synchronize with ENV3.
		//
		// At the first period when an exponential counter period larger than one
		// is used (decay or release), one extra cycle is spent before the envelope is
		// decremented. The envelope output is then delayed one cycle until the state
		// is changed to attack. Now one cycle less will be spent before the envelope
		// is incremented, and the situation is normalized.
		// The delay is probably caused by the comparison with the exponential counter,
		// and does not seem to affect the rate counter. This has been verified by
		// timing 256 consecutive complete envelopes with A = D = R = 1, S = 0, using
		// CIA1 timer A and B in linked mode. If the rate counter is not affected the
		// period of each complete envelope is
		// (255 + 162*1 + 39*2 + 28*4 + 12*8 + 8*16 + 6*30)*32 = 756*32 = 32352
		// which corresponds exactly to the timed value divided by the number of
		// complete envelopes.
		// NB! This one cycle delay is not modeled.


		// From the sustain levels it follows that both the low and high 4 bits of the
		// envelope counter are compared to the 4-bit sustain value.
		// This has been verified by sampling ENV3
		//
		private static readonly uint[] sustainLevel =
		{
			0x00,
			0x11,
			0x22,
			0x33,
			0x44,
			0x55,
			0x66,
			0x77,
			0x88,
			0x99,
			0xaa,
			0xbb,
			0xcc,
			0xdd,
			0xee,
			0xff,
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EnvelopeGenerator()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			envelopeCounter = 0;

			attack = 0;
			decay = 0;
			sustain = 0;
			release = 0;

			gate = 0;

			rateCounter = 0;
			exponentialCounter = 0;
			exponentialCounterPeriod = 1;

			state = State.Release;
			ratePeriod = rateCounterPeriod[release];
			holdZero = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint ReadEnv()
		{
			return Output();
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - 1 cycle
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock()
		{
			// Check for ADSR delay bug.
			// If the rate counter comparison value is set below the current value of the
			// rate counter, the counter will continue counting up until it wraps around
			// to zero at 2^15 = 0x8000, and then count ratePeriod - 1 before the
			// envelope can finally be stepped.
			// This has been verified by sampling ENV3
			if ((++rateCounter & 0x8000) != 0)
			{
				++rateCounter;
				rateCounter &= 0x7fff;
			}

			if (rateCounter != ratePeriod)
				return;

			rateCounter = 0;

			// The first envelope step in the attack state also resets the exponential
			// counter. This has been verified by sampling ENV3
			if ((state == State.Attack) || (++exponentialCounter == exponentialCounterPeriod))
			{
				exponentialCounter = 0;

				// Check whether the envelope counter is frozen at zero
				if (holdZero)
					return;

				switch (state)
				{
					case State.Attack:
					{
						// The envelope counter can flip from 0xff to 0x00 by changing state to
						// release, then to attack. The envelope counter is then frozen at
						// zero; to unlock this situation the state must be changed to release,
						// then to attack. This has been verified by sampling ENV3
						++envelopeCounter;
						envelopeCounter &= 0xff;

						if (envelopeCounter == 0xff)
						{
							state = State.DecaySustain;
							ratePeriod = rateCounterPeriod[decay];
						}
						break;
					}

					case State.DecaySustain:
					{
						if (envelopeCounter != sustainLevel[sustain])
							--envelopeCounter;

						break;
					}

					case State.Release:
					{
						// The envelope counter can flip from 0x00 to 0xff by changing state to
						// attack, then to release. The envelope counter will then continue
						// counting down in the release state.
						// This has been verified by sampling ENV3.
						// NB! The operation below requires two's complement integer
						--envelopeCounter;
						envelopeCounter &= 0xff;
						break;
					}
				}

				// Check for change of exponential counter period
				switch (envelopeCounter)
				{
					case 0xff:
					{
						exponentialCounterPeriod = 1;
						break;
					}

					case 0x5d:
					{
						exponentialCounterPeriod = 2;
						break;
					}

					case 0x36:
					{
						exponentialCounterPeriod = 4;
						break;
					}

					case 0x1a:
					{
						exponentialCounterPeriod = 8;
						break;
					}

					case 0x0e:
					{
						exponentialCounterPeriod = 16;
						break;
					}

					case 0x06:
					{
						exponentialCounterPeriod = 30;
						break;
					}

					case 0x00:
					{
						exponentialCounterPeriod = 1;

						// When the envelope counter is changed to zero, it is frozen at zero.
						// This has been verified by sampling ENV3
						holdZero = true;
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking - delta cycles
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock(int delta)
		{
			// Check for ADSR delay bug.
			// If the rate counter comparison value is set below the current value of the
			// rate counter, the counter will continue counting up until it wraps around
			// to zero at 2^15 = 0x8000, and then count ratePeriod - 1 before the
			// envelope can finally be stepped.
			// This has been verified by sampling ENV3

			// NB! The operation below requires two's complement integer
			int rateStep = (int)(ratePeriod - rateCounter);
			if (rateStep <= 0)
				rateStep += 0x7fff;

			while (delta != 0)
			{
				if (delta < rateStep)
				{
					rateCounter = (uint)(rateCounter + delta);

					if ((rateCounter & 0x8000) != 0)
					{
						++rateCounter;
						rateCounter &= 0x7fff;
					}

					return;
				}

				rateCounter = 0;
				delta -= rateStep;

				// The first envelope step in the attack state also resets the exponential
				// counter. This has been verified by sampling ENV3
				if ((state == State.Attack) || (++exponentialCounter == exponentialCounterPeriod))
				{
					exponentialCounter = 0;

					// Check whether the envelope counter is frozen at zero
					if (holdZero)
					{
						rateStep = (int)ratePeriod;
						continue;
					}

					switch (state)
					{
						case State.Attack:
						{
							// The envelope counter can flip from 0xff to 0x00 by changing state to
							// release, then to attack. The envelope counter is then frozen at
							// zero; to unlock this situation the state must be changed to release,
							// then to attack. This has been verified by sampling ENV3
							++envelopeCounter;
							envelopeCounter &= 0xff;

							if (envelopeCounter == 0xff)
							{
								state = State.DecaySustain;
								ratePeriod = rateCounterPeriod[decay];
							}
							break;
						}

						case State.DecaySustain:
						{
							if (envelopeCounter != sustainLevel[sustain])
								--envelopeCounter;

							break;
						}

						case State.Release:
						{
							// The envelope counter can flip from 0x00 to 0xff by changing state to
							// attack, then to release. The envelope counter will then continue
							// counting down in the release state.
							// This has been verified by sampling ENV3.
							// NB! The operation below requires two's complement integer
							--envelopeCounter;
							envelopeCounter &= 0xff;
							break;
						}
					}

					// Check for change of exponential counter period
					switch (envelopeCounter)
					{
						case 0xff:
						{
							exponentialCounterPeriod = 1;
							break;
						}

						case 0x5d:
						{
							exponentialCounterPeriod = 2;
							break;
						}

						case 0x36:
						{
							exponentialCounterPeriod = 4;
							break;
						}

						case 0x1a:
						{
							exponentialCounterPeriod = 8;
							break;
						}

						case 0x0e:
						{
							exponentialCounterPeriod = 16;
							break;
						}

						case 0x06:
						{
							exponentialCounterPeriod = 30;
							break;
						}

						case 0x00:
						{
							exponentialCounterPeriod = 1;

							// When the envelope counter is changed to zero, it is frozen at zero.
							// This has been verified by sampling ENV3
							holdZero = true;
							break;
						}
					}
				}

				rateStep = (int)ratePeriod;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Read the envelope generator output
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint Output()
		{
			return envelopeCounter;
		}

		#region Register methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteControlReg(uint control)
		{
			uint gateNext = control & 0x01;

			// The rate counter is never reset, thus there will be a delay before the
			// envelope counter starts counting up (attack) or down (release)

			// Gate bit on: Start attack, decay, sustain
			if ((gate == 0) && (gateNext != 0))
			{
				state = State.Attack;
				ratePeriod = rateCounterPeriod[attack];

				// Switching to attack state unlocks the zero freeze
				holdZero = false;
			}
			else
			{
				// Gate bit off: Start release
				if ((gate != 0) && (gateNext == 0))
				{
					state = State.Release;
					ratePeriod = rateCounterPeriod[release];
				}
			}

			gate = gateNext;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteAttackDecay(uint attackDecay)
		{
			attack = (attackDecay >> 4) & 0x0f;
			decay = attackDecay & 0x0f;

			if (state == State.Attack)
				ratePeriod = rateCounterPeriod[attack];
			else if (state == State.DecaySustain)
				ratePeriod = rateCounterPeriod[decay];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void WriteSustainRelease(uint sustainRelease)
		{
			sustain = (sustainRelease >> 4) & 0x0f;
			release = sustainRelease & 0x0f;

			if (state == State.Release)
				ratePeriod = rateCounterPeriod[release];
		}
		#endregion
	}
}
