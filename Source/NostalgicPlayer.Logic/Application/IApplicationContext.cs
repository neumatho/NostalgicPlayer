/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Logic.Application
{
	/// <summary>
	/// Holds the context of the application
	/// </summary>
	public interface IApplicationContext
	{
		/// <summary>
		/// Dependency injection container
		/// </summary>
		Container Container { get; }
	}
}
