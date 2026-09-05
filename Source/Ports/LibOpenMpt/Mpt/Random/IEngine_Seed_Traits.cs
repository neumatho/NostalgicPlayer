/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IEngine_Seed_Traits
	{
		static abstract IEngine_Seed_Traits Create(Seed_Seq seed);

		static abstract size_t Seed_Bits { get; }
	}
}
