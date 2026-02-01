/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Float_ScalarProduct
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_float FF_ScalarProduct_Float_C(CPointer<c_float> v1, CPointer<c_float> v2, c_int len)
		{
			c_float p = 0.0f;

			for (c_int i = 0; i < len; i++)
				p += v1[i] * v2[i];

			return p;
		}
	}
}
