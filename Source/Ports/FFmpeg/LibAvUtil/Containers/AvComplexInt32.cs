/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct AvComplexInt32 : ITxComplex, ITxComplexType<int32_t>
	{
		private int32_t _re;
		private int32_t _im;

		/// <summary>
		/// 
		/// </summary>
		public int32_t Re
		{
			get => _re;
			set => _re = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public int32_t Im
		{
			get => _im;
			set => _im = value;
		}
	}
}
