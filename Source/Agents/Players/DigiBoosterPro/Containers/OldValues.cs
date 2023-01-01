/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Holds old parameter values for effects supporting parameter reuse
	/// </summary>
	internal struct OldValues
	{
		public uint8_t VolumeSlide;				// Effect A00
		public uint8_t PanningSlide;			// Effect P00
		public uint8_t PortamentoUp;			// Effect 100
		public uint8_t PortamentoDown;			// Effect 200
		public uint8_t PortamentoSpeed;			// Effect 300
		public uint8_t VolumeSlide5;			// Effect 500
		public uint8_t Vibrato;					// Effect 400
		public uint8_t Vibrato6;				// Effect 600
	}
}
