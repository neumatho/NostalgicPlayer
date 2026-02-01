/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.LibAvUtil.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class Test_Display : TestBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Display_()
		{
			RunTest("display");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override c_int DoTest()
		{
			CPointer<int32_t> matrix = new CPointer<int32_t>(9);

			// Set the matrix to 90 degrees
			Display.Av_Display_Rotation_Set(matrix, 90);
			Print_Matrix(matrix);
			printf("degrees: %f\n", Display.Av_Display_Rotation_Get(matrix));

			// Set the matrix to -45 degrees
			Display.Av_Display_Rotation_Set(matrix, -45);
			Print_Matrix(matrix);
			printf("degrees: %f\n", Display.Av_Display_Rotation_Get(matrix));

			// Flip horizontal
			Display.Av_Display_Matrix_Flip(matrix, 1, 0);
			Print_Matrix(matrix);
			printf("degrees: %f\n", Display.Av_Display_Rotation_Get(matrix));

			// Flip vertical
			Display.Av_Display_Matrix_Flip(matrix, 0, 1);
			Print_Matrix(matrix);
			printf("degrees: %f\n", Display.Av_Display_Rotation_Get(matrix));

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Print_Matrix(CPointer<int32_t> matrix)
		{
			for (c_int i = 0; i < 3; ++i)
			{
				c_int j;

				for (j = 0; j < 3 - 1; ++j)
					printf("%d ", matrix[(i * 3) + j]);

				printf("%d\n", matrix[(i * 3) + j]);
			}
		}
	}
}
