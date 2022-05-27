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
	/// Vorbis comment entry structure used in VORBIS_COMMENT blocks.
	///
	/// For convenience, the APIs maintain a trailing NUL character at the end of
	/// entry which is not counted toward Length
	/// </summary>
	internal class Flac__StreamMetadata_VorbisComment_Entry
	{
		public Flac__uint32 Length;
		public Flac__byte[] Entry;
	}
}
