/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundFactory.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public Instrument DefaultInstrument { get; set; }
		public Dictionary<uint, Instrument> InstrumentLookup { get; set; }
		public Instrument[] SoundTable { get; set; }

		public bool FadeOutFlag { get; set; }
		public bool FadeInFlag { get; set; }
		public byte FadeOutVolume { get; set; }
		public byte FadeOutCounter { get; set; }
		public byte FadeOutSpeed { get; set;}

		public byte RequestCounter { get; set; }

		public HashSet<uint> TakenOpcodes { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			GlobalPlayingInfo clone = (GlobalPlayingInfo)MemberwiseClone();

			clone.InstrumentLookup = new Dictionary<uint, Instrument>(InstrumentLookup);
			clone.SoundTable = ArrayHelper.CloneArray(SoundTable);

			foreach (KeyValuePair<uint, Instrument> pair in InstrumentLookup)
			{
				Instrument clonedInstrument = pair.Value.MakeDeepClone();
				clone.InstrumentLookup[pair.Key] = clonedInstrument;

				// Replace all occurrences of the original instrument reference in the cloned sound table
				for (int i = 0; i < clone.SoundTable.Length; i++)
				{
					if (SoundTable[i] == pair.Value)
						clone.SoundTable[i] = clonedInstrument;
				}
			}

			clone.TakenOpcodes = new HashSet<uint>(TakenOpcodes);

			return clone;
		}
	}
}
