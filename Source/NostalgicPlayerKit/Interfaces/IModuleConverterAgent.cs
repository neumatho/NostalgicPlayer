/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can convert one module format to another
	/// </summary>
	public interface IModuleConverterAgent : IAgentWorker
	{
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(PlayerFileInfo fileInfo);

		/// <summary>
		/// Return the size of the converted module without samples if
		/// possible. 0 means unknown
		/// </summary>
		int ConvertedModuleLength(PlayerFileInfo fileInfo);

		/// <summary>
		/// Convert the module and store the result in the stream given
		/// </summary>
		AgentResult Convert(PlayerFileInfo fileInfo, ConverterStream converterStream, out string errorMessage);

		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		string OriginalFormat { get; }
	}
}
