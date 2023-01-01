/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// Representation of SID voice block
	/// </summary>
	internal class Voice
	{
		private readonly WaveformGenerator waveformGenerator;

		private readonly EnvelopeGenerator envelopeGenerator;

		/// The DAC LUT for analog waveform output
		private float[] wavDac;		// This is initialized in the SID constructor

		/// The DAC LUT for analog envelope output
		private float[] envDac;		// This is initialized in the SID constructor

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Voice()
		{
			waveformGenerator = new WaveformGenerator();
			envelopeGenerator = new EnvelopeGenerator();
		}



		/********************************************************************/
		/// <summary>
		/// Set the analog DAC emulation for waveform generator. Must be
		/// called before any operation
		/// </summary>
		/********************************************************************/
		public void SetWavDac(float[] dac)
		{
			wavDac = dac;
		}



		/********************************************************************/
		/// <summary>
		/// Set the analog DAC emulation for envelope. Must be called before
		/// any operation
		/// </summary>
		/********************************************************************/
		public void SetEnvDac(float[] dac)
		{
			envDac = dac;
		}



		/********************************************************************/
		/// <summary>
		/// Amplitude modulated waveform output.
		///
		/// The waveform DAC generates a voltage between virtual ground and
		/// Vdd (5-12 V for the 6581 and 4,75-9 V for the 8580) corresponding
		/// to oscillator state 0 .. 4095.
		///
		/// The envelope DAC generates a voltage between waveform gen output
		/// and the virtual ground level, corresponding to envelope state
		/// 0 .. 255.
		///
		/// Ideal range [-2048*255, 2047*255]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Output(WaveformGenerator ringModulator)
		{
			uint wav = waveformGenerator.Output(ringModulator);
			uint env = envelopeGenerator.Output();

			// DAC imperfections are emulated by using the digital output
			// as an index into a DAC lookup table
			return (int)(wavDac[wav] * envDac[env]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public WaveformGenerator Wave()
		{
			return waveformGenerator;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public EnvelopeGenerator Envelope()
		{
			return envelopeGenerator;
		}



		/********************************************************************/
		/// <summary>
		/// Write control register
		/// </summary>
		/********************************************************************/
		public void WriteControl_Reg(byte control)
		{
			waveformGenerator.WriteControl_Reg(control);
			envelopeGenerator.WriteControl_Reg(control);
		}



		/********************************************************************/
		/// <summary>
		/// SID reset
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			waveformGenerator.Reset();
			envelopeGenerator.Reset();
		}
	}
}
