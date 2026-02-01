/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// A channel layout is a 64-bits integer with a bit set for every channel.
	/// The number of bits set must be equal to the number of channels.
	/// The value 0 means that the channel layout is not known.
	/// Note this data structure is not powerful enough to handle channels
	/// combinations that have the same channel multiple times, such as
	/// dual-mono
	/// </summary>
	[Flags]
	public enum AvChannelFlags : uint64_t
	{
		/// <summary>
		/// 
		/// </summary>
		Front_Left = 1UL << AvChannel.Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Front_Right = 1UL << AvChannel.Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Front_Center = 1UL << AvChannel.Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Low_Frequency = 1UL << AvChannel.Low_Frequency,

		/// <summary>
		/// 
		/// </summary>
		Back_Left = 1UL << AvChannel.Back_Left,

		/// <summary>
		/// 
		/// </summary>
		Back_Right = 1UL << AvChannel.Back_Right,

		/// <summary>
		/// 
		/// </summary>
		Front_Left_Of_Center = 1UL << AvChannel.Front_Left_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		Front_Right_Of_Center = 1UL << AvChannel.Front_Right_Of_Center,

		/// <summary>
		/// 
		/// </summary>
		Back_Center = 1UL << AvChannel.Back_Center,

		/// <summary>
		/// 
		/// </summary>
		Side_Left = 1UL << AvChannel.Side_Left,

		/// <summary>
		/// 
		/// </summary>
		Side_Right = 1UL << AvChannel.Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Top_Center = 1UL << AvChannel.Top_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Left = 1UL << AvChannel.Top_Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Center = 1UL << AvChannel.Top_Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Front_Right = 1UL << AvChannel.Top_Front_Right,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Left = 1UL << AvChannel.Top_Back_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Center = 1UL << AvChannel.Top_Back_Center,

		/// <summary>
		/// 
		/// </summary>
		Top_Back_Right = 1UL << AvChannel.Top_Back_Right,

		/// <summary>
		/// 
		/// </summary>
		Stereo_Left = 1UL << AvChannel.Stereo_Left,

		/// <summary>
		/// 
		/// </summary>
		Stereo_Right = 1UL << AvChannel.Stereo_Right,

		/// <summary>
		/// 
		/// </summary>
		Wide_Left = 1UL << AvChannel.Wide_Left,

		/// <summary>
		/// 
		/// </summary>
		Wide_Right = 1UL << AvChannel.Wide_Right,

		/// <summary>
		/// 
		/// </summary>
		Surround_Direct_Left = 1UL << AvChannel.Surround_Direct_Left,

		/// <summary>
		/// 
		/// </summary>
		Surround_Direct_Right = 1UL << AvChannel.Surround_Direct_Right,

		/// <summary>
		/// 
		/// </summary>
		Low_Frequency_2 = 1UL << AvChannel.Low_Frequency_2,

		/// <summary>
		/// 
		/// </summary>
		Top_Side_Left = 1UL << AvChannel.Top_Side_Left,

		/// <summary>
		/// 
		/// </summary>
		Top_Side_Right = 1UL << AvChannel.Top_Side_Right,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Center = 1UL << AvChannel.Bottom_Front_Center,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Left = 1UL << AvChannel.Bottom_Front_Left,

		/// <summary>
		/// 
		/// </summary>
		Bottom_Front_Right = 1UL << AvChannel.Bottom_Front_Right,

		/// <summary>
		/// +90 degrees, Lss, SiL
		/// </summary>
		Side_Surround_Left = 1UL << AvChannel.Side_Surround_Left,

		/// <summary>
		/// -90 degrees, Rss, SiR
		/// </summary>
		Side_Surround_Right = 1UL << AvChannel.Side_Surround_Right,

		/// <summary>
		/// +110 degrees, Lvs, TpLS
		/// </summary>
		Top_Surround_Left = 1UL << AvChannel.Top_Surround_Left,

		/// <summary>
		/// -110 degrees, Rvs, TpRS
		/// </summary>
		Top_Surround_Right = 1UL << AvChannel.Top_Surround_Right,

		/// <summary>
		/// 
		/// </summary>
		Binaural_Left = 1UL << AvChannel.Binaural_Left,

		/// <summary>
		/// 
		/// </summary>
		Binaural_Right = 1UL << AvChannel.Binaural_Right
	}
}
