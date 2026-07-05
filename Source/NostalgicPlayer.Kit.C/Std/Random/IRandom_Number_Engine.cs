/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.C.Std.Random
{
	/// <summary>
	/// Common interface implemented by the random number engines in this
	/// namespace.
	///
	/// This has no direct equivalent in C++, where engines conform to the
	/// RandomNumberEngine named requirement and are used via templates
	/// (duck typing). In C# it is needed so that a
	/// <see cref="Discard_Block_Engine{TEngine, TResult}"/> can operate on its
	/// wrapped base engine through a generic type constraint
	/// </summary>
	public interface IRandom_Number_Engine<TResult>
	{
		/// <summary>
		/// Returns the smallest value that the engine can produce (C++ min())
		/// </summary>
		TResult min();

		/// <summary>
		/// Returns the largest value that the engine can produce (C++ max())
		/// </summary>
		TResult max();

		/// <summary>
		/// Advances the engine's state and returns the generated value
		/// (C++ operator())
		/// </summary>
		TResult Invoke();

		/// <summary>
		/// Advances the engine's state by z steps, as if by calling
		/// <see cref="Invoke"/> z times and discarding the results
		/// (C++ discard())
		/// </summary>
		void discard(c_ulong_long z);

		/// <summary>
		/// Reseeds the engine using the default seed (C++ seed())
		/// </summary>
		void seed();

		/// <summary>
		/// Reseeds the engine using the given value (C++ seed(result_type))
		/// </summary>
		void seed(TResult value);

		/// <summary>
		/// Reseeds the engine using the given seed sequence (C++ seed(Sseq＆))
		/// </summary>
		void seed(Seed_Seq q);
	}
}
