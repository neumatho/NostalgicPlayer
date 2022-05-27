/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Protected
{
	/// <summary>
	/// 
	/// </summary>
	internal class Flac__StreamDecoderProtected
	{
		public Flac__StreamDecoderState State;
		public Flac__StreamDecoderInitStatus InitState;
		public uint32_t Channels;
		public Flac__ChannelAssignment Channel_Assignment;
		public uint32_t Bits_Per_Sample;
		public uint32_t Sample_Rate;		// In Hz
		public uint32_t BlockSize;			// In samples (per channel)
		public Flac__bool Md5_Checking;		// If true, generate MD5 signature of decoded data and compare against signature in the STREAMINFO metadata
	}
}
