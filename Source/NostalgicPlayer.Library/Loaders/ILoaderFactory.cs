/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Factory implementation to create new instances of the right loader
	/// </summary>
	public interface ILoaderFactory
	{
		/// <summary>
		/// Create new instance of the normal loader
		/// </summary>
		Loader CreateLoader();

		/// <summary>
		/// Create new instance of the stream loader
		/// </summary>
		StreamLoader CreateStreamLoader();
	}
}
