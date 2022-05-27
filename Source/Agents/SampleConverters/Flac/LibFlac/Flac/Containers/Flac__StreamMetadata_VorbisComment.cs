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
	/// FLAC VORBIS_COMMENT structure
	/// </summary>
	internal class Flac__StreamMetadata_VorbisComment : IMetadata
	{
		public Flac__StreamMetadata_VorbisComment_Entry Vendor_String = new Flac__StreamMetadata_VorbisComment_Entry();
		public Flac__uint32 Num_Comments;
		public Flac__StreamMetadata_VorbisComment_Entry[] Comments;
	}
}
