/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Library.Sound.Mixer.Containers
{
	/// <summary>
	/// Contains mixer information needed when mixing
	/// </summary>
	internal class MixerInfo : PlayerMixerInfo
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerInfo()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MixerInfo(MixerInfo source)
		{
			StereoSeparator = source.StereoSeparator;
			InterpolationMode = source.InterpolationMode;
			SurroundMode = source.SurroundMode;
			SwapSpeakers = source.SwapSpeakers;
			EmulateFilter = source.EmulateFilter;
			EnableEqualizer = source.EnableEqualizer;
			DisableCenterSpeaker = source.DisableCenterSpeaker;

			Array.Copy(source.ChannelsEnabled, ChannelsEnabled, ChannelsEnabled.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if speaker channels should be swapped
		/// </summary>
		/********************************************************************/
		public bool SwapSpeakers
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// If set and a player set the filter, the mixer will add a low-pass
		/// filter to the output. This is the same, when the power led is
		/// turned on, on the Amiga
		/// </summary>
		/********************************************************************/
		public bool EmulateFilter
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Indicate if equalizer is enabled
		/// </summary>
		/********************************************************************/
		public bool EnableEqualizer
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// If true, the output center speaker is ignored and the sound is
		/// mixed into the front left and right speakers instead
		/// </summary>
		/********************************************************************/
		public bool DisableCenterSpeaker
		{
			get; init;
		}
	}
}
