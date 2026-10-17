/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// IT Header extension: Save history
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	internal struct ItHistoryStruct : IClearable
	{
		/// <summary>
		/// DOS / FAT date when the file was opened / created in the editor. For details, read https://docs.microsoft.com/de-de/windows/win32/api/winbase/nf-winbase-dosdatetimetofiletime
		/// </summary>
		public uint16le FatDate;

		/// <summary>
		/// DOS / FAT time when the file was opened / created in the editor
		/// </summary>
		public uint16le FatTime;

		/// <summary>
		/// The time how long the file was open in the editor, in 1/18.2th seconds. (= ticks of the DOS timer)
		/// </summary>
		public uint32le RunTime;

		/********************************************************************/
		/// <summary>
		/// Convert an ItHistoryStruct to OpenMPT's internal edit history
		/// representation
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(FileHistory mptHistory)
		{
			// Decode FAT date and time
			mptHistory.LoadDate = DateTime.MinValue;

			if ((FatDate != 0) || (FatTime != 0))
			{
				c_int year = ((FatDate >> 9) & 0x7f) + 1980;
				c_int month = OpenMpt.Clamp((FatDate >> 5) & 0x0f, 1, 12);
				c_int day = OpenMpt.Clamp(FatDate & 0x1f, 1, 31);
				c_int hours = OpenMpt.Clamp((FatTime >> 11) & 0x1f, 0, 23);
				c_int minutes = OpenMpt.Clamp((FatTime >> 5) & 0x3f, 0, 59);
				c_int seconds = OpenMpt.Clamp((FatTime & 0x1f) * 2, 0, 59);

				// DateTime does not accept days past the end of the month
				day = Math.Min(day, DateTime.DaysInMonth(year, month));

				mptHistory.LoadDate = new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Unspecified);
			}

			mptHistory.OpenTime = (uint32)(RunTime * (FileHistory.History_Timer_Precision / 18.2));
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
