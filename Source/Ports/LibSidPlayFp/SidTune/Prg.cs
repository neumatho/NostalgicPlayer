/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidPlayFp;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp.SidTune
{
	/// <summary>
	/// Handle loading of the PRG file format
	/// </summary>
	internal sealed class Prg : SidTuneBase
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static SidTuneBase Load(string fileName, byte[] dataBuf)
		{
			string ext = Path.GetExtension(fileName);
			if ((ext != ".prg") && (ext != ".c64"))
				return null;

			if (dataBuf.Length < 2)
				throw new LoadErrorException(Resources.IDS_SID_ERR_TRUNCATED);

			Prg tune = new Prg();
			tune.Load();

			return tune;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Load()
		{
			info.formatString = Resources.IDS_SID_FORMAT_PRG;

			// Automatic settings
			info.songs = 1;
			info.startSong = 1;
			info.compatibility = SidTuneInfo.compatibility_t.COMPATIBILITY_BASIC;

			// Create the speed/clock setting table
			ConvertOldStyleSpeedToTables(~0U, info.clockSpeed);
		}
		#endregion
	}
}
