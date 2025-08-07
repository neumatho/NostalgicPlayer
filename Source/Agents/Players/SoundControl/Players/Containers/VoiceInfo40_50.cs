/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Holds playing information for a single voice.
	/// Only used by the SoundControl 4.0/5.0 player
	/// </summary>
	internal class VoiceInfo40_50 : IVoiceInfo
	{
		public ushort WaitCounter { get; set; }
		public byte[] Track { get; set; }
		public int TrackPosition { get; set; }

		public short Transpose { get; set; }
		public ushort TransposedNote { get; set; }
		public ushort SampleTransposedNote { get; set; }
		public ushort Period { get; set; }

		public ushort InstrumentNumber { get; set; }
		public ushort SampleCommandWaitCounter { get; set; }
		public SampleCommandInfo[] SampleCommandList { get; set; }
		public int SampleCommandPosition { get; set; }
		public PlaySampleCommand PlaySampleCommand { get; set; }

		public ushort SampleNumber { get; set; }
		public Sample Sample { get; set; }
		public ushort SampleLength { get; set; }
		public sbyte[] SampleData { get; set; }

		public ushort Volume { get; set; }

		public Stack<int> RepeatListStack { get; set; } = new Stack<int>();

		public EnvelopeCommand EnvelopeCommand { get; set; }
		public ushort EnvelopeCounter { get; set; }
		public short EnvelopeVolume { get; set; }
		public bool StartEnvelopeRelease { get; set; }

		public ushort[] RepeatListValues { get; set; } = new ushort[71];

		// These properties store the hardware values.
		// These are needed since the player can only change some of the values
		public sbyte[] Hardware_SampleData { get; set; }
		public uint Hardware_StartOffset { get; set; }
		public ushort Hardware_SampleLength { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IVoiceInfo MakeDeepClone()
		{
			VoiceInfo40_50 clone = (VoiceInfo40_50)MemberwiseClone();

			clone.RepeatListStack = new Stack<int>(RepeatListStack);
			clone.RepeatListValues = ArrayHelper.CloneArray(RepeatListValues);

			return clone;
		}
	}
}
