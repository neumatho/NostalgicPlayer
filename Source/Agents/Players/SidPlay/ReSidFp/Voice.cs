/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
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
		/// Amplitude modulated waveform output.
		///
		/// The waveform DAC generates a voltage between 5 and 12 V
		/// (4,76 - 9 V for the 8580) corresponding to oscillator state 0 .. 4095.
		///
		/// The envelope DAC generates a voltage between waveform gen output and
		/// the 5V level, corresponding to envelope state 0 .. 255.
		///
		/// Ideal range [-2048*255, 2047*255]
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Output(WaveformGenerator ringModulator)
		{
			return (int)(waveformGenerator.Output(ringModulator) * envelopeGenerator.Output());
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
