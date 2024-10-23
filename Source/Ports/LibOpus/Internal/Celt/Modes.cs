/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Modes
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static CeltMode Opus_Custom_Mode_Create(opus_int32 Fs, c_int frame_size, out OpusError error)
		{
			for (c_int i = 0; i < Constants.Total_Modes; i++)
			{
				for (c_int j = 0; j < 4; j++)
				{
					if ((Fs == Static_Modes.Static_Mode_List[i].Fs) && ((frame_size << j) == (Static_Modes.Static_Mode_List[i].shortMdctSize * Static_Modes.Static_Mode_List[i].nbShortMdcts)))
					{
						error = OpusError.Ok;

						return Static_Modes.Static_Mode_List[i];
					}
				}
			}

			error = OpusError.Bad_Arg;

			return null;
		}
	}
}
