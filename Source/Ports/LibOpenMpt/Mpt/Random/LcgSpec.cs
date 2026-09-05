/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Lcg_Msvc = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Lcg_Engine<uint, ushort, Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.Lcg_Msvc_Spec>;

using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ILcgSpec<T> where T : IUnsignedNumber<T>
	{
		static abstract T M { get; }
		static abstract T A { get; }
		static abstract T C { get; }
		static abstract T Result_Mask { get; }
		static abstract c_int Result_Shift { get; }
		static abstract c_int Result_Bits { get; }
	}

	internal readonly struct Lcg_Msvc_Spec : ILcgSpec<uint32>
	{
		public static uint32 M => 0;
		public static uint32 A => 214013;
		public static uint32 C => 2531011;
		public static uint32 Result_Mask => 0x7fff0000;
		public static c_int Result_Shift => 16;
		public static c_int Result_Bits => 15;
	}
}
