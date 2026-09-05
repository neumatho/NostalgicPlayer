/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Dithers
{
	/// <summary>
	/// 
	/// </summary>
	internal class DitherNamesOpenMpt
	{
		/********************************************************************/
		/// <summary>
		/// Returns the name of the given dither mode (C++ GetModeName())
		/// </summary>
		/********************************************************************/
		public string GetModeName(size_t mode)
		{
			switch (mode)
			{
				case 0:
					// No dither
					return "no";

				case 1:
					// Chosen by OpenMPT code, might change
					return "default";

				case 2:
					// Rectangular, 0.5 bit depth, no noise shaping (original ModPlug Tracker)
					return "0.5 bit";

				case 3:
					// Rectangular, 1 bit depth, simple 1st order noise shaping
					return "1 bit";

				default:
					return string.Empty;
			}
		}
	}
}
