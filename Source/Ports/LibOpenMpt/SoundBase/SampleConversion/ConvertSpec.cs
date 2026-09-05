/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IByteOrder16
	{
		static abstract c_int Lo { get; }
		static abstract c_int Hi { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct LittleEndian16 : IByteOrder16
	{
		public static c_int Lo => 0;
		public static c_int Hi => 1;
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct BigEndian16 : IByteOrder16
	{
		public static c_int Lo => 1;
		public static c_int Hi => 0;
	}

	/// <summary>
	/// 
	/// </summary>
	internal interface IOffset16
	{
		static abstract uint16 Offset { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct Offset_0 : IOffset16
	{
		public static uint16 Offset => 0;
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct Offset_0x8000 : IOffset16
	{
		public static uint16 Offset => 0x8000;
	}

	/// <summary>
	/// 
	/// </summary>
	internal interface IFractionalBits
	{
		static abstract c_int FractionalBits { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct MixFractionalBits : IFractionalBits
	{
		public static c_int FractionalBits => MixSample.MixSampleIntTraits.Mix_Fractional_Bits;
	}

	/// <summary>
	/// 
	/// </summary>
	internal interface IClipOutput
	{
		static abstract bool ClipOutput { get; }
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct ClipOutput_True : IClipOutput
	{
		public static bool ClipOutput => true;
	}

	/// <summary>
	/// 
	/// </summary>
	internal readonly struct ClipOutput_False : IClipOutput
	{
		public static bool ClipOutput => false;
	}
}
