/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
{
	/// <summary>
	/// Different effects
	/// </summary>
	internal enum Effect
	{
		SetArpeggio = 0x80,
		SetPortamento = 0x81,
		SetInstrument = 0x82,
		EndSong = 0x83,
		ChangeAdsrSpeed = 0x84,
		EndSong2 = 0x85,
		EndOfTrack = 0xff
	}
}
