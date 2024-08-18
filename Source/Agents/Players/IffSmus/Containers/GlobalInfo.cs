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
		public int StartTime;
		public int EndTime;

		public ushort NewTempo;
		public ushort Tune;
		public ushort Volume;

		public ulong[] TracksEnabled;
		public List<Instrument> Instruments = new List<Instrument>();
		public int[] TrackStartPositions;
		public TrackInfo[] TracksInfo;
	}
}
