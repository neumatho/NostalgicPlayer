/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// XM file header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 80)]
	internal struct XmFileHeader : IClearable
	{
		#region XmHeaderFlags
		public const uint16 LinearSlides = 0x01;
		public const uint16 ExtendedFilterRange = 0x1000;
		#endregion

		/// <summary>
		/// "Extended Module: "
		/// </summary>
		public String17 Signature;

		/// <summary>
		/// Song Name, space-padded
		/// </summary>
		public String20 SongName;

		/// <summary>
		/// DOS EOF Character (0x1A)
		/// </summary>
		public uint8le Eof;

		/// <summary>
		/// Software that was used to create the XM file
		/// </summary>
		public String20 TrackerName;

		/// <summary>
		/// File version (1.02 - 1.04 are supported)
		/// </summary>
		public uint16le Version;

		/// <summary>
		/// Header Size
		/// </summary>
		public uint32le Size;

		/// <summary>
		/// Number of Orders
		/// </summary>
		public uint16le Orders;

		/// <summary>
		/// Restart Position
		/// </summary>
		public uint16le RestartPos;

		/// <summary>
		/// Number of Channels
		/// </summary>
		public uint16le Channels;

		/// <summary>
		/// Number of Patterns
		/// </summary>
		public uint16le Patterns;

		/// <summary>
		/// Number of Instruments
		/// </summary>
		public uint16le Instruments;

		/// <summary>
		/// Song Flags
		/// </summary>
		public uint16le Flags;

		/// <summary>
		/// Default Speed
		/// </summary>
		public uint16le Speed;

		/// <summary>
		/// Default Tempo
		/// </summary>
		public uint16le Tempo;

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
