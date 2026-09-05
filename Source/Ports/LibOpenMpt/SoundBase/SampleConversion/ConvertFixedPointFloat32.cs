/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// TNE: The original ConvertFixedPoint‹somefloat32, int32,
	/// fractionalBits›. The factor is a member initialized by the
	/// constructor there, and a static since Convert() is static here
	/// </summary>
	internal readonly struct ConvertFixedPointFloat32<TFractionalBits> : ISampleConvert<c_float, int32> where TFractionalBits : IFractionalBits
	{
		private static readonly c_float factor = 1.0f / (c_float)(1 << TFractionalBits.FractionalBits);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_float Convert(int32 val)
		{
			return (c_float)val * factor;
		}
	}
}
