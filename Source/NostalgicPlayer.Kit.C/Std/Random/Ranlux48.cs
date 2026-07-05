/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# port of the C++ standard library std::ranlux48.
	///
	/// This corresponds to the C++ type alias:
	/// discard_block_engine&lt;ranlux48_base, 389, 11&gt;
	/// </summary>
	public class Ranlux48 : Discard_Block_Engine<Ranlux48_Base, uint64_t>
	{
		/********************************************************************/
		/// <summary>
		/// Constructs the engine with a default constructed base engine
		/// </summary>
		/********************************************************************/
		public Ranlux48() : base(389, 11)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds the base engine with the given
		/// value
		/// </summary>
		/********************************************************************/
		public Ranlux48(uint64_t value) : base(389, 11, value)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructs the engine and seeds the base engine with the given
		/// seed sequence
		/// </summary>
		/********************************************************************/
		public Ranlux48(Seed_Seq q) : base(389, 11, q)
		{
		}
	}
}
