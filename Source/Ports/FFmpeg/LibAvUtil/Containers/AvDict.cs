/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Flags that influence behavior of the matching of keys or insertion to the dictionary
	/// </summary>
	[Flags]
	public enum AvDict
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// Only get an entry with exact-case key match. Only relevant in av_dict_get()
		/// </summary>
		Match_Case = 1,

		/// <summary>
		/// Return first entry in a dictionary whose first part corresponds to the search key,
		/// ignoring the suffix of the found key string. Only relevant in av_dict_get()
		/// </summary>
		Ignore_Suffix = 2,

		/// <summary>
		/// Take ownership of a key that's been
		/// allocated with av_malloc() or another memory allocation function
		/// </summary>
		Dont_Strdup_Key = 4,

		/// <summary>
		/// Take ownership of a value that's been
		/// allocated with av_malloc() or another memory allocation function
		/// </summary>
		Dont_Strdup_Val = 8,

		/// <summary>
		/// Don't overwrite existing entries
		/// </summary>
		Dont_Overwrite = 16,

		/// <summary>
		/// If the entry already exists, append to it. Note that no
		/// delimiter is added, the strings are simply concatenated
		/// </summary>
		Append = 32,

		/// <summary>
		/// Allow to store several equal keys in the dictionary
		/// </summary>
		MultiKey = 64,

		/// <summary>
		/// If inserting a value that already exists for a key, do nothing. Only relevant with AV_DICT_MULTIKEY
		/// </summary>
		Dedup = 128
	}
}
