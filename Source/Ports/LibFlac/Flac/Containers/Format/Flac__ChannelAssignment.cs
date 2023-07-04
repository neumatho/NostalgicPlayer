/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// An enumeration of the available channel assignments
	/// </summary>
	public enum Flac__ChannelAssignment
	{
		/// <summary>
		/// Independent channels
		/// </summary>
		Independent = 0,

		/// <summary>
		/// Left+side stereo
		/// </summary>
		Left_Side = 1,

		/// <summary>
		/// Right+side stereo
		/// </summary>
		Right_Side = 2,

		/// <summary>
		/// Mid+side stereo
		/// </summary>
		Mid_Side = 3
	}
}
