/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// YUV colorspace type.
	/// These values match the ones defined by ISO/IEC 23091-2_2019 subclause 8.3
	/// </summary>
	public enum AvColorSpace
	{
		/// <summary>
		/// Order of coefficients is actually GBR, also IEC 61966-2-1 (sRGB), YZX and ST 428-1
		/// </summary>
		Rgb = 0,

		/// <summary>
		/// Also ITU-R BT1361 / IEC 61966-2-4 xvYCC709 / derived in SMPTE RP 177 Annex B
		/// </summary>
		Bt709 = 1,

		/// <summary>
		/// 
		/// </summary>
		Unspecified = 2,

		/// <summary>
		/// Reserved for future use by ITU-T and ISO/IEC just like 15-255 are
		/// </summary>
		Reserved = 3,

		/// <summary>
		/// FCC Title 47 Code of Federal Regulations 73.682 (a)(20)
		/// </summary>
		Fcc = 4,

		/// <summary>
		/// Also ITU-R BT601-6 625 / ITU-R BT1358 625 / ITU-R BT1700 625 PAL and SECAM / IEC 61966-2-4 xvYCC601
		/// </summary>
		Bt470Bg = 5,

		/// <summary>
		/// Also ITU-R BT601-6 525 / ITU-R BT1358 525 / ITU-R BT1700 NTSC / functionally identical to above
		/// </summary>
		Smpte170M = 6,

		/// <summary>
		/// Derived from 170M primaries and D65 white point, 170M is derived from BT470 System M's primaries
		/// </summary>
		Smpte240M = 7,

		/// <summary>
		/// Used by Dirac / VC-2 and H.264 FRext, see ITU-T SG16
		/// </summary>
		Ycgco = 8,

		/// <summary>
		/// 
		/// </summary>
		Ycocg = Ycgco,

		/// <summary>
		/// ITU-R BT2020 non-constant luminance system
		/// </summary>
		Bt2020_Ncl = 9,

		/// <summary>
		/// ITU-R BT2020 constant luminance system
		/// </summary>
		Bt2020_Cl = 10,

		/// <summary>
		/// SMPTE 2085, Y'D'zD'x
		/// </summary>
		Smpte2085 = 11,

		/// <summary>
		/// Chromaticity-derived non-constant luminance system
		/// </summary>
		Chroma_Derived_Ncl = 12,

		/// <summary>
		/// Chromaticity-derived constant luminance system
		/// </summary>
		Chroma_Derived_Cl = 13,

		/// <summary>
		/// ITU-R BT.2100-0, ICtCp
		/// </summary>
		Ictcp = 14,

		/// <summary>
		/// SMPTE ST 2128, IPT-C2
		/// </summary>
		Ipt_C2 = 15,

		/// <summary>
		/// YCgCo-R, even addition of bits
		/// </summary>
		Ycgco_Re = 16,

		/// <summary>
		/// YCgCo-R, odd addition of bits
		/// </summary>
		Ycgco_Ro = 17,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
