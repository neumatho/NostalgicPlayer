/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Events;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for output agents
	/// </summary>
	public abstract class OutputAgentBase : IOutputAgent
	{
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the output agent supports
		/// </summary>
		/********************************************************************/
		public virtual OutputSupportFlag SupportFlags => OutputSupportFlag.None;



		/********************************************************************/
		/// <summary>
		/// Will initialize the output driver
		/// </summary>
		/********************************************************************/
		public virtual AgentResult Initialize(out string errorMessage)
		{
			errorMessage = string.Empty;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Will shutdown the output driver
		/// </summary>
		/********************************************************************/
		public virtual void Shutdown()
		{
		}



		/********************************************************************/
		/// <summary>
		/// Tell the engine to begin playing
		/// </summary>
		/********************************************************************/
		public abstract void Play();



		/********************************************************************/
		/// <summary>
		/// Tell the engine to stop playing
		/// </summary>
		/********************************************************************/
		public abstract void Stop();



		/********************************************************************/
		/// <summary>
		/// Tell the engine to pause playing
		/// </summary>
		/********************************************************************/
		public abstract void Pause();



		/********************************************************************/
		/// <summary>
		/// Will set the master volume (0-256)
		/// </summary>
		/********************************************************************/
		public abstract void SetMasterVolume(int volume);



		/********************************************************************/
		/// <summary>
		/// Will switch the stream to read the sound data from without
		/// interrupting the sound
		/// </summary>
		/********************************************************************/
		public abstract AgentResult SwitchStream(SoundStream soundStream, string fileName, string moduleName, string author, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Event called if the player fails while playing
		/// </summary>
		/********************************************************************/
		public event PlayerFailedEventHandler PlayerFailed;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Called if the player fails while playing
		/// </summary>
		/********************************************************************/
		protected void OnPlayerFailed(string errorMessage)
		{
			if (PlayerFailed != null)
				PlayerFailed(this, new PlayerFailedEventArgs(errorMessage));
		}
		#endregion
	}
}
