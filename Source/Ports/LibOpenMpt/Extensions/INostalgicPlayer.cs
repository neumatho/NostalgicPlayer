/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Extensions
{
	/// <summary>
	/// This extension implements functionality needed by
	/// NostalgicPlayer which the normal API don't support
	/// </summary>
	public interface INostalgicPlayer : IExtension
	{
		/// <summary>
		/// Return sample information for the given sample
		/// </summary>
		SampleInformation GetSampleInformation(int32_t sampleNumber);

		/// <summary>
		/// Return true if the module uses surround
		/// </summary>
		bool DoesModuleUseSurround();

		/// <summary>
		/// Enable/disable surround
		/// </summary>
		void EnableSurround(bool enabled);

		/// <summary>
		/// Get current number of samples per tick
		/// </summary>
		uint GetSamplesPerTick();

		/// <summary>
		/// Will return the visualizer channels
		/// </summary>
		ChannelChanged[] GetVisualChannels();
	}
}
