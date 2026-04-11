/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
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
		public Snapshot(PlayerInfo playerInfo, VoiceVars[] voices, udword rate)
		{
			PlayerInfo = playerInfo.MakeDeepClone();
			Voices = ArrayHelper.CloneObjectArray(voices);
			Rate = rate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PlayerInfo PlayerInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public VoiceVars[] Voices
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public udword Rate
		{
			get;
		}
	}
}
