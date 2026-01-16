/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using SimpleInjector;

namespace Polycode.NostalgicPlayer.Library.Application
{
	/// <summary>
	/// Holds the context of the application
	/// </summary>
	internal class ApplicationContext : IApplicationContext
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ApplicationContext(string[] args)
		{
			Arguments = args;

			Container = new Container();
		}



		/********************************************************************/
		/// <summary>
		/// Dependency injection container
		/// </summary>
		/********************************************************************/
		public Container Container
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Command line arguments
		/// </summary>
		/********************************************************************/
		public string[] Arguments
		{
			get;
		}
	}
}
