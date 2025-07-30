/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Containers
{
	/// <summary>
	/// Holds static global information
	/// </summary>
	internal class GlobalInfo
	{
		public int StartTime { get; set; }
		public int EndTime { get; set; }

		public ushort NewTempo { get; set; }
		public ushort Tune { get; set; }
		public ushort Volume { get; set; }

		public ulong[] TracksEnabled { get; set; }
		public List<Instrument> Instruments { get; } = new List<Instrument>();
		public int[] TrackStartPositions { get; set; }
		public TrackInfo[] TracksInfo { get; set; }
	}
}
