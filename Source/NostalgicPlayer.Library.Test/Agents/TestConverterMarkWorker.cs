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
	/// Converter which marks the sample data instead of copying it into the
	/// converter stream
	/// </summary>
	internal class TestConverterMarkWorker : TestConverterWorkerBase
	{
		/// <summary>
		/// The name this converter is registered with
		/// </summary>
		public const string TypeName = "Test Converter (Mark)";

		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter can read
		/// </summary>
		/********************************************************************/
		protected override string SourceMark => TestModuleData.OriginalMark;



		/********************************************************************/
		/// <summary>
		/// The mark of the format this converter creates
		/// </summary>
		/********************************************************************/
		protected override string TargetMark => TestModuleData.FirstConvertedMark;



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
