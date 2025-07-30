/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Midi_Macro_Data
	{
		public Midi_Macro[] Param { get; } = ArrayHelper.InitializeArray<Midi_Macro>(16);
		public Midi_Macro[] Fixed { get; } = ArrayHelper.InitializeArray<Midi_Macro>(128);
	}
}
