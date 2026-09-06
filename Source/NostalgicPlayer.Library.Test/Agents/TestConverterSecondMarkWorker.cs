/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Test.Loaders;

namespace Polycode.NostalgicPlayer.Library.Test.Agents
{
	/// <summary>
	/// Converter which takes the output of TestConverterMarkWorker and converts
	/// it into a third format. It marks the sample data as well, which makes it
	/// possible to test a chain of two converters that both mark
	/// </summary>
	internal class TestConverterSecondMarkWorker : TestConverterWorkerBase
	{
		/// <summary>
		/// The name this converter is registered with
		/// </summary>
		public const string TypeName = "Test Converter (Second Mark)";

		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter can read
		/// </summary>
		/********************************************************************/
		protected override string SourceMark => TestModuleData.FirstConvertedMark;



		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter creates
		/// </summary>
		/********************************************************************/
		protected override string TargetMark => TestModuleData.SecondConvertedMark;



		/********************************************************************/
		/// <summary>
		/// Read a single sample length from the module. The format created by
		/// the first converter stores them as 32-bit values
		/// </summary>
		/********************************************************************/
		protected override int ReadSampleLength(ModuleStream moduleStream)
		{
			return (int)moduleStream.Read_B_UINT32();
		}



		/********************************************************************/
		/// <summary>
		/// Store the sample data in the converter stream
		/// </summary>
		/********************************************************************/
		protected override void WriteSampleData(ModuleStream moduleStream, ConverterStream converterStream, int sampleNumber, int length)
		{
			moduleStream.SetSampleDataInfo(sampleNumber, length);
			converterStream.WriteSampleDataMarker(sampleNumber, length);
		}
	}
}
