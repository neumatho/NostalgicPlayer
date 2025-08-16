/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.ObjectModel;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

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
		public NewSampleData(int[] buffer, ReadOnlyDictionary<SpeakerFlag, int> channelMapping, int channelCount)
		{
			SampleData = buffer;
			ChannelMapping = channelMapping;
			ChannelCount = channelCount;
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
		/// Tells which channel each speaker use
		/// </summary>
		/********************************************************************/
		public ReadOnlyDictionary<SpeakerFlag, int> ChannelMapping
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Hold the number of channels used in SampleData
		/// </summary>
		/********************************************************************/
		public int ChannelCount
		{
			get;
		}
	}
}
