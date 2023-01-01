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
	internal class ScanConvertOldVolToNew : ScanBlock
	{
		private readonly bool hex;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ScanConvertOldVolToNew(bool volHex)
		{
			hex = volHex;
		}

		#region Overrides
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		protected override void CmdOperation(MedCmd cmd)
		{
			if (cmd.GetCmd() == 0x0c)
			{
				byte data = cmd.GetData2();
				if (data != 0)
					cmd.SetData(data, 0);
				else
				{
					if (!hex)
					{
						data = cmd.GetDataB();
						if ((data != 0) && (data < 128))
						{
							data = (byte)((data >> 4) * 10 + (data & 0x0f));
							cmd.SetData(data, 0);
						}
					}
				}
			}
		}
		#endregion
	}
}
