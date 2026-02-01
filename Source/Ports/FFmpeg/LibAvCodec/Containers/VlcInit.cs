/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum VlcInit
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Use_Static = 1,

		/// <summary>
		/// 
		/// </summary>
		Static_Overlong = 2 | Use_Static,

		/// <summary>
		/// If VLC_INIT_INPUT_LE is set, the LSB bit of the codes used to
		/// initialize the VLC table is the first bit to be read
		/// </summary>
		Input_Le = 4,

		/// <summary>
		/// If set the VLC is intended for a little endian bitstream reader
		/// </summary>
		Output_Le = 8,

		/// <summary>
		/// 
		/// </summary>
		Le = Input_Le | Output_Le
	}
}
