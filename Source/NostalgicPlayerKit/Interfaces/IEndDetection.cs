/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Players implementing this can detect when the end of the module is reached
	/// </summary>
	public interface IEndDetection
	{
		/// <summary>
		/// This flag is set to true, when end is reached
		/// </summary>
		bool HasEndReached { get; set; }
	}
}
