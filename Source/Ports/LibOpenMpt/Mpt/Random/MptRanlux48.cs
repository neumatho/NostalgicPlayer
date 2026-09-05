/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// TNE: This does not exists in OpenMPT. I have just made it, so
	/// it is possible to access seed_bits and result_bits
	/// </summary>
	internal class MptRanlux48 : Ranlux48, IEngine_Seed_Traits, IEngine_Traits<uint64_t>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private MptRanlux48(Seed_Seq seed) : base(seed)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static IEngine_Seed_Traits Create(Seed_Seq seed)
		{
			return new MptRanlux48(seed);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Seed_Bits => (size_t)new Ranlux48().Base().word_size;




		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Result_Bits => Base().word_size;
	}
}
