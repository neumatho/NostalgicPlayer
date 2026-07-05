/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# port of the C++ standard library std::mt19937.
	///
	/// This corresponds to the C++ type alias:
	/// mersenne_twister_engine&lt;uint_fast32_t, 32, 624, 397, 31, 0x9908b0df,
	/// 11, 0xffffffff, 7, 0x9d2c5680, 15, 0xefc60000, 18, 1812433253&gt;
	/// </summary>
	public class Mt19937 : Mersenne_Twister_Engine<uint32_t>
	{
		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the default seed
		/// </summary>
		/********************************************************************/
		public Mt19937() : base(32, 624, 397, 31, 0x9908b0dfU, 11, 0xffffffffU, 7, 0x9d2c5680U, 15, 0xefc60000U, 18, 1812433253U)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given value
		/// </summary>
		/********************************************************************/
		public Mt19937(uint32_t value) : base(32, 624, 397, 31, 0x9908b0dfU, 11, 0xffffffffU, 7, 0x9d2c5680U, 15, 0xefc60000U, 18, 1812433253U, value)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given seed sequence
		/// </summary>
		/********************************************************************/
		public Mt19937(Seed_Seq q) : base(32, 624, 397, 31, 0x9908b0dfU, 11, 0xffffffffU, 7, 0x9d2c5680U, 15, 0xefc60000U, 18, 1812433253U, q)
		{
		}
	}
}
