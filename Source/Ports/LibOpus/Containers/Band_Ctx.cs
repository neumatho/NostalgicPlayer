/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Band_Ctx : IDeepCloneable<Band_Ctx>
	{
		public bool encode;
		public bool resynth;
		public CeltMode m;
		public c_int i;
		public c_int intensity;
		public Spread spread;
		public c_int tf_change;
		public Ec_Ctx ec;
		public opus_int32 remaining_bits;
		public Pointer<celt_ener> bandE;
		public opus_uint32 seed;
		public c_int arch;
		public c_int theta_round;
		public bool disable_inv;
		public bool avoid_split_noise;

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Band_Ctx MakeDeepClone()
		{
			return new Band_Ctx
			{
				encode = encode,
				resynth = resynth,
				m = m,
				i = i,
				intensity = intensity,
				spread = spread,
				tf_change = tf_change,
				ec = ec,
				remaining_bits = remaining_bits,
				bandE = bandE,
				seed = seed,
				arch = arch,
				theta_round = theta_round,
				disable_inv = disable_inv,
				avoid_split_noise = avoid_split_noise
			};
		}
	}
}
