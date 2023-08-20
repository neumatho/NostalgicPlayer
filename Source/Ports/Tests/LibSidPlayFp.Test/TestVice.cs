/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Polycode.NostalgicPlayer.Agent.Player.SidPlay.Roms;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Builders.ReSidFpBuilder;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibSidPlayFp.Test
{
	/// <summary>
	/// Run all the test from Vice
	/// (start in debugger for output)
	/// </summary>
	[TestFixture]
	public class TestVice
	{
		#region ViceException class
		/// <summary>
		/// 
		/// </summary>
		public class ViceException : Exception
		{
			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public ViceException(string message) : base(message)
			{
			}
		}
		#endregion

		// Screen codes conversion table (0x01 = no output)
		private static readonly char[] chrTab =
		{
			'@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
			'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\u0001', '\u0001', '\u0001', '\u0001',
			' ', '!', '\u0001', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?',
			'@', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
			'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '[', '$', ']', ' ', ' ',

			// alternative: CHR$(92=0x5c) => ISO Latin-1(0xa3)
			'-', '#', '|', '-', '-', '-', '-', '|', '|', '\\', '\\', '/', '\\', '\\', '/', '/',
			'\\', '#', '_', '#', '|', '/', 'X', 'O', '#', '|', '#', '+', '|', '|', '&', '\\',

			// 0x80-0xFF
			'\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001',
			'\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001', '\u0001',
			' ', '|', '#', '-', '-', '|', '#', '|', '#', '/', '|', '|', '/', '\\', '\\', '-',
			'/', '-', '-', '|', '|', '|', '|', '-', '-', '-', '/', '\\', '\\', '/', '/', '#',
			'@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
			'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '+', '|', '|', '&', '\\',
			' ', '|', '#', '-', '-', '|', '#', '|', '#', '/', '|', '|', '/', '\\', '\\', '-',
			'/', '-', '-', '|', '|', '|', '|', '-', '-', '-', '/', '\\', '\\', '/', '/', '#'
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[Test]
		public void Test()
		{
			string viceDirectory = Path.Combine(GetSolutionDirectory(), "Vice");
			int successCount = 0;
			int failedCount = 0;

			// Open test file and run tests
			using (StreamReader sr = new StreamReader(Path.Combine(viceDirectory, "testlist")))
			{
				int lineNumber = 0;

				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					lineNumber++;

					if (string.IsNullOrEmpty(line))
						continue;

					if (line[0] == '#')
						continue;

					string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					Debug.WriteLine($"{lineNumber} - Running test {args[0]}");
					TestContext.Progress.WriteLine($"{lineNumber} - Running test {args[0]}");

					try
					{
						RunTest(viceDirectory, args);
					}
					catch(ViceException ex)
					{
						if (ex.Message == "OK")
							successCount++;
						else
						{
							TestContext.Progress.WriteLine(">>> Failed");
							failedCount++;
						}
					}
				}
			}

			Debug.WriteLine($"Successful tests: {successCount} - Failed tests {failedCount}");
			TestContext.Progress.WriteLine($"Successful tests: {successCount} - Failed tests {failedCount}");

			Assert.AreEqual(0, failedCount);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Find the solution directory
		/// </summary>
		/********************************************************************/
		private string GetSolutionDirectory()
		{
			string directory = Environment.CurrentDirectory;

			while (!directory.EndsWith("\\LibSidPlayFp.Test"))
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
		/// Run the given test
		/// </summary>
		/********************************************************************/
		private void RunTest(string viceDirectory, string[] args)
		{
			SidPlayFp engine = new SidPlayFp();

			engine.SetRoms(Kernal.Data, Basic.Data, Character.Data);

			SidConfig config = engine.Config();
			config.powerOnDelay = 0x1267;

			int i = 1;
			while (i < args.Length)
			{
				if ((args[i][0] == '-') && (args.Length > 1))
				{
					if (args[i].Substring(1) == "-sid")
					{
						i++;
						if (args[i] == "old")
						{
							config.sidEmulation = new ReSidFpBuilder();
							config.sidEmulation.Create(1);
							config.forceSidModel = true;
							config.defaultSidModel = SidConfig.sid_model_t.MOS6581;
						}
						else if (args[i] == "new")
						{
							config.sidEmulation = new ReSidFpBuilder();
							config.sidEmulation.Create(1);
							config.forceSidModel = true;
							config.defaultSidModel = SidConfig.sid_model_t.MOS8580;
						}
						else
							throw new Exception($"{args[0]}: Unrecognized SID model");
					}

					if (args[i].Substring(1) == "-cia")
					{
						i++;
						if (args[i] == "old")
							config.ciaModel = SidConfig.cia_model_t.MOS6526;
						else if (args[i] == "new")
							config.ciaModel = SidConfig.cia_model_t.MOS8521;
						else if (args[i] == "4485")
							config.ciaModel = SidConfig.cia_model_t.MOS6526W4485;
						else
							throw new Exception($"{args[0]}: Unrecognized CIA model");
					}

					if (args[i].Substring(1) == "-vic")
					{
						i++;
						if (args[i] == "pal")
						{
							config.defaultC64Model = SidConfig.c64_model_t.PAL;
							config.forceC64Model = true;
						}
						else if (args[i] == "ntsc")
						{
							config.defaultC64Model = SidConfig.c64_model_t.NTSC;
							config.forceC64Model = true;
						}
						else if (args[i] == "oldntsc")
						{
							config.defaultC64Model = SidConfig.c64_model_t.OLD_NTSC;
							config.forceC64Model = true;
						}
						else if (args[i] == "drean")
						{
							config.defaultC64Model = SidConfig.c64_model_t.DREAN;
							config.forceC64Model = true;
						}
						else
							throw new Exception($"{args[0]}: Unrecognized VIC II model");
					}
				}

				i++;
			}

			engine.Config(config);

			string name = Path.Combine(viceDirectory, "VICE-testprogs", args[0].Replace('/', '\\') + ".prg");
			SidTune tune;

			using (FileStream fs = File.OpenRead(name))
			{
				using (ModuleStream ms = new ModuleStream(fs, true))
				{
					tune = new SidTune(new PlayerFileInfo(name, ms, null));
					if (!tune.GetStatus())
						throw new Exception($"{args[0]}: {tune.StatusString()}");

					tune.SelectSong(0);

					if (!engine.Load(tune))
						throw new Exception($"{args[0]}: {engine.Error()}");
				}
			}

			engine.SetTestHook((addr, data) =>
			{
				if (addr == 0xd7ff)
				{
					if (data == 0)
						throw new ViceException("OK");

					if (data == 0xff)
						throw new ViceException("KO");
				}
#if false
				if ((addr >= 1024) && (addr <= 2047))
				{
					Console.Write(chrTab[data]);
					Debug.Write(chrTab[data]);
				}
#endif
			});

			for (;;)
				engine.Play(null, null, 0);
		}
		#endregion
	}
}
