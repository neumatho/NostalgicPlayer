/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.External.Homepage.Models.VersionHistory
{
	/// <summary>
	/// A single bullet
	/// </summary>
	public class HistoryBulletModel
	{
		/// <summary>
		/// Holds the text to show
		/// </summary>
		public string BulletText { get; set; }

		/// <summary>
		/// If set, holds sub-bullets
		/// </summary>
		public HistoryBulletModel[] SubBullets { get; set; }
	}
}
