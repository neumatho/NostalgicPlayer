/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Kit.Containers
{
	/// <summary>
	/// Used to tell visual agents about a channel change
	/// </summary>
	public class ChannelChanged
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public ChannelChanged(bool enabled, bool muted, bool noteKicked, short sampleNumber, sbyte octave, sbyte note, uint sampleLength, bool looping, bool samplePositionRelative, int? samplePosition, ushort? volume, uint? frequency)
		{
			Enabled = enabled;
			Muted = muted;
			NoteKicked = noteKicked;
			SampleNumber = sampleNumber;
			Octave = octave;
			Note = note;
			SampleLength = sampleLength;
			Looping = looping;
			SamplePositionRelative = samplePositionRelative;
			SamplePosition = samplePosition;
			Volume = volume;
			Frequency = frequency;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the channel are enabled (via mixer settings)
		/// </summary>
		/********************************************************************/
		public bool Enabled
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the current channel has been muted
		/// </summary>
		/********************************************************************/
		public bool Muted
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Tells if a new note has just started playing
		/// </summary>
		/********************************************************************/
		public bool NoteKicked
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current sample number being played or -1 if unknown
		/// </summary>
		/********************************************************************/
		public short SampleNumber
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current octave the note is played with or -1 if unknown
		/// </summary>
		/********************************************************************/
		public sbyte Octave
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current note which is played or -1 if unknown
		/// </summary>
		/********************************************************************/
		public sbyte Note
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the length of the sample in samples
		/// </summary>
		/********************************************************************/
		public uint SampleLength
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if the sample is looping or not
		/// </summary>
		/********************************************************************/
		public bool Looping
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// If sample position has a value, this indicates if the position is
		/// a relative or absolute position
		/// </summary>
		/********************************************************************/
		public bool SamplePositionRelative
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds new sample position if set
		/// </summary>
		/********************************************************************/
		public int? SamplePosition
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current volume on the channel
		/// </summary>
		/********************************************************************/
		public ushort? Volume
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the current frequency on the channel
		/// </summary>
		/********************************************************************/
		public uint? Frequency
		{
			get;
		}
	}
}
