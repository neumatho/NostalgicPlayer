/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum MixLevels : uint8
	{
		Original = 0,
		v1_17RC1 = 1,
		v1_17RC2 = 2,
		v1_17RC3 = 3,
		Compatible = 4,
		CompatibleFT2 = 5,

		NumMixLevels
	}
}
