/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IStreamSeek
	{
		/// <summary>
		/// Tells whether the stream supports seeking or not
		/// </summary>
		bool CanSeek { get; }

		/// <summary>
		/// Set the stream to the current position. Return the new stream to
		/// use
		/// </summary>
		Stream SetPosition(long newPosition);
	}
}
