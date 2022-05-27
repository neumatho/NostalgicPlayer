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
	/// FLAC frame footer structure
	/// </summary>
	internal class Flac__FrameFooter
	{
		/// <summary>
		/// CRC-16 (polynomial = x^16 + x^15 + x^2 + x^0, initialized with 0)
		/// of the bytes before the crc, back to and including the frame header
		/// sync code
		/// </summary>
		public Flac__uint16 Crc;
	}
}
