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
	/// An enumeration of the available entropy coding methods
	/// </summary>
	internal enum Flac__EntropyCodingMethodType
	{
		/// <summary>
		/// Residual is coded by partitioning into contexts, each with it's own 4-bit rice parameter
		/// </summary>
		Partitioned_Rice = 0,

		/// <summary>
		/// Residual is coded by partitioning into contexts, each with it's own 5-bit rice parameter
		/// </summary>
		Partitioned_Rice2 = 1,
	}
}
