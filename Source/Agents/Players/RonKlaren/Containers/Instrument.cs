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
		public int SampleNumber;
		public int VibratoNumber;
		public InstrumentType Type;
		public byte PhaseSpeed;
		public byte PhaseLengthInWords;
		public byte VibratoSpeed;
		public byte VibratoDepth;
		public byte VibratoDelay;
		public AdsrPoint[] Adsr = ArrayHelper.InitializeArray<AdsrPoint>(4);
		public sbyte PhaseValue;
		public bool PhaseDirection;
		public byte PhasePosition;

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
