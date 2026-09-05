/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// TNE: The original ConvertFixedPoint‹int16, int32, fractionalBits›
	/// </summary>
	internal readonly struct ConvertFixedPointInt16<TFractionalBits> : ISampleConvert<int16, int32> where TFractionalBits : IFractionalBits
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int16 Convert(int32 val)
		{
			c_int shiftBits = TFractionalBits.FractionalBits + 1 - (sizeof(int16) * 8);

			val = Arithmetic_Shift.RShift_Signed(val + (1 << (shiftBits - 1)), shiftBits);	// Round

			if (val < int16.MinValue)
				val = int16.MinValue;

			if (val > int16.MaxValue)
				val = int16.MaxValue;

			return (int16)val;
		}
	}
}
