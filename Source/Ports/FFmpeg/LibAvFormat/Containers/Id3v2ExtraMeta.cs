/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class Id3v2ExtraMeta
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Tag;

		/// <summary>
		/// 
		/// </summary>
		public Id3v2ExtraMeta Next;

		/// <summary>
		/// 
		/// </summary>
		public Id3v2ExtraMetadataUnion Data;
	}
}
