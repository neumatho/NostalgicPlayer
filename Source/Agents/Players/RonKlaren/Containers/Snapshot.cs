/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.RonKlaren.Containers
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
		public Snapshot(GlobalPlayingInfo playingInfo, VoiceInfo[] voices, Instrument[] instruments, sbyte[][] synthSamples)
		{
			PlayingInfo = playingInfo.MakeDeepClone();
			Voices = ArrayHelper.CloneObjectArray(voices);
			Instruments = ArrayHelper.CloneObjectArray(instruments);
			SynthSamples = ArrayHelper.CloneArray(synthSamples);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo PlayingInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public VoiceInfo[] Voices
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Instrument[] Instruments
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public sbyte[][] SynthSamples
		{
			get;
		}
	}
}
