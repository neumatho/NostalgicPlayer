/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Library.Test.Loaders;

namespace Polycode.NostalgicPlayer.Library.Test.Agents
{
	/// <summary>
	/// Player which seeks around in the module instead of reading it strictly
	/// forwards. It first reads both blocks of music data and then reads the
	/// samples backwards
	/// </summary>
	internal class TestSeekingPlayerWorker : TestPlayerWorkerBase
	{
		/// <summary>
		/// The name this player is registered with
		/// </summary>
		public const string TypeName = "Test Seeking Player";

		/********************************************************************/
		/// <summary>
		/// Read all the music data first and then the samples in reverse
		/// order. The offsets used are the ones the module has when all the
		/// data is present, since that is the only thing a player knows
		/// about
		/// </summary>
		/********************************************************************/
		protected override void ReadModule(ModuleStream moduleStream)
		{
			moduleStream.Seek(0, SeekOrigin.Begin);
			LoadHeader(moduleStream);

			moduleStream.Seek(TestModuleData.GetConvertedSongDataOffset(), SeekOrigin.Begin);
			LoadSongData(moduleStream);

			// Skip over the first two samples to get to the second block of
			// music data
			moduleStream.Seek(TestModuleData.GetConvertedMoreSongDataOffset(), SeekOrigin.Begin);
			LoadMoreSongData(moduleStream);

			// Now take the samples backwards
			for (int i = TestModuleData.NumberOfSamples - 1; i >= 0; i--)
			{
				moduleStream.Seek(TestModuleData.GetConvertedSampleOffset(i), SeekOrigin.Begin);
				LoadSampleData(moduleStream, i);
			}
		}
	}
}
