/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.IffSmus.Instruments
{
	/// <summary>
	/// Contains play information for a single voice when using Form instrument
	/// </summary>
	internal class FormPlayInfo : IDeepCloneable<FormPlayInfo>
	{
		public sbyte[] LoopSampleData;
		public uint LoopStart;
		public ushort LoopLengthInWords;

		public ushort MappedNote;
		public ushort VolumeMultiply;

		public byte Octave;
		public byte Note;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public FormPlayInfo MakeDeepClone()
		{
			return (FormPlayInfo)MemberwiseClone();
		}
	}
}
