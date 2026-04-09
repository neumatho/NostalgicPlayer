/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibTfmxAudioDecoder.Chris.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class VoiceVars : IDeepCloneable<VoiceVars>
	{
		public uword OutputPeriod;

		public ubyte VoiceNum;			// 0 = first
		public sbyte EffectsMode;

		public sbyte Volume;

		public ubyte Note;
		public ubyte NotePrevious;
		public ubyte NoteVolume;
		public uword Period;
		public sword Detune;
		public bool KeyUp;

		public
		(
			udword Offset,
			udword OffsetSaved,
			udword Step,
			udword StepSaved,
			sword Wait,
			ubyte Loop,
			bool Skip,
			bool ExtraWait
		) Macro;

		public sword WaitOnDmaCount;
		public uword WaitOnDmaPrevLoops;

		public ubyte AddBeginCount;
		public ubyte AddBeginArg;
		public sdword AddBeginOffset;

		public
		(
			ubyte Time,
			ubyte Count,
			sbyte Intensity,
			sword Delta
		) Vibrato;

		public
		(
			ubyte Flag,
			ubyte Count,
			ubyte Speed,
			ubyte Target
		) Envelope;

		public
		(
			ubyte Count,
			ubyte Wait,
			uword Speed,
			uword Period
		) Portamento;

		public
		(
			udword Start,
			uword Length
		) Sample;

		public
		(
			udword Offset,
			uword Length
		) PaulaOrig;

		public
		(
			udword SourceOffset,
			uword SourceLength,
			udword TargetOffset,
			uword TargetLength,
			sbyte LastSample,

			(
				uword Speed,
				uword Count,
				uword InterDelta,
				sword InterMod
			) Op1,

			(
				uword Speed,
				uword Count,
				udword Offset,
				sword Delta
			) Op2,

			(
				uword Speed,
				uword Count,
				udword Offset,
				sword Delta
			) Op3
		) Sid;

		public
		(
			ubyte Macro,
			sbyte Count,
			sbyte Speed,
			sbyte Flag,
			ubyte Mode,
			bool BlockWait,
			ubyte Mask,

			(
				udword Offset,
				uword Pos
			) Arp
		) Rnd;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceVars MakeDeepClone()
		{
			return (VoiceVars)MemberwiseClone();
		}
	}
}
