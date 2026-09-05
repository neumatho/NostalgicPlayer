/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class FileHistory : IEquatable<FileHistory>
	{
		public const c_double History_Timer_Precision = 18.2;

		/// <summary>
		/// Date when the file was loaded in the tracker or created.
		/// </summary>
		public DateTime? LoadDate;

		/// <summary>
		/// Time the file was open in the editor, in 1/18.2th seconds (frequency of a standard DOS timer, to keep compatibility with Impulse Tracker easy)
		/// </summary>
		public uint32 OpenTime = 0;

		/********************************************************************/
		/// <summary>
		/// Returns true if the date component is valid. Some formats only
		/// store edit time, not edit date
		/// </summary>
		/********************************************************************/
		public bool HasValidDate()
		{
			return LoadDate.HasValue;
		}



		/********************************************************************/
		/// <summary>
		/// Return the date as a (possibly truncated if not enough precision
		/// is available) ISO 8601 formatted date
		/// </summary>
		/********************************************************************/
		public string AsIso8601(LogicalTimezone internalTimezone)
		{
			if (!LoadDate.HasValue)
				return string.Empty;

			DateTime date = LoadDate.Value;

			if (OpenTime > 0)
			{
				// Calculate the date when editing finished
				c_double openSeconds = OpenTime / History_Timer_Precision;
				date = date.AddSeconds(SaturateRound.Saturate_Round<int64, c_double>(openSeconds));
			}

			return MptTime.ToShortenedIso8601(date, internalTimezone);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (FileHistory me, FileHistory other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return (me.LoadDate == other.LoadDate) && (me.OpenTime == other.OpenTime);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (FileHistory me, FileHistory other)
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
			return (obj is FileHistory other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(FileHistory other)
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

			hash.Add(LoadDate);
			hash.Add(OpenTime);

			return hash.ToHashCode();
		}
	}
}
