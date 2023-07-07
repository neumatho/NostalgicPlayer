/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// If you support multiple formats and have a single identify method,
	/// which can find out the right format, you can derive from this interface
	/// to speed up the detection. Normally, the Identify() method will be called
	/// on every format you support, but if you derive from this interface, only
	/// the IdentifyFormat() method here will be called
	/// </summary>
	public interface IAgentMultipleFormatIdentify
	{
		/// <summary>
		/// Try to identify which format are used in the given stream and
		/// return the worker for the format if found
		/// </summary>
		IdentifyFormatInfo IdentifyFormat(Stream dataStream);
	}
}
