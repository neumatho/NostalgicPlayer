/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Library.Application;
using Polycode.NostalgicPlayer.Logic.Composition;
using Polycode.NostalgicPlayer.Platform.Composition;

namespace Polycode.NostalgicPlayer.Client.ConsolePlayer
{
	/// <summary>
	/// NostalgicPlayer console player. Mainly used to easily test players
	/// </summary>
	public static class Program
	{
		/********************************************************************/
		/// <summary>
		/// Main entry point
		/// </summary>
		/********************************************************************/
		public static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Music player to play old-school Amiga and PC modules.");
				Console.WriteLine();
				Console.WriteLine("Syntax: NostalgicPlayerConsole <file> | <url>");
				return;
			}

			try
			{
				using (ApplicationBuilder builder = new ApplicationBuilder(args))
				{
					builder.ConfigureContainer(context =>
					{
						context.Container.RegisterLogic();
						context.Container.RegisterPlatform();
						context.Container.RegisterSingleton<IApplicationHost, ConsoleApplication>();
					})
					.Build()
					.Run();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Program failed with exception: " + ex);
			}
		}
	}
}
