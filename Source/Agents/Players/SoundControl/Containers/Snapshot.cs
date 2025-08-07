/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.SoundControl.Containers
{
	/// <summary>
	/// Holds all the information about the player state at a specific time
	/// </summary>
	internal class Snapshot : ISnapshot
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Snapshot(IGlobalPlayingInfo playingInfo, IVoiceInfo[] voices)
		{
			PlayingInfo = playingInfo.MakeDeepClone();
			Voices = ArrayHelper.CloneObjectArray(voices);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IGlobalPlayingInfo PlayingInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public IVoiceInfo[] Voices
		{
			get;
		}
	}
}
