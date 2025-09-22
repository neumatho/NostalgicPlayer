/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for module player agents
	/// </summary>
	public abstract class ModulePlayerAgentBase : PlayerAgentBase, IModulePlayerAgent
	{
		private static readonly SubSongInfo subSongInfo = new SubSongInfo(1, 0);

		/// <summary>
		/// Holds the mixer frequency. It is only set if BufferMode is set in the SupportFlags.
		/// </summary>
		protected uint mixerFreq;

		/// <summary>
		/// Holds the number of channels to output. It is only set if BufferMode is set in the SupportFlags.
		/// </summary>
		protected int mixerChannels;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		protected ModulePlayerAgentBase()
		{
			VirtualChannels = null;
			PlayingFrequency = 50.0f;
		}

		#region IModulePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public virtual ModulePlayerSupportFlag SupportFlags => ModulePlayerSupportFlag.None;



		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Load(PlayerFileInfo fileInfo, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Return a string containing a warning string. If there is no
		/// warning, an empty string is returned
		/// </summary>
		/********************************************************************/
		public virtual string GetWarning()
		{
			return string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public virtual bool InitPlayer(out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the current song
		/// </summary>
		/********************************************************************/
		public virtual bool InitSound(int songNumber, out string errorMessage)
		{
			errorMessage = string.Empty;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the current song
		/// </summary>
		/********************************************************************/
		public virtual void CleanupSound()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Is only called if BufferMode is set in the SupportFlags. It tells
		/// your player what frequency the NostalgicPlayer mixer is using.
		/// You can use it if you want or you can use your own output
		/// frequency, but if you also using BufferDirect, you need to use
		/// this frequency and number of channels
		/// </summary>
		/********************************************************************/
		public virtual void SetOutputFormat(uint mixerFrequency, int channels)
		{
			mixerFreq = mixerFrequency;
			mixerChannels = channels;
		}



		/********************************************************************/
		/// <summary>
		/// Is only called if BufferDirect is set in the SupportFlags. It
		/// tells your player about the different mixer settings you need to
		/// take care of
		/// </summary>
		/********************************************************************/
		public virtual void ChangeMixerConfiguration(PlayerMixerInfo mixerInfo)
		{
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public abstract void Play();



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses
		/// </summary>
		/********************************************************************/
		public virtual SpeakerFlag SpeakerFlags => SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;



		/********************************************************************/
		/// <summary>
		/// Return information about sub-songs
		/// </summary>
		/********************************************************************/
		public virtual SubSongInfo SubSongs => subSongInfo;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module want to reserve
		/// </summary>
		/********************************************************************/
		public virtual int VirtualChannelCount => ModuleChannelCount;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the module use
		/// </summary>
		/********************************************************************/
		public virtual int ModuleChannelCount => 4;



		/********************************************************************/
		/// <summary>
		/// Returns all the instruments available in the module. If none,
		/// null is returned
		/// </summary>
		/********************************************************************/
		public virtual IEnumerable<InstrumentInfo> Instruments => null;



		/********************************************************************/
		/// <summary>
		/// Returns all the samples available in the module. If none, null
		/// is returned
		/// </summary>
		/********************************************************************/
		public virtual IEnumerable<SampleInfo> Samples => null;



		/********************************************************************/
		/// <summary>
		/// Holds all the virtual channel instances used to play the samples
		/// </summary>
		/********************************************************************/
		public virtual IChannel[] VirtualChannels
		{
			get; set;
		}



		/********************************************************************/
		/// <summary>
		/// Holds the channels used by visuals. Only needed for players using
		/// buffer mode if possible
		/// </summary>
		/********************************************************************/
		public virtual ChannelChanged[] VisualChannels => null;



		/********************************************************************/
		/// <summary>
		/// Return an effect master instance if the player adds extra mixer
		/// effects to the output
		/// </summary>
		/********************************************************************/
		public virtual IEffectMaster EffectMaster => null;



		/********************************************************************/
		/// <summary>
		/// Return the current playing frequency
		/// </summary>
		/********************************************************************/
		public virtual float PlayingFrequency
		{
			get; protected set;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current state of the Amiga filter
		/// </summary>
		/********************************************************************/
		public bool AmigaFilter
		{
			get; protected set;
		} = false;



		/********************************************************************/
		/// <summary>
		/// Event called when the player change sub-song
		/// </summary>
		/********************************************************************/
		public event SubSongChangedEventHandler SubSongChanged;
		#endregion

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this every time your player change the sub-song
		/// </summary>
		/********************************************************************/
		protected void OnSubSongChanged(SubSongChangedEventArgs e)
		{
			if (!doNotTrigEvents && (SubSongChanged != null))
				SubSongChanged(this, e);
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency from the BPM you give and change the
		/// playing speed
		/// </summary>
		/********************************************************************/
		protected void SetBpmTempo(ushort bpm)
		{
			PlayingFrequency = bpm / 2.5f;
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the frequency from an Amiga CIA value and change the
		/// playing speed
		/// </summary>
		/********************************************************************/
		protected void SetCiaTimerTempo(ushort ciaTimerValue)
		{
			PlayingFrequency = 709379.0f / ciaTimerValue;
		}



		/********************************************************************/
		/// <summary>
		/// Return the frequency from a period
		/// </summary>
		/********************************************************************/
		protected uint PeriodToFrequency(ushort period)
		{
			return period != 0 ? 3546895U / period : 0;
		}
		#endregion
	}
}
