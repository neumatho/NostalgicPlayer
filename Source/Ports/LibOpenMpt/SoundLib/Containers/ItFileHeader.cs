/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// IT file header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 192)]
	internal struct ItFileHeader : IClearable
	{
		#region Header Flags
		public const uint16 UseStereoPlayback = 0x01;
		public const uint16 Vol0Optimisations = 0x02;
		public const uint16 InstrumentMode = 0x04;
		public const uint16 LinearSlides = 0x08;
		public const uint16 ItOldEffects = 0x10;
		public const uint16 ItCompatGxx = 0x20;
		public const uint16 UseMidiPitchController = 0x40;
		public const uint16 ReqEmbeddedMidiConfig = 0x80;
		public const uint16 ExtendedFilterRange = 0x1000;
		#endregion

		#region Special Flags
		public const uint16 EmbedSongMessage = 0x01;
		public const uint16 EmbedEditHistory = 0x02;
		public const uint16 EmbedPatternHighlights = 0x04;
		public const uint16 EmbedMidiConfiguration = 0x08;
		#endregion

		/// <summary>
		/// Magic Bytes (IMPM)
		/// </summary>
		public Array4 Id;

		/// <summary>
		/// Song Name, null-terminated (but may also contain nulls)
		/// </summary>
		public String26 SongName;

		/// <summary>
		/// Rows per Beat highlight
		/// </summary>
		public uint8le Highlight_Minor;

		/// <summary>
		/// Rows per Measure highlight
		/// </summary>
		public uint8le Highlight_Major;

		/// <summary>
		/// Number of Orders
		/// </summary>
		public uint16le OrdNum;

		/// <summary>
		/// Number of Instruments
		/// </summary>
		public uint16le InsNum;

		/// <summary>
		/// Number of Samples
		/// </summary>
		public uint16le SmpNum;

		/// <summary>
		/// Number of Patterns
		/// </summary>
		public uint16le PatNum;

		/// <summary>
		/// "Made With" Tracker
		/// </summary>
		public uint16le Cwtv;

		/// <summary>
		/// "Compatible With" Tracker
		/// </summary>
		public uint16le Cmwt;

		/// <summary>
		/// Header Flags
		/// </summary>
		public uint16le Flags;

		/// <summary>
		/// Special Flags, for embedding extra information
		/// </summary>
		public uint16le Special;

		/// <summary>
		/// Global Volume (0...128)
		/// </summary>
		public uint8le GlobalVol;

		/// <summary>
		/// Master Volume (0...128), referred to as Sample Volume in OpenMPT
		/// </summary>
		public uint8le Mv;

		/// <summary>
		/// Initial Speed (1...255)
		/// </summary>
		public uint8le Speed;

		/// <summary>
		/// Initial Tempo (31...255)
		/// </summary>
		public uint8le Tempo;

		/// <summary>
		/// Pan Separation (0...128)
		/// </summary>
		public uint8le Sep;

		/// <summary>
		/// Pitch Wheel Depth
		/// </summary>
		public uint8le Pwd;

		/// <summary>
		/// Length of Song Message
		/// </summary>
		public uint16le MsgLength;

		/// <summary>
		/// Offset of Song Message in File (IT crops message after first null)
		/// </summary>
		public uint32le MsgOffset;

		/// <summary>
		/// Some IT versions save an edit timer here. ChibiTracker writes "CHBI" here. OpenMPT and Schism Tracker save extended version information here
		/// </summary>
		public uint32le Reserved;

		/// <summary>
		/// Initial Channel Panning
		/// </summary>
		public Array64 ChnPan;

		/// <summary>
		/// Initial Channel Volume
		/// </summary>
		public Array64 ChnVol;

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
