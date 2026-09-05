/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class ModPlug_Engine<TState, TValue, TSpec> : IEngine_Traits<TValue> where TState : IBinaryInteger<TState>, IUnsignedNumber<TState> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue> where TSpec : IModPlug_Spec<TState>
	{
		private TState state1;
		private TState state2;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ModPlug_Engine(TState seed1, TState seed2)
		{
			state1 = seed1;
			state2 = seed2;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Result_Bits => Marshal.SizeOf<TValue>() * 8;



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
			return TValue.AllBitsSet;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public TValue Invoke()
		{
			TState a = state1;
			TState b = state2;

			a = Bit.Rotl(a, TSpec.Rol1);
			a ^= TSpec.X1;
			a += TSpec.X2 + (b * TSpec.X3);
			b += Bit.Rotl(a, TSpec.Rol2) * TSpec.X4;

			state1 = a;
			state2 = b;

			return TValue.CreateTruncating(b);
		}
	}
}
