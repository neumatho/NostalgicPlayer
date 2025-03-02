﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about new sample data
	/// </summary>
	public class NewSampleData
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public NewSampleData(int[] buffer, int[] channelMapping)
		{
			SampleData = buffer;
			ChannelMapping = channelMapping;
		}



		/********************************************************************/
		/// <summary>
		/// An array with the sample data
		/// </summary>
		/********************************************************************/
		public int[] SampleData
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold indexes for each channel into the sample data, e.g. for
		/// normal stereo output it will be 0, 1. If speaker swapping has
		/// been enabled, it will be 1, 0
		/// </summary>
		/********************************************************************/
		public int[] ChannelMapping
		{
			get;
		}
	}
}
