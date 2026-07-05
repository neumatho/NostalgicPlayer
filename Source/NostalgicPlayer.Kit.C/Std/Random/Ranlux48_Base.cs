/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# port of the C++ standard library std::ranlux48_base.
	///
	/// This corresponds to the C++ type alias:
	/// subtract_with_carry_engine‹uint_fast64_t, 48, 5, 12›
	/// </summary>
	public class Ranlux48_Base : Subtract_With_Carry_Engine<uint64_t>
	{
		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the default seed
		/// </summary>
		/********************************************************************/
		public Ranlux48_Base() : base(48, 5, 12)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given value
		/// </summary>
		/********************************************************************/
		public Ranlux48_Base(uint64_t value) : base(48, 5, 12, value)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds it with the given seed sequence
		/// </summary>
		/********************************************************************/
		public Ranlux48_Base(Seed_Seq q) : base(48, 5, 12, q)
		{
		}
	}
}
