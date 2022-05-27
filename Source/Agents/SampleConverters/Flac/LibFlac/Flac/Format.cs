/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Diagnostics;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Format
	{
		public static readonly Flac__byte[] Flac__Stream_Sync_String = { 0x66, 0x4c, 0x61, 0x43 };	// fLaC

		/********************************************************************/
		/// <summary>
		/// Tests that a sample rate is valid for FLAC
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_Sample_Rate_Is_Valid(uint32_t sample_Rate)
		{
			if ((sample_Rate == 0) || (sample_Rate > Constants.Flac__Max_Sample_Rate))
				return false;
			else
				return true;
		}



		/********************************************************************/
		/// <summary>
		/// Tests that a sample rate is valid for the FLAC subset. The subset
		/// rules for valid sample rates are slightly more complex since the
		/// rate has to be expressible completely in the frame header
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_Sample_Rate_Is_Subset(uint32_t sample_Rate)
		{
			if (!Flac__Format_Sample_Rate_Is_Valid(sample_Rate) || ((sample_Rate >= (1 << 16)) && !((sample_Rate % 1000 == 0) || (sample_Rate % 10 == 0))))
				return false;
			else
				return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check a Vorbis comment entry name to see if it conforms to the
		/// Vorbis comment specification.
		///
		/// Vorbis comment names must be composed only of characters from
		/// [0x20-0x3C,0x3E-0x7D]
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_VorbisComment_Entry_Name_Is_Legal(string name)
		{
			foreach (char c in name)
			{
				if ((c < 0x20) || (c == 0x3d) || (c > 0x7d))
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check a Vorbis comment entry value to see if it conforms to the
		/// Vorbis comment specification.
		///
		/// Vorbis comment values must be valid UTF-8 sequences
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_VorbisComment_Entry_Value_Is_Legal(Flac__byte[] value, uint32_t length)
		{
			uint32_t offset = 0;

			if (length == uint32_t.MaxValue)
			{
				while (value[offset] != 0)
				{
					uint32_t n = Utf8Len(value, offset);
					if (n == 0)
						return false;

					offset += n;
				}
			}
			else
			{
				uint32_t end = length;

				while (offset < end)
				{
					uint32_t n = Utf8Len(value, offset);
					if (n == 0)
						return false;

					offset += n;
				}

				if (offset != end)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check a Vorbis comment entry to see if it conforms to the Vorbis
		/// comment specification.
		///
		/// Vorbis comment entries must be of the form 'name=value', and
		/// 'name' and 'value' must be legal according to
		/// FLAC__Format_VorbisComment_Entry_Name_Is_Legal() and
		/// FLAC__Format_VorbisComment_Entry_Value_Is_Legal() respectively
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_VorbisComment_Entry_Is_Legal(Flac__byte[] entry, uint32_t length)
		{
			uint32_t s, end;

			for (s = 0, end = length; (s < end) && (entry[s] != '='); s++)
			{
				if ((entry[s] < 0x20) || (entry[s] > 0x7d))
					return false;
			}

			if (s == end)
				return false;

			s++;	// Skip '='

			while (s < end)
			{
				uint32_t n = Utf8Len(entry, s);
				if (n == 0)
					return false;

				s += n;
			}

			if (s != end)
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(Flac__EntropyCodingMethod_PartitionedRiceContents @object)
		{
			Debug.Assert(@object != null);

			@object.Parameters = null;
			@object.Raw_Bits = null;
			@object.Capacity_By_Order = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Clear(Flac__EntropyCodingMethod_PartitionedRiceContents @object)
		{
			Debug.Assert(@object != null);

			if (@object.Parameters != null)
				@object.Parameters = null;

			if (@object.Raw_Bits != null)
				@object.Raw_Bits = null;

			Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Init(@object);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_Entropy_Coding_Method_Partitioned_Rice_Contents_Ensure_Size(Flac__EntropyCodingMethod_PartitionedRiceContents @object, uint32_t max_Partition_Order)
		{
			Debug.Assert(@object != null);

			Debug.Assert((@object.Capacity_By_Order > 0) || ((@object.Parameters == null) && (@object.Raw_Bits == null)));

			if (@object.Capacity_By_Order < max_Partition_Order)
			{
				if ((@object.Parameters = Alloc.Safe_Realloc(@object.Parameters, 1U << (int)max_Partition_Order)) == null)
					return false;

				if ((@object.Raw_Bits = Alloc.Safe_Realloc(@object.Raw_Bits, 1U << (int)max_Partition_Order)) == null)
					return false;

				Array.Clear(@object.Raw_Bits);
				@object.Capacity_By_Order = max_Partition_Order;
			}

			return true;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Also disallows non-shortest-form encodings, c.f.
		///   http://www.unicode.org/versions/corrigendum1.html
		/// and a more clear explanation at the end of this section:
		///   http://www.cl.cam.ac.uk/~mgk25/unicode.html#utf-8
		/// </summary>
		/********************************************************************/
		private static uint32_t Utf8Len(Flac__byte[] utf8, uint32_t offset)
		{
			Debug.Assert(utf8 != null);

			if (offset >= utf8.Length)
				return 1;

			if ((utf8[offset] & 0x80) == 0)
				return 1;

			if ((offset + 1) >= utf8.Length)
				return 0;

			if (((utf8[offset] & 0xe0) == 0xc0) && ((utf8[offset + 1] & 0xc0) == 0x80))
			{
				if ((utf8[offset] & 0xfe) == 0xc0)		// Overlong sequence check
					return 0;

				return 2;
			}

			if ((offset + 2) >= utf8.Length)
				return 0;
			
			if (((utf8[offset] & 0xf0) == 0xe0) && ((utf8[offset + 1] & 0xc0) == 0x80) && ((utf8[offset + 2] & 0xc0) == 0x80))
			{
				if ((utf8[offset] == 0xe0) && (utf8[offset + 1] & 0xe0) == 0x80)		// Overlong sequence check
					return 0;

				// Illegal surrogates check (U+D800...U+DFFF and U+FFFE...U+FFFF)
				if ((utf8[offset] == 0xed) && (utf8[offset + 1] & 0xe0) == 0xa0)		// D800-DFFF
					return 0;

				if ((utf8[offset] == 0xef) && (utf8[offset + 1] == 0xbf) && (utf8[offset + 2] & 0xfe) == 0xbe)	// FFFE-FFFF
					return 0;

				return 3;
			}

			if ((offset + 3) >= utf8.Length)
				return 0;

			if (((utf8[offset] & 0xf8) == 0xf0) && ((utf8[offset + 1] & 0xc0) == 0x80) && ((utf8[offset + 2] & 0xc0) == 0x80) && ((utf8[offset + 3] & 0xc0) == 0x80))
			{
				if ((utf8[offset] == 0xf0) && (utf8[offset + 1] & 0xf0) == 0x80)		// Overlong sequence check
					return 0;

				return 4;
			}

			if ((offset + 4) >= utf8.Length)
				return 0;

			if (((utf8[offset] & 0xfc) == 0xf8) && ((utf8[offset + 1] & 0xc0) == 0x80) && ((utf8[offset + 2] & 0xc0) == 0x80) && ((utf8[offset + 3] & 0xc0) == 0x80) && ((utf8[offset + 4] & 0xc0) == 0x80))
			{
				if ((utf8[offset] == 0xf8) && (utf8[offset + 1] & 0xf8) == 0x80)		// Overlong sequence check
					return 0;

				return 5;
			}

			if ((offset + 5) >= utf8.Length)
				return 0;

			if (((utf8[offset] & 0xfe) == 0xfc) && ((utf8[offset + 1] & 0xc0) == 0x80) && ((utf8[offset + 2] & 0xc0) == 0x80) && ((utf8[offset + 3] & 0xc0) == 0x80) && ((utf8[offset + 4] & 0xc0) == 0x80) && ((utf8[offset + 5] & 0xc0) == 0x80))
			{
				if ((utf8[offset] == 0xfc) && (utf8[offset + 1] & 0xfc) == 0x80)		// Overlong sequence check
					return 0;

				return 6;
			}

			return 0;
		}
		#endregion
	}
}
