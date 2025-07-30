/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.PumaTracker.Containers
{
	/// <summary>
	/// Holds playing information for a single voice
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public Track[] Track { get; set; }
		public int TrackPosition { get; set; }

		public byte RowCounter { get; set; }

		public InstrumentCommand[] VolumeCommands { get; set; }
		public byte VolumeCommandPosition { get; set; }

		public InstrumentCommand[] FrequencyCommands { get; set; }
		public byte FrequencyCommandPosition { get; set; }

		public sbyte InstrumentTranspose { get; set; }
		public sbyte NoteTranspose { get; set; }
		public byte TransposedNote { get; set; }

		public byte[] SampleData { get; set; }
		public ushort SampleLength { get; set; }
		public byte SampleNumber { get; set; }

		public ushort Period { get; set; }
		public byte Volume { get; set; }

		public short PortamentoAddValue { get; set; }

		public VoiceFlag VoiceFlag { get; set; }

		public byte VolumeSlideCounter { get; set; }
		public byte VolumeSlideRemainingTime { get; set; }
		public sbyte VolumeSlideDelta { get; set; }
		public sbyte VolumeSlideDirection { get; set; }
		public short VolumeSlideValue { get; set; }

		public byte FrequencyCounter { get; set; }
		public int FrequencyVaryAddValue { get; set; }
		public int FrequencyVaryValue { get; set; }

		public byte WaveformChangeCounter { get; set; }
		public sbyte WaveformAddValue { get; set; }
		public sbyte WaveformValue { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
