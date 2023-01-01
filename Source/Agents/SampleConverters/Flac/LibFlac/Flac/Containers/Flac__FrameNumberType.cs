/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// An enumeration of the possible frame numbering methods
	/// </summary>
	internal enum Flac__FrameNumberType
	{
		/// <summary>
		/// Number contains the frame number
		/// </summary>
		Frame_Number,

		/// <summary>
		/// Number contains the sample number of first sample in frame
		/// </summary>
		Sample_Number
	}
}
