/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Dns.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Track : IDeepCloneable<Track>
	{
		public ubyte Num;
		public bool Off;
		public bool KeyDown;
		public ubyte AssignedVoiceNum;

		public ubyte Pt;				// Pattern number
		public sbyte Tr;				// Transpose signed
		public ubyte St;				// Sound transpose

		public
		(
			udword Offset,
			ubyte Note,
			ubyte Sample,
			ubyte AddVolume
		) Pattern;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Track MakeDeepClone()
		{
			return (Track)MemberwiseClone();
		}
	}
}
