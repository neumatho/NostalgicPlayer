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
	public struct AvComplexDouble : ITxComplex, ITxComplexType<c_double>
	{
		private c_double _re;
		private c_double _im;

		/// <summary>
		/// 
		/// </summary>
		public c_double Re
		{
			get => _re;
			set => _re = value;
		}

		/// <summary>
		/// 
		/// </summary>
		public c_double Im
		{
			get => _im;
			set => _im = value;
		}
	}
}
