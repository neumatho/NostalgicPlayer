/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.ReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class EnvelopeGenerator
	{
		// A 15 bit [LFSR] is used to implement the envelope rates, in effect dividing
		// the clock to the envelope counter by the currently selected rate period.
		//
		// In addition, another 5 bit counter is used to implement the exponential envelope decay,
		// in effect further dividing the clock to the envelope counter.
		// The period of this counter is set to 1, 2, 4, 8, 16, 30 at the envelope counter
		// values 255, 93, 54, 26, 14, 6, respectively.
		//
		// [LFSR]: https://en.wikipedia.org/wiki/Linear_feedback_shift_register

		/// <summary>
		/// The envelope state machine's distinct states. In addition to this,
		/// envelope has a hold mode, which freezes envelope counter to zero
		/// </summary>
		private enum State
		{
			ATTACK, DECAY_SUSTAIN, RELEASE
		}

		// Lookup table to convert from attack, decay, or release value to rate
		// counter period.
		//
		// The rate counter is a 15 bit register which is left shifted each cycle.
		// When the counter reaches a specific comparison value,
		// the envelope counter is incremented (attack) or decremented
		// (decay/release) and the rate counter is resetted.
		//
		// see [kevtris.org](http://blog.kevtris.org/?p=13)
		private static readonly uint[] adsrTable = new uint[16]
		{
			0x007f,
			0x3000,
			0x1e00,
			0x0660,
			0x0182,
			0x5573,
			0x000e,
			0x3805,
			0x2424,
			0x2220,
			0x090c,
			0x0ecd,
			0x010e,
			0x23f7,
			0x5237,
			0x64a8
		};

		/// <summary>
		/// XOR shift register for ADSR prescaling
		/// </summary>
		private uint lfsr = 0x7fff;

		/// <summary>
		/// Comparison value (period) of the rate counter before next event
		/// </summary>
		private uint rate = 0;

		/// <summary>
		/// During release mode, the SID approximates envelope decay via piecewise
		/// linear decay rate
		/// </summary>
		private uint exponential_counter = 0;

		// Comparison value (period) of the exponential decay counter before next decrement
		private uint exponential_counter_period = 1;
		private uint new_exponential_counter_period = 0;

		private uint state_pipeline = 0;

		private uint envelope_pipeline = 0;

		private uint exponential_pipeline = 0;

		// Current envelope state
		private State state = State.RELEASE;
		private State next_state = State.RELEASE;

		/// <summary>
		/// Whether counter is enabled. Only switching to ATTACK can release envelope
		/// </summary>
		internal bool counter_enabled = true;

		/// <summary>
		/// Gate bit
		/// </summary>
		private bool gate = false;

		private bool resetLfsr = false;

		/// <summary>
		/// The current digital value of envelope output
		/// </summary>
		internal byte envelope_counter = 0xaa;

		/// <summary>
		/// Attack register
		/// </summary>
		private byte attack = 0;

		/// <summary>
		/// Decay register
		/// </summary>
		private byte decay = 0;

		/// <summary>
		/// Sustain register
		/// </summary>
		private byte sustain = 0;

		/// <summary>
		/// Release register
		/// </summary>
		private byte release = 0;

		/// <summary>
		/// The ENV3 value, sampled at the first phase of the clock
		/// </summary>
		private byte env3 = 0;

		/********************************************************************/
		/// <summary>
		/// Get the Envelope Generator digital output
		/// </summary>
		/********************************************************************/
		public uint Output()
		{
			return envelope_counter;
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			// Counter is not changed on reset
			envelope_pipeline = 0;

			state_pipeline = 0;

			attack = 0;
			decay = 0;
			sustain = 0;
			release = 0;

			gate = false;

			resetLfsr = true;

			exponential_counter = 0;
			exponential_counter_period = 1;
			new_exponential_counter_period = 0;

			state = State.RELEASE;
			counter_enabled = true;
			rate = adsrTable[release];
		}



		/********************************************************************/
		/// <summary>
		/// Write control register
		/// </summary>
		/********************************************************************/
		public void WriteControl_Reg(byte control)
		{
			bool gate_next = (control & 0x01) != 0;

			if (gate_next != gate)
			{
				gate = gate_next;

				// The rate counter is never reset, thus there will be a delay before the
				// envelope counter starts counting up (attack) or down (release)
				if (gate_next)
				{
					// Gate bit on: Start attack, decay, sustain
					next_state = State.ATTACK;
					state_pipeline = 2;

					if (resetLfsr || (exponential_pipeline == 2))
						envelope_pipeline = (exponential_counter_period == 1) || (exponential_pipeline == 2) ? (uint)2 : 4;
					else if (exponential_pipeline == 1)
						state_pipeline = 3;
				}
				else
				{
					// Gate bit off: Start release
					next_state = State.RELEASE;
					state_pipeline = envelope_pipeline > 0 ? (uint)3 : 2;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Write attack/decay register
		/// </summary>
		/********************************************************************/
		public void WriteAttack_Decay(byte attack_decay)
		{
			attack = (byte)((attack_decay >> 4) & 0x0f);
			decay = (byte)(attack_decay & 0x0f);

			if (state == State.ATTACK)
				rate = adsrTable[attack];
			else if (state == State.DECAY_SUSTAIN)
				rate = adsrTable[decay];
		}



		/********************************************************************/
		/// <summary>
		/// Write sustain/release register
		/// </summary>
		/********************************************************************/
		public void WriteSustain_Release(byte sustain_release)
		{
			// From the sustain levels it follows that both the low and high 4 bits
			// of the envelope counter are compared to the 4-bit sustain value
			// This has been verified by sampling ENV3.
			//
			// For a detailed description see:
			// http://ploguechipsounds.blogspot.it/2010/11/new-research-on-sid-adsr.html
			sustain = (byte)((sustain_release & 0xf0) | ((sustain_release >> 4) & 0x0f));

			release = (byte)(sustain_release & 0x0f);

			if (state == State.RELEASE)
				rate = adsrTable[release];
		}



		/********************************************************************/
		/// <summary>
		/// Return the envelope current value
		/// </summary>
		/********************************************************************/
		public byte ReadEnv()
		{
			return env3;
		}



		/********************************************************************/
		/// <summary>
		/// SID clocking
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clock()
		{
			env3 = envelope_counter;

			if (new_exponential_counter_period > 0)
			{
				exponential_counter_period = new_exponential_counter_period;
				new_exponential_counter_period = 0;
			}

			if (state_pipeline != 0)
				State_Change();

			if ((envelope_pipeline != 0) && (--envelope_pipeline == 0))
			{
				if (counter_enabled)
				{
					if (state == State.ATTACK)
					{
						if (++envelope_counter == 0xff)
						{
							next_state = State.DECAY_SUSTAIN;
							state_pipeline = 3;
						}
					}
					else if ((state == State.DECAY_SUSTAIN) || (state == State.RELEASE))
					{
						if (--envelope_counter == 0x00)
							counter_enabled = false;
					}

					Set_Exponential_Counter();
				}
			}
			else if ((exponential_pipeline != 0) && (--exponential_pipeline == 0))
			{
				exponential_counter = 0;

				if (((state == State.DECAY_SUSTAIN) && (envelope_counter != sustain)) || (state == State.RELEASE))
				{
					// The envelope counter can flip from 0x00 to 0xff by changing state to
					// attack, then to release. The envelope counter will then continue
					// counting down in the release state.
					// This has been verified by sampling ENV3
					envelope_pipeline = 1;
				}
			}
			else if (resetLfsr)
			{
				lfsr = 0x7fff;
				resetLfsr = false;

				if (state == State.ATTACK)
				{
					// The first envelope step in the attack state also resets the exponential
					// counter. This has been verified by sampling ENV3
					exponential_counter = 0;	// NOTE this is actually delayed one cycle, not modeled

					// The envelope counter can flip from 0xff to 0x00 by changing state to
					// release, then to attack. The envelope counter is then frozen at
					// zero; to unlock this situation the state must be changed to release,
					// then to attack. This has been verified by sampling ENV3
					envelope_pipeline = 2;
				}
				else
				{
					if (counter_enabled && (++exponential_counter == exponential_counter_period))
						exponential_pipeline = exponential_counter_period != 1 ? (uint)2 : 1;
				}
			}

			// ADSR delay bug.
			// If the rate counter comparison value is set below the current value of the
			// rate counter, the counter will continue counting up until it wraps around
			// to zero at 2^15 = 0x8000, and then count rate_period - 1 before the
			// envelope can constly be stepped.
			// This has been verified by sampling ENV3.

			// Check to see if LFSR matches table value
			if (lfsr != rate)
			{
				// It wasn't a match, clock the LFSR once
				// by performing XOR on last 2 bits
				uint feedback = ((lfsr << 14) ^ (lfsr << 13)) & 0x4000;
				lfsr = (lfsr >> 1) | feedback;
			}
			else
				resetLfsr = true;
		}



		/********************************************************************/
		/// <summary>
		/// This is what happens on chip during state switching, based on die
		/// reverse engineering and transistor level emulation.
		///
		/// Attack
		///
		///  0 - Gate on
		///  1 - Counting direction changes
		///      During this cycle the decay rate is "accidentally" activated
		///  2 - Counter is being inverted
		///      Now the attack rate is correctly activated
		///      Counter is enabled
		///  3 - Counter will be counting upward from now on
		///
		/// Decay
		///
		///  0 - Counter == $ff
		///  1 - Counting direction changes
		///      The attack state is still active
		///  2 - Counter is being inverted
		///      During this cycle the decay state is activated
		///  3 - Counter will be counting downward from now on
		///
		/// Release
		///
		///  0 - Gate off
		///  1 - During this cycle the release state is activated if coming from sustain/decay
		/// *2 - Counter is being inverted, the release state is activated
		/// *3 - Counter will be counting downward from now on
		///
		///  (* only if coming directly from Attack state)
		///
		/// Freeze
		///
		///  0 - Counter == $00
		///  1 - Nothing
		///  2 - Counter is disabled
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void State_Change()
		{
			state_pipeline--;

			switch (next_state)
			{
				case State.ATTACK:
				{
					if (state_pipeline == 1)
					{
						// The decay rate is "accidentally" enabled during first cycle of attach phase
						rate = adsrTable[decay];
					}
					else if (state_pipeline == 0)
					{
						state = State.ATTACK;

						// The attack rate is correctly enabled during second cycle of attach phase
						rate = adsrTable[attack];
						counter_enabled = true;
					}
					break;
				}

				case State.DECAY_SUSTAIN:
				{
					if (state_pipeline == 0)
					{
						state = State.DECAY_SUSTAIN;
						rate = adsrTable[decay];
					}
					break;
				}

				case State.RELEASE:
				{
					if (((state == State.ATTACK) && (state_pipeline == 0)) || ((state == State.DECAY_SUSTAIN) && (state_pipeline == 1)))
					{
						state = State.RELEASE;
						rate = adsrTable[release];
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Set_Exponential_Counter()
		{
			// Check for change of exponential counter period.
			//
			// For a detailed description see:
			// http://ploguechipsounds.blogspot.it/2010/03/sid-6581r3-adsr-tables-up-close.html
			switch (envelope_counter)
			{
				case 0xff:
				case 0x00:
				{
					new_exponential_counter_period = 1;
					break;
				}

				case 0x5d:
				{
					new_exponential_counter_period = 2;
					break;
				}

				case 0x36:
				{
					new_exponential_counter_period = 4;
					break;
				}

				case 0x1a:
				{
					new_exponential_counter_period = 8;
					break;
				}

				case 0x0e:
				{
					new_exponential_counter_period = 16;
					break;
				}

				case 0x06:
				{
					new_exponential_counter_period = 30;
					break;
				}
			}
		}
	}
}
