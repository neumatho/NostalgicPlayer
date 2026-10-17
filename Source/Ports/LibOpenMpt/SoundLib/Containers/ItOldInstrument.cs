/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Old Impulse Instrument Format (cmwt ‹ 0x200)
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 554)]
	internal struct ItOldInstrument : IClearable
	{
		#region ItOldInstrFlags
		public const uint8 EnvEnabled = 0x01;
		public const uint8 EnvLoop = 0x02;
		public const uint8 EnvSustain = 0x04;
		#endregion

		/// <summary>
		/// Magic Bytes (IMPI)
		/// </summary>
		public Array4 Id;

		/// <summary>
		/// DOS Filename, null-terminated
		/// </summary>
		public String13 FileName;

		/// <summary>
		/// Volume Envelope Flags
		/// </summary>
		public uint8le Flags;

		/// <summary>
		/// Envelope Loop Start
		/// </summary>
		public uint8le Vls;

		/// <summary>
		/// Envelope Loop End
		/// </summary>
		public uint8le Vle;

		/// <summary>
		/// Envelope Sustain Start
		/// </summary>
		public uint8le Sls;

		/// <summary>
		/// Envelope Sustain End
		/// </summary>
		public uint8le Sle;

		/// <summary>
		/// Reserved
		/// </summary>
		public Array2 Reserved1;

		/// <summary>
		/// Instrument Fadeout (0...128)
		/// </summary>
		public uint16le FadeOut;

		/// <summary>
		/// New Note Action
		/// </summary>
		public uint8le Nna;

		/// <summary>
		/// Duplicate Note Check Type
		/// </summary>
		public uint8le Dnc;

		/// <summary>
		/// Tracker ID
		/// </summary>
		public uint16le TrkVers;

		/// <summary>
		/// Number of embedded samples
		/// </summary>
		public uint8le Nos;

		/// <summary>
		/// Reserved
		/// </summary>
		public uint8 Reserved2;

		/// <summary>
		/// Instrument Name, null-terminated (but may also contain nulls)
		/// </summary>
		public String26 Name;

		/// <summary>
		/// Even more reserved bytes
		/// </summary>
		public Array6 Reserved3;

		/// <summary>
		/// Sample / Transpose map
		/// </summary>
		public Array240 Keyboard;

		/// <summary>
		/// This appears to be a pre-computed (interpolated) version of the volume envelope data found below
		/// </summary>
		public Array200 VolEnv;

		/// <summary>
		/// Volume Envelope Node Positions / Values
		/// </summary>
		public Array50 Nodes;

		/********************************************************************/
		/// <summary>
		/// Convert an ITOldInstrument to OpenMPT's internal instrument
		/// representation
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModInstrument mptIns)
		{
			// Header
			if (CMemory.memcmp(Id.ToArray().data(), "IMPI", 4) != 0)
				return;

			mptIns.Name.Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, Name.ToArray()));
			mptIns.FileName.Assign(MptString.ReadBuf(ReadWriteMode.NullTerminated, FileName.ToArray()));

			// Volume / Panning
			mptIns.nFadeOut = (uint32)(FadeOut << 6);
			mptIns.nGlobalVol = 64;
			mptIns.nPan = 128;

			// NNA Stuff
			mptIns.nNna = (NewNoteAction)Nna;
			mptIns.nDct = (DuplicateCheckType)Dnc;

			// Sample Map
			array<byte> keyboard = Keyboard.ToArray();
			for (size_t i = 0; i < 120; i++)
			{
				uint8 note = keyboard[i * 2];
				SampleIndex ins = keyboard[(i * 2) + 1];

				if (ins < Snd_Def.Max_Samples)
					mptIns.Keyboard[i] = ins;

				if (note < 120)
					mptIns.NoteMap[i] = (uint8)(note + 1);
				else
					mptIns.NoteMap[i] = (uint8)(i + 1);
			}

			// Volume Envelope Flags
			mptIns.VolEnv.dwFlags.Set(EnvelopeFlags.Enabled, (Flags & EnvEnabled) != 0);
			mptIns.VolEnv.dwFlags.Set(EnvelopeFlags.Loop, (Flags & EnvLoop) != 0);
			mptIns.VolEnv.dwFlags.Set(EnvelopeFlags.Sustain, (Flags & EnvSustain) != 0);

			// Volume Envelope Loops
			mptIns.VolEnv.nLoopStart = Vls;
			mptIns.VolEnv.nLoopEnd = Vle;
			mptIns.VolEnv.nSustainStart = Sls;
			mptIns.VolEnv.nSustainEnd = Sle;
			mptIns.VolEnv.resize(25);

			// Volume Envelope Data
			array<byte> nodes = Nodes.ToArray();
			for (uint32 i = 0; i < 25; i++)
			{
				if ((mptIns.VolEnv[i].Tick = nodes[i * 2]) == 0xff)
				{
					mptIns.VolEnv.resize(i);
					break;
				}

				mptIns.VolEnv[i].Value = nodes[(i * 2) + 1];
			}

			if (Math.Max(mptIns.VolEnv.nLoopStart, mptIns.VolEnv.nLoopEnd) >= mptIns.VolEnv.size())
				mptIns.VolEnv.dwFlags.Reset(EnvelopeFlags.Loop);

			if (Math.Max(mptIns.VolEnv.nSustainStart, mptIns.VolEnv.nSustainEnd) >= mptIns.VolEnv.size())
				mptIns.VolEnv.dwFlags.Reset(EnvelopeFlags.Sustain);
		}



		/********************************************************************/
		/// <summary>
		/// Clear all members
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			this = default;
		}
	}
}
