/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris
{
	/// <summary>
	/// 
	/// </summary>
	public partial class TfmxDecoder
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private udword GetPattOffset(ubyte pt)
		{
			// With this TFMX format it's always an array of offsets to patterns
			return offsets.Header + MyEndian.ReadBEUdword(pBuf, (udword)(offsets.Patterns + (pt << 2)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ProcessPattern(Track tr)
		{
			c_int evalMaxLoops = Recurse_Limit;		// NB! Around 8 would suffice

			do
			{
				tr.Pattern.EvalNext = false;

				// Offset to current step position within pattern
				udword p = (udword)(tr.Pattern.Offset + (tr.Pattern.Step << 2));

				// Fetch pattern entry, four bytes aka 'aabbcdee'
				playerInfo.Cmd.Aa = pBuf[p];
				playerInfo.Cmd.Bb = pBuf[p + 1];
				playerInfo.Cmd.Cd = pBuf[p + 2];
				playerInfo.Cmd.Ee = pBuf[p + 3];

				ubyte aaBak = playerInfo.Cmd.Aa;

				if (playerInfo.Cmd.Aa < 0xf0)	// >= 0xf0 pattern state command
				{
					if ((playerInfo.Cmd.Aa < 0xc0) && (playerInfo.Cmd.Aa >= 0x7f))	// Note + wait (instead of detune)
					{
						tr.Pattern.Wait = playerInfo.Cmd.Ee;
						playerInfo.Cmd.Ee = 0;
					}

					playerInfo.Cmd.Aa = (ubyte)(playerInfo.Cmd.Aa + tr.Tr);

					if (aaBak < 0xc0)
						playerInfo.Cmd.Aa &= 0x3f;

					if (tr.On)
						NoteCmd();

					if ((aaBak >= 0xc0) || (aaBak < 0x7f))
					{
						tr.Pattern.Step++;
						tr.Pattern.EvalNext = true;
					}
					else
						tr.Pattern.Step++;
				}
				else	// cmd.aa >= 0xf0   pattern state command
				{
					ubyte command = (ubyte)(playerInfo.Cmd.Aa & 0xf);
					patternCmdUsed[command] = true;

					pattCmdFuncs[command](tr);
				}
			}
			while (tr.Pattern.EvalNext && (--evalMaxLoops > 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Nop(Track tr)
		{
			tr.Pattern.Step++;
			tr.Pattern.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_End(Track tr)
		{
			tr.Pt = 0xff;

			if (playerInfo.Sequencer.Step.Current == playerInfo.Sequencer.Step.Last)
			{
				songEnd = true;
				triggerRestart = true;
				return;
			}
			else
				playerInfo.Sequencer.Step.Current++;

			ProcessTrackStep();
			playerInfo.Sequencer.Step.Next = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Loop(Track tr)
		{
			if (tr.Pattern.Loops == 0)	// End of loop
			{
				tr.Pattern.Loops = -1;
				tr.Pattern.Step++;
				tr.Pattern.EvalNext = true;
				return;
			}
			else if (tr.Pattern.Loops == -1)	// Init permitted
			{
				tr.Pattern.Loops = (sbyte)(playerInfo.Cmd.Bb - 1);

				// This would be an infinite loop that potentially affects
				// song-end detection, if all tracks loop endlessly.
				// So, let's evaluate that elsewhere
				if (tr.Pattern.Loops == -1)		// Infinite loop
					tr.Pattern.InfiniteLoop = true;
			}
			else
				tr.Pattern.Loops--;

			tr.Pattern.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			tr.Pattern.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Goto(Track tr)
		{
			tr.Pt = playerInfo.Cmd.Bb;
			tr.Pattern.Offset = GetPattOffset(tr.Pt);
			tr.Pattern.Step = MyEndian.MakeWord(playerInfo.Cmd.Cd, playerInfo.Cmd.Ee);
			tr.Pattern.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Wait(Track tr)
		{
			tr.Pattern.Wait = playerInfo.Cmd.Bb;
			tr.Pattern.Step++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Stop(Track tr)
		{
			tr.Pt = 0xff;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Note(Track tr)
		{
			NoteCmd();

			tr.Pattern.Step++;
			tr.Pattern.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_SaveAndGoto(Track tr)
		{
			tr.Pattern.OffsetSaved = tr.Pattern.Offset;
			tr.Pattern.StepSaved = tr.Pattern.Step;

			PattCmd_Goto(tr);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_ReturnFromGoto(Track tr)
		{
			tr.Pattern.Offset = tr.Pattern.OffsetSaved;
			tr.Pattern.Step = tr.Pattern.StepSaved;
			tr.Pattern.Step++;
			tr.Pattern.EvalNext = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PattCmd_Fade(Track tr)
		{
			FadeInit(playerInfo.Cmd.Ee, playerInfo.Cmd.Bb);

			tr.Pattern.Step++;
			tr.Pattern.EvalNext = true;
		}
	}
}
