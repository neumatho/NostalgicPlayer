/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// C# equivalent of the C++ UniformRandomBitGenerator named requirement
	/// (see the C++ standard, [rand.req.urng]).
	///
	/// A source of uniformly distributed unsigned integer values in the
	/// closed range [min(), max()]. This is all a distribution needs from a
	/// generator, which is why it is kept separate from
	/// <see cref="IRandom_Number_Engine{TResult}"/>: an engine is always a
	/// generator, but a generator does not have to be a full engine.
	///
	/// In C++ this is a named requirement checked by the compiler when a
	/// template is instantiated (duck typing). C# has no such mechanism, so
	/// it is expressed as an interface instead
	/// </summary>
	public interface IUniform_Random_Bit_Generator<TResult>
	{
		/// <summary>
		/// Returns the smallest value that the generator can produce
		/// (C++ min())
		/// </summary>
		TResult min();

		/// <summary>
		/// Returns the largest value that the generator can produce
		/// (C++ max())
		/// </summary>
		TResult max();

		/// <summary>
		/// Advances the generator's state and returns the generated value
		/// (C++ operator())
		/// </summary>
		TResult Invoke();
	}
}
