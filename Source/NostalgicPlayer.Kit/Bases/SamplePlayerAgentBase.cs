/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Containers.Flags;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Bases
{
	/// <summary>
	/// Base class that can be used for sample player agents
	/// </summary>
	public abstract class SamplePlayerAgentBase : PlayerAgentBase, ISamplePlayerAgent
	{
		/// <summary>
		/// Holds the stream with the sample to play
		/// </summary>
		protected ModuleStream modStream;

		#region ISamplePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the player supports
		/// </summary>
		/********************************************************************/
		public virtual SamplePlayerSupportFlag SupportFlags => SamplePlayerSupportFlag.None;



		/********************************************************************/
		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		/********************************************************************/
		public abstract AgentResult LoadHeaderInfo(ModuleStream moduleStream, out string errorMessage);



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public virtual bool InitPlayer(ModuleStream moduleStream, out string errorMessage)
		{
			errorMessage = string.Empty;

			modStream = moduleStream;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public virtual void CleanupPlayer()
		{
			modStream = null;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public virtual bool InitSound(out string errorMessage)
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
		#endregion

		#region ISampleAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public abstract int LoadDataBlock(int[][] outputBuffer, int countInFrames);



		/********************************************************************/
		/// <summary>
		/// Return which speakers the player uses.
		/// 
		/// Note that the outputBuffer in LoadDataBlock match the defined
		/// order in SpeakerFlag enum
		/// </summary>
		/********************************************************************/
		public virtual SpeakerFlag SpeakerFlags => ChannelCount == 1 ? SpeakerFlag.FrontCenter : SpeakerFlag.FrontLeft | SpeakerFlag.FrontRight;



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public abstract int ChannelCount { get; }



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public abstract int Frequency { get; }
		#endregion
	}
}
