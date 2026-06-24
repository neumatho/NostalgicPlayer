/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Containers;
using Polycode.NostalgicPlayer.Ports.LibReSidFp.Exceptions;

namespace Polycode.NostalgicPlayer.Ports.LibReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	public class ReSidFp
	{
		private readonly Sid sid;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ReSidFp()
		{
			sid = new Sid();
		}



		/********************************************************************/
		/// <summary>
		/// Set chip model
		/// </summary>
		/********************************************************************/
		public bool SetChipModel(ChipModel model)
		{
			try
			{
				sid.SetChipModel(model);
				return true;
			}
			catch (SidErrorException)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Get currently emulated chip model
		/// </summary>
		/********************************************************************/
		public ChipModel GetChipModel()
		{
			return sid.GetChipModel();
		}



		/********************************************************************/
		/// <summary>
		/// Set combined waveforms strength
		/// </summary>
		/********************************************************************/
		public bool SetCombinedWaveforms(CombinedWaveforms cws)
		{
			try
			{
				sid.SetCombinedWaveforms(cws);
				return true;
			}
			catch (SidErrorException)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			sid.Reset();
		}



		/********************************************************************/
		/// <summary>
		/// 16-bit input (EXT IN). Write 16-bit sample to audio input.
		/// NB! The caller is responsible for keeping the value within 16
		/// bits. Note that to mix in an external audio signal the signal
		/// should be resampled to ~1MHz first
		/// </summary>
		/********************************************************************/
		public void Input(int value)
		{
			sid.Input(value);
		}



		/********************************************************************/
		/// <summary>
		/// Read registers without altering state
		/// </summary>
		/********************************************************************/
		public byte Peek(int offset)
		{
			return sid.Peek(offset);
		}



		/********************************************************************/
		/// <summary>
		/// Read registers.
		///
		/// Reading a write only register returns the last char written to
		/// any SID register.
		/// The individual bits in this value start to fade down towards zero
		/// after a few cycles.
		/// All bits reach zero within approximately $2000 - $4000 cycles on
		/// the 6581 and dar longer on the 8580.
		/// It has been claimed that this fading happens in an orderly
		/// fashion, however sampling of write only registers reveals that
		/// this is not the case.
		/// NOTE: This is not correctly modeled.
		/// The actual use of write only registers has largely been made in
		/// the belief that all SID registers are readable.
		/// To support this belief the read would have to be done immediately
		/// after a write to the same register (remember that an intermediate
		/// write to another register would yield that value instead).
		/// With this in mind we return the last value written to any SID
		/// register for a number of cycles dependent on the chip model
		/// without modeling the bit fading.
		/// </summary>
		/********************************************************************/
		public byte Read(int offset)
		{
			return sid.Read(offset);
		}



		/********************************************************************/
		/// <summary>
		/// Write registers
		/// </summary>
		/********************************************************************/
		public void Write(int offset, byte value)
		{
			sid.Write(offset, value);
		}



		/********************************************************************/
		/// <summary>
		/// Setting of SID sampling parameters.
		///
		/// Use a clock frequency of 985248Hz for PAL C64, 1022730Hz for
		/// NTSC C64. The default end of passband frequency is
		/// pass_freq = 0.9*sample_freq/2 for sample frequencies up to
		/// ~ 44.1kHz, and 20kHz for higher sample frequencies.
		///
		/// For resampling, the ratio between the clock frequency and the
		/// sample frequency is limited as follows:
		/// 125*clock_freq/sample_freq ‹ 16384
		/// E.g. provided a clock frequency of ~ 1MHz, the sample frequency
		/// can not be set lower than ~ 8kHz. A lower sample frequency would
		/// make the resampling code overfill its 16k sample ring buffer.
		///
		/// The end of passband frequency is also limited:
		/// pass_freq ‹= 0.9*sample_freq/2
		///
		/// E.g. for a 44.1kHz sampling rate the end of passband frequency
		/// is limited to slightly below 20kHz.
		/// This constraint ensures that the FIR table is not overfilled
		/// </summary>
		/********************************************************************/
		public bool SetSamplingParameters(double clockFrequency, SamplingMethod method, double samplingFrequency)
		{
			try
			{
				sid.SetSamplingParameters(clockFrequency, method, samplingFrequency);
				return true;
			}
			catch (SidErrorException)
			{
				return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clock SID forward producing audio using chosen output resampling
		/// algorithm.
		///
		/// Note:
		/// User must make sure to have enough space for the buffer.
		/// The number of samples produced can be approximated with this
		/// formula: ceil(cycles/clockFrequency*samplingFrequency)
		/// </summary>
		/********************************************************************/
		public int Clock(uint cycles, short[] buf, int bufPos)
		{
			return sid.Clock(cycles, buf, bufPos);
		}



		/********************************************************************/
		/// <summary>
		/// Clock SID forward producing audio using chosen output resampling
		/// algorithm
		/// </summary>
		/********************************************************************/
		public int Clock(short[] buf, int bufPos, int bufSize)
		{
			return sid.Clock(buf, bufPos, bufSize);
		}



		/********************************************************************/
		/// <summary>
		/// Clock SID forward with no audio production.
		/// Only the digital parts are emulated, the analog stage is ignored
		/// </summary>
		/********************************************************************/
		public void ClockDigital(uint cycles)
		{
			sid.ClockDigital(cycles);
		}



		/********************************************************************/
		/// <summary>
		/// Clock SID forward with no audio production.
		///
		/// Note:
		/// You can't mix this method of clocking with the audio-producing
		/// clock() because components that don't affect OSC3/ENV3 are not
		/// emulated
		/// </summary>
		/********************************************************************/
		public void ClockSilent(uint cycles)
		{
			sid.ClockSilent(cycles);
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter curve
		/// </summary>
		/********************************************************************/
		public void SetFilter6581Curve(double filterCurve)
		{
			sid.SetFilter6581Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Set 6581 filter offset and range
		/// </summary>
		/********************************************************************/
		public void SetFilter6581Range(double adjustment)
		{
			sid.SetFilter6581Range(adjustment);
		}



		/********************************************************************/
		/// <summary>
		/// Set filter curve type
		/// </summary>
		/********************************************************************/
		public void SetFilter8580Curve(double filterCurve)
		{
			sid.SetFilter8580Curve(filterCurve);
		}



		/********************************************************************/
		/// <summary>
		/// Enable filter emulation
		/// </summary>
		/********************************************************************/
		public void EnableFilter(bool enable)
		{
			sid.EnableFilter(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Enable/disable old caps for 6581 model.
		/// When enabled the filter cutoff is lower
		/// </summary>
		/********************************************************************/
		public void EnableOld6581Caps(bool enable)
		{
			sid.EnableOld6581Caps(enable);
		}



		/********************************************************************/
		/// <summary>
		/// Set paddle coordinates
		/// </summary>
		/********************************************************************/
		public void SetPaddle(byte x, byte y)
		{
			sid.SetPaddle(x, y);
		}



		/********************************************************************/
		/// <summary>
		/// Save current state.
		///
		/// Note: The save state is not portable across different builds and
		/// may change in future versions
		/// </summary>
		/********************************************************************/
		public IState SaveState()
		{
			return State.SaveState(sid);
		}



		/********************************************************************/
		/// <summary>
		/// Restore saved state
		/// </summary>
		/********************************************************************/
		public void RestoreState(IState state)
		{
			State.RestoreState(sid, state);
		}
	}
}
