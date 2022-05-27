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
	/// FLAC metadata block structure
	/// </summary>
	internal class Flac__StreamMetadata
	{
		/// <summary>
		/// The type of the metadata block; used to determine what the
		/// Data points to. If type >= Undefined then Data.Unknown must be used
		/// </summary>
		public Flac__MetadataType Type;

		/// <summary>
		/// True if this metadata block is the last, else false
		/// </summary>
		public Flac__bool Is_Last;

		/// <summary>
		/// Length, in bytes, of the block data as it appears in the stream
		/// </summary>
		public uint32_t Length;

		/// <summary>
		/// Block data; use Type value to determine which to use
		/// </summary>
		public IMetadata Data;
	}
}
