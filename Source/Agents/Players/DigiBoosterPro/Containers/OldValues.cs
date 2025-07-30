/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DigiBoosterPro.Containers
{
	/// <summary>
	/// Holds old parameter values for effects supporting parameter reuse
	/// </summary>
	internal class OldValues : IDeepCloneable<OldValues>
	{
		public uint8_t VolumeSlide { get; set; }		// Effect A00
		public uint8_t PanningSlide { get; set; }		// Effect P00
		public uint8_t PortamentoUp { get; set; }		// Effect 100
		public uint8_t PortamentoDown { get; set; }		// Effect 200
		public uint8_t PortamentoSpeed { get; set; }	// Effect 300
		public uint8_t VolumeSlide5 { get; set; }		// Effect 500
		public uint8_t Vibrato { get; set; }			// Effect 400
		public uint8_t Vibrato6 { get; set; }			// Effect 600

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public OldValues MakeDeepClone()
		{
			return (OldValues)MemberwiseClone();
		}
	}
}
