/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Crc
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ICrcSpec<T> where T : IUnsignedNumber<T>
	{
		static abstract T Polynomial { get; }
		static abstract T Initial { get; }
		static abstract T ResultXor { get; }
		static abstract bool ReverseData { get; }
	}

	internal readonly struct Crc16Spec : ICrcSpec<uint16>
	{
		public static uint16 Polynomial => 0x8005;
		public static uint16 Initial => 0;
		public static uint16 ResultXor => 0;
		public static bool ReverseData => true;
	}

	internal readonly struct Crc32Spec : ICrcSpec<uint32>
	{
		public static uint32 Polynomial => 0x04c11db7;
		public static uint32 Initial => 0xffffffff;
		public static uint32 ResultXor => 0xffffffff;
		public static bool ReverseData => true;
	}

	internal readonly struct Crc32_OggSpec : ICrcSpec<uint32>
	{
		public static uint32 Polynomial => 0x04c11db7;
		public static uint32 Initial => 0;
		public static uint32 ResultXor => 0;
		public static bool ReverseData => false;
	}

	internal readonly struct Crc32cSpec : ICrcSpec<uint32>
	{
		public static uint32 Polynomial => 0x1edc6f41;
		public static uint32 Initial => 0xffffffff;
		public static uint32 ResultXor => 0xffffffff;
		public static bool ReverseData => true;
	}

	internal readonly struct Crc64_JonesSpec : ICrcSpec<uint64>
	{
		public static uint64 Polynomial => 0xad93d23594c935a9;
		public static uint64 Initial => 0xffffffffffffffff;
		public static uint64 ResultXor => 0;
		public static bool ReverseData => true;
	}
}
