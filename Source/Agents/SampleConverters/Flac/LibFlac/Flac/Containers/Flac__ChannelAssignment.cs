/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// An enumeration of the available channel assignments
	/// </summary>
	internal enum Flac__ChannelAssignment
	{
		/// <summary>
		/// Independent channels
		/// </summary>
		Independent = 0,

		/// <summary>
		/// Left+side stereo
		/// </summary>
		Left_Side = 1,

		/// <summary>
		/// Right+side stereo
		/// </summary>
		Right_Side = 2,

		/// <summary>
		/// Mid+side stereo
		/// </summary>
		Mid_Side = 3
	}
}
