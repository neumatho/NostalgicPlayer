/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C.Std.Random;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// TNE: The part of engine_traits‹Trng› that does not depend on
	/// result_type. This is only used where an engine is passed along or
	/// stored without its values being read, which in the original is done
	/// with the concrete engine type
	/// </summary>
	internal interface IEngine_Traits
	{
		c_int Result_Bits { get; }
	}

	/// <summary>
	/// TNE: Corresponds to engine_traits‹Trng›, where TResult is its
	/// result_type. In the original the engine itself is the generator that
	/// the traits describe, which is why this also is a
	/// <see cref="IUniform_Random_Bit_Generator{TResult}"/>
	/// </summary>
	internal interface IEngine_Traits<TResult> : IEngine_Traits, IUniform_Random_Bit_Generator<TResult>
	{
	}
}
