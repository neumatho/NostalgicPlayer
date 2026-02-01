/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvChannelMask : uint64_t
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// 
		/// </summary>
		Mono = AvChannelFlags.Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Stereo = AvChannelFlags.Front_Left | AvChannelFlags.Front_Right,

		/// <summary>
		/// 
		/// </summary>
		TwoPointOne = Stereo | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		Two_One = Stereo | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		Surround = Stereo | AvChannelFlags.Front_Center,

		/// <summary>
		/// 
		/// </summary>
		ThreePointOne = Surround | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		FourPointZero = Surround | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		FourPointOne = FourPointZero | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		Two_Two = Stereo | AvChannelFlags.Side_Left | AvChannelFlags.Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Quad = Stereo | AvChannelFlags.Back_Left | AvChannelFlags.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		FivePointZero = Surround | AvChannelFlags.Side_Left | AvChannelFlags.Side_Right,

		/// <summary>
		/// 
		/// </summary>
		FivePointOne = FivePointZero | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		FivePointZero_Back = Surround | AvChannelFlags.Back_Left | AvChannelFlags.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		FivePointOne_Back = FivePointZero_Back | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		SixPointZero = FivePointZero | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		SixPointZero_Front = Two_Two | AvChannelFlags.Front_Left_Of_Center | AvChannelFlags.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		Hexagonal = FivePointZero_Back | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		ThreePointOnePointTwo = ThreePointOne | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		SixPointOne = FivePointOne | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		SixPointOne_Back = FivePointOne_Back | AvChannelFlags.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		SixPointOne_Front = SixPointZero_Front | AvChannelFlags.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		SevenPointZero = FivePointZero | AvChannelFlags.Back_Left | AvChannelFlags.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointZero_Front = FivePointZero | AvChannelFlags.Front_Left_Of_Center | AvChannelFlags.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOne = FivePointOne | AvChannelFlags.Back_Left | AvChannelFlags.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOne_Wide = FivePointOne | AvChannelFlags.Front_Left_Of_Center | AvChannelFlags.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOne_Wide_Back = FivePointOne_Back | AvChannelFlags.Front_Left_Of_Center | AvChannelFlags.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		FivePointOnePointTwo = FivePointOne | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		FivePointOnePointTwo_Back = FivePointOne_Back | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Octagonal = FivePointZero | AvChannelFlags.Back_Left | AvChannelFlags.Back_Center | AvChannelFlags.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		Cube = Quad | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right | AvChannelFlags.Top_Back_Left | AvChannelFlags.Top_Back_Right,

		/// <summary>
		/// 
		/// </summary>
		FivePointOnePointFour_Back = FivePointOnePointTwo | AvChannelFlags.Top_Back_Left | AvChannelFlags.Top_Back_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOnePointTwo = SevenPointOne | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOnePointFour_Back = SevenPointOnePointTwo | AvChannelFlags.Top_Back_Left | AvChannelFlags.Top_Back_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointTwoPointThree = SevenPointOnePointTwo | AvChannelFlags.Top_Back_Center | AvChannelFlags.Low_Frequency_2,

		/// <summary>
		/// 
		/// </summary>
		NinePointOnePointFour_Back = SevenPointOnePointFour_Back | AvChannelFlags.Front_Left_Of_Center | AvChannelFlags.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		NinePointOnePointSix = NinePointOnePointFour_Back | AvChannelFlags.Top_Side_Left | AvChannelFlags.Top_Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Hexadecagonal = Octagonal | AvChannelFlags.Wide_Left | AvChannelFlags.Wide_Right | AvChannelFlags.Top_Back_Left | AvChannelFlags.Top_Back_Right | AvChannelFlags.Top_Back_Center | AvChannelFlags.Top_Front_Center | AvChannelFlags.Top_Front_Left | AvChannelFlags.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Binaural = AvChannelFlags.Binaural_Left | AvChannelFlags.Binaural_Right,

		/// <summary>
		/// 
		/// </summary>
		Stereo_Downmix = AvChannelFlags.Stereo_Left | AvChannelFlags.Stereo_Right,

		/// <summary>
		/// 
		/// </summary>
		TwentyTwoPointTwo = NinePointOnePointSix | AvChannelFlags.Back_Center | AvChannelFlags.Low_Frequency_2 | AvChannelFlags.Top_Front_Center | AvChannelFlags.Top_Center | AvChannelFlags.Top_Back_Center | AvChannelFlags.Bottom_Front_Center | AvChannelFlags.Bottom_Front_Left | AvChannelFlags.Bottom_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		SevenPointOne_Top_Back = FivePointOnePointTwo_Back
	}
}
