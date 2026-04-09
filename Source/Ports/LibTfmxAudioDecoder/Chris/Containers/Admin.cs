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
	internal class Admin : IDeepCloneable<Admin>
	{
		public sword Speed;			// Speed
		public sword Count;			// Speed count
		public c_int StartSpeed;
		public c_int StartSong;

		public bool Initialized;	// True => restartable
		public uword RandomWord;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Admin MakeDeepClone()
		{
			return (Admin)MemberwiseClone();
		}
	}
}
