/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// TFMX's sequencer is designed as a track table with N columns (= tracks).
	/// Each track can assign patterns to any audio channel or execute a small
	/// number of commands to affect either the track or song progression.
	///
	/// Some modules use the LOOP command to escape from their initial start/end
	/// range within the track table.
	///
	/// The size of the track table (and thus the number of lines/steps within it)
	/// cannot be determined reliably, unfortunately. In some files there are data
	/// within track table range, which look like pattern data before the actual
	/// beginning of the patterns. As such, not much can be done about figuring out
	/// whether a song definition's track step number is valid
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetPositions()
		{
			return playerInfo.Sequencer.Step.Last - playerInfo.Sequencer.Step.First + 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override c_int GetPlayingPosition()
		{
			return playerInfo.Sequencer.Step.Current - playerInfo.Sequencer.Step.First;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override ubyte[] GetPlayingTracks()
		{
			c_int voiceCount = voices;

			if (voiceCount == 8)
				voiceCount--;

			return playerInfo.Track.Select(x => x.Pt).Take(voiceCount).ToArray();
		}



		/********************************************************************/
		/// <summary>
		/// Track Mute is a feature of the TFMX file format and editor, but
		/// isn't used much outside the editor itself. For the majority of
		/// files all tracks are set to ON. In only 7 files, some tracks are
		/// set to OFF and lead to missing voices in two cases
		/// </summary>
		/********************************************************************/
		private bool GetTrackMute(ubyte t)
		{
			if (variant.NoTrackMute)
				return true;

			return 0 == MyEndian.ReadBEUword(pBuf, (udword)(offsets.Header + 0x1c0 + (t << 1)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessTrackStep()
		{
			c_int evalMaxLoops = Recurse_Limit;		// NB! Around 5 would suffice

			do
			{
				playerInfo.Sequencer.EvalNext = false;

				if (playerInfo.Sequencer.Step.Current > (Track_Steps_Max - 1))	// Fubur then
					playerInfo.Sequencer.Step.Current = playerInfo.Sequencer.Step.First;

				playerInfo.Sequencer.StepSeenBefore[playerInfo.Sequencer.Step.Current] = true;

				udword stepOffset = (udword)(offsets.TrackTable + (playerInfo.Sequencer.Step.Current << 4));

				// 0xEFFE is the prefix of a track command
				if (0xeffe == MyEndian.ReadBEUword(pBuf, stepOffset))
				{
					stepOffset += 2;

					uword command = MyEndian.ReadBEUword(pBuf, stepOffset);
					stepOffset += 2;

					if (command > Track_Cmd_Max)	// Fubar then
						command = 0;        // Choose command "Stop" as override

					trackCmdUsed[command] = true;

					trackCmdFuncs[command](stepOffset);
				}
				else
				{
					// Set PT TR for each track
					for (ubyte t = 0; t < playerInfo.Sequencer.Tracks; t++)
					{
						Track tr = playerInfo.Track[t];

						tr.Pt = pBuf[stepOffset++];
						tr.Tr = (sbyte)pBuf[stepOffset++];

						if (tr.Pt < 0x80)
						{
							tr.Pattern.Offset = GetPattOffset(tr.Pt);
							tr.Pattern.Step = 0;
							tr.Pattern.Wait = 0;
							tr.Pattern.Loops = -1;
							tr.Pattern.InfiniteLoop = false;
						}
					}
				}
			}
			while (playerInfo.Sequencer.EvalNext && (--evalMaxLoops > 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TrackCmd_Stop(udword stepOffset)
		{
			songEnd = true;
			triggerRestart = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TrackCmd_Loop(udword stepOffset)
		{
			if (playerInfo.Sequencer.Loops == 0)		// End of loop
			{
				playerInfo.Sequencer.Loops = -1;		// Unlock
				playerInfo.Sequencer.Step.Current++;
			}
			else if (playerInfo.Sequencer.Loops < 0)	// Unlocked? Loop init permitted
			{
				playerInfo.Sequencer.Step.Current = MyEndian.ReadBEUword(pBuf, stepOffset);
				playerInfo.Sequencer.Loops = (sword)(MyEndian.ReadBEUword(pBuf, stepOffset + 2) - 1);

				if ((playerInfo.Sequencer.Step.Current > (Track_Steps_Max - 1)) || (playerInfo.Sequencer.Step.Current > playerInfo.Sequencer.Step.Last))
				{
					songEnd = true;
					triggerRestart = true;
				}
				// Starting a loop with a negative count would be infinite
				else if (playerInfo.Sequencer.StepSeenBefore[playerInfo.Sequencer.Step.Current] && (playerInfo.Sequencer.Loops < 0))
					songEnd = true;

				// Limit number of loops. Only "Ramses" title sets 0xf00, and "mdat.cyberzerk-ingame" subsongs
				// set a number of 0x2e00 loops
				if ((playerInfo.Sequencer.Loops == 0xeff) || (playerInfo.Sequencer.Loops > 0x100))
					playerInfo.Sequencer.Loops = 0;
			}
			else
			{
				playerInfo.Sequencer.Loops--;
				playerInfo.Sequencer.Step.Current = MyEndian.ReadBEUword(pBuf, stepOffset);
			}

			playerInfo.Sequencer.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TrackCmd_Speed(udword stepOffset)
		{
			playerInfo.Admin.Speed = playerInfo.Admin.Count = (sword)MyEndian.ReadBEUword(pBuf, stepOffset);

			// Ignore negative values like 0xff00
			uword arg2 = (uword)(0x81ff & MyEndian.ReadBEUword(pBuf, stepOffset + 2));

			if ((arg2 != 0) && (arg2 < 0x200))
			{
				if (arg2 < 32)
					arg2 = 125;

				SetRate(arg2);
			}

			playerInfo.Sequencer.Step.Current++;
			playerInfo.Sequencer.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TrackCmd_Fade(udword stepOffset)
		{
			FadeInit(pBuf[stepOffset + 3], pBuf[stepOffset + 1]);

			playerInfo.Sequencer.Step.Current++;
			playerInfo.Sequencer.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TrackCmd_7V(udword stepOffset)
		{
			sword arg2 = (sword)MyEndian.ReadBEUword(pBuf, stepOffset + 2);

			if (arg2 >= 0)
			{
				sbyte x = (sbyte)pBuf[stepOffset + 3];

				if (x < -0x20)
					x = -0x20;

				SetBpm((uword)((125 * 100) / (100 + x)));
			}

			playerInfo.Sequencer.Step.Current++;
			playerInfo.Sequencer.EvalNext = true;
		}
	}
}
