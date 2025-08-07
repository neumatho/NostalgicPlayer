/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Players.Containers
{
	/// <summary>
	/// Holds playing information for a single voice.
	/// Only used by the SoundControl 3.x player
	/// </summary>
	internal class VoiceInfo3x : IVoiceInfo
	{
		public ushort WaitCounter { get; set; }
		public byte[] Track { get; set; }
		public int TrackPosition { get; set; }

		public sbyte Transpose { get; set; }		// Only used by 3.2 player

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public IVoiceInfo MakeDeepClone()
		{
			return (VoiceInfo3x)MemberwiseClone();
		}
	}
}
