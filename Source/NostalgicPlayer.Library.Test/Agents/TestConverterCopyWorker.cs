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
	/// Converter which copies the sample data into the converter stream
	/// </summary>
	internal class TestConverterCopyWorker : TestConverterWorkerBase
	{
		/// <summary>
		/// The name this converter is registered with
		/// </summary>
		public const string TypeName = "Test Converter (Copy)";

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
			byte[] sampleData = new byte[length];
			moduleStream.ReadInto(sampleData, 0, length);

			converterStream.Write(sampleData, 0, length);
		}
	}
}
