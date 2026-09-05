/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using Modplug_Dither = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.ModPlug_Engine<uint, uint, Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random.ModPlug_Dither_Spec>;

using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Random
{
	/// <summary>
	/// TNE: Holds the non-type template arguments of the original
	/// mpt::rng::modplug template
	/// </summary>
	internal interface IModPlug_Spec<T> where T : IUnsignedNumber<T>
	{
		static abstract T X1 { get; }
		static abstract T X2 { get; }
		static abstract T X3 { get; }
		static abstract T X4 { get; }
		static abstract c_int Rol1 { get; }
		static abstract c_int Rol2 { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct ModPlug_Dither_Spec : IModPlug_Spec<uint32>
	{
		public static uint32 X1 => 0x10204080;
		public static uint32 X2 => 0x78649e7d;
		public static uint32 X3 => 4;
		public static uint32 X4 => 5;
		public static c_int Rol1 => 1;
		public static c_int Rol2 => 16;
	}
}
