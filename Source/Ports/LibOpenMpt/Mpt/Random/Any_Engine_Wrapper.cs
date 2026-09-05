/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal class Any_Engine_Wrapper<TValue, TRng_Value> : Any_Engine<TValue> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>, IMinMaxValue<TValue> where TRng_Value : IBinaryInteger<TRng_Value>, IUnsignedNumber<TRng_Value>
	{
		private readonly IEngine_Traits<TRng_Value> m_Prng;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Any_Engine_Wrapper(IEngine_Traits<TRng_Value> prng)
		{
			m_Prng = prng;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override TValue Invoke()
		{
			return Random.Random_<TValue, TRng_Value>(m_Prng);
		}
	}
}
