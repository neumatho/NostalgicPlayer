/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundDsp.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class SwRvbRefDelay
	{
		/// <summary>
		/// 
		/// </summary>
		public uint32 nDelayPos;

		/// <summary>
		/// 
		/// </summary>
		public uint32 nPreDifPos;

		/// <summary>
		/// 
		/// </summary>
		public uint32 nRefOutPos;

		/// <summary>
		/// Reflections linear master gain
		/// </summary>
		public int32 lMasterGain;

		/// <summary>
		/// Room low-pass coefficients
		/// </summary>
		public LR16 nCoeffs;

		/// <summary>
		/// Room low-pass history
		/// </summary>
		public LR16 History;

		/// <summary>
		/// Prediffusion coefficients
		/// </summary>
		public LR16 nPreDifCoeffs;

		/// <summary>
		/// Master reflections gain
		/// </summary>
		public LR16 ReflectionsGain;

		/// <summary>
		/// Up to 8 SW Reflections
		/// </summary>
		public readonly SwRvbReflection[] Reflections = ArrayHelper.InitializeArray<SwRvbReflection>(8);

		/// <summary>
		/// Reflections delay buffer
		/// </summary>
		public readonly LR16[] RefDelayBuffer = new LR16[CReverb.SndMix_Reflections_Delay_Mask + 1];

		/// <summary>
		/// Pre-diffusion
		/// </summary>
		public readonly LR16[] PreDifBuffer = new LR16[CReverb.SndMix_PreDiffusion_Delay_Mask + 1];

		/// <summary>
		/// Stereo output of reflections
		/// </summary>
		public readonly LR16[] RefOut = new LR16[CReverb.SndMix_Reverb_Delay_Mask + 1];
	}
}
