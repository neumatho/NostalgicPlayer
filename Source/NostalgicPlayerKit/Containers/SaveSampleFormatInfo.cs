/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Tell the format to save a sample in
	/// </summary>
	public class SaveSampleFormatInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SaveSampleFormatInfo(int bits, int channels, int frequency)
		{
			Bits = bits;
			Channels = channels;
			Frequency = frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of bits each sample is
		/// </summary>
		/********************************************************************/
		public int Bits
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the number of channels. This can either be 1 for mono or
		/// 2 for stereo
		/// </summary>
		/********************************************************************/
		public int Channels
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample frequency
		/// </summary>
		/********************************************************************/
		public int Frequency
		{
			get;
		}
	}
}
