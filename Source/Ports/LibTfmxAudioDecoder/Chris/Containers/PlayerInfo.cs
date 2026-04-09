/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
{
	/// <summary>
	/// Contains all global player info that changes
	/// </summary>
	internal class PlayerInfo : IDeepCloneable<PlayerInfo>
	{
		public Admin Admin;
		public Track[] Track;
		public Cmd Cmd;

		public bool MacroEvalAgain;

		public
		(
			ubyte Tracks,
			bool EvalNext,				// Whether to evaluate next PT.TR pair
			sword Loops,

			(
				c_int First,
				c_int Last,
				c_int Current,
				ubyte Size,
				bool Next				// Whether a pattern triggered next track step
			) Step,

			bool[] StepSeenBefore
		) Sequencer;

		public
		(
			ubyte Volume,
			ubyte Target,
			ubyte Speed,
			ubyte Count,
			sbyte Delta,
			bool Active
		) Fade;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public PlayerInfo MakeDeepClone()
		{
			PlayerInfo clone = (PlayerInfo)MemberwiseClone();

			clone.Admin = Admin.MakeDeepClone();
			clone.Track = ArrayHelper.CloneObjectArray(Track);
			clone.Cmd = Cmd.MakeDeepClone();
			clone.Sequencer.StepSeenBefore = ArrayHelper.CloneArray(Sequencer.StepSeenBefore);

			return clone;
		}
	}
}
