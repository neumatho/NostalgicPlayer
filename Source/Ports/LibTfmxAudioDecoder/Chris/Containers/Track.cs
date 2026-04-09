/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Track : IDeepCloneable<Track>
	{
		public ubyte Pt;
		public sbyte Tr;
		public bool On;					// Mute flag

		public Pattern Pattern = new Pattern();

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Track MakeDeepClone()
		{
			Track clone = (Track)MemberwiseClone();

			clone.Pattern = Pattern.MakeDeepClone();

			return clone;
		}
	}
}
