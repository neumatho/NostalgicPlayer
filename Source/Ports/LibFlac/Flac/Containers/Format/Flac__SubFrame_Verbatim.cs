/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format
{
	/// <summary>
	/// Verbatim subframe
	/// </summary>
	public class Flac__SubFrame_Verbatim : ISubFrame
	{
		/// <summary>
		/// A Flac__int32 pointer to verbatim signal
		/// </summary>
		public Flac__int32[] Data32;

		/// <summary>
		/// A Flac__int64 pointer to verbatim signal
		/// </summary>
		public Flac__int64[] Data64;

		/// <summary>
		/// 
		/// </summary>
		public Flac__VerbatimSubFrameDataType Data_Type;
	}
}