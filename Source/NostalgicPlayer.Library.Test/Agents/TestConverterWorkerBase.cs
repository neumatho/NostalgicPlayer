/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Test.Loaders;

namespace Polycode.NostalgicPlayer.Library.Test.Agents
{
	/// <summary>
	/// Base class for the test converters. It converts the music data into
	/// another format and leaves it to the derived class to decide how the
	/// sample data is stored in the converter stream
	/// </summary>
	internal abstract class TestConverterWorkerBase : ModuleConverterAgentBase
	{
		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter can read
		/// </summary>
		/********************************************************************/
		protected abstract string SourceMark
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter creates
		/// </summary>
		/********************************************************************/
		protected abstract string TargetMark
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			ModuleStream moduleStream = fileInfo.ModuleStream;
			moduleStream.Seek(0, SeekOrigin.Begin);

			return moduleStream.ReadMark() == SourceMark ? AgentResult.Ok : AgentResult.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		/********************************************************************/
		public override AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			ModuleStream moduleStream = fileInfo.ModuleStream;
			moduleStream.Seek(0, SeekOrigin.Begin);

			// Skip the mark of the source format
			moduleStream.ReadMark();

			int[] sampleLengths = new int[TestModuleData.NumberOfSamples];

			for (int i = 0; i < sampleLengths.Length; i++)
				sampleLengths[i] = ReadSampleLength(moduleStream);

			// Write the header of the new format
			converterStream.WriteMark(TargetMark);

			foreach (int length in sampleLengths)
				converterStream.Write_B_UINT32((uint)length);

			// The data has to be written in the same order as it is read, since
			// the sample data is stored between the music data
			ConvertSongData(moduleStream, converterStream, TestModuleData.SongDataLength);

			WriteSampleData(moduleStream, converterStream, sampleLengths[0]);
			WriteSampleData(moduleStream, converterStream, sampleLengths[1]);

			ConvertSongData(moduleStream, converterStream, TestModuleData.MoreSongDataLength);

			WriteSampleData(moduleStream, converterStream, sampleLengths[2]);

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Return the original format
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => TestModuleData.OriginalFormat;



		/********************************************************************/
		/// <summary>
		/// Read a single sample length from the module. The original format
		/// stores them as 16-bit values
		/// </summary>
		/********************************************************************/
		protected virtual int ReadSampleLength(ModuleStream moduleStream)
		{
			return moduleStream.Read_B_UINT16();
		}



		/********************************************************************/
		/// <summary>
		/// Store the sample data in the converter stream
		/// </summary>
		/********************************************************************/
		protected abstract void WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream, int length);

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Read a block of music data and write it in the new format. Every
		/// byte is increased by one, so the tests can see that the music data
		/// really has been converted
		/// </summary>
		/********************************************************************/
		private static void ConvertSongData(ModuleStream moduleStream, ConverterStream converterStream, int length)
		{
			byte[] songData = new byte[length];
			moduleStream.ReadInto(songData, 0, length);

			for (int i = 0; i < length; i++)
				songData[i]++;

			converterStream.Write(songData, 0, length);
		}
		#endregion
	}
}
