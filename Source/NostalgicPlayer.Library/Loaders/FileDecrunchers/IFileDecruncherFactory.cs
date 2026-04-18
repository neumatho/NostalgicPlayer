/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Loaders.FileDecrunchers
{
	/// <summary>
	/// Factory class to create file decruncher instances
	/// </summary>
	internal interface IFileDecruncherFactory
	{
		/// <summary>
		/// Create a new instance of the single file decruncher
		/// </summary>
		SingleFileDecruncher GetSingleFileDecruncher();
	}
}
