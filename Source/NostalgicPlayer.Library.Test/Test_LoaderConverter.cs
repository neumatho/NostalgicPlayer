/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Loaders;
using Polycode.NostalgicPlayer.Library.Test.Agents;
using Polycode.NostalgicPlayer.Library.Test.Helpers;
using Polycode.NostalgicPlayer.Library.Test.Loaders;

namespace Polycode.NostalgicPlayer.Library.Test
{
	/// <summary>
	/// Test that a module which has to be converted before it can be played,
	/// is presented to the player in the right way. The tests only check what
	/// the player reads back, not how the loader and converter store the data
	/// internally
	/// </summary>
	[TestClass]
	public class Test_LoaderConverter
	{
		#region Tests using a player which reads the module forwards
		/********************************************************************/
		/// <summary>
		/// The converter changes the music data into another format and
		/// copies the sample data into the converter stream
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Converter_Copies_SampleData()
		{
			LoadModuleAndCheckPlayerData(GetCopyConverter(), CreateForwardPlayer(), ForwardReadOrder, TestConverterCopyWorker.TypeName, TestModuleData.FirstConvertedMark, 1);
		}



		/********************************************************************/
		/// <summary>
		/// The converter changes the music data into another format and
		/// marks the sample data in the converter stream. The player has to
		/// end up with exactly the same data as when the sample data is
		/// copied
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Converter_Marks_SampleData()
		{
			LoadModuleAndCheckPlayerData(GetMarkConverter(), CreateForwardPlayer(), ForwardReadOrder, TestConverterMarkWorker.TypeName, TestModuleData.FirstConvertedMark, 1);
		}



		/********************************************************************/
		/// <summary>
		/// The converter changes the music data into another format and
		/// marks the sample data. The result is then taken through a second
		/// converter, which changes the music data into a third format and
		/// marks the sample data as well
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Converter_Marks_SampleData_Chained()
		{
			LoadModuleAndCheckPlayerData(GetChainedMarkConverters(), CreateForwardPlayer(), ForwardReadOrder, TestConverterMarkWorker.TypeName, TestModuleData.SecondConvertedMark, 2);
		}
		#endregion

