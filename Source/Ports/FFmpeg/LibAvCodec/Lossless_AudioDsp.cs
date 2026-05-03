/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	public static class Lossless_AudioDsp
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void FF_LLAudDsp_Init(LLAudDspContext c)
		{
			c.ScalarProduct_And_MAdd_Int16 = ScalarProduct_And_MAdd_Int16_C;
			c.ScalarProduct_And_MAdd_Int32 = ScalarProduct_And_MAdd_Int32_C;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int32_t ScalarProduct_And_MAdd_Int16_C(CPointer<int16_t> v1, CPointer<int16_t> v2, CPointer<int16_t> v3, c_int order, c_int mul)
		{
			c_uint res = 0;

			do
			{
				res += (c_uint)(v1[0] * v2[0, 1]);
				v1[0] += (int16_t)(mul * v3[0, 1]);
				v1++;
				res += (c_uint)(v1[0] * v2[0, 1]);
				v1[0] += (int16_t)(mul * v3[0, 1]);
				v1++;
			}
			while ((order -= 2) != 0);

			return (int32_t)res;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int32_t ScalarProduct_And_MAdd_Int32_C(CPointer<int16_t> v1, CPointer<int32_t> v2, CPointer<int16_t> v3, c_int order, c_int mul)
		{
			c_int res = 0;

			do
			{
				res += (c_int)(v1[0] * (c_uint)v2[0, 1]);
				v1[0] += (int16_t)(mul * v3[0, 1]);
				v1++;
				res += (c_int)(v1[0] * (c_uint)v2[0, 1]);
				v1[0] += (int16_t)(mul * v3[0, 1]);
				v1++;
			}
			while ((order -= 2) != 0);

			return res;
		}
		#endregion
	}
}
