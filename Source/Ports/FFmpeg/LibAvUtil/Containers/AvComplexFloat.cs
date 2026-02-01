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
	public struct AvComplexFloat : ITxComplex, ITxComplexType<c_float>
	{
		private c_float _re;
		private c_float _im;

		/// <summary>
		/// 
		/// </summary>
		public c_float Re
		{
			get => _re;
			set => _re = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public c_float Im
		{
			get => _im;
			set => _im = value;
		}
	}
}
