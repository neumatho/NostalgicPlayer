/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Interfaces;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Containers
{
	/// <summary>
	/// Different configuration settings for the mixer
	/// </summary>
	public class MixerConfiguration
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerConfiguration()
		{
			ChannelsEnabled = new bool[IChannel.MaxNumberOfChannels];
			Array.Fill(ChannelsEnabled, true);
		}



		/********************************************************************/
		/// <summary>
		/// The stereo separation in percent
		/// </summary>
		/********************************************************************/
		public int StereoSeparator
		{
			get; set;
		} = 100;



		/********************************************************************/
		/// <summary>
		/// The visuals latency in milliseconds
		/// </summary>
		/********************************************************************/
		public int VisualsLatency
		{
			get; set;
		} = 0;



		/********************************************************************/
		/// <summary>
		/// Indicate if interpolation is enabled
		/// </summary>
		/********************************************************************/
		public bool EnableInterpolation
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Swap left and right speakers
		/// </summary>
		/********************************************************************/
		public bool SwapSpeakers
		{
			get; set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Indicate if Amiga filter should be emulated
		/// </summary>
		/********************************************************************/
		public bool EnableAmigaFilter
		{
			get; set;
		} = true;



		/********************************************************************/
		/// <summary>
		/// Holds an array telling which channels to enable/disable. If null,
		/// all channels are enabled
		/// </summary>
		/********************************************************************/
		public bool[] ChannelsEnabled
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// An instance that implements extra channels to the mixer
		/// </summary>
		/********************************************************************/
		public IExtraChannels ExtraChannels
		{
			get; set;
		} = null;
	}
}
