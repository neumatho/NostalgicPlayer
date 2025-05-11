/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal partial class Player
	{
		/********************************************************************/
		/// <summary>
		/// Process a pattern loop effect with the parameter fxp. A parameter
		/// of 0 will set the loop target, and a parameter of 1-15 (most
		/// formats) or 1-255 (OctaMED) will perform a loop.
		///
		/// The compatibility logic for Pattern Loop is complex, so a
		/// flow_control argument is taken such that the scan can use this
		/// function directly.
		///
		/// If the development tests ever start building against effects.c,
		/// this can be moved back to effects.c
		/// </summary>
		/********************************************************************/
		public void LibXmp_Process_Pattern_Loop(Flow_Control f, c_int chn, c_int row, c_int fxP)
		{
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;

			ref c_int start = ref f.Loop[chn].Start;
			ref c_int count = ref f.Loop[chn].Count;
			bool looped = false;

			// Digital Tracker: Only the first E60 or E6x is handled per row
			if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_First_Effect) && (f.Loop_Param >= 0))
				return;

			f.Loop_Param = fxP;

			// Scream Tracker 3, Digital Tracker, Octalyser, and probably others
			// use global loop targets and counts. Later versions of Digital
			// Tracker use a global target but per-track counts
			if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Global_Target))
				start = ref f.Loop_Start;

			if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Global_Count))
				count = ref f.Loop_Count;

			if (fxP == 0)
			{
				// Mark start of loop
				// Liquid Tracker: M60 is ignored for channels with count >= 1
				if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Ignore_Target) && (count >= 1))
					return;

				start = row;

				if (Common.Has_Quirk(m, Quirk_Flag.Ft2Bugs))
					f.JumpLine = row;
			}
			else
			{
				// End of loop
				if (start < 0)
				{
					// Scream Tracker 3.01b: if SB0 wasn't used, the first
					// SBx used will set the loop target to its row
					if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Init_SameRow))
						start = row;
					else
						start = 0;
				}

				if (count != 0)
				{
					if (--count != 0)
					{
						f.Loop_Dest = start;
						looped = true;
					}
					else
					{
						// S3M and IT: loop termination advances the
						// loop target past SBx
						if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_End_Advances))
							start = row + 1;

						// Liquid Tracker cancels any other loop jumps
						// this row started on loop termination
						if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_End_Cancels))
							f.Loop_Dest = -1;

						f.Loop_Active_Num--;
					}
				}
				else
				{
					// Modplug Tracker: only begin a loop if no
					// other channel is currently looping
					if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_One_At_A_Time))
					{
						for (c_int i = 0; i < mod.Chn; i++)
						{
							if ((i != chn) && (f.Loop[i].Count != 0))
								return;
						}
					}

					count = fxP;

					f.Loop_Dest = start;
					f.Loop_Active_Num++;
					looped = true;
				}
			}

			// Hacks for loop jumps altering prior position jumps/breaks
			if (looped && f.PBreak)
			{
				// Many implementations use the same variable for both the
				// jump/break destination row and the loop destination row
				if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Shared_Break))
					f.JumpLine = f.Loop_Dest;

				// Various players e.g. ST3, IT will block prior breaks
				if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Unset_Break) && (f.Jump < 0))
					f.PBreak = false;

				// Various players e.g. ST3, IT will block prior jumps
				if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Unset_Jump) && (f.Jump >= 0))
				{
					f.PBreak = false;
					f.Jump = -1;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Process a pattern jump effect with the parameter fxp. This
		/// function will not actually perform the jump, but it will prepare
		/// the flow variables to perform a jump (unless prevented)
		/// </summary>
		/********************************************************************/
		public void LibXmp_Process_Pattern_Jump(Flow_Control f, c_int fxP)
		{
			Module_Data m = ctx.M;

			// Some formats and trackers e.g. S3M, Modplug Tracker 1.16 will
			// prevent jumps from being executed when a loop jump occurs
			if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Delay_Jump) && (f.Loop_Dest >= 0))
				return;

			f.PBreak = true;
			f.Jump = fxP;

			// Effect B resets effect D in lower channels
			f.JumpLine = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Process a pattern break effect with the parameter fxp. This
		/// function will not actually perform the jump, but it will prepare
		/// the flow variables to perform a jump (unless prevented)
		/// </summary>
		/********************************************************************/
		public void LibXmp_Process_Pattern_Break(Flow_Control f, c_int fxP)
		{
			Module_Data m = ctx.M;

			// Some formats and trackers e.g. S3M, IT 2.00+, Modplug Tracker 1.16
			// will prevent breaks from being executed when a jump occurs
			if (Common.Has_Flow_Mode(m, FlowMode_Flag.Loop_Delay_Break) && (f.Loop_Dest >= 0))
				return;

			f.PBreak = true;
			f.JumpLine = fxP;
		}



		/********************************************************************/
		/// <summary>
		/// Process a line jump within current position 'ord' to row 'fxp'.
		/// This function will not actually perform the jump, but it will
		/// prepare the flow variables to perform a jump (unless prevented)
		/// </summary>
		/********************************************************************/
		public void LibXmp_Process_Line_Jump(Flow_Control f, c_int ord, c_int fxP)
		{
			// In Digital Symphony, this can be combined with position jump
			// (like pattern break) and overrides the pattern break line in
			// lower channels
			if (!f.PBreak)
			{
				f.PBreak = true;
				f.Jump = ord;
			}

			f.JumpLine = fxP;
			f.Jump_In_Pat = ord;
		}
	}
}
