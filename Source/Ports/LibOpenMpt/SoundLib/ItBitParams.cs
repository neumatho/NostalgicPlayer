/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Algorithm parameters for IT sample compression and decompression
	/// </summary>
	internal interface IItBitParams<T> where T : IBinaryInteger<T>
	{
		static abstract T[] LowerTab { get; }
		static abstract T[] UpperTab { get; }
		static abstract int8 FetchA { get; }
		static abstract int8 LowerB { get; }
		static abstract int8 UpperB { get; }
		static abstract int8 DefWidth { get; }
		static abstract c_int Mask { get; }
	}

	/// <summary>
	/// Algorithm parameters for 16-Bit samples
	/// </summary>
	internal readonly struct It16BitParams : IItBitParams<int16>
	{
		private static readonly int16[] lowerTab = [ 0, -1, -3, -7, -15, -31, -56, -120, -248, -504, -1016, -2040, -4088, -8184, -16376, -32760, -32768 ];
		private static readonly int16[] upperTab = [ 0, 1, 3, 7, 15, 31, 55, 119, 247, 503, 1015, 2039, 4087, 8183, 16375, 32759, 32767 ];

		public static int16[] LowerTab => lowerTab;
		public static int16[] UpperTab => upperTab;
		public static int8 FetchA => 4;
		public static int8 LowerB => -8;
		public static int8 UpperB => 7;
		public static int8 DefWidth => 17;
		public static c_int Mask => 0xffff;
	}

	/// <summary>
	/// Algorithm parameters for 8-Bit samples
	/// </summary>
	internal readonly struct It8BitParams : IItBitParams<int8>
	{
		private static readonly int8[] lowerTab = [ 0, -1, -3, -7, -15, -31, -60, -124, -128 ];
		private static readonly int8[] upperTab = [ 0, 1, 3, 7, 15, 31, 59, 123, 127 ];

		public static int8[] LowerTab => lowerTab;
		public static int8[] UpperTab => upperTab;
		public static int8 FetchA => 3;
		public static int8 LowerB => -4;
		public static int8 UpperB => 3;
		public static int8 DefWidth => 9;
		public static c_int Mask => 0xff;
	}
}
