/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.Mo3Converter.Containers
{
	/// <summary>
	/// Holds a single row
	/// </summary>
	internal class TrackRow
	{
		public byte Instrument;
		public byte Note;
		public List<(Effect, byte)> Effects;
	}
}
