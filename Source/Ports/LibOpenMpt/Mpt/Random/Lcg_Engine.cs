/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std.Random;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lcg_Engine<TState, TValue, TSpec> : IEngine_Seed_Traits, IEngine_Traits<TValue> where TState : IBinaryInteger<TState>, IUnsignedNumber<TState> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue> where TSpec : ILcgSpec<TState>
	{
		private TState state;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Lcg_Engine(Seed_Seq seed)
		{
			state = Seed_State(seed);

			// We return results from the current state and update state after returning. results in better pipelining
			Invoke();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static IEngine_Seed_Traits Create(Seed_Seq seed)
		{
			return new Lcg_Engine<TState, TValue, TSpec>(seed);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Seed_Bits => (size_t)Marshal.SizeOf<TState>() * 8;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Result_Bits => TSpec.Result_Bits;



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TValue min()
		{
			return TValue.Zero;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TValue max()
		{
			return TValue.CreateTruncating(TSpec.Result_Mask >> TSpec.Result_Shift);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TValue Invoke()
		{
			// We return results from the current state and update state after returning. results in better pipelining
			TState s = state;
			TValue result = TValue.CreateTruncating((s & TSpec.Result_Mask) >> TSpec.Result_Shift);

			s = Numeric.Modulo_If_Not_Zero(TSpec.M, (TSpec.A * s) + TSpec.C);
			state = s;

			return result;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TState Seed_State(Seed_Seq seed)
		{
			TState result = TState.Zero;

			c_uint[] seeds = new c_uint[Numeric.Align_Up<size_t>(Seed_Bits, sizeof(c_uint) * 8) / (sizeof(c_uint) * 8)];
			seed.generate(seeds);

			foreach (c_uint seed_Value in seeds)
			{
				result <<= 16;
				result <<= 16;
				result |= TState.CreateTruncating(seed_Value);
			}

			return result;
		}
		#endregion
	}
}
