/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using System.Runtime.InteropServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase
{
	/// <summary>
	/// 
	/// </summary>
	internal class FixedPointSampleTraits<T> where T : INumber<T>, IShiftOperators<T, c_int, c_int>
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public FixedPointSampleTraits(size_t mix_Headroom_Bits, size_t filter_Headroom_Bits)
		{
			Mix_Headroom_Bits = (c_int)mix_Headroom_Bits;
			Mix_Fractional_Bits = (c_int)((Marshal.SizeOf<T>() * 8) - 1 - Mix_Headroom_Bits);			// Excluding sign bit
			Filter_Precision_Bits = (c_int)(((size_t)Marshal.SizeOf<T>() * 8) - filter_Headroom_Bits);	// Including sign bit
			Mix_Scale = T.One << Mix_Fractional_Bits;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Mix_Headroom_Bits
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Mix_Fractional_Bits
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Filter_Precision_Bits
		{
			get; init;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_float Mix_Scale
		{
			get; init;
		}
	}
}
