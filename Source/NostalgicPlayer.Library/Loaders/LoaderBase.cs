/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Library.Loaders
{
	/// <summary>
	/// Base class for all loaders
	/// </summary>
	public abstract class LoaderBase : LoaderInfoBase
	{
		/// <summary>
		/// Will try to find a player that understand the source and then
		/// load it into memory or prepare it
		/// </summary>
		public abstract bool Load(string source, out string errorMessage);

		/// <summary>
		/// Will unload the loaded file and free it from memory
		/// </summary>
		public abstract void Unload();
	}
}
