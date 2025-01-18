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
		public Track[] Track;
		public int TrackPosition;

		public byte RowCounter;

		public InstrumentCommand[] VolumeCommands;
		public byte VolumeCommandPosition;

		public InstrumentCommand[] FrequencyCommands;
		public byte FrequencyCommandPosition;

		public sbyte InstrumentTranspose;
		public sbyte NoteTranspose;
		public byte TransposedNote;

		public byte[] SampleData;
		public ushort SampleLength;
		public byte SampleNumber;

		public ushort Period;
		public byte Volume;

		public short PortamentoAddValue;

		public VoiceFlag VoiceFlag;

		public byte VolumeSlideCounter;
		public byte VolumeSlideRemainingTime;
		public sbyte VolumeSlideDelta;
		public sbyte VolumeSlideDirection;
		public short VolumeSlideValue;

		public byte FrequencyCounter;
		public int FrequencyVaryAddValue;
		public int FrequencyVaryValue;

		public byte WaveformChangeCounter;
		public sbyte WaveformAddValue;
		public sbyte WaveformValue;

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
