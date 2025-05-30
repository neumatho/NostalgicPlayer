/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Player can provide extra information about the module they are playing
	/// </summary>
	public interface IModuleInformation
	{
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		bool GetInformationString(int line, out string description, out string value);

		/// <summary>
		/// Return all module information changed since last call
		/// </summary>
		ModuleInfoChanged[] GetChangedInformation();
	}
}
