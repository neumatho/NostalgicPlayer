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
	/// SeekPoint structure used in SEEKTABLE blocks
	/// </summary>
	internal class Flac__StreamMetadata_SeekPoint
	{
		/// <summary>
		/// The sample number of the target frame
		/// </summary>
		public Flac__uint64 Sample_Number;

		/// <summary>
		/// The offset, in bytes, of the target frame with respect to
		/// beginning of the first frame
		/// </summary>
		public Flac__uint64 Stream_Offset;

		/// <summary>
		/// The number of samples in the target frame
		/// </summary>
		public uint32_t Frame_Samples;
	}
}
