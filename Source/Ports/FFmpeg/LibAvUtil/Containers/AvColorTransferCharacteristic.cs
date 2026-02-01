/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Color Transfer Characteristic.
	/// These values match the ones defined by ISO/IEC 23091-2_2019 subclause 8.2
	/// </summary>
	public enum AvColorTransferCharacteristic
	{

		/// <summary>
		/// 
		/// </summary>
		Reserved0 = 0,

		/// <summary>
		/// Also ITU-R BT1361
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
		/// Also ITU-R BT470M / ITU-R BT1700 625 PAL and SECAM
		/// </summary>
		Gamma22 = 4,

		/// <summary>
		/// Also ITU-R BT470BG
		/// </summary>
		Gamma28 = 5,

		/// <summary>
		/// Also ITU-R BT601-6 525 or 625 / ITU-R BT1358 525 or 625 / ITU-R BT1700 NTSC
		/// </summary>
		Smpte170M = 6,

		/// <summary>
		/// 
		/// </summary>
		Smpte240M = 7,

		/// <summary>
		/// "Linear transfer characteristics"
		/// </summary>
		Linear = 8,

		/// <summary>
		/// "Logarithmic transfer characteristic (100:1 range)"
		/// </summary>
		Log = 9,

		/// <summary>
		/// "Logarithmic transfer characteristic (100 * Sqrt(10) : 1 range)"
		/// </summary>
		Log_Sqrt = 10,

		/// <summary>
		/// IEC 61966-2-4
		/// </summary>
		Iec61966_2_4 = 11,

		/// <summary>
		/// ITU-R BT1361 Extended Colour Gamut
		/// </summary>
		Bt1361_Ecg = 12,

		/// <summary>
		/// IEC 61966-2-1 (sRGB or sYCC)
		/// </summary>
		Iec61966_2_1 = 13,

		/// <summary>
		/// ITU-R BT2020 for 10-bit system
		/// </summary>
		Bt2020_10 = 14,

		/// <summary>
		/// ITU-R BT2020 for 12-bit system
		/// </summary>
		Bt2020_12 = 15,

		/// <summary>
		/// SMPTE ST 2084 for 10-, 12-, 14- and 16-bit systems
		/// </summary>
		Smpte2084 = 16,

		/// <summary>
		/// 
		/// </summary>
		Smptest2084 = Smpte2084,

		/// <summary>
		/// SMPTE ST 428-1
		/// </summary>
		Smpte428 = 17,

		/// <summary>
		/// 
		/// </summary>
		Smptest428_1 = Smpte428,

		/// <summary>
		/// ARIB STD-B67, known as "Hybrid log-gamma"
		/// </summary>
		Arib_Std_B67 = 18,

		/// <summary>
		/// Not part of ABI
		/// </summary>
		Nb
	}
}
