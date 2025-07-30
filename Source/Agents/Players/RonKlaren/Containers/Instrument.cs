/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
{
	/// <summary>
	/// Holds information about a single instrument
	/// </summary>
	internal class Instrument : IDeepCloneable<Instrument>
	{
		public int SampleNumber { get; set; }
		public int VibratoNumber { get; set; }
		public InstrumentType Type { get; set; }
		public byte PhaseSpeed { get; set; }
		public byte PhaseLengthInWords { get; set; }
		public byte VibratoSpeed { get; set; }
		public byte VibratoDepth { get; set; }
		public byte VibratoDelay { get; set; }
		public AdsrPoint[] Adsr { get; set; } = ArrayHelper.InitializeArray<AdsrPoint>(4);
		public sbyte PhaseValue { get; set; }
		public bool PhaseDirection { get; set; }
		public byte PhasePosition { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Instrument MakeDeepClone()
		{
			return (Instrument)MemberwiseClone();
		}
	}
}
