/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Factory class to find and create the right instrument
	/// </summary>
	internal static class InstrumentFactory
	{
		/********************************************************************/
		/// <summary>
		/// Create the right instrument format based on the first bytes of
		/// the instrument file
		/// </summary>
		/********************************************************************/
		public static IInstrumentFormat CreateInstrumentFormat(byte[] firstBytes)
		{
			if (SynthesisFormat.Identify(firstBytes))
				return new SynthesisFormat();

			if (SampledSoundFormat.Identify(firstBytes))
				return new SampledSoundFormat();

			if (FormFormat.Identify(firstBytes))
				return new FormFormat();

			return null;
		}
	}
}
