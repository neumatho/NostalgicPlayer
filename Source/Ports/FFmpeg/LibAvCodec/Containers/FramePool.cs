/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FramePool : RefCount, IRefCountData
	{
		/// <summary>
		/// Pools for each data plane. For audio all the planes have the same size,
		/// so only pools[0] is used
		/// </summary>
		public readonly AvBufferPool[] Pools = new AvBufferPool[4];

		/// <summary>
		/// 
		/// </summary>
		public FormatUnion Format;

		/// <summary>
		/// 
		/// </summary>
		public c_int Width;

		/// <summary>
		/// 
		/// </summary>
		public c_int Height;

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] Stride_Align = new c_int[AvFrame.Av_Num_Data_Pointers];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] LineSize = new c_int[4];

		/// <summary>
		/// 
		/// </summary>
		public c_int Planes;

		/// <summary>
		/// 
		/// </summary>
		public c_int Channels;

		/// <summary>
		/// 
		/// </summary>
		public c_int Samples;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Format = new FormatUnion();
			Width = 0;
			Height = 0;
			Planes = 0;
			Channels = 0;
			Samples = 0;

			Array.Clear(Pools);
			Array.Clear(Stride_Align);
			Array.Clear(LineSize);
		}
	}
}
