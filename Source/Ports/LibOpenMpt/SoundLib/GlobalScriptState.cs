/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class GlobalScriptState : InstrumentSynth.States, IDeepCloneable<GlobalScriptState>
	{
		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void Initialize(CSoundFile sndFile)//XX 1022
		{
			if (!sndFile.m_GlobalScript.empty())
				states.assign(sndFile.GetNumChannels(), new State());
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void NextTick(PlayState playState, CSoundFile sndFile)//XX 1029
		{
			if (sndFile.m_GlobalScript.empty())
				return;

			states.resize(sndFile.GetNumChannels());

			for (ChannelIndex chn = 0; chn < sndFile.GetNumChannels(); chn++)
			{
				State state = states[chn];
				ModChannel modChn = playState.Chn[chn];

				if ((modChn.RowCommand.Command == ModCommandCommand.Med_Synth_Jump) && (playState.m_nTickCount == 0))
					states[chn].JumpToPosition(sndFile.m_GlobalScript, modChn.RowCommand.Param);

				state.NextTick(sndFile.m_GlobalScript, playState, chn, sndFile, this);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public void ApplyChannelState(PlayState playState, ChannelIndex chn, ref int32 period, CSoundFile sndFile)//XX 1045
		{
			if (sndFile.m_GlobalScript.empty())
				return;

			for (ChannelIndex s = 0; s < states.size(); s++)
			{
				if (states[s].FtmRealChannel(s, sndFile) == chn)
					states[s].ApplyChannelState(playState.Chn[chn], ref period, sndFile);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public override GlobalScriptState MakeDeepClone()
		{
			GlobalScriptState clone = new GlobalScriptState();

			CopyTo(clone);

			return clone;
		}
	}
}
