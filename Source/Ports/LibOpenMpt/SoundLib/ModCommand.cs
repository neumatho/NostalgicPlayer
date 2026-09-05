/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using ModCommandNote = System.Byte;
global using ModCommandInstr = System.Byte;
global using ModCommandVol = System.Byte;
global using ModCommandVolCmd = Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.VolumeCommand;
global using ModCommandCommand = Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.EffectCommand;
global using ModCommandParam = System.Byte;

using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Various functions for writing effects to patterns, converting ModCommands, etc.
	/// </summary>
	internal class ModCommand : IEquatable<ModCommand>, IClearable, IDeepCloneable<ModCommand>, ICopyTo<ModCommand>
	{
		// Note definitions
		public const uint8 Note_None = 0;						// Empty note cell
		public const uint8 Note_Min = 1;						// Minimum note value
		public const uint8 Note_Max = 128;						// Maximum note value
		public const uint8 Note_MiddleC = (5 * 12) + Note_Min;	// Minimum note value
		public const uint8 Note_KeyOff = 0xff;					// === (Note Off, releases envelope / fades samples, stops plugin note)
		public const uint8 Note_NoteCut = 0xfe;					// ^^^ (Cuts sample / stops all plugin notes)
		public const uint8 Note_Fade = 0xfd;					// ~~~ (Fades samples, stops plugin note)
		public const uint8 Note_Pc = 0xfc;						// Param Control 'note'. Changes param value on first tick
		public const uint8 Note_Pcs = 0xfb;						// Param Control (Smooth) 'note'. Interpolates param value during the whole row
		public const uint8 Note_Min_Special = Note_Pcs;
		public const uint8 Note_Max_Special = Note_KeyOff;

		public uint8 Note = Note_None;
		public uint8 Instr = 0;
		public VolumeCommand VolCmd = VolumeCommand.None;
		public EffectCommand Command = EffectCommand.None;
		public uint8 Vol = 0;
		public uint8 Param = 0;

		/********************************************************************/
		/// <summary>
		/// Clears ModCommand
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Note = Note_None;
			Instr = 0;
			VolCmd = VolumeCommand.None;
			Command = EffectCommand.None;
			Vol = 0;
			Param = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if ModCommand is empty, false otherwise
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty()
		{
			return (Note == Note_None) && (Instr == 0) && (VolCmd == VolumeCommand.None) && (Command == EffectCommand.None);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is NOTE_PC or NOTE_PCS
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPcNote()
		{
			return IsPcNote(Note);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is NOTE_PC or NOTE_PCS
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsPcNote(uint8 note)
		{
			return (note == Note_Pc) || (note == Note_Pcs);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is a valid musical note
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsNote()
		{
			return Algorithm.Is_In_Range(Note, Note_Min, Note_Max);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is a valid musical note
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNote(uint8 note)
		{
			return Algorithm.Is_In_Range(note, Note_Min, Note_Max);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is a valid musical note or the
		/// note entry is empty
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsNoteOrEmpty()
		{
			return (Note == Note_None) || IsNote();
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if and only if note is a valid musical note or the
		/// note entry is empty
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNoteOrEmpty(ModCommandNote note)
		{
			return (note == Note_None) || IsNote(note);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if any of the commands in this cell trigger a tone
		/// portamento
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsTonePortamento()
		{
			return (Command == EffectCommand.TonePortamento) || (Command == EffectCommand.TonePortaVol) || (Command == EffectCommand.TonePorta_Duration) || (VolCmd == VolumeCommand.TonePortamento);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the command is a regular volume slide
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsNormalVolumeSlide()
		{
			return (Command == EffectCommand.VolumeSlide) || (Command == EffectCommand.VibratoVol) || (Command == EffectCommand.TonePortaVol);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the note is inside the Amiga frequency range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAmigaNote()
		{
			return IsAmigaNote(Note);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if the note is inside the Amiga frequency range
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAmigaNote(ModCommandNote note)
		{
			return !IsNote(note) || ((note >= (Note_MiddleC - 12)) && (note < (Note_MiddleC + 24)));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool IsAnyPitchSlide()//XX 954
		{
			switch (Command)
			{
				case EffectCommand.PortamentoUp:
				case EffectCommand.PortamentoDown:
				case EffectCommand.TonePortamento:
				case EffectCommand.TonePortaVol:
				case EffectCommand.NoteSlideUp:
				case EffectCommand.NoteSlideDown:
				case EffectCommand.NoteSlideUpRetrig:
				case EffectCommand.NoteSlideDownRetrig:
				case EffectCommand.Auto_PortaUp:
				case EffectCommand.Auto_PortaDown:
				case EffectCommand.Auto_PortaUp_Fine:
				case EffectCommand.Auto_PortaDown_Fine:
				case EffectCommand.Auto_Portamento_FC:
				case EffectCommand.TonePorta_Duration:
					return true;

				case EffectCommand.ModCmdEx:
				case EffectCommand.XFinePortaUpDown:
				{
					if ((Param >= 0x10) && (Param <= 0x2f))
						return true;

					break;
				}
			}

			switch (VolCmd)
			{
				case VolumeCommand.TonePortamento:
				case VolumeCommand.PortaUp:
				case VolumeCommand.PortaDown:
					return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator == (ModCommand me, ModCommand other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.Note == other.Note) && (me.Instr == other.Instr) && (me.VolCmd == other.VolCmd) && (me.Command == other.Command) &&
				   (((me.VolCmd == VolumeCommand.None) && !me.IsPcNote()) || (me.Vol == other.Vol)) &&
				   (((me.Command == EffectCommand.None) && !me.IsPcNote()) || (me.Param == other.Param));
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (ModCommand me, ModCommand other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is ModCommand other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ModCommand other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			HashCode hash = new HashCode();

			hash.Add(Note);
			hash.Add(Instr);
			hash.Add(VolCmd);
			hash.Add(Command);
			hash.Add(Vol);
			hash.Add(Param);

			return hash.ToHashCode();
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ModCommand MakeDeepClone()
		{
			return (ModCommand)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object into another object
		/// </summary>
		/********************************************************************/
		public void CopyTo(ModCommand destination)
		{
			destination.Note = Note;
			destination.Instr = Instr;
			destination.VolCmd = VolCmd;
			destination.Command = Command;
			destination.Vol = Vol;
			destination.Param = Param;
		}
	}
}
