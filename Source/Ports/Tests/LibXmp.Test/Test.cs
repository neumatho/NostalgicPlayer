/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// Helper methods to the different tests
	/// </summary>
	public abstract partial class Test
	{
		/// <summary>
		/// 
		/// </summary>
		protected readonly string dataDirectory;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected Test()
		{
			string solutionDirectory = GetSolutionDirectory();
			dataDirectory = Path.Combine(solutionDirectory, "Data");

			Ports.LibXmp.LibXmp.UnitTestMode = true;
		}



		/********************************************************************/
		/// <summary>
		/// Find the solution directory
		/// </summary>
		/********************************************************************/
		private string GetSolutionDirectory()
		{
			string directory = Environment.CurrentDirectory;

			while (!directory.EndsWith("\\LibXmp.Test"))
			{
				int index = directory.LastIndexOf('\\');
				if (index == -1)
					throw new Exception("Could not find solution directory");

				directory = directory.Substring(0, index);
			}

			return directory;
		}



		/********************************************************************/
		/// <summary>
		/// Open the given file in the data directory
		/// </summary>
		/********************************************************************/
		protected Stream OpenStream(string path, string fileName)
		{
			try
			{
				return new FileStream(Path.Combine(path, fileName), FileMode.Open, FileAccess.Read);
			}
			catch (IOException)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load a file into the given context
		/// </summary>
		/********************************************************************/
		protected c_int LoadModule(string path, string fileName, Ports.LibXmp.LibXmp ctx)
		{
			using (Stream modStream = OpenStream(path, fileName))
			{
				return ctx.Xmp_Load_Module_From_File(modStream);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is_Big_Endian()
		{
			return !BitConverter.IsLittleEndian;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private c_double LibXmp_Round(c_double val)
		{
			return (val >= 0.0) ? Math.Floor(val + 0.5) : Math.Ceiling(val - 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// Get period from note
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected c_int Note_To_Period(c_int n)
		{
			return (c_int)LibXmp_Round(13696.0 / Math.Pow(2, (c_double)n / 12));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected c_int Period(Xmp_Frame_Info info)
		{
			return (c_int)LibXmp_Round(1.0 * info.Channel_Info[0].Period / 4096);
		}
	}
}
