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
	/// Contents of a Rice partitioned residual
	/// </summary>
	internal class Flac__EntropyCodingMethod_PartitionedRiceContents
	{
		/// <summary>
		/// The Rice parameters for each context
		/// </summary>
		public uint32_t[] Parameters;

		/// <summary>
		/// Widths for escape-coded partitions. Will be non-zero for escaped
		/// partitions and zero for unescaped partitions
		/// </summary>
		public uint32_t[] Raw_Bits;

		/// <summary>
		/// The capacity of the parameters and raw bits arrays
		/// specified as an order, i.e. the number of array elements
		/// allocated is 2 ^ Capacity_By_Order
		/// </summary>
		public uint32_t Capacity_By_Order;
	}
}
