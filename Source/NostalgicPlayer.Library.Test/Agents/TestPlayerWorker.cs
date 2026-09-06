/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Library.Test.Agents
{
	/// <summary>
	/// Player which reads the module strictly forwards, which means it never
	/// needs to seek
	/// </summary>
	internal class TestPlayerWorker : TestPlayerWorkerBase
	{
		/// <summary>
		/// The name this player is registered with
		/// </summary>
		public const string TypeName = "Test Player";

		/********************************************************************/
		/// <summary>
		/// Read the module from the beginning to the end, in the same order
		/// as the data is stored
		/// </summary>
		/********************************************************************/
		protected override void ReadModule(ModuleStream moduleStream)
		{
			LoadHeader(moduleStream);
			LoadSongData(moduleStream);

			LoadSampleData(moduleStream, 0);
			LoadSampleData(moduleStream, 1);

			LoadMoreSongData(moduleStream);

			LoadSampleData(moduleStream, 2);
		}
	}
}
