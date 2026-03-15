/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.VoodooSupremeSynthesizer.Containers
{
	/// <summary>
	/// Different track effects
	/// </summary>
	internal enum Effect
	{
		Gosub = 0x81,
		Return = 0x82,
		StartLoop = 0x83,
		DoLoop = 0x84,
		SetSample = 0x85,
		SetVolumeEnvelope = 0x86,
		SetPeriodTable = 0x87,
		SetWaveformTable = 0x88,
		Portamento = 0x89,
		SetTranspose = 0x8a,
		Goto = 0x8b,
		SetResetFlags = 0x8c,
		SetWaveformMask = 0x8d,
		NoteCut = 0xff
	}
}
