/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Library.Loaders;

namespace Polycode.NostalgicPlayer.Library.Containers
{
	/// <summary>
	/// Different configuration settings for the player
	/// </summary>
	public class PlayerConfiguration
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayerConfiguration(IOutputAgent outputAgent, LoaderInfoBase loaderInfo, SurroundMode surroundMode, bool disableCenterSpeaker, MixerConfiguration mixerConfiguration)
		{
			OutputAgent = outputAgent;
			LoaderInfo = loaderInfo;
			SurroundMode = surroundMode;
			DisableCenterSpeaker = disableCenterSpeaker;
			MixerConfiguration = mixerConfiguration;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the output agent to use
		/// </summary>
		/********************************************************************/
		public IOutputAgent OutputAgent
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the loader object that has loaded the module
		/// </summary>
		/********************************************************************/
		public LoaderInfoBase LoaderInfo
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the surround mode to use
		/// </summary>
		/********************************************************************/
		public SurroundMode SurroundMode
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// If true, the output center speaker is ignored and the sound is
		/// mixed into the front left and right speakers instead
		/// </summary>
		/********************************************************************/
		public bool DisableCenterSpeaker
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the mixer configuration
		/// </summary>
		/********************************************************************/
		public MixerConfiguration MixerConfiguration
		{
			get;
		}
	}
}