		#region Tests using a player which seeks around in the module
		/********************************************************************/
		/// <summary>
		/// Same as Test_Converter_Copies_SampleData(), but the player reads
		/// both blocks of music data first and then takes the samples
		/// backwards
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SeekingPlayer_Converter_Copies_SampleData()
		{
			LoadModuleAndCheckPlayerData(GetCopyConverter(), CreateSeekingPlayer(), SeekingReadOrder, TestConverterCopyWorker.TypeName, TestModuleData.FirstConvertedMark, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Same as Test_Converter_Marks_SampleData(), but the player reads
		/// both blocks of music data first and then takes the samples
		/// backwards
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SeekingPlayer_Converter_Marks_SampleData()
		{
			LoadModuleAndCheckPlayerData(GetMarkConverter(), CreateSeekingPlayer(), SeekingReadOrder, TestConverterMarkWorker.TypeName, TestModuleData.FirstConvertedMark, 1);
		}



		/********************************************************************/
		/// <summary>
		/// Same as Test_Converter_Marks_SampleData_Chained(), but the player
		/// reads both blocks of music data first and then takes the samples
		/// backwards
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_SeekingPlayer_Converter_Marks_SampleData_Chained()
		{
			LoadModuleAndCheckPlayerData(GetChainedMarkConverters(), CreateSeekingPlayer(), SeekingReadOrder, TestConverterMarkWorker.TypeName, TestModuleData.SecondConvertedMark, 2);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// The order a player reading the module forwards will read the data
		/// </summary>
		/********************************************************************/
		private static string[] ForwardReadOrder =>
		[
			TestPlayerWorkerBase.MarkRead,
			TestPlayerWorkerBase.SampleLengthsRead,
			TestPlayerWorkerBase.SongDataRead,
			TestPlayerWorkerBase.GetSampleReadName(0),
			TestPlayerWorkerBase.GetSampleReadName(1),
			TestPlayerWorkerBase.MoreSongDataRead,
			TestPlayerWorkerBase.GetSampleReadName(2)
		];



		/********************************************************************/
		/// <summary>
		/// The order a player seeking around in the module will read the
		/// data
		/// </summary>
		/********************************************************************/
		private static string[] SeekingReadOrder =>
		[
			TestPlayerWorkerBase.MarkRead,
			TestPlayerWorkerBase.SampleLengthsRead,
			TestPlayerWorkerBase.SongDataRead,
			TestPlayerWorkerBase.MoreSongDataRead,
			TestPlayerWorkerBase.GetSampleReadName(2),
			TestPlayerWorkerBase.GetSampleReadName(1),
			TestPlayerWorkerBase.GetSampleReadName(0)
		];



		/********************************************************************/
		/// <summary>
		/// Return a single converter which copies the sample data
		/// </summary>
		/********************************************************************/
		private static AgentInfo[] GetCopyConverter()
		{
			return
			[
				TestAgent.CreateAgentInfo(TestConverterCopyWorker.TypeName, () => new TestConverterCopyWorker())
			];
		}



		/********************************************************************/
		/// <summary>
		/// Return a single converter which marks the sample data
		/// </summary>
		/********************************************************************/
		private static AgentInfo[] GetMarkConverter()
		{
			return
			[
				TestAgent.CreateAgentInfo(TestConverterMarkWorker.TypeName, () => new TestConverterMarkWorker())
			];
		}



		/********************************************************************/
		/// <summary>
		/// Return two converters which both mark the sample data, so the
		/// module has to be converted twice
		/// </summary>
		/********************************************************************/
		private static AgentInfo[] GetChainedMarkConverters()
		{
			return
			[
				TestAgent.CreateAgentInfo(TestConverterMarkWorker.TypeName, () => new TestConverterMarkWorker()),
				TestAgent.CreateAgentInfo(TestConverterSecondMarkWorker.TypeName, () => new TestConverterSecondMarkWorker())
			];
		}



		/********************************************************************/
		/// <summary>
		/// Return a player which reads the module forwards
		/// </summary>
		/********************************************************************/
		private static AgentInfo CreateForwardPlayer()
		{
			return TestAgent.CreateAgentInfo(TestPlayerWorker.TypeName, () => new TestPlayerWorker());
		}



		/********************************************************************/
		/// <summary>
		/// Return a player which seeks around in the module
		/// </summary>
		/********************************************************************/
		private static AgentInfo CreateSeekingPlayer()
		{
			return TestAgent.CreateAgentInfo(TestSeekingPlayerWorker.TypeName, () => new TestSeekingPlayerWorker());
		}



		/********************************************************************/
		/// <summary>
		/// Load the test module using the given converters and player, and
		/// check that the player got the data it should
		/// </summary>
		/********************************************************************/
		private static void LoadModuleAndCheckPlayerData(AgentInfo[] converters, AgentInfo player, string[] expectedReadOrder, string expectedConverterName, string expectedMark, int numberOfConversions)
		{
			TestAgentManager agentManager = new TestAgentManager(converters, [ player ]);

			using (TestLoader testLoader = new TestLoader(TestModuleData.BuildOriginalModule()))
			{
				Loader loader = new Loader(agentManager, null, new TestPlayerFactory());

				Assert.IsTrue(loader.FindPlayer(testLoader, out string errorMessage), errorMessage);
				Assert.AreEqual(string.Empty, errorMessage);

				Assert.IsTrue(loader.LoadModule(testLoader, out errorMessage), errorMessage);
				Assert.AreEqual(string.Empty, errorMessage);

				// The module has been converted, so the loader has to tell which
				// converter was used and which format the module originally had
				Assert.IsNotNull(loader.ConverterAgentInfo);
				Assert.AreEqual(expectedConverterName, loader.ConverterAgentInfo.TypeName);
				Assert.AreEqual(TestModuleData.OriginalFormat, loader.Format);

				CheckPlayerData(loader.WorkerAgent, expectedReadOrder, expectedMark, numberOfConversions);

				loader.Unload();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check that the player read the converted music data and the
		/// original sample data in the right order
		/// </summary>
		/********************************************************************/
		private static void CheckPlayerData(IAgentWorker workerAgent, string[] expectedReadOrder, string expectedMark, int numberOfConversions)
		{
			TestPlayerWorkerBase player = workerAgent as TestPlayerWorkerBase;
			Assert.IsNotNull(player);

			// The data has to arrive in the order the player asked for it
			CollectionAssert.AreEqual(expectedReadOrder, player.ReadOrder);

			// The music data has been converted
			Assert.AreEqual(expectedMark, player.Mark);
			CollectionAssert.AreEqual(TestModuleData.SampleLengths, player.SampleLengths);
			CollectionAssert.AreEqual(TestModuleData.GetSongData(numberOfConversions), player.SongData);
			CollectionAssert.AreEqual(TestModuleData.GetMoreSongData(numberOfConversions), player.MoreSongData);

			// The sample data is untouched by the converters
			for (int i = 0; i < TestModuleData.NumberOfSamples; i++)
			{
				Assert.AreEqual(TestModuleData.SampleLengths[i], player.SampleReadCount[i], $"Wrong number of bytes read for sample {i}");
				CollectionAssert.AreEqual(TestModuleData.GetSampleData(i), player.SampleData[i], $"Wrong sample data for sample {i}");
			}
		}
		#endregion
	}
}
