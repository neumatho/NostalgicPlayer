/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SwLateReverb
	{
		/// <summary>
		/// Reverb delay (in samples)
		/// </summary>
		public uint32 nReverbDelay;

		/// <summary>
		/// Delay line position
		/// </summary>
		public uint32 nDelayPos;

		/// <summary>
		/// Reverb diffusion
		/// </summary>
		public readonly LR16[] nDifCoeffs = new LR16[2];

		/// <summary>
		/// Reverb DC decay
		/// </summary>
		public readonly LR16[] nDecayDC = new LR16[2];

		/// <summary>
		/// Reverb HF decay
		/// </summary>
		public readonly LR16[] nDecayLP = new LR16[2];

		/// <summary>
		/// Low-pass history
		/// </summary>
		public readonly LR16[] LPHistory = new LR16[2];

		/// <summary>
		/// 2nd diffuser input gains
		/// </summary>
		public readonly LR16[] Dif2InGains = new LR16[2];

		/// <summary>
		/// 4x2 Reverb output gains
		/// </summary>
		public readonly LR16[] RvbOutGains = new LR16[2];

		/// <summary>
		/// Late reverb master gain
		/// </summary>
		public int32 lMasterGain;

		/// <summary>
		/// 
		/// </summary>
		public int32 lDummyAlign;

		// Tank delay lines

		/// <summary>
		/// {dif1_l, dif1_r}
		/// </summary>
		public readonly LR16[] Diffusion1 = new LR16[CReverb.RvbDly_Mask + 1];

		/// <summary>
		/// {dif2_l, dif2_r}
		/// </summary>
		public readonly LR16[] Diffusion2 = new LR16[CReverb.RvbDly_Mask + 1];

		/// <summary>
		/// {dly1_l, dly1_r}
		/// </summary>
		public readonly LR16[] Delay1 = new LR16[CReverb.RvbDly_Mask + 1];

		/// <summary>
		/// {dly2_l, dly2_r}
		/// </summary>
		public readonly LR16[] Delay2 = new LR16[CReverb.RvbDly_Mask + 1];
	}
}
