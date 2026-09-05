/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt
{
	/// <summary>
	/// libopenmpt public c++ interface
	/// </summary>
	public static class LibOpenMpt
	{
		/********************************************************************/
		/// <summary>
		/// Return a list of all available formats
		/// </summary>
		/********************************************************************/
		public static IEnumerable<FileFormat> GetFormats()
		{
			foreach (CSoundFile.FileFormatLoader format in CSoundFile.GetFormats())
			{
				yield return new FileFormat
				{
					Id = format.Id,
					Name = format.Name,
					Description = format.Description
				};
			}
		}



		/********************************************************************/
		/// <summary>
		/// Probe the provided bytes from the beginning of a file for
		/// supported file format headers to find out whether libopenmpt
		/// might be able to open it
		/// </summary>
		/********************************************************************/
		public static Probe_File_Header_Result Probe_File_Header(Probe_File_Header_Flags flags, Stream stream, out Probe_File_Header_Info info)//XX 157
		{
			return Module_Impl.Probe_File_Header(flags, stream, out info);
		}
	}
}
