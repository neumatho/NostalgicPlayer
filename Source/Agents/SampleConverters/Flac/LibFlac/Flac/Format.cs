/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
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
		private class SeekPoint_Compare : IComparer<Flac__StreamMetadata_SeekPoint>
		{
			/********************************************************************/
			/// <summary>
			/// Used as the sort predicate
			/// </summary>
			/********************************************************************/
			public int Compare(Flac__StreamMetadata_SeekPoint l, Flac__StreamMetadata_SeekPoint r)
			{
				// We don't just 'return l.sample_Number - r.sample_Number' since the result (Flac__int64) might overflow an 'int'
				if (l.Sample_Number == r.Sample_Number)
					return 0;
				else if (l.Sample_Number < r.Sample_Number)
					return -1;
				else
					return 1;
			}
		}

		public static readonly Flac__byte[] Flac__Stream_Sync_String = { 0x66, 0x4c, 0x61, 0x43 };	// fLaC

		/********************************************************************/
		/// <summary>
		/// Return the vendor string
		/// </summary>
		/********************************************************************/
		public static string Flac__Vendor_String => "NostalgicPlayer (reference libFLAC 1.3.4 20220220)";



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
		/// Tests that a blocksize at the given sample rate is valid for the
		/// FLAC subset
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_BlockSize_Is_Subset(uint32_t blockSize, uint32_t sample_Rate)
		{
			if (blockSize > 16384)
				return false;
			else if ((sample_Rate <= 48000) && (blockSize > 4608))
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
		/// Check a seek table to see if it conforms to the FLAC
		/// specification. See the format specification for limits on the
		/// contents of the seek table
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_SeekTable_Is_Legal(Flac__StreamMetadata_SeekTable seek_Table)
		{
			Flac__uint64 prev_Sample_Number = 0;
			Flac__bool got_Prev = false;

			Debug.Assert(seek_Table != null);

			for (uint32_t i = 0; i < seek_Table.Num_Points; i++)
			{
				if (got_Prev)
				{
					if ((seek_Table.Points[i].Sample_Number != Constants.Flac__Stream_Metadata_SeekPoint_Placeholder) && (seek_Table.Points[i].Sample_Number <= prev_Sample_Number))
						return false;
				}

				prev_Sample_Number = seek_Table.Points[i].Sample_Number;
				got_Prev = true;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sort a seek table's seek points according to the format
		/// specification. This includes a "unique-ification" step to remove
		/// duplicates, i.e. seek points with identical sample_number values.
		/// Duplicate seek points are converted into placeholder points and
		/// sorted to the end of the table
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__Format_SeekTable_Sort(Flac__StreamMetadata_SeekTable seek_Table)
		{
			Debug.Assert(seek_Table != null);

			if (seek_Table.Num_Points == 0)
				return 0;

			// Sort the seekpoints
			Array.Sort(seek_Table.Points, 0, (int)seek_Table.Num_Points, new SeekPoint_Compare());

			// Uniquify the seekpoints
			Flac__bool first = true;

			uint32_t j = 0;
			for (uint32_t i = 0; i < seek_Table.Num_Points; i++)
			{
				if (seek_Table.Points[i].Sample_Number != Constants.Flac__Stream_Metadata_SeekPoint_Placeholder)
				{
					if (!first)
					{
						if (seek_Table.Points[i].Sample_Number == seek_Table.Points[j - 1].Sample_Number)
							continue;
					}
				}

				first = false;
				seek_Table.Points[j++] = seek_Table.Points[i];
			}

			for (uint32_t i = j; i < seek_Table.Num_Points; i++)
			{
				seek_Table.Points[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
				seek_Table.Points[i].Stream_Offset = 0;
				seek_Table.Points[i].Frame_Samples = 0;
			}

			return j;
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
		/// Check a cue sheet to see if it conforms to the FLAC specification.
		/// See the format specification for limits on the contents of the
		/// cue sheet
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_CueSheet_Is_Legal(Flac__StreamMetadata_CueSheet cue_Sheet, Flac__bool check_Cd_Da_Subset, out string violation)
		{
			if (check_Cd_Da_Subset)
			{
				if (cue_Sheet.Lead_In < 2 * 44100)
				{
					violation = "CD-DA cue sheet must have a lead-in length of at least 2 seconds";
					return false;
				}

				if ((cue_Sheet.Lead_In % 588) != 0)
				{
					violation = "CD-DA cue sheet lead-in length must be evenly divisible by 588 samples";
					return false;
				}
			}

			if (cue_Sheet.Num_Tracks == 0)
			{
				violation = "Cue sheet must have at least one track (the lead-out)";
				return false;
			}

			if (check_Cd_Da_Subset && (cue_Sheet.Tracks[cue_Sheet.Num_Tracks - 1].Number != 170))
			{
				violation = "CD-DA cue sheet must have a lead-out track number 170 (0xAA)";
				return false;
			}

			for (uint32_t i = 0; i < cue_Sheet.Num_Tracks; i++)
			{
				if (cue_Sheet.Tracks[i].Number == 0)
				{
					violation = "Cue sheet may not have a track number 0";
					return false;
				}

				if (check_Cd_Da_Subset)
				{
					if (!(((cue_Sheet.Tracks[i].Number >= 1) && (cue_Sheet.Tracks[i].Number <= 99)) || (cue_Sheet.Tracks[i].Number == 170)))
					{
						violation = "CD-DA cue sheet track number must be 1-99 or 170";
						return false;
					}
				}

				if (check_Cd_Da_Subset && ((cue_Sheet.Tracks[i].Offset % 588) != 0))
				{
					if (i == cue_Sheet.Num_Tracks - 1)
						violation = "CD-DA cue sheet lead-out offset must be evenly divisible by 588 samples";
					else
						violation = "CD-DA cue sheet track offset must be evenly divisible by 588 samples";

					return false;
				}

				if (i < cue_Sheet.Num_Tracks - 1)
				{
					if (cue_Sheet.Tracks[i].Num_Indices == 0)
					{
						violation = "Cue sheet track must have at least one index point";
						return false;
					}

					if (cue_Sheet.Tracks[i].Indices[0].Number > 1)
					{
						violation = "Cue sheet track's first index number must be 0 or 1";
						return false;
					}
				}

				for (uint32_t j = 0; j < cue_Sheet.Tracks[i].Num_Indices; j++)
				{
					if (check_Cd_Da_Subset && ((cue_Sheet.Tracks[i].Indices[j].Offset % 588) != 0))
					{
						violation = "CD-DA cue sheet track index offset must be evenly divisible by 588 samples";
						return false;
					}

					if (j > 0)
					{
						if (cue_Sheet.Tracks[i].Indices[j].Number != (cue_Sheet.Tracks[i].Indices[j - 1].Number + 1))
						{
							violation = "Cue sheet track index numbers must increase by 1";
							return false;
						}
					}
				}
			}

			violation = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check a PICTURE block to see if it conforms to the FLAC
		/// specification. See the format specification for limits on the
		/// contents of the PICTURE block
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Format_Picture_Is_Legal(Flac__StreamMetadata_Picture picture, out string violation)
		{
			for (int p = 0; picture.Mime_Type[p] != 0; p++)
			{
				if ((picture.Mime_Type[p] < 0x20) || (picture.Mime_Type[p] > 0x7e))
				{
					violation = "MIME type string must contain only printable ASCII characters (0x20-0x7e)";
					return false;
				}
			}

			for (uint32_t b = 0; picture.Description[b] != 0; )
			{
				uint32_t n = Utf8Len(picture.Description, b);
				if (n == 0)
				{
					violation = "Description string must be valid UTF-8";
					return false;
				}

				b += n;
			}

			violation = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__Format_Get_Max_Rice_Partition_Order_From_BlockSize(uint32_t blockSize)
		{
			uint32_t max_Rice_Partition_Order = 0;

			while ((blockSize & 1) == 0)
			{
				max_Rice_Partition_Order++;
				blockSize >>= 1;
			}

			return Math.Min(Constants.Flac__Max_Rice_Partition_Order, max_Rice_Partition_Order);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32_t Flac__Format_Get_Max_Rice_Partition_Order_From_BlockSize_Limited_Max_And_Predictor_Order(uint32_t limit, uint32_t blockSize, uint32_t predictor_Order)
		{
			uint32_t max_Rice_Partition_Order = limit;

			while ((max_Rice_Partition_Order > 0) && ((blockSize >> (int)max_Rice_Partition_Order) <= predictor_Order))
				max_Rice_Partition_Order--;

			Debug.Assert(((max_Rice_Partition_Order == 0) && (blockSize >= predictor_Order)) || ((max_Rice_Partition_Order > 0) && ((blockSize >> (int)max_Rice_Partition_Order) > predictor_Order)));

			return max_Rice_Partition_Order;
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
