/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusiclineEditor.Containers
{
	/// <summary>
	/// Holds a pointer to sample data
	/// </summary>
	internal struct SamplePointer
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SamplePointer(sbyte[] sampleData, uint startOffset = 0)
		{
			SampleData = sampleData;
			StartOffset = startOffset;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample data itself
		/// </summary>
		/********************************************************************/
		public sbyte[] SampleData { get; }



		/********************************************************************/
		/// <summary>
		/// Where to start playing in the data
		/// </summary>
		/********************************************************************/
		public uint StartOffset { get; set; }
	}
}
