/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Formats
{
	/// <summary>
	/// All supported module formats derive from this interface
	/// </summary>
	internal interface IFormatSaver
	{
		/// <summary>
		/// Save in module into the implemented format
		/// </summary>
		bool SaveModule(Mo3Module module, ModuleStream moduleStream, ConverterStream converterStream, out string errorMessage);
	}
}
