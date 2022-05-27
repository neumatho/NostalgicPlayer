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
	/// FLAC SEEKTABLE structure
	///
	/// NOTE: From the format specification:
	/// - The seek points must be sorted by ascending sample number.
	/// - Each seek point's sample number must be the first sample of the
	///   target frame.
	/// - Each seek point's sample number must be unique within the table.
	/// - Existence of a SEEKTABLE block implies a correct setting of
	///   Total_Samples in the StreamInfo block.
	/// - Behaviour is undefined when more than one SEEKTABLE block is
	///   present in a stream.
	/// </summary>
	internal class Flac__StreamMetadata_SeekTable : IMetadata
	{
		public uint32_t Num_Points;
		public Flac__StreamMetadata_SeekPoint[] Points;
	}
}
