/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player
{
	/// <summary>
	/// 
	/// </summary>
	internal class Channel_Data : IDeepCloneable<Channel_Data>
	{
		/// <summary>
		/// Channel flags
		/// </summary>
		public Channel_Flag Flags;

		/// <summary>
		/// Persistent effect channel flags
		/// </summary>
		public Channel_Flag Per_Flags;

		/// <summary>
		/// Note release, fadeout or end
		/// </summary>
		public Note_Flag Note_Flags;

		/// <summary>
		/// Note number
		/// </summary>
		public c_int Note;

		/// <summary>
		/// Key number
		/// </summary>
		public c_int Key;

		/// <summary>
		/// Amiga or linear period
		/// </summary>
		public c_double Period;

		/// <summary>
		/// MED period/pitch adjustment factor hack
		/// </summary>
		public c_double Per_Adj;

		/// <summary>
		/// Guess what
		/// </summary>
		public c_int FineTune;

		/// <summary>
		/// Instrument number
		/// </summary>
		public c_int Ins;

		/// <summary>
		/// Last instrument
		/// </summary>
		public c_int Old_Ins;

		/// <summary>
		/// Sample number
		/// </summary>
		public c_int Smp;

		/// <summary>
		/// Master vol -- for IT track vol effect
		/// </summary>
		public c_int MasterVol;

		/// <summary>
		/// Note delay in frames
		/// </summary>
		public c_int Delay;

		/// <summary>
		/// Key off counter
		/// </summary>
		public c_int KeyOff;

		/// <summary>
		/// Current fadeout (release) value
		/// </summary>
		public c_int FadeOut;

		/// <summary>
		/// Instrument fadeout value
		/// </summary>
		public c_int Ins_Fade;

		/// <summary>
		/// Current volume
		/// </summary>
		public c_int Volume;

		/// <summary>
		/// Global volume for instrument for IT
		/// </summary>
		public c_int Gvl;

		/// <summary>
		/// Random volume variation
		/// </summary>
		public c_int Rvv;

		/// <summary>
		/// Random pan variation
		/// </summary>
		public c_int Rpv;

		/// <summary>
		/// Split channel
		/// </summary>
		public uint8 Split;

		/// <summary>
		/// Split channel pair
		/// </summary>
		public uint8 Pair;

		/// <summary>
		/// Volume envelope index
		/// </summary>
		public c_int V_Idx;

		/// <summary>
		/// Pan envelope index
		/// </summary>
		public c_int P_Idx;

		/// <summary>
		/// Freq envelope index
		/// </summary>
		public c_int F_Idx;

		/// <summary>
		/// Key number for portamento target -- needed to handle IT portamento xpo
		/// </summary>
		public c_int Key_Porta;

		public (
			Lfo.Lfo Lfo,
			c_int Memory
		) Vibrato = ( new Lfo.Lfo(), 0 );

		public (
			Lfo.Lfo Lfo,
			c_int Memory
		) Tremolo = ( new Lfo.Lfo(), 0 );

		public (
			Lfo.Lfo Lfo,
			c_int Memory
		) Panbrello = ( new Lfo.Lfo(), 0 );

		public (
			int8[] Val,
			c_int Size,
			c_int Count,
			c_int Memory
		) Arpeggio = ( new int8[16], 0, 0, 0 );

		public (
			Lfo.Lfo Lfo,
			c_int Sweep
		) InsVib = ( new Lfo.Lfo(), 0 );

		public (
			c_int Val,
			c_int Val2,		// For fx9 bug emulation
			c_int Memory
		) Offset;

		public (
			c_int Val,			// Retrig value
			c_int Count,		// Retrig counter
			c_int Type,			// Retrig type
			c_int Limit			// Number of retrigs
		) Retrig;

		public (
			c_int Up,			// Tremor value
			c_int Down,
			c_int Count,		// Tremor counter
			c_int Memory		// Tremor memory
		) Tremor;

		public (
			c_int Slide,		// Volume slide value
			c_int FSlide,		// Fine volume slide value
			c_int Slide2,		// Volume slide value
			c_int Memory,		// Volume slide effect memory
			c_int FSlide2,
			c_int Memory2,		// Volume slide effect memory
			c_int Target		// Target for persistent volslide
		) Vol;

		public (
			c_int Up_Memory,	// Fine volume slide up memory (XM)
			c_int Down_Memory	// Fine volume slide down memory (XM)
		) Fine_Vol;

		public (
			c_int Slide,		// Global volume slide value
			c_int FSlide,		// Fine global volume slide value
			c_int Memory		// Global volume memory is saved per channel
		) GVol;

		public (
			c_int Slide,		// Track volume slide value
			c_int FSlide,		// Track fine volume slide value
			c_int Memory		// Track volume slide effect memory
		) TrackVol;

		public (
			c_int Slide,		// Frequency slide value
			c_double FSlide,	// Fine frequency slide value
			c_int Memory,		// Portamento effect memory
			c_int Down_Memory	// Portamento down effect memory (XM)
		) Freq;

		public (
			c_double Target,	// Target period for tone portamento
			c_int Dir,			// Tone portamento up/down direction
			c_int Slide,		// Delta for tone portamento
			c_int Memory,		// Tone portamento effect memory
			c_int Note_Memory	// Tone portamento note memory (ULT)
		) Porta;

		public (
			c_int Up_Memory,	// FT2 has separate memories for these
			c_int Down_Memory,	// cases (see Porta-LinkMem.xm)
			c_int Xf_Up_Memory,
			c_int Xf_Dn_Memory
		) Fine_Porta;

		public (
			c_int Val,			// Current pan value
			c_int Slide,		// Pan slide value
			c_int FSlide,		// Pan fine slide value
			c_int Memory,		// Pan slide effect memory
			bool Surround		// Surround channel flag
		) Pan;

		public (
			c_int Speed,
			c_int Count,
			c_int Pos
		) InvLoop;

		public (
			c_int Slide,		// IT tempo slide
			c_int Dummy			// Need this, because there has to be at least two items
		) Tempo;

		public (
			c_int CutOff,		// IT filter cutoff frequency
			c_int Resonance,	// IT filter resonance
			c_int Envelope,		// IT filter envelope
			bool Can_Disable	// IT hack: allow disabling for cutoff 127
		) Filter;

		public (
			c_float Val,		// Current macro effect (use float for slides)
			c_float Target,		// Current macro target (smooth macro)
			c_float Slide,		// Current macro slide (smooth macro)
			c_int Active,		// Current active parameterized macro
			c_int FinalVol,		// Previous tick calculated volume (0-0x400)
			c_int NotePan		// Previous tick note panning (0x80 center)
		) Macro;

		public (
			c_int Slide,		// PTM note slide amount
			c_int FSlide,		// OKT fine note slide amount
			c_int Speed,		// PTM note slide speed
			c_int Count			// PTM note slide counter
		) NoteSlide;

		public IChannelExtra Extra;

		public Xmp_Event Delayed_Event = new Xmp_Event();

		/// <summary>
		/// IT save instrument emulation
		/// </summary>
		public c_int Delayed_Ins;

		/// <summary>
		/// Period
		/// </summary>
		public c_int Info_Period;

		/// <summary>
		/// Linear pitchbend
		/// </summary>
		public c_int Info_PitchBend;

		/// <summary>
		/// Position before mixing
		/// </summary>
		public c_int Info_Position;

		/// <summary>
		/// Final volume including envelopes
		/// </summary>
		public c_int Info_FinalVol;

		/// <summary>
		/// Final pan including envelopes
		/// </summary>
		public c_int Info_FinalPan;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Channel_Data MakeDeepClone()
		{
			Channel_Data clone = (Channel_Data)MemberwiseClone();

			clone.Arpeggio.Val = ArrayHelper.CloneArray(Arpeggio.Val);

			clone.Vibrato.Lfo = Vibrato.Lfo.MakeDeepClone();
			clone.Tremolo.Lfo = Tremolo.Lfo.MakeDeepClone();
			clone.Panbrello.Lfo = Panbrello.Lfo.MakeDeepClone();
			clone.InsVib.Lfo = InsVib.Lfo.MakeDeepClone();
			clone.Delayed_Event = Delayed_Event.MakeDeepClone();

			if (Extra != null)
			{
				clone.Extra = Extra.MakeDeepClone();
				clone.Extra.SetChannel(clone);
			}

			return clone;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Flags = 0;
			Per_Flags = 0;
			Note_Flags = 0;
			Note = 0;
			Key = 0;
			Period = 0;
			Per_Adj = 0;
			FineTune = 0;
			Ins = 0;
			Old_Ins = 0;
			Smp = 0;
			MasterVol = 0;
			Delay = 0;
			KeyOff = 0;
			FadeOut = 0;
			Ins_Fade = 0;
			Volume = 0;
			Gvl = 0;
			Rvv = 0;
			Rpv = 0;
			Split = 0;
			Pair = 0;
			V_Idx = 0;
			P_Idx = 0;
			F_Idx = 0;
			Key_Porta = 0;

			Vibrato.Lfo.Clear();
			Vibrato.Memory = 0;

			Tremolo.Lfo.Clear();
			Tremolo.Memory = 0;

			Panbrello.Lfo.Clear();
			Panbrello.Memory = 0;

			Array.Clear(Arpeggio.Val);
			Arpeggio.Size = 0;
			Arpeggio.Count = 0;
			Arpeggio.Memory = 0;

			InsVib.Lfo.Clear();
			InsVib.Sweep = 0;

			Offset.Val = 0;
			Offset.Val2 = 0;
			Offset.Memory = 0;

			Retrig.Val = 0;
			Retrig.Count = 0;
			Retrig.Type = 0;
			Retrig.Limit = 0;

			Tremor.Up = 0;
			Tremor.Down = 0;
			Tremor.Count = 0;
			Tremor.Memory = 0;

			Vol.Slide = 0;
			Vol.FSlide = 0;
			Vol.Slide2 = 0;
			Vol.Memory = 0;
			Vol.FSlide2 = 0;
			Vol.Memory2 = 0;
			Vol.Target = 0;

			Fine_Vol.Up_Memory = 0;
			Fine_Vol.Down_Memory = 0;

			GVol.Slide = 0;
			GVol.FSlide = 0;
			GVol.Memory = 0;

			TrackVol.Slide = 0;
			TrackVol.FSlide = 0;
			TrackVol.Memory = 0;

			Freq.Slide = 0;
			Freq.FSlide = 0;
			Freq.Memory = 0;

			Porta.Target = 0;
			Porta.Dir = 0;
			Porta.Memory = 0;
			Porta.Note_Memory = 0;

			Fine_Porta.Up_Memory = 0;
			Fine_Porta.Down_Memory = 0;

			Pan.Val = 0;
			Pan.Slide = 0;
			Pan.FSlide = 0;
			Pan.Memory = 0;
			Pan.Surround = false;

			InvLoop.Speed = 0;
			InvLoop.Count = 0;
			InvLoop.Pos = 0;

			Tempo.Slide = 0;
			Tempo.Dummy = 0;

			Filter.CutOff = 0;
			Filter.Resonance = 0;
			Filter.Envelope = 0;
			Filter.Can_Disable = false;

			Macro.Val = 0;
			Macro.Target = 0;
			Macro.Slide = 0;
			Macro.Active = 0;
			Macro.FinalVol = 0;
			Macro.NotePan = 0;

			NoteSlide.Slide = 0;
			NoteSlide.FSlide = 0;
			NoteSlide.Speed = 0;
			NoteSlide.Count = 0;

			Extra = null;

			Delayed_Event.Clear();

			Delayed_Ins = 0;
			Info_Period = 0;
			Info_PitchBend = 0;
			Info_Position = 0;
			Info_FinalVol = 0;
			Info_FinalPan = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyFrom(Channel_Data other)
		{
			Flags = other.Flags;
			Per_Flags = other.Per_Flags;
			Note_Flags = other.Note_Flags;
			Note = other.Note;
			Key = other.Key;
			Period = other.Period;
			Per_Adj = other.Per_Adj;
			FineTune = other.FineTune;
			Ins = other.Ins;
			Old_Ins = other.Old_Ins;
			Smp = other.Smp;
			MasterVol = other.MasterVol;
			Delay = other.Delay;
			KeyOff = other.KeyOff;
			FadeOut = other.FadeOut;
			Ins_Fade = other.Ins_Fade;
			Volume = other.Volume;
			Gvl = other.Gvl;
			Rvv = other.Rvv;
			Rpv = other.Rpv;
			Split = other.Split;
			Pair = other.Pair;
			V_Idx = other.V_Idx;
			P_Idx = other.P_Idx;
			F_Idx = other.F_Idx;
			Key_Porta = other.Key_Porta;

			Vibrato.Lfo.CopyFrom(other.Vibrato.Lfo);
			Vibrato.Memory = other.Vibrato.Memory;

			Tremolo.Lfo.CopyFrom(other.Tremolo.Lfo);
			Tremolo.Memory = other.Tremolo.Memory;

			Panbrello.Lfo.CopyFrom(other.Panbrello.Lfo);
			Panbrello.Memory = other.Panbrello.Memory;

			Array.Copy(other.Arpeggio.Val, Arpeggio.Val, Arpeggio.Val.Length);
			Arpeggio.Size = other.Arpeggio.Size;
			Arpeggio.Count = other.Arpeggio.Count;
			Arpeggio.Memory = other.Arpeggio.Memory;

			InsVib.Lfo.CopyFrom(other.InsVib.Lfo);
			InsVib.Sweep = other.InsVib.Sweep;

			Offset.Val = other.Offset.Val;
			Offset.Val2 = other.Offset.Val2;
			Offset.Memory = other.Offset.Memory;

			Retrig.Val = other.Retrig.Val;
			Retrig.Count = other.Retrig.Count;
			Retrig.Type = other.Retrig.Type;
			Retrig.Limit = other.Retrig.Limit;

			Tremor.Up = other.Tremor.Up;
			Tremor.Down = other.Tremor.Down;
			Tremor.Count = other.Tremor.Count;
			Tremor.Memory = other.Tremor.Memory;

			Vol.Slide = other.Vol.Slide;
			Vol.FSlide = other.Vol.FSlide;
			Vol.Slide2 = other.Vol.Slide2;
			Vol.Memory = other.Vol.Memory;
			Vol.FSlide2 = other.Vol.FSlide2;
			Vol.Memory2 = other.Vol.Memory2;
			Vol.Target = other.Vol.Target;

			Fine_Vol.Up_Memory = other.Fine_Vol.Up_Memory;
			Fine_Vol.Down_Memory = other.Fine_Vol.Down_Memory;

			GVol.Slide = other.GVol.Slide;
			GVol.FSlide = other.GVol.FSlide;
			GVol.Memory = other.GVol.Memory;

			TrackVol.Slide = other.TrackVol.Slide;
			TrackVol.FSlide = other.TrackVol.FSlide;
			TrackVol.Memory = other.TrackVol.Memory;

			Freq.Slide = other.Freq.Slide;
			Freq.FSlide = other.Freq.FSlide;
			Freq.Memory = other.Freq.Memory;

			Porta.Target = other.Porta.Target;
			Porta.Dir = other.Porta.Dir;
			Porta.Memory = other.Porta.Memory;
			Porta.Note_Memory = other.Porta.Note_Memory;

			Fine_Porta.Up_Memory = other.Fine_Porta.Up_Memory;
			Fine_Porta.Down_Memory = other.Fine_Porta.Down_Memory;

			Pan.Val = other.Pan.Val;
			Pan.Slide = other.Pan.Slide;
			Pan.FSlide = other.Pan.FSlide;
			Pan.Memory = other.Pan.Memory;
			Pan.Surround = other.Pan.Surround;

			InvLoop.Speed = other.InvLoop.Speed;
			InvLoop.Count = other.InvLoop.Count;
			InvLoop.Pos = other.InvLoop.Pos;

			Tempo.Slide = other.Tempo.Slide;
			Tempo.Dummy = other.Tempo.Dummy;

			Filter.CutOff = other.Filter.CutOff;
			Filter.Resonance = other.Filter.Resonance;
			Filter.Envelope = other.Filter.Envelope;
			Filter.Can_Disable = other.Filter.Can_Disable;

			Macro.Val = other.Macro.Val;
			Macro.Target = other.Macro.Target;
			Macro.Slide = other.Macro.Slide;
			Macro.Active = other.Macro.Active;
			Macro.FinalVol = other.Macro.FinalVol;
			Macro.NotePan = other.Macro.NotePan;

			NoteSlide.Slide = other.NoteSlide.Slide;
			NoteSlide.FSlide = other.NoteSlide.FSlide;
			NoteSlide.Speed = other.NoteSlide.Speed;
			NoteSlide.Count = other.NoteSlide.Count;

			Extra = other.Extra;

			Delayed_Event.CopyFrom(other.Delayed_Event);

			Delayed_Ins = other.Delayed_Ins;
			Info_Period = other.Info_Period;
			Info_PitchBend = other.Info_PitchBend;
			Info_Position = other.Info_Position;
			Info_FinalVol = other.Info_FinalVol;
			Info_FinalPan = other.Info_FinalPan;
		}
	}
}
