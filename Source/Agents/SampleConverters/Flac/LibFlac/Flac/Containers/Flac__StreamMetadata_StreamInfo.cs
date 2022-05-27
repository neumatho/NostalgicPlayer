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
	/// FLAC STREAMINFO structure
	/// </summary>
	internal class Flac__StreamMetadata_StreamInfo : IMetadata
	{
		public uint32_t Min_BlockSize, Max_BlockSize;
		public uint32_t Min_FrameSize, Max_FrameSize;
		public uint32_t Sample_Rate;
		public uint32_t Channels;
		public uint32_t Bits_Per_Sample;
		public Flac__uint64 Total_Samples;
		public Flac__byte[] Md5Sum = new Flac__byte[16];
	}
}
