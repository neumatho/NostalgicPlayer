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
	/// S3M file header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 96)]
	internal struct S3MFileHeader : IClearable
	{
		#region Magic bytes
		public const uint8 IdEof = 0x1a;
		public const uint8 IdS3MType = 0x10;
		public const uint8 IdPanning = 0xfc;
		#endregion

		#region Tracker Versions in the cwtv field
		public const uint16 TrackerMask = 0xf000;
		public const uint16 VersionMask = 0x0fff;

		public const uint16 TrkScreamTracker = 0x1000;
		public const uint16 TrkImagoOrpheus = 0x2000;
		public const uint16 TrkImpulseTracker = 0x3000;
		public const uint16 TrkSchismTracker = 0x4000;
		public const uint16 TrkOpenMpt = 0x5000;
		public const uint16 TrkBeRoTracker = 0x6000;
		public const uint16 TrkCreamTracker = 0x7000;

		public const uint16 TrkAkord = 0x0208;
		public const uint16 TrkSt3_00 = 0x1300;
		public const uint16 TrkSt3_01 = 0x1301;
		public const uint16 TrkSt3_20 = 0x1320;
		public const uint16 TrkIt1_Old = 0x3320;
		public const uint16 TrkIt2_07 = 0x3207;
		public const uint16 TrkIt2_14 = 0x3214;
		public const uint16 TrkBeRoTrackerOld = 0x4100;		// Used from 2004 to 2012
		public const uint16 TrkGraoumfTracker = 0x5447;
		public const uint16 TrkNesMusa = 0x5700;
		public const uint16 TrkCamoto = 0xca00;
		public const uint16 TrkPlayerPro = 0x2013;			// PlayerPRO on Intel doesn't byte-swap the tracker ID bytes
		#endregion

		#region Flags
		/// <summary>
		/// Vibrato is twice as deep. Cannot be enabled from UI
		/// </summary>
		public const uint16 St2Vibrato = 0x01;

		/// <summary>
		/// Volume 0 optimisations
		/// </summary>
		public const uint16 ZeroVolOptim = 0x08;

		/// <summary>
		/// Enforce Amiga limits
		/// </summary>
		public const uint16 AmigaLimits = 0x10;

		/// <summary>
		/// Fast volume slides (like in ST3.00)
		/// </summary>
		public const uint16 FastVolumeSlides = 0x40;
		#endregion

		#region S3M Format Versions
		/// <summary>
		/// Old version, signed samples
		/// </summary>
		public const uint16 OldVersion = 0x01;

		/// <summary>
		/// New version, unsigned samples
		/// </summary>
		public const uint16 NewVersion = 0x02;
		#endregion

		/// <summary>
		/// Song Title
		/// </summary>
		public String28 Name;

		/// <summary>
		/// Supposed to be 0x1A, but even ST3 seems to ignore this sometimes (see STRSHINE.S3M by Purple Motion)
		/// </summary>
		public uint8le DosEof;

		/// <summary>
		/// File Type, 0x10 = ST3 module
		/// </summary>
		public uint8le FileType;

		/// <summary>
		/// Reserved
		/// </summary>
		public Array2 Reserved1;

		/// <summary>
		/// Number of order items
		/// </summary>
		public uint16le OrdNum;

		/// <summary>
		/// Number of sample parapointers
		/// </summary>
		public uint16le SmpNum;

		/// <summary>
		/// Number of pattern parapointers
		/// </summary>
		public uint16le PatNum;

		/// <summary>
		/// Flags, see S3MHeaderFlags
		/// </summary>
		public uint16le Flags;

		/// <summary>
		/// "Made With" Tracker ID, see S3MTrackerVersions
		/// </summary>
		public uint16le Cwtv;

		/// <summary>
		/// Format Version, see S3MFormatVersion
		/// </summary>
		public uint16le FormatVersion;

		/// <summary>
		/// "SCRM" magic bytes
		/// </summary>
		public Array4 Magic;

		/// <summary>
		/// Default Global Volume (0...64)
		/// </summary>
		public uint8le GlobalVol;

		/// <summary>
		/// Default Speed (1...254)
		/// </summary>
		public uint8le Speed;

		/// <summary>
		/// Default Tempo (33...255)
		/// </summary>
		public uint8le Tempo;

		/// <summary>
		/// Sample Volume (0...127, stereo if high bit is set)
		/// </summary>
		public uint8le MasterVolume;

		/// <summary>
		/// Number of channels used for ultra click removal
		/// </summary>
		public uint8le UltraClicks;

		/// <summary>
		/// 0xFC =› read extended panning table
		/// </summary>
		public uint8le UsePanningTable;

		/// <summary>
		/// Schism Tracker and OpenMPT use this for their extended version information
		/// </summary>
		public uint16le Reserved2;

		/// <summary>
		/// Impulse Tracker hides its edit timer here
		/// </summary>
		public uint32le Reserved3;

		/// <summary>
		/// 
		/// </summary>
		public uint16le Reserved4;

		/// <summary>
		/// Pointer to special custom data (unused)
		/// </summary>
		public uint16le Special;

		/// <summary>
		/// Channel setup
		/// </summary>
		public Array32 Channels;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint8 GetNumChannels()
		{
			uint8 numChannels = 4;

			for (uint8 i = 0; i < 32; i++)
			{
				if (Channels[i] != 0xff)
					numChannels = (uint8)(i + 1);
			}

			return numChannels;
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
