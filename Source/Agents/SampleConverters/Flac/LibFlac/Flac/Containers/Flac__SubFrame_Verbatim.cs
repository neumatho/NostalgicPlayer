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
	/// Verbatim subframe
	/// </summary>
	internal class Flac__SubFrame_Verbatim : ISubFrame
	{
		/// <summary>
		/// A pointer to verbatim signal
		/// </summary>
		public Flac__int32[] Data;
	}
}