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
	internal class EnvironmentReverb
	{
		/// <summary>
		/// Late reverb gain (mB)
		/// </summary>
		public int32 ReverbLevel;

		/// <summary>
		/// Master reflections gain (mB)
		/// </summary>
		public int32 ReflectionsLevel;

		/// <summary>
		/// Room gain HF (mB)
		/// </summary>
		public int32 RoomHF;

		/// <summary>
		/// Reverb tank decay (0-7fff scale)
		/// </summary>
		public uint32 ReverbDecay;

		/// <summary>
		/// Reverb pre-diffusion amount (+/- 32K scale)
		/// </summary>
		public int32 PreDiffusion;

		/// <summary>
		/// Reverb tank diffusion (+/- 32K scale)
		/// </summary>
		public int32 TankDiffusion;

		/// <summary>
		/// Reverb delay (in samples)
		/// </summary>
		public uint32 ReverbDelay;

		/// <summary>
		/// HF tank gain [0.0, 1.0]
		/// </summary>
		public c_float flReverbDamping;

		/// <summary>
		/// Reverb decay time (in samples)
		/// </summary>
		public int32 ReverbDecaySamples;

		/// <summary>
		/// 
		/// </summary>
		public readonly EnvironmentReflection[] Reflections = ArrayHelper.InitializeArray<EnvironmentReflection>(CReverb.Environment_NumReflections);
	}
}
