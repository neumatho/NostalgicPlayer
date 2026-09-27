/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.FileFormat_Base
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Magic
	{
		/********************************************************************/
		/// <summary>
		/// Functions to create 4-byte and 2-byte magic byte identifiers in
		/// little-endian format.
		/// Use this together with uint32le/uint16le file members
		/// </summary>
		/********************************************************************/
		public static uint32 MagicLE(string id)
		{
			return (uint32)(((uint8)id[3] << 24) | ((uint8)id[2] << 16) | ((uint8)id[1] << 8) | (uint8)id[0]);
		}



		/********************************************************************/
		/// <summary>
		/// Functions to create 4-byte and 2-byte magic byte identifiers in
		/// big-endian format.
		/// Use this together with uint32be/uint16be file members.
		/// Note: Historically, some magic bytes in MPT-specific fields are
		/// reversed (due to the use of multi-char literals).
		/// Such fields turned up reversed in files, so MagicBE is used to
		/// keep them readable in the code
		/// </summary>
		/********************************************************************/
		public static uint32 MagicBE(string id)
		{
			return (uint32)(((uint8)id[0] << 24) | ((uint8)id[1] << 16) | ((uint8)id[2] << 8) | (uint8)id[3]);
		}



		/********************************************************************/
		/// <summary>
		/// Converts a little-endian magic back into its 4 character string
		/// representation. Use this when you need to switch on a magic
		/// </summary>
		/********************************************************************/
		public static string MagicToStringLE(uint32 magic)
		{
			return new string([ (char)(magic & 0xff), (char)((magic >> 8) & 0xff), (char)((magic >> 16) & 0xff), (char)(magic >> 24) ]);
		}



		/********************************************************************/
		/// <summary>
		/// Converts a big-endian magic back into its 4 character string
		/// representation. Use this when you need to switch on a magic
		/// </summary>
		/********************************************************************/
		public static string MagicToStringBE(uint32 magic)
		{
			return new string([ (char)(magic >> 24), (char)((magic >> 16) & 0xff), (char)((magic >> 8) & 0xff), (char)(magic & 0xff) ]);
		}
	}
}
