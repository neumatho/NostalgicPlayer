/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Chromaticity coordinates of the source primaries.
	/// These values match the ones defined by ISO/IEC 23091-2_2019 subclause 8.1 and ITU-T H.273
	/// </summary>
	public enum AvColorPrimaries
	{
		/// <summary>
		/// 
		/// </summary>
		Reserved0 = 0,

		/// <summary>
		/// also ITU-R BT1361 / IEC 61966-2-4 / SMPTE RP 177 Annex B
		/// </summary>
		Bt709 = 1,

		/// <summary>
		/// 
		/// </summary>
		Unspecified = 2,

		/// <summary>
		/// 
		/// </summary>
		Reserved = 3,

		/// <summary>
		/// Also FCC Title 47 Code of Federal Regulations 73.682 (a)(20)
		/// </summary>
		Bt470M = 4,

		/// <summary>
		/// Also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL and SECAM
		/// </summary>
		Bt470Bg = 5,

		/// <summary>
		/// Also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC
		/// </summary>
		Smpte170M = 6,

		/// <summary>
		/// Identical to above, also called "SMPTE C" even though it uses D65
		/// </summary>
		Smpte240M = 7,

		/// <summary>
		/// Colour filters using Illuminant C
		/// </summary>
		Film = 8,

		/// <summary>
		/// ITU-R BT2020
		/// </summary>
		Bt2020 = 9,

		/// <summary>
		/// SMPTE ST 428-1 (CIE 1931 XYZ)
		/// </summary>
		Smpte428 = 10,

		/// <summary>
		/// 
		/// </summary>
		Smptest428_1 = Smpte428,

		/// <summary>
		/// SMPTE ST 431-2 (2011) / DCI P3
		/// </summary>
		Smpte431 = 11,

		/// <summary>
		/// SMPTE ST 432-1 (2010) / P3 D65 / Display P3
		/// </summary>
		Smpte432 = 12,

		/// <summary>
		/// EBU Tech. 3213-E (nothing there) / one of JEDEC P22 group phosphors
		/// </summary>
		Ebu3213 = 22,

		/// <summary>
		/// 
		/// </summary>
		Jedec_P22 = Ebu3213,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
