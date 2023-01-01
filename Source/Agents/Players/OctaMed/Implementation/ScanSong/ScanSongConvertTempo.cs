/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.ScanSong
{
	/// <summary>
	/// 
	/// </summary>
	internal class ScanSongConvertTempo : ScanSong
	{
		private static readonly byte[] bpmVals = { 179, 164, 152, 141, 131, 123, 116, 110, 104 };

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void CmdOperation(MedCmd cmd)
		{
			byte data;

			if ((cmd.GetCmd() == 0x0f) && ((data = cmd.GetDataB()) != 0) && (data <= 240))
				cmd.SetData(data >= 10 ? (byte)99 : bpmVals[data - 1]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void SubSongOperation(SubSong ss)
		{
			ss.SetTempoMode(true);		// BPM tempo
			ss.SetTempoLpb(4);

			ushort oldTempo = ss.GetTempoBpm();
			ss.SetTempoBpm(oldTempo >= 10 ? (ushort)99 : bpmVals[oldTempo - 1]);
		}
		#endregion
	}
}
