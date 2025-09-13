/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Polycode.NostalgicPlayer.Controls.Theme.Interfaces;

namespace Polycode.NostalgicPlayer.Controls.Theme
{
	/// <summary>
	/// Register class
	/// </summary>
	public static class ThemeManagerRegister
	{
		/********************************************************************/
		/// <summary>
		/// Make sure all needed classes are registered in the dependency
		/// injection collection
		/// </summary>
		/********************************************************************/
		public static void RegisterThemeManager(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IThemeManager, ThemeManager>();
		}
	}
}
