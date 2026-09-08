/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Test.Loaders;

namespace Polycode.NostalgicPlayer.Library.Test.Agents
{
	/// <summary>
	/// Base class for the test players. They do not play anything, but read
	/// the module exactly like a real player does and remember what they read
	/// and in which order
	/// </summary>
	internal abstract class TestPlayerWorkerBase : ModulePlayerAgentBase
	{
		/// <summary>
		/// The names added to ReadOrder for the different parts of the module
		/// </summary>
		public const string MarkRead = "Mark";
		/// <summary></summary>
		public const string SampleLengthsRead = "SampleLengths";
		/// <summary></summary>
		public const string SongDataRead = "SongData";
		/// <summary></summary>
		public const string MoreSongDataRead = "MoreSongData";

		/********************************************************************/
		/// <summary>
		/// Returns the file extensions this player understands. It does on
		/// purpose not match the extension of the test module, so the loader
		/// has to convert the module before the player is found
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => [ "cnv" ];



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;
			moduleStream.Seek(0, SeekOrigin.Begin);

			string mark = moduleStream.ReadMark();

			return (mark == TestModuleData.FirstConvertedMark) || (mark == TestModuleData.SecondConvertedMark) ? AgentResult.Ok : AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			ReadModule(fileInfo.ModuleStream);

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public override void Play()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Return the name added to ReadOrder when the given sample is read
		/// </summary>
		/********************************************************************/
		public static string GetSampleReadName(int sampleNumber)
		{
			return $"Sample{sampleNumber}";
		}



		/********************************************************************/
		/// <summary>
		/// Holds the format mark read from the module
		/// </summary>
		/********************************************************************/
		public string Mark
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample lengths read from the module
		/// </summary>
		/********************************************************************/
		public int[] SampleLengths
		{
			get;
		} = new int[TestModuleData.NumberOfSamples];



		/********************************************************************/
		/// <summary>
		/// Holds the music data read before the first sample
		/// </summary>
		/********************************************************************/
		public byte[] SongData
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the music data read between sample 1 and sample 2
		/// </summary>
		/********************************************************************/
		public byte[] MoreSongData
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the sample data read for each sample
		/// </summary>
		/********************************************************************/
		public sbyte[][] SampleData
		{
			get;
		} = new sbyte[TestModuleData.NumberOfSamples][];



		/********************************************************************/
		/// <summary>
		/// Holds the number of bytes actually read for each sample
		/// </summary>
		/********************************************************************/
		public int[] SampleReadCount
		{
			get;
		} = new int[TestModuleData.NumberOfSamples];



		/********************************************************************/
		/// <summary>
		/// Holds the order in which the different parts of the module has
		/// been read
		/// </summary>
		/********************************************************************/
		public List<string> ReadOrder
		{
			get;
		} = new List<string>();

		#region Methods used by the derived players
		/********************************************************************/
		/// <summary>
		/// Read the module in the way this player does it
		/// </summary>
		/********************************************************************/
		protected abstract void ReadModule(ModuleStream moduleStream);



		/********************************************************************/
		/// <summary>
		/// Read the header of the module
		/// </summary>
		/********************************************************************/
		protected void LoadHeader(ModuleStream moduleStream)
		{
			Mark = moduleStream.ReadMark();
			ReadOrder.Add(MarkRead);

			for (int i = 0; i < SampleLengths.Length; i++)
				SampleLengths[i] = (int)moduleStream.Read_B_UINT32();

			ReadOrder.Add(SampleLengthsRead);
		}



		/********************************************************************/
		/// <summary>
		/// Read the music data stored before the first sample
		/// </summary>
		/********************************************************************/
		protected void LoadSongData(ModuleStream moduleStream)
		{
			SongData = new byte[TestModuleData.SongDataLength];
			moduleStream.ReadInto(SongData, 0, SongData.Length);

			ReadOrder.Add(SongDataRead);
		}



		/********************************************************************/
		/// <summary>
		/// Read the music data stored between sample 1 and sample 2
		/// </summary>
		/********************************************************************/
		protected void LoadMoreSongData(ModuleStream moduleStream)
		{
			MoreSongData = new byte[TestModuleData.MoreSongDataLength];
			moduleStream.ReadInto(MoreSongData, 0, MoreSongData.Length);

			ReadOrder.Add(MoreSongDataRead);
		}



		/********************************************************************/
		/// <summary>
		/// Read the sample data for a single sample. This is done exactly as
		/// a real player does it, which means the player does not know if the
		/// module has been converted or not
		/// </summary>
		/********************************************************************/
		protected void LoadSampleData(ModuleStream moduleStream, int sampleNumber)
		{
			int length = SampleLengths[sampleNumber];
			sbyte[] sampleData = new sbyte[length];

			SampleReadCount[sampleNumber] = moduleStream.ReadSampleData(sampleData, length);

			SampleData[sampleNumber] = sampleData;
			ReadOrder.Add(GetSampleReadName(sampleNumber));
		}
		#endregion
	}
}
