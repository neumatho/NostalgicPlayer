/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 82)]
	internal struct ItEnvelope
	{
		#region Envelope Flags
		public const uint8 EnvEnabled = 0x01;
		public const uint8 EnvLoop = 0x02;
		public const uint8 EnvSustain = 0x04;
		public const uint8 EnvCarry = 0x08;
		public const uint8 EnvFilter = 0x80;
		#endregion

		#region Node
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
		public struct Node
		{
			public int8le Value;
			public uint16le Tick;
		}
		#endregion

		/// <summary>
		/// Envelope Flags
		/// </summary>
		public uint8 Flags;

		/// <summary>
		/// Number of Envelope Nodes
		/// </summary>
		public uint8 Num;

		/// <summary>
		/// Loop Start
		/// </summary>
		public uint8 Lpb;

		/// <summary>
		/// Loop End
		/// </summary>
		public uint8 Lpe;

		/// <summary>
		/// Sustain Start
		/// </summary>
		public uint8 Slb;

		/// <summary>
		/// Sustain End
		/// </summary>
		public uint8 Sle;

		/// <summary>
		/// Envelope Node Positions / Values
		/// </summary>
		public Array25<Node> Data;

		/// <summary>
		/// Reserved
		/// </summary>
		public uint8 Reserved;

		/********************************************************************/
		/// <summary>
		/// Convert IT/MPTM envelope data into OpenMPT's internal envelope
		/// format - To be used by ITInstrToMPT()
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(InstrumentEnvelope mptEnv, uint8 envOffset, uint8 maxNodes)
		{
			// Envelope Flags
			mptEnv.dwFlags.Set(EnvelopeFlags.Enabled, (Flags & EnvEnabled) != 0);
			mptEnv.dwFlags.Set(EnvelopeFlags.Loop, (Flags & EnvLoop) != 0);
			mptEnv.dwFlags.Set(EnvelopeFlags.Sustain, (Flags & EnvSustain) != 0);
			mptEnv.dwFlags.Set(EnvelopeFlags.Carry, (Flags & EnvCarry) != 0);

			// Nodes and Loops
			mptEnv.resize(Math.Min(Num, maxNodes));
			mptEnv.nLoopStart = Math.Min(Lpb, maxNodes);
			mptEnv.nLoopEnd = Math.Clamp(Lpe, mptEnv.nLoopStart, maxNodes);
			mptEnv.nSustainStart = Math.Min(Slb, maxNodes);
			mptEnv.nSustainEnd = Math.Clamp(Sle, mptEnv.nSustainStart, maxNodes);

			// Envelope Data
			// Attention: Full MPTM envelope is stored in extended instrument properties
			array<Node> data = Data.ToArray();
			for (uint32 ev = 0; ev < Math.Min((uint8)25, Num); ev++)
			{
				mptEnv[ev].Value = (uint8)OpenMpt.Clamp<int8, int8>((int8)(data[ev].Value + envOffset), 0, 64);
				mptEnv[ev].Tick = data[ev].Tick;

				if ((ev > 0) && (mptEnv[ev].Tick < mptEnv[ev - 1].Tick) && ((mptEnv[ev].Tick & 0xff00) == 0))
				{
					// Fix broken envelopes... Instruments 2 and 3 in NoGap.it by Werewolf have envelope points where the high byte of envelope nodes is missing.
					// NoGap.it was saved with MPT 1.07 - 1.09, which *normally* doesn't do this in IT files.
					// However... It turns out that MPT 1.07 omitted the high byte of envelope nodes when saving an XI instrument file, and it looks like
					// Instrument 2 and 3 in NoGap.it were loaded from XI files
					mptEnv[ev].Tick |= (uint16)(mptEnv[ev - 1].Tick & 0xff00);

					if (mptEnv[ev].Tick < mptEnv[ev - 1].Tick)
						mptEnv[ev].Tick += 0x100;
				}
			}
		}
	}
}
