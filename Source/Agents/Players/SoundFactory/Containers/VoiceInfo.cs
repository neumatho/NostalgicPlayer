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
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public int ChannelNumber { get; set; }

		public bool VoiceEnabled { get; set; }

		public uint StartPosition { get; set; }
		public uint CurrentPosition { get; set; }

		public byte CurrentInstrument { get; set; }

		public ushort NoteDuration { get; set; }
		public ushort NoteDuration2 { get; set; }
		public byte Note { get; set; }
		public sbyte Transpose { get; set; }

		public byte FineTune { get; set; }
		public ushort Period { get; set; }

		public byte CurrentVolume { get; set;}
		public byte Volume { get; set; }

		public ushort ActivePeriod { get; set; }
		public byte PortamentoCounter { get; set; }

		public bool ArpeggioFlag { get; set; }
		public byte ArpeggioCounter { get; set; }

		public byte VibratoDelay { get; set; }
		public byte VibratoCounter { get; set; }
		public byte VibratoCounter2 { get; set; }
		public short VibratoRelative { get; set; }
		public sbyte VibratoStep { get; set; }

		public byte TremoloCounter { get; set; }
		public sbyte TremoloStep { get; set; }
		public byte TremoloVolume { get; set; }

		public EnvelopeState EnvelopeState { get; set; }
		public byte EnvelopeCounter { get; set; }

		public byte PhasingCounter { get; set; }
		public sbyte PhasingStep { get; set; }
		public sbyte PhasingRelative { get; set; }

		public byte FilterCounter { get; set; }
		public sbyte FilterStep { get; set; }
		public byte FilterRelative { get; set; }

		public Stack<uint> Stack { get; set; }

		public byte NoteStartFlag { get; set; }
		public byte NoteStartFlag1 { get; set; }

		public sbyte[] PhasingBuffer { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			VoiceInfo clone = (VoiceInfo)MemberwiseClone();

			clone.Stack = new Stack<uint>(Stack);
			clone.PhasingBuffer = ArrayHelper.CloneArray(PhasingBuffer);

			return clone;
		}
	}
}
