/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal abstract class Any_Engine<TValue> : IEngine_Traits<TValue> where TValue : IBinaryInteger<TValue>, IUnsignedNumber<TValue>, IMinMaxValue<TValue>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Any_Engine()
		{
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TValue min()
		{
			return TValue.MinValue;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public TValue max()
		{
			return TValue.MaxValue;
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
		public abstract TValue Invoke();
	}
}
