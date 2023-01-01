/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DeltaMusic10.Containers
{
	/// <summary>
	/// The different effects
	/// </summary>
	internal enum Effect
	{
		None = 0x00,						// 00
		SetSpeed,							// 01
		SlideUp,							// 02
		SlideDown,							// 03
		SetFilter,							// 04
		SetVibratoWait,						// 05
		SetVibratoStep,						// 06
		SetVibratoLength,					// 07
		SetBendRate,						// 08
		SetPortamento,						// 09
		SetVolume,							// 0A
		SetArp1,							// 0B
		SetArp2,							// 0C
		SetArp3,							// 0D
		SetArp4,							// 0E
		SetArp5,							// 0F
		SetArp6,							// 10
		SetArp7,							// 11
		SetArp8,							// 12
		SetArp1_5,							// 13
		SetArp2_6,							// 14
		SetArp3_7,							// 15
		SetArp4_8,							// 16
		SetAttackStep,						// 17
		SetAttackDelay,						// 18
		SetDecayStep,						// 19
		SetDecayDelay,						// 1A
		SetSustain1,						// 1B
		SetSustain2,						// 1C
		SetReleaseStep,						// 1D
		SetReleaseDelay						// 1E
	}
}
