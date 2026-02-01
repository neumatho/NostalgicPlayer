/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil
{
	/// <summary>
	/// 
	/// </summary>
	public static class Display
	{
		/********************************************************************/
		/// <summary>
		/// Extract the rotation component of the transformation matrix
		/// </summary>
		/********************************************************************/
		public static c_double Av_Display_Rotation_Get(CPointer<int32_t> matrix)//XX 35
		{
			c_double[] scale = new c_double[2];

			scale[0] = CMath.hypot(Conv_Fp(matrix[0]), Conv_Fp(matrix[3]));
			scale[1] = CMath.hypot(Conv_Fp(matrix[1]), Conv_Fp(matrix[4]));

			if ((scale[0] == 0.0) || (scale[1] == 0.0))
				return c_double.NaN;

			c_double rotation = CMath.atan2(Conv_Fp(matrix[1]) / scale[1], Conv_Fp(matrix[0]) / scale[0]) * 180.0 / Math.PI;

			return -rotation;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize a transformation matrix describing a pure clockwise
		/// rotation by the specified angle (in degrees)
		/// </summary>
		/********************************************************************/
		public static void Av_Display_Rotation_Set(CPointer<int32_t> matrix, c_double angle)//XX 51
		{
			c_double radians = -angle * Math.PI / 180.0;
			c_double c = CMath.cos(radians);
			c_double s = CMath.sin(radians);

			CMemory.memset(matrix, 0, 9);

			matrix[0] = Conv_Db(c);
			matrix[1] = Conv_Db(-s);
			matrix[3] = Conv_Db(s);
			matrix[4] = Conv_Db(c);
			matrix[8] = 1 << 30;
		}



		/********************************************************************/
		/// <summary>
		/// Flip the input matrix horizontally and/or vertically
		/// </summary>
		/********************************************************************/
		public static void Av_Display_Matrix_Flip(CPointer<int32_t> matrix, c_int hFlip, c_int vFlip)//XX 66
		{
			c_int[] flip = [ 1 - (2 * (hFlip != 0 ? 1 : 0)), 1 - (2 * (vFlip != 0 ? 1 : 0)), 1 ];

			if ((hFlip != 0) || (vFlip != 0))
			{
				for (c_int i = 0; i < 9; i++)
					matrix[i] *= flip[i % 3];
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Fixed point to double
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_double Conv_Fp(int32_t x)//XX 30
		{
			return ((c_double)x / (1 << 16));
		}



		/********************************************************************/
		/// <summary>
		/// Double to fixed point
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int32_t Conv_Db(c_double x)//XX 33
		{
			return (int32_t)(x * (1 << 16));
		}
		#endregion
	}
}
