/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constant
	{
		public const int Index_Size = 1000;

		// Compat
		public const ptrdiff_t PtrDiff_Max = ptrdiff_t.MaxValue / 2;
		public const off_t Off_Max = off_t.MaxValue / 2;

		// Decode
		public const int NToM_Max = 8;				// Maximum allowed factor for upsampling
		public const int NToM_Max_Freq = 96000;		// Maximum frequency to upsample to / downsample from
		public const int NToM_Mul = 32768;

		// Frame
		public const int Num_Channels = 2;
		public const int MaxFrameSize = 3456;

		// L3Tabs
		public const Real Cos6_1 = 8.66025404e-01f;
		public const Real Cos6_2 = 5.00000000e-01f;

		// Mpg123Lib
		public const int SBLimit = 32;
		public const int SSLimit = 18;

		public const int S32_Rescale = 65536;

		public const int Scale_Block = 12;

		public const int Short_Scale = 32768;

		public const int Mpg123_Rates = 9;
		public const int Mpg123_Encodings = 12;
	}
}
