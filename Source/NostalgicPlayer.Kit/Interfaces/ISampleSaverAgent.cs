/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can save samples
	/// </summary>
	public interface ISampleSaverAgent : IAgentWorker
	{
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		SampleSaverSupportFlag SaverSupportFlags { get; }

		/// <summary>
		/// Return the file extension that is used by the saver
		///
		/// Has to be in lowercase
		/// </summary>
		string FileExtension { get; }

		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage);

		/// <summary>
		/// Cleanup the saver
		/// </summary>
		void CleanupSaver();

		/// <summary>
		/// Save the header of the sample
		/// </summary>
		void SaveHeader(Stream stream);

		/// <summary>
		/// Save a part of the sample
		/// </summary>
		void SaveData(Stream stream, int[] buffer, int length);

		/// <summary>
		/// Save the tail of the sample
		/// </summary>
		void SaveTail(Stream stream);
	}
}
