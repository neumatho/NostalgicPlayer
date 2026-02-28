/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Helper class to use dependency injection
	/// </summary>
	public static class DependencyInjection
	{
		/********************************************************************/
		/// <summary>
		/// The container object.
		///
		/// This is only temporary and used until everything has been moved
		/// into the DI container
		/// </summary>
		/********************************************************************/
		public static Container Container { get; set; }
	}
}
