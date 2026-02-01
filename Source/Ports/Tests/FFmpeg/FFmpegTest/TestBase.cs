/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;

namespace Polycode.NostalgicPlayer.Ports.Tests.FFmpeg.FFmpegTest
{
	/// <summary>
	/// Base class for all tests
	/// </summary>
	public abstract class TestBase
	{
		private static readonly Lock testLock = new Lock();

		/********************************************************************/
		/// <summary>
		/// Run a test, collect all output from standard output and compare
		/// it against a reference file
		/// </summary>
		/********************************************************************/
		protected void RunTest(string referenceFile)
		{
			TextWriter originalWriter = Console.Out;

			lock (testLock)
			{
				try
				{
					string result;

					c_int oldLogLevel = Log.Av_Log_Get_Level();
					Log.Av_Log_Set_Level(Log.Av_Log_Quiet);

					try
					{
						UnitTest.EnableUnitTest();

						using (StringWriter stringWriter = new StringWriter())
						{
							Console.SetOut(stringWriter);

							c_int ret = DoTest();
							result = stringWriter.ToString();

							Assert.AreEqual(0, ret, "Test returned an error:\n" + result);
						}
					}
					finally
					{
						Log.Av_Log_Set_Level(oldLogLevel);
					}

					if (referenceFile == null)
					{
						if (!string.IsNullOrEmpty(result))
							Assert.Fail("Test returned output where it shouldn't:\n" + result);

						return;
					}

					string directory = Path.Combine(GetSolutionDirectory(), "FFmpegTest", "Fate");
					referenceFile = Path.Combine(directory, referenceFile);

					using (StringReader stringReader = new StringReader(result))
					{
						using (FileStream fileStream = new FileStream(referenceFile, FileMode.Open, FileAccess.Read))
						{
							int lineNumber = 1;

							using (StreamReader streamReader = new StreamReader(fileStream))
							{
								for (;;)
								{
									string expectedLine = streamReader.ReadLine();
									string actualLine = stringReader.ReadLine();

									if ((expectedLine == null) && (actualLine != null))
										Assert.Fail($"More output written than expected after line {lineNumber}");
									else if ((expectedLine != null) && (actualLine == null))
										Assert.Fail($"Missing output from line {lineNumber}");
									else if ((expectedLine == null) && (actualLine == null))
										break;

									Assert.AreEqual(expectedLine, actualLine.TrimEnd(), $"Failed at line {lineNumber}");
									lineNumber++;
								}
							}
						}
					}
				}
				finally
				{
					Console.SetOut(originalWriter);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected abstract c_int DoTest();



		/********************************************************************/
		/// <summary>
		/// Simulate printf
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void printf(string fmt, params object[] args)
		{
			CConsole.printf(fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// Simulate printf
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void printf(CPointer<char> fmt, params object[] args)
		{
			CConsole.printf(fmt, args);
		}



		/********************************************************************/
		/// <summary>
		/// Find the solution directory
		/// </summary>
		/********************************************************************/
		private string GetSolutionDirectory()
		{
			string directory = Environment.CurrentDirectory;

			while (!directory.EndsWith("\\FFmpeg"))
			{
				int index = directory.LastIndexOf('\\');
				if (index == -1)
					throw new Exception("Could not find solution directory");

				directory = directory.Substring(0, index);
			}

			return directory;
		}
	}
}
