/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibSwResample
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Rematrix
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Swri_Rematrix_Init(SwrContext s)//XX 454
		{
			throw new NotImplementedException();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Swri_Rematrix_Free(SwrContext s)//XX 562
		{
			Mem.Av_FreeP(ref s.Native_Matrix);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Swri_Rematrix(SwrContext s, AudioData @out, AudioData @in, c_int len, c_int mustCopy)//XX 567
		{
			throw new NotImplementedException();
		}
	}
}
