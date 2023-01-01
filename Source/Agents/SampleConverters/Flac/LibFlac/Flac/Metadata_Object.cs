/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Diagnostics;
using System.Text;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac
{
	/// <summary>
	/// This module provides functions for creating and manipulating FLAC
	/// metadata blocks in memory, and three progressively more powerful
	/// interfaces for traversing and editing metadata in native FLAC files.
	///
	/// There are three metadata interfaces of increasing complexity:
	///
	/// Level 0:
	/// Read-only access to the STREAMINFO, VORBIS_COMMENT, CUESHEET, and
	/// PICTURE blocks.
	///
	/// Level 1:
	/// Read-write access to all metadata blocks. This level is write-
	/// efficient in most cases (more on this below), and uses less memory
	/// than level 2.
	///
	/// Level 2:
	/// Read-write access to all metadata blocks. This level is write-
	/// efficient in all cases, but uses more memory since all metadata for
	/// the whole file is read into memory and manipulated before writing
	/// out again.
	///
	/// What do we mean by efficient? Since FLAC metadata appears at the
	/// beginning of the file, when writing metadata back to a FLAC file
	/// it is possible to grow or shrink the metadata such that the entire
	/// file must be rewritten. However, if the size remains the same during
	/// changes or PADDING blocks are utilized, only the metadata needs to be
	/// overwritten, which is much faster.
	///
	/// Efficient means the whole file is rewritten at most one time, and only
	/// when necessary. Level 1 is not efficient only in the case that you
	/// cause more than one metadata block to grow or shrink beyond what can
	/// be accommodated by padding. In this case you should probably use level
	/// 2, which allows you to edit all the metadata for a file in memory and
	/// write it out all at once.
	///
	/// All levels know how to skip over and not disturb an ID3v2 tag at the
	/// front of the file.
	///
	/// All levels access files via their filenames. In addition, level 2
	/// has additional alternative read and write functions that take an I/O
	/// handle and callbacks, for situations where access by filename is not
	/// possible.
	///
	/// In addition to the three interfaces, this module defines functions for
	/// creating and manipulating various metadata objects in memory. As we see
	/// from the Format module, FLAC metadata blocks in memory are very primitive
	/// structures for storing information in an efficient way. Reading
	/// information from the structures is easy but creating or modifying them
	/// directly is more complex. The metadata object routines here facilitate
	/// this by taking care of the consistency and memory management drudgery.
	///
	/// Unless you will be using the level 1 or 2 interfaces to modify existing
	/// metadata however, you will not probably not need these.
	///
	/// From a dependency standpoint, none of the encoders or decoders require
	/// the metadata module. This is so that embedded users can strip out the
	/// metadata module from libFLAC to reduce the size and complexity
	/// </summary>
	internal static class Metadata_Object
	{
		/********************************************************************/
		/// <summary>
		/// Create a new metadata object instance of the given type.
		///
		/// The object will be "empty"; i.e. values and data pointers will
		/// be zeroed, with the exception of
		/// FLAC__METADATA_TYPE_VORBIS_COMMENT, which will have the vendor
		/// string set (but zero comments).
		///
		/// Do not pass in a value greater than or equal to
		/// FLAC__METADATA_TYPE_UNDEFINED unless you really know what you're
		/// doing
		/// <param name="type">Type of object to create</param>
		/// <returns>NULL if there was an error allocating memory or the type code is greater than FLAC__MAX_METADATA_TYPE_CODE, else the new instance</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__StreamMetadata Flac__Metadata_Object_New(Flac__MetadataType type)
		{
			if (type > Flac__MetadataType.Max_Metadata_Type)
				return null;

			Flac__StreamMetadata @object = new Flac__StreamMetadata();
			@object.Is_Last = false;
			@object.Type = type;

			switch (type)
			{
				case Flac__MetadataType.StreamInfo:
				{
					@object.Length = Constants.Flac__Stream_Metadata_StreamInfo_Length;
					@object.Data = new Flac__StreamMetadata_StreamInfo();
					break;
				}

				case Flac__MetadataType.Padding:
				{
					@object.Data = new Flac__StreamMetadata_Padding();
					break;
				}

				case Flac__MetadataType.Application:
				{
					@object.Length = Constants.Flac__Stream_Metadata_Application_Id_Len / 8;
					@object.Data = new Flac__StreamMetadata_Application();
					break;
				}

				case Flac__MetadataType.SeekTable:
				{
					@object.Data = new Flac__StreamMetadata_SeekTable();
					break;
				}

				case Flac__MetadataType.Vorbis_Comment:
				{
					Flac__StreamMetadata_VorbisComment metaVorbisComment = new Flac__StreamMetadata_VorbisComment();
					@object.Data = metaVorbisComment;

					metaVorbisComment.Vendor_String.Length = (uint32_t)Encoding.UTF8.GetByteCount(Format.Flac__Vendor_String);

					if (!Copy_Bytes(ref metaVorbisComment.Vendor_String.Entry, Encoding.UTF8.GetBytes(Format.Flac__Vendor_String), metaVorbisComment.Vendor_String.Length + 1))
						return null;

					VorbisComment_Calculate_Length(@object);
					break;
				}

				case Flac__MetadataType.CueSheet:
				{
					@object.Data = new Flac__StreamMetadata_CueSheet();
					CueSheet_Calculate_Length(@object);
					break;
				}

				case Flac__MetadataType.Picture:
				{
					@object.Length = (
						Constants.Flac__Stream_Metadata_Picture_Type_Len +
						Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len + // Empty mime_type string
						Constants.Flac__Stream_Metadata_Picture_Description_Length_Len + // Empty description string
						Constants.Flac__Stream_Metadata_Picture_Width_Len +
						Constants.Flac__Stream_Metadata_Picture_Height_Len +
						Constants.Flac__Stream_Metadata_Picture_Depth_Len +
						Constants.Flac__Stream_Metadata_Picture_Colors_Len +
						Constants.Flac__Stream_Metadata_Picture_Data_Length_Len +
						0	// No data
					) / 8;

					Flac__StreamMetadata_Picture metaPicture = new Flac__StreamMetadata_Picture();
					@object.Data = metaPicture;

					metaPicture.Type = Flac__StreamMetadata_Picture_Type.Other;

					// Now initialize mime_type and description with empty strings to make things easier on the client
					if (!Copy_CString(Encoding.ASCII, ref metaPicture.Mime_Type, string.Empty))
						return null;

					if (!Copy_CString(Encoding.UTF8, ref metaPicture.Description, string.Empty))
						return null;

					break;
				}

				default:
				{
					@object.Data = new Flac__StreamMetadata_Unknown();
					break;
				}
			}

			return @object;
		}



		/********************************************************************/
		/// <summary>
		/// Create a copy of an existing metadata object.
		///
		/// The copy is a "deep" copy, i.e. dynamically allocated data
		/// within the object is also copied. The caller takes ownership of
		/// the new block and is responsible for freeing it with
		/// FLAC__metadata_object_delete()
		/// <param name="object">Pointer to object to copy</param>
		/// <returns>NULL if there was an error allocating memory, else the new instance</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__StreamMetadata Flac__Metadata_Object_Clone(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object != null);

			Flac__StreamMetadata to = Flac__Metadata_Object_New(@object.Type);
			if (to != null)
			{
				to.Is_Last = @object.Is_Last;
				to.Type = @object.Type;
				to.Length = @object.Length;

				Flac__bool success;

				switch (to.Type)
				{
					case Flac__MetadataType.StreamInfo:
					{
						success = CloneStreamInfo(@object, to);
						break;
					}

					case Flac__MetadataType.Padding:
					{
						success = true;
						break;
					}

					case Flac__MetadataType.Application:
					{
						success = CloneApplication(@object, to);
						break;
					}

					case Flac__MetadataType.SeekTable:
					{
						success = CloneSeekTable(@object, to);
						break;
					}

					case Flac__MetadataType.Vorbis_Comment:
					{
						success = CloneVorbisComment(@object, to);
						break;
					}

					case Flac__MetadataType.CueSheet:
					{
						success = CloneCueSheet(@object, to);
						break;
					}

					case Flac__MetadataType.Picture:
					{
						success = ClonePicture(@object, to);
						break;
					}

					default:
					{
						success = CloneUnknown(@object, to);
						break;
					}
				}

				if (!success)
				{
					Flac__Metadata_Object_Delete(to);
					return null;
				}
			}

			return to;
		}



		/********************************************************************/
		/// <summary>
		/// Free a metadata object. Deletes the object pointed to by object.
		///
		/// The delete is a "deep" delete, i.e. dynamically allocated data
		/// within the object is also deleted
		/// <param name="object">A pointer to an existing object</param>
		/// </summary>
		/********************************************************************/
		public static void Flac__Metadata_Object_Delete(Flac__StreamMetadata @object)
		{
			Flac__Metadata_Object_Delete_Data(@object);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the application data of an APPLICATION block.
		///
		/// If copy is true, a copy of the data is stored; otherwise, the
		/// object takes ownership of the pointer. The existing data will be
		/// freed if this function is successful, otherwise the original data
		/// will remain if copy is true and new fails.
		/// <param name="object">A pointer to an existing APPLICATION object</param>
		/// <param name="data">A pointer to the data to set</param>
		/// <param name="length">The length of data in bytes</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and new fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_Application_Set_Data(Flac__StreamMetadata @object, Flac__byte[] data, uint32_t length, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Application);
			Debug.Assert(((data != null) && (length > 0)) || ((data == null) && (length == 0) && (copy == false)));

			// Do the copy first so that if we fail we leave the object untouched
			if (copy)
			{
				if (!Copy_Bytes(ref ((Flac__StreamMetadata_Application)@object.Data).Data, data, length))
					return false;
			}
			else
				((Flac__StreamMetadata_Application)@object.Data).Data = data;

			@object.Length = Constants.Flac__Stream_Metadata_Application_Id_Len / 8 + length;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Resize the seekpoint array.
		///
		/// If the size shrinks, elements will truncated; if it grows,
		/// new placeholder points will be added to the end
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="new_Num_Points">The desired length of the array; may be 0</param>
		/// <returns>False if memory allocation error, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Resize_Points(Flac__StreamMetadata @object, uint32_t new_Num_Points)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

			if (metaSeekTable.Points == null)
			{
				Debug.Assert(metaSeekTable.Num_Points == 0);

				if (new_Num_Points == 0)
					return true;
				else
				{
					metaSeekTable.Points = SeekPoint_Array_New(new_Num_Points);
					if (metaSeekTable.Points == null)
						return false;
				}
			}
			else
			{
				size_t old_Size = metaSeekTable.Num_Points;
				size_t new_Size = new_Num_Points;

				Debug.Assert(metaSeekTable.Num_Points > 0);

				if (new_Size == 0)
					metaSeekTable.Points = null;
				else
				{
					metaSeekTable.Points = Alloc.Safe_Realloc(metaSeekTable.Points, new_Size);
					if (metaSeekTable.Points == null)
						return false;

					// If growing, set new elements to placeholders
					if (new_Size > old_Size)
					{
						for (uint32_t i = metaSeekTable.Num_Points; i < new_Num_Points; i++)
						{
							metaSeekTable.Points[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
							metaSeekTable.Points[i].Stream_Offset = 0;
							metaSeekTable.Points[i].Frame_Samples = 0;
						}
					}
				}
			}

			metaSeekTable.Num_Points = new_Num_Points;

			SeekTable_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Set a seekpoint in a seektable
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="point_Num">Index into seekpoint array to set</param>
		/// <param name="point">The point to set</param>
		/// </summary>
		/********************************************************************/
		public static void Flac__Metadata_Object_SeekTable_Set_Point(Flac__StreamMetadata @object, uint32_t point_Num, Flac__StreamMetadata_SeekPoint point)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;
			Debug.Assert(point_Num <= metaSeekTable.Num_Points);

			Flac__StreamMetadata_SeekPoint newPoint = new Flac__StreamMetadata_SeekPoint();
			newPoint.Sample_Number = point.Sample_Number;
			newPoint.Stream_Offset = point.Stream_Offset;
			newPoint.Frame_Samples = point.Frame_Samples;

			metaSeekTable.Points[point_Num] = newPoint;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a seekpoint into a seektable
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="point_Num">Index into seekpoint array to set</param>
		/// <param name="point">The point to set</param>
		/// <returns>False if memory allocation error, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Insert_Point(Flac__StreamMetadata @object, uint32_t point_Num, Flac__StreamMetadata_SeekPoint point)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;
			Debug.Assert(point_Num <= metaSeekTable.Num_Points);

			if (!Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points + 1))
				return false;

			// Move all points >= point_Num forward one space
			for (uint32_t i = metaSeekTable.Num_Points - 1; i > point_Num; i--)
				metaSeekTable.Points[i] = metaSeekTable.Points[i - 1];

			Flac__Metadata_Object_SeekTable_Set_Point(@object, point_Num, point);
			SeekTable_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Delete a seekpoint from a seektable
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="point_Num">Index into seekpoint array to set</param>
		/// <returns>False if memory allocation error, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Delete_Point(Flac__StreamMetadata @object, uint32_t point_Num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;
			Debug.Assert(point_Num <= metaSeekTable.Num_Points);

			// Move all points > point_Num backward one space
			for (uint32_t i = point_Num; i < metaSeekTable.Num_Points - 1; i++)
				metaSeekTable.Points[i] = metaSeekTable.Points[i + 1];

			return Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Check a seektable to see if it conforms to the FLAC
		/// specification. See the format specification for limits on the
		/// contents of the seektable
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <returns>False if seek table is illegal, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Is_Legal(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			return Format.Flac__Format_SeekTable_Is_Legal((Flac__StreamMetadata_SeekTable)@object.Data);
		}



		/********************************************************************/
		/// <summary>
		/// Append a number of placeholder points to the end of a seek table.
		///
		/// As with the other ..._seektable_template_... functions, you
		/// should call FLAC__Metadata_Object_Seektable_Template_Sort() when
		/// finished to make the seek table legal
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="num">The number of placeholder points to append</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Append_Placeholders(Flac__StreamMetadata @object, uint32_t num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			if (num > 0)
			{
				// WATCHOUT: We rely on the fact that growing the array adds PLACEHOLDERS at the end
				return Flac__Metadata_Object_SeekTable_Resize_Points(@object, ((Flac__StreamMetadata_SeekTable)@object.Data).Num_Points + num);
			}
			else
				return true;
		}



		/********************************************************************/
		/// <summary>
		/// Append a specific seek point template to the end of a seek table.
		///
		/// As with the other ..._seektable_template_... functions, you
		/// should call FLAC__Metadata_Object_Seektable_Template_Sort() when
		/// finished to make the seek table legal
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="sample_Number">The sample number of the seek point template</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Append_Point(Flac__StreamMetadata @object, uint64_t sample_Number)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

			if (!Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points + 1))
				return false;

			metaSeekTable.Points[metaSeekTable.Num_Points - 1].Sample_Number = sample_Number;
			metaSeekTable.Points[metaSeekTable.Num_Points - 1].Stream_Offset = 0;
			metaSeekTable.Points[metaSeekTable.Num_Points - 1].Frame_Samples = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Append specific seek point templates to the end of a seek table.
		///
		/// As with the other ..._seektable_template_... functions, you
		/// should call FLAC__Metadata_Object_Seektable_Template_Sort() when
		/// finished to make the seek table legal
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="sample_Numbers">An array of sample numbers for the seek points</param>
		/// <param name="num">The number of seek point templates to append</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Append_Points(Flac__StreamMetadata @object, Flac__uint64[] sample_Numbers, uint32_t num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			if (num > 0)
			{
				Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

				uint32_t i = metaSeekTable.Num_Points;

				if (!Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points + num))
					return false;

				for (uint32_t j = 0; j < num; i++, j++)
				{
					metaSeekTable.Points[i].Sample_Number = sample_Numbers[j];
					metaSeekTable.Points[i].Stream_Offset = 0;
					metaSeekTable.Points[i].Frame_Samples = 0;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Append a set of evenly-spaced seek point templates to the end of
		/// a seek table.
		///
		/// As with the other ..._seektable_template_... functions, you
		/// should call FLAC__Metadata_Object_Seektable_Template_Sort() when
		/// finished to make the seek table legal
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="num">The number of placeholder points to append</param>
		/// <param name="total_Samples">The total number of samples to be encoded; the seekpoints will be spaced approximately total_samples / num samples apart</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points(Flac__StreamMetadata @object, uint32_t num, Flac__uint64 total_Samples)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);
			Debug.Assert(total_Samples > 0);

			if ((num > 0) && (total_Samples > 0))
			{
				Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

				uint32_t i = metaSeekTable.Num_Points;

				if (!Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points + num))
					return false;

				for (uint32_t j = 0; j < num; i++, j++)
				{
					metaSeekTable.Points[i].Sample_Number = total_Samples * j / num;
					metaSeekTable.Points[i].Stream_Offset = 0;
					metaSeekTable.Points[i].Frame_Samples = 0;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Append a set of evenly-spaced seek point templates to the end of
		/// a seek table.
		///
		/// As with the other ..._seektable_template_... functions, you
		/// should call FLAC__Metadata_Object_Seektable_Template_Sort() when
		/// finished to make the seek table legal
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="samples">The number of samples apart to space the placeholder points. The first point will be at sample 0, the second at sample samples, then 2*samples, and so on. As long as samples and total_Samples are greater than 0, there will always be at least one seekpoint at sample 0</param>
		/// <param name="total_Samples">The total number of samples to be encoded; the seekpoints will be spaced samples samples apart</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points_By_Samples(Flac__StreamMetadata @object, uint32_t samples, Flac__uint64 total_Samples)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);
			Debug.Assert(samples > 0);
			Debug.Assert(total_Samples > 0);

			if ((samples > 0) && (total_Samples > 0))
			{
				Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

				Flac__uint64 num = 1 + total_Samples / samples;	// 1+ for the first sample at 0

				// Now account for the fact that we don't place a seekpoint at "total_Samples" since samples are number from 0:
				if ((total_Samples % samples) == 0)
					num--;

				// Put a strict upper bound on the number of allowed seek points
				if (num > 32768)
				{
					// Set the bound and recalculate samples accordingly
					num = 32768;
					samples = (uint32_t)(total_Samples / num);
				}

				uint32_t i = metaSeekTable.Num_Points;

				if (!Flac__Metadata_Object_SeekTable_Resize_Points(@object, metaSeekTable.Num_Points + (uint32_t)num))
					return false;

				Flac__uint64 sample = 0;

				for (uint32_t j = 0; j < num; i++, j++, sample += samples)
				{
					metaSeekTable.Points[i].Sample_Number = sample;
					metaSeekTable.Points[i].Stream_Offset = 0;
					metaSeekTable.Points[i].Frame_Samples = 0;
				}
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sort a seek table's seek points according to the format
		/// specification, removing duplicates
		/// <param name="object">A pointer to an existing SEEKTABLE object</param>
		/// <param name="compact">If false, behaves like FLAC__Format_Seektable_Sort(). If true, duplicates are deleted and the seek table is shrunk appropriately; the number of placeholder points present in the seek table will be the same after the call as before</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_SeekTable_Template_Sort(Flac__StreamMetadata @object, Flac__bool compact)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			uint32_t unique = Format.Flac__Format_SeekTable_Sort((Flac__StreamMetadata_SeekTable)@object.Data);

			return !compact || Flac__Metadata_Object_SeekTable_Resize_Points(@object, unique);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the vendor string in a VORBIS_COMMENT block.
		///
		/// For convenience, a trailing NUL is added to the entry if it
		/// doesn't have one already.
		///
		/// If copy is true, a copy of the entry is stored; otherwise, the
		/// object takes ownership of the entry.Entry pointer.
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="entry">The entry to set the vendor string to</param>
		/// <param name="copy">See above</param>
		/// <returns>False if memory allocation fails or entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Set_Vendor_String(Flac__StreamMetadata @object, Flac__StreamMetadata_VorbisComment_Entry entry, Flac__bool copy)
		{
			if (!Format.Flac__Format_VorbisComment_Entry_Value_Is_Legal(entry.Entry, entry.Length))
				return false;

			return VorbisComment_Set_Entry(@object, ((Flac__StreamMetadata_VorbisComment)@object.Data).Vendor_String, entry, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Resize the comment array.
		///
		/// If the size shrinks, elements will truncated; if it grows, new
		/// empty fields will be added to the end
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="new_Num_Comments">The desired length of the array; may be 0</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Resize_Comments(Flac__StreamMetadata @object, uint32_t new_Num_Comments)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

			if (metaVorbisComment.Comments == null)
			{
				Debug.Assert(metaVorbisComment.Num_Comments == 0);

				if (new_Num_Comments == 0)
					return true;
				else
				{
					metaVorbisComment.Comments = VorbisComment_Entry_Array_New(new_Num_Comments);
					if (metaVorbisComment.Comments == null)
						return false;
				}
			}
			else
			{
				size_t old_Size = metaVorbisComment.Num_Comments;
				size_t new_Size = new_Num_Comments;

				Debug.Assert(metaVorbisComment.Num_Comments > 0);

				// If shrinking, free the truncated entries
				if (new_Num_Comments < metaVorbisComment.Num_Comments)
				{
					for (uint32_t i = new_Num_Comments; i < metaVorbisComment.Num_Comments; i++)
					{
						if (metaVorbisComment.Comments[i] != null)
							metaVorbisComment.Comments[i] = null;
					}
				}

				if (new_Size == 0)
					metaVorbisComment.Comments = null;
				else
					Array.Resize(ref metaVorbisComment.Comments, (int)new_Size);

				// If growing, zero all the length/pointers of new elements
				if (new_Size > old_Size)
				{
					for (uint32_t i = metaVorbisComment.Num_Comments; i < new_Size; i++)
						metaVorbisComment.Comments[i] = new Flac__StreamMetadata_VorbisComment_Entry();
				}
			}

			metaVorbisComment.Num_Comments = new_Num_Comments;

			VorbisComment_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sets a comment in a VORBIS_COMMENT block.
		///
		/// For convenience, a trailing NUL is added to the entry if it
		/// doesn't have one already.
		///
		/// If copy is true, a copy of the entry is stored; otherwise, the
		/// object takes ownership of the entry.Entry pointer
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="comment_Num">Index into comment array to set</param>
		/// <param name="entry">The comment to insert</param>
		/// <param name="copy">See above</param>
		/// <returns>False if memory allocation fails or entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Set_Comment(Flac__StreamMetadata @object, uint32_t comment_Num, Flac__StreamMetadata_VorbisComment_Entry entry, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			if (!Format.Flac__Format_VorbisComment_Entry_Is_Legal(entry.Entry, entry.Length))
				return false;

			return VorbisComment_Set_Entry(@object, ((Flac__StreamMetadata_VorbisComment)@object.Data).Comments[comment_Num], entry, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Insert a comment in a VORBIS_COMMENT block at the given index.
		///
		/// For convenience, a trailing NUL is added to the entry if it
		/// doesn't have one already.
		///
		/// If copy is true, a copy of the entry is stored; otherwise, the
		/// object takes ownership of the entry.Entry pointer
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="comment_Num">The index at which to insert the comment. The comments at and after comment_Num move right one position. To append a comment to the end, set comment_Num to object->Data.num_Comments</param>
		/// <param name="entry">The comment to insert</param>
		/// <param name="copy">See above</param>
		/// <returns>False if memory allocation fails or entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Insert_Comment(Flac__StreamMetadata @object, uint32_t comment_Num, Flac__StreamMetadata_VorbisComment_Entry entry, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)@object.Data;
			Debug.Assert(comment_Num <= vc.Num_Comments);

			if (!Format.Flac__Format_VorbisComment_Entry_Is_Legal(entry.Entry, entry.Length))
				return false;

			if (!Flac__Metadata_Object_VorbisComment_Resize_Comments(@object, vc.Num_Comments + 1))
				return false;

			// Move all comments >= comment_Num forward one space
			Array.Copy(vc.Comments, comment_Num, vc.Comments, comment_Num + 1, vc.Num_Comments - 1 - comment_Num);
			vc.Comments[comment_Num] = new Flac__StreamMetadata_VorbisComment_Entry();

			return Flac__Metadata_Object_VorbisComment_Set_Comment(@object, comment_Num, entry, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Replaces comments in a VORBIS_COMMENT block with a new one.
		///
		/// For convenience, a trailing NUL is added to the entry if it
		/// doesn't have one already.
		///
		/// Depending on the value of all, either all or just the first
		/// comment whose field name(s) match the given entry's name will be
		/// replaced by the given entry. If no comments match, entry will
		/// simply be appended.
		///
		/// If copy is true, a copy of the entry is stored; otherwise, the
		/// object takes ownership of the entry.Entry pointer
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="entry">The comment to insert</param>
		/// <param name="all">If true, all comments whose field name matches entry's field name will be removed, and entry will be inserted at the position of the first matching comment. If false, only the first comment whose field name matches entry's field name will be replaced with entry</param>
		/// <param name="copy">See above</param>
		/// <returns>False if memory allocation fails or entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Replace_Comment(Flac__StreamMetadata @object, Flac__StreamMetadata_VorbisComment_Entry entry, Flac__bool all, Flac__bool copy)
		{
			Debug.Assert((entry.Entry != null) && (entry.Length > 0));

			if (!Format.Flac__Format_VorbisComment_Entry_Is_Legal(entry.Entry, entry.Length))
				return false;

			int eq = Array.IndexOf(entry.Entry, (byte)'=');
			if (eq == -1)
				return false;

			size_t field_Name_Length = (size_t)eq;
			string field_Name = Encoding.ASCII.GetString(entry.Entry, 0, (int)field_Name_Length);

			int i = VorbisComment_Find_Entry_From(@object, 0, field_Name);
			if (i >= 0)
			{
				uint32_t indx = (uint32_t)i;

				if (!Flac__Metadata_Object_VorbisComment_Set_Comment(@object, indx, entry, copy))
					return false;

				Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

				entry = metaVorbisComment.Comments[indx];
				indx++;	// Skip over replaced comment

				if (all && (indx < metaVorbisComment.Num_Comments))
				{
					i = VorbisComment_Find_Entry_From(@object, indx, field_Name);

					while (i >= 0)
					{
						indx = (uint32_t)i;

						if (!Flac__Metadata_Object_VorbisComment_Delete_Comment(@object, indx))
							return false;

						if (indx < metaVorbisComment.Num_Comments)
							i = VorbisComment_Find_Entry_From(@object, indx, field_Name);
						else
							i = -1;
					}
				}

				return true;
			}
			else
				return Flac__Metadata_Object_VorbisComment_Append_Comment(@object, entry, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Delete a comment in a VORBIS_COMMENT block at the given index
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="comment_Num">The index of the comment to delete</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Delete_Comment(Flac__StreamMetadata @object, uint32_t comment_Num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)@object.Data;
			Debug.Assert(comment_Num <= vc.Num_Comments);

			// Move all comments > comment_Num backward one space
			Array.Copy(vc.Comments, comment_Num + 1, vc.Comments, comment_Num, vc.Num_Comments - comment_Num - 1);

			return Flac__Metadata_Object_VorbisComment_Resize_Comments(@object, vc.Num_Comments - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Appends a comment to a VORBIS_COMMENT block.
		///
		/// For convenience, a trailing NUL is added to the entry if it
		/// doesn't have one already.
		///
		/// If copy is true, a copy of the entry is stored; otherwise, the
		/// object takes ownership of the entry.Entry pointer
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="entry">The comment to insert</param>
		/// <param name="copy">See above</param>
		/// <returns>False if memory allocation fails or entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Append_Comment(Flac__StreamMetadata @object, Flac__StreamMetadata_VorbisComment_Entry entry, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			return Flac__Metadata_Object_VorbisComment_Insert_Comment(@object, ((Flac__StreamMetadata_VorbisComment)@object.Data).Num_Comments, entry, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Creates a Vorbis comment entry from NUL-terminated name and value
		/// strings
		/// <param name="entry">A Vorbis comment entry is returned here</param>
		/// <param name="field_Name">The field name</param>
		/// <param name="field_Value">The field value</param>
		/// <returns>False if new fails, or if field_Name or field_Value does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Entry_From_Name_Value_Pair(out Flac__StreamMetadata_VorbisComment_Entry entry, string field_Name, string field_Value)
		{
			Debug.Assert(field_Name != null);
			Debug.Assert(field_Value != null);

			entry = null;

			if (!Format.Flac__Format_VorbisComment_Entry_Name_Is_Legal(field_Name))
				return false;

			byte[] encodedFieldName = Encoding.ASCII.GetBytes(field_Name);
			encodedFieldName = Alloc.Safe_Realloc(encodedFieldName, (size_t)encodedFieldName.Length + 1);

			byte[] encodedFieldValue = Encoding.UTF8.GetBytes(field_Value);
			encodedFieldValue = Alloc.Safe_Realloc(encodedFieldValue, (size_t)encodedFieldValue.Length + 1);

			if (!Format.Flac__Format_VorbisComment_Entry_Value_Is_Legal(encodedFieldValue, uint32_t.MaxValue))
				return false;

			Flac__StreamMetadata_VorbisComment_Entry newEntry = new Flac__StreamMetadata_VorbisComment_Entry();

			size_t nn = (size_t)encodedFieldName.Length - 1;
			size_t nv = (size_t)encodedFieldValue.Length - 1;
			newEntry.Length = nn + 1 + nv;
			newEntry.Entry = Alloc.Safe_MAlloc_Add_4Op<Flac__byte>(nn, 1, nv, 1);
			if (newEntry.Entry == null)
				return false;

			Array.Copy(encodedFieldName, 0, newEntry.Entry, 0, nn);
			newEntry.Entry[nn] = (byte)'=';
			Array.Copy(encodedFieldValue, 0, newEntry.Entry, nn + 1, nv);
			newEntry.Entry[newEntry.Length] = 0x00;

			entry = newEntry;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Splits a Vorbis comment entry into name and value strings
		/// <param name="entry">An existing Vorbis comment entry</param>
		/// <param name="field_Name">Where the returned field name will be stored</param>
		/// <param name="field_Value">Where the returned field value will be stored</param>
		/// <returns>False if memory allocation fails or if entry does not comply with the Vorbis comment specification, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Entry_To_Name_Value_Pair(Flac__StreamMetadata_VorbisComment_Entry entry, out string field_Name, out string field_Value)
		{
			Debug.Assert((entry.Entry != null) && (entry.Length > 0));

			field_Name = null;
			field_Value = null;

			if (!Format.Flac__Format_VorbisComment_Entry_Is_Legal(entry.Entry, entry.Length))
				return false;

			size_t eq = (size_t)Array.IndexOf(entry.Entry, (byte)'=');
			size_t nn = eq;
			size_t nv = entry.Length - nn - 1;	// -1 for the '='

			field_Name = Encoding.ASCII.GetString(entry.Entry, 0, (int)nn);
			field_Value = Encoding.UTF8.GetString(entry.Entry, (int)nn + 1, (int)nv);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the given Vorbis comment entry's field name matches the
		/// given field name
		/// <param name="entry">An existing Vorbis comment entry</param>
		/// <param name="field_Name">The field name to check</param>
		/// <returns>True if the field names match, else false</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_VorbisComment_Entry_Matches(Flac__StreamMetadata_VorbisComment_Entry entry, string field_Name)
		{
			Debug.Assert((entry.Entry != null) && (entry.Length > 0));

			int eq = Array.IndexOf(entry.Entry, (byte)'=');
			if (eq == -1)
				return false;

			return (eq == field_Name.Length) && Encoding.ASCII.GetString(entry.Entry, 0, eq).Equals(field_Name, StringComparison.InvariantCultureIgnoreCase);
		}



		/********************************************************************/
		/// <summary>
		/// Find a Vorbis comment with the given field name.
		///
		/// The search begins at entry number offset; use an offset of 0 to
		/// search from the beginning of the comment array
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="offset">The offset into the comment array from where to start the search</param>
		/// <param name="field_Name">The field name of the comment to find</param>
		/// <returns>The offset in the comment array of the first comment whose field name matches field_name, or -1 if no match was found</returns>
		/// </summary>
		/********************************************************************/
		public static int Flac__Metadata_Object_VorbisComment_Find_Entry_From(Flac__StreamMetadata @object, uint32_t offset, string field_Name)
		{
			Debug.Assert(field_Name != null);

			return VorbisComment_Find_Entry_From(@object, offset, field_Name);
		}



		/********************************************************************/
		/// <summary>
		/// Remove first Vorbis comment matching the given field name.
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="field_Name">The field name of comment to delete</param>
		/// <returns>-1 for memory allocation error, 0 for no matching entries, 1 for one matching entry deleted</returns>
		/// </summary>
		/********************************************************************/
		public static int Flac__Metadata_Object_VorbisComment_Remove_Entry_Matching(Flac__StreamMetadata @object, string field_Name)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

			for (uint32_t i = 0; i < metaVorbisComment.Num_Comments; i++)
			{
				if (Flac__Metadata_Object_VorbisComment_Entry_Matches(metaVorbisComment.Comments[i], field_Name))
				{
					if (!Flac__Metadata_Object_VorbisComment_Delete_Comment(@object, i))
						return -1;
					else
						return 1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Remove all Vorbis comments matching the given field name.
		/// <param name="object">A pointer to an existing VORBIS_COMMENT object</param>
		/// <param name="field_Name">The field name of comments to delete</param>
		/// <returns>-1 for memory allocation error, 0 for no matching entries, else the number of matching entries deleted</returns>
		/// </summary>
		/********************************************************************/
		public static int Flac__Metadata_Object_VorbisComment_Remove_Entries_Matching(Flac__StreamMetadata @object, string field_Name)
		{
			Flac__bool ok = true;
			uint32_t matching = 0;

			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

			// Must delete from end to start otherwise it will interfere with our iteration
			for (int i = (int)metaVorbisComment.Num_Comments - 1; ok && (i >= 0); i--)
			{
				if (Flac__Metadata_Object_VorbisComment_Entry_Matches(metaVorbisComment.Comments[i], field_Name))
				{
					matching++;
					ok &= Flac__Metadata_Object_VorbisComment_Delete_Comment(@object, (uint32_t)i);
				}
			}

			return ok ? (int)matching : -1;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new CUESHEET track instance.
		///
		/// The object will be "empty"; i.e. values and data pointers will
		/// be 0
		/// <returns>NULL if there was an error allocating memory, else the new instance</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__StreamMetadata_CueSheet_Track Flac__Metadata_Object_CueSheet_Track_New()
		{
			return new Flac__StreamMetadata_CueSheet_Track();
		}



		/********************************************************************/
		/// <summary>
		/// Create a copy of an existing CUESHEET track object.
		///
		/// The copy is a "deep" copy, i.e. dynamically allocated data within
		/// the object is also copied. The caller takes ownership of the new
		/// object and is responsible for freeing it with
		/// Flac__Metadata_Object_CueSheet_Track_Delete()
		/// <param name="object">Pointer to object to copy</param>
		/// <returns>NULL if there was an error allocating memory, else the new instance</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__StreamMetadata_CueSheet_Track Flac__Metadata_Object_CueSheet_Track_Clone(Flac__StreamMetadata_CueSheet_Track @object)
		{
			Debug.Assert(@object != null);

			Flac__StreamMetadata_CueSheet_Track to = Flac__Metadata_Object_CueSheet_Track_New();
			if (to != null)
			{
				if (!Copy_Track(to, @object))
				{
					Flac__Metadata_Object_CueSheet_Track_Delete(to);
					return null;
				}
			}

			return to;
		}



		/********************************************************************/
		/// <summary>
		/// Delete a CUESHEET track object
		/// <param name="object">A pointer to an existing CUESHEET track object</param>
		/// </summary>
		/********************************************************************/
		public static void Flac__Metadata_Object_CueSheet_Track_Delete(Flac__StreamMetadata_CueSheet_Track @object)
		{
			Flac__Metadata_Object_CueSheet_Track_Delete_Data(@object);
		}



		/********************************************************************/
		/// <summary>
		/// Resize a track's index point array.
		///
		/// If the size shrinks, elements will truncated; if it grows, new
		/// blank indices will be added to the end
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index of the track to modify. NOTE: this is not necessarily the same as the track's number field</param>
		/// <param name="new_Num_Indices">The desired length of the array; may be 0</param>
		/// <returns>False if memory allocation error, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Track_Resize_Indices(Flac__StreamMetadata @object, uint32_t track_Num, uint32_t new_Num_Indices)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num < metaCueSheet.Num_Tracks);

			Flac__StreamMetadata_CueSheet_Track track = metaCueSheet.Tracks[track_Num];

			if (track.Indices == null)
			{
				Debug.Assert(track.Num_Indices == 0);

				if (new_Num_Indices == 0)
					return true;
				else
				{
					track.Indices = CueSheet_Track_Index_Array_New(new_Num_Indices);
					if (track.Indices == null)
						return false;
				}
			}
			else
			{
				size_t old_Size = track.Num_Indices;
				size_t new_Size = new_Num_Indices;

				Debug.Assert(track.Num_Indices > 0);

				if (new_Size == 0)
					track.Indices = null;
				else
				{
					track.Indices = Alloc.Safe_Realloc(track.Indices, new_Size);
					if (track.Indices == null)
						return false;
				}

				// If growing, zero all the length/pointers of new elements
				if (new_Size > old_Size)
				{
					for (uint32_t i = track.Num_Indices; i < new_Size; i++)
						track.Indices[i] = new Flac__StreamMetadata_CueSheet_Index();
				}
			}

			track.Num_Indices = (Flac__byte)new_Num_Indices;

			CueSheet_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Insert an index point in a CUESHEET track at the given index
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index of the track to modify. NOTE: this is not necessarily the same as the track's number field</param>
		/// <param name="index_Num">The index into the track's index array at which to insert the index point. NOTE: this is not necessarily the same as the index point's number field. The indices at and after index_Num move right one position. To append an index point to the end, set index_Num to object.Data.Tracks[track_Num].Num_Indices</param>
		/// <param name="indx">The index point to insert</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Track_Insert_Index(Flac__StreamMetadata @object, uint32_t track_Num, uint32_t index_Num, Flac__StreamMetadata_CueSheet_Index indx)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num <= cs.Num_Tracks);
			Debug.Assert(index_Num <= cs.Tracks[track_Num].Num_Indices);

			Flac__StreamMetadata_CueSheet_Track track = cs.Tracks[track_Num];

			if (!Flac__Metadata_Object_CueSheet_Track_Resize_Indices(@object, track_Num, (uint32_t)track.Num_Indices + 1))
				return false;

			// Move all indices >= index_Num forward one space
			Array.Copy(track.Indices, index_Num, track.Indices, index_Num + 1, track.Num_Indices - 1 - index_Num);
			track.Indices[index_Num] = indx;

			CueSheet_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Insert a blank index point in a CUESHEET track at the given
		/// index.
		///
		/// A blank index point is one in which all field values are zero
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index of the track to modify. NOTE: this is not necessarily the same as the track's number field</param>
		/// <param name="index_Num">The index into the track's index array at which to insert the index point. NOTE: this is not necessarily the same as the index point's number field. The indices at and after index_Num move right one position. To append an index point to the end, set index_Num to object.Data.Tracks[track_Num].Num_Indices</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Track_Insert_Blank_Index(Flac__StreamMetadata @object, uint32_t track_Num, uint32_t index_Num)
		{
			Flac__StreamMetadata_CueSheet_Index indx = new Flac__StreamMetadata_CueSheet_Index();

			return Flac__Metadata_Object_CueSheet_Track_Insert_Index(@object, track_Num, index_Num, indx);
		}



		/********************************************************************/
		/// <summary>
		/// Delete an index point in a CUESHEET track at the given index
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index into the track array of the track to modify. NOTE: this is not necessarily the same as the track's number field</param>
		/// <param name="index_Num">The index into the track's index array of the index to delete. NOTE: this is not necessarily the same as the index's number field</param>
		/// <returns>False if memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Track_Delete_Index(Flac__StreamMetadata @object, uint32_t track_Num, uint32_t index_Num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num <= cs.Num_Tracks);
			Debug.Assert(index_Num <= cs.Tracks[track_Num].Num_Indices);

			Flac__StreamMetadata_CueSheet_Track track = cs.Tracks[track_Num];

			// Move all indices > index_Num backward one space
			Array.Copy(track.Indices, index_Num + 1, track.Indices, index_Num, track.Num_Indices - index_Num - 1);

			return Flac__Metadata_Object_CueSheet_Track_Resize_Indices(@object, track_Num, (uint32_t)track.Num_Indices - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Resize the track array.
		///
		/// If the size shrinks, elements will truncated; if it grows, new
		/// blank tracks will be added to the end
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="new_Num_Tracks">The desired length of the array; may be 0</param>
		/// <returns>False if memory allocation error, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Resize_Tracks(Flac__StreamMetadata @object, uint32_t new_Num_Tracks)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)@object.Data;

			if (metaCueSheet.Tracks == null)
			{
				Debug.Assert(metaCueSheet.Num_Tracks == 0);

				if (new_Num_Tracks == 0)
					return true;
				else
				{
					metaCueSheet.Tracks = CueSheet_Track_Array_New(new_Num_Tracks);
					if (metaCueSheet.Tracks == null)
						return false;
				}
			}
			else
			{
				size_t old_Size = metaCueSheet.Num_Tracks;
				size_t new_Size = new_Num_Tracks;

				Debug.Assert(metaCueSheet.Num_Tracks > 0);

				// If shrinking, free the truncated entries
				if (new_Num_Tracks < metaCueSheet.Num_Tracks)
				{
					for (uint32_t i = new_Num_Tracks; i < metaCueSheet.Num_Tracks; i++)
						metaCueSheet.Tracks[i].Indices = null;
				}

				if (new_Size == 0)
					metaCueSheet.Tracks = null;
				else
				{
					metaCueSheet.Tracks = Alloc.Safe_Realloc(metaCueSheet.Tracks, new_Size);
					if (metaCueSheet.Tracks == null)
						return false;
				}

				// If growing, zero all the length/pointers of new elements
				if (new_Size > old_Size)
				{
					for (uint32_t i = metaCueSheet.Num_Tracks; i < new_Size; i++)
						metaCueSheet.Tracks[i] = new Flac__StreamMetadata_CueSheet_Track();
				}
			}

			metaCueSheet.Num_Tracks = new_Num_Tracks;

			CueSheet_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sets a track in a CUESHEET block.
		///
		/// If copy is true, a copy of the track is stored; otherwise, the
		/// object takes ownership of the track pointer
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">Index into track array to set. NOTE: this is not necessarily the same as the track's number field</param>
		/// <param name="track">The track to set the track to</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Set_Track(Flac__StreamMetadata @object, uint32_t track_Num, Flac__StreamMetadata_CueSheet_Track track, Flac__bool copy)
		{
			Debug.Assert(@object != null);

			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num <= cs.Num_Tracks);

			return CueSheet_Set_Track(@object, cs.Tracks[track_Num], track, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Insert a track in a CUESHEET block at the given index.
		///
		/// If copy is true, a copy of the track is stored; otherwise, the
		/// object takes ownership of the track pointer
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index at which to insert the track. NOTE: this is not necessarily the same as the track's number field. The tracks at and after track_Num move right one position. To append a track to the end, set track_Num to object.Data.Num_Tracks</param>
		/// <param name="track">The track to insert</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Insert_Track(Flac__StreamMetadata @object, uint32_t track_Num, Flac__StreamMetadata_CueSheet_Track track, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num <= cs.Num_Tracks);

			if (!Flac__Metadata_Object_CueSheet_Resize_Tracks(@object, cs.Num_Tracks + 1))
				return false;

			// Move all tracks >= track_Num forward one space
			Array.Copy(cs.Tracks, track_Num, cs.Tracks, track_Num + 1, cs.Num_Tracks - 1 - track_Num);
			cs.Tracks[track_Num] = new Flac__StreamMetadata_CueSheet_Track();

			return Flac__Metadata_Object_CueSheet_Set_Track(@object, track_Num, track, copy);
		}



		/********************************************************************/
		/// <summary>
		/// Insert a blank track in a CUESHEET block at the given index.
		///
		/// A blank track is one in which all field values are zero
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index at which to insert the track. NOTE: this is not necessarily the same as the track's number field. The tracks at and after track_Num move right one position. To append a track to the end, set track_Num to object.Data.Num_Tracks</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Insert_Blank_Track(Flac__StreamMetadata @object, uint32_t track_Num)
		{
			Flac__StreamMetadata_CueSheet_Track track = new Flac__StreamMetadata_CueSheet_Track();

			return Flac__Metadata_Object_CueSheet_Insert_Track(@object, track_Num, track, false);
		}



		/********************************************************************/
		/// <summary>
		/// Delete a track in a CUESHEET block at the given index
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="track_Num">The index into the track array of the track to delete. NOTE: this is not necessarily the same as the track's number field</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Delete_Track(Flac__StreamMetadata @object, uint32_t track_Num)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)@object.Data;
			Debug.Assert(track_Num <= cs.Num_Tracks);

			// Move all tracks > track_Num backward one space
			Array.Copy(cs.Tracks, track_Num + 1, cs.Tracks, track_Num, cs.Num_Tracks - track_Num - 1);

			return Flac__Metadata_Object_CueSheet_Resize_Tracks(@object, cs.Num_Tracks - 1);
		}



		/********************************************************************/
		/// <summary>
		/// Check a cue sheet to see if it conforms to the FLAC specification.
		/// See the format specification for limits on the contents of the
		/// cue sheet
		/// <param name="object">A pointer to an existing CUESHEET object</param>
		/// <param name="check_Cd_Da_Subset">If true, check CUESHEET against more stringent requirements for a CD-DA (audio) disc</param>
		/// <param name="violation">If there is a violation, a string explanation of the violation will be returned here</param>
		/// <returns>False if cue sheet is illegal, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_CueSheet_Is_Legal(Flac__StreamMetadata @object, Flac__bool check_Cd_Da_Subset, out string violation)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			return Format.Flac__Format_CueSheet_Is_Legal((Flac__StreamMetadata_CueSheet)@object.Data, check_Cd_Da_Subset, out violation);
		}



		/********************************************************************/
		/// <summary>
		/// Sets the MIME type of a PICTURE block.
		///
		/// If copy is true, a copy of the string is stored; otherwise, the
		/// object takes ownership of the pointer. The existing string will
		/// be freed if this function is successful, otherwise the original
		/// string will remain if copy is true and allocation fails
		/// <param name="object">A pointer to an existing PICTURE object</param>
		/// <param name="mime_Type">The mime type string. The string must be ASCII characters 0x20-0x7e</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_Picture_Set_Mime_Type(Flac__StreamMetadata @object, string mime_Type, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Picture);
			Debug.Assert(mime_Type != null);

			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)@object.Data;

			Flac__byte[] old = metaPicture.Mime_Type;
			size_t old_Length = old != null ? (size_t)old.Length - 1 : 0;

			Flac__byte[] @new = Encoding.ASCII.GetBytes(mime_Type);
			size_t new_Length = (size_t)@new.Length;

			// Here we should do the copy, but since we always creates a new byte array, this argument is not used
			metaPicture.Mime_Type = new byte[new_Length + 1];
			Array.Copy(@new, metaPicture.Mime_Type, new_Length);

			@object.Length -= old_Length;
			@object.Length += new_Length;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the description of a PICTURE block.
		///
		/// If copy is true, a copy of the string is stored; otherwise, the
		/// object takes ownership of the pointer. The existing string will
		/// be freed if this function is successful, otherwise the original
		/// string will remain if copy is true and allocation fails
		/// <param name="object">A pointer to an existing PICTURE object</param>
		/// <param name="description">The description string. The string must be valid UTF-8</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_Picture_Set_Description(Flac__StreamMetadata @object, string description, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Picture);
			Debug.Assert(description != null);

			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)@object.Data;

			Flac__byte[] old = metaPicture.Description;
			size_t old_Length = old != null ? (size_t)old.Length - 1 : 0;

			Flac__byte[] @new = Encoding.UTF8.GetBytes(description);
			size_t new_Length = (size_t)@new.Length;

			// Here we should do the copy, but since we always creates a new byte array, this argument is not used
			metaPicture.Description = new byte[new_Length + 1];
			Array.Copy(@new, metaPicture.Description, new_Length);

			@object.Length -= old_Length;
			@object.Length += new_Length;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the description of a PICTURE block.
		///
		/// If copy is true, a copy of the string is stored; otherwise, the
		/// object takes ownership of the pointer. The existing string will
		/// be freed if this function is successful, otherwise the original
		/// string will remain if copy is true and allocation fails
		/// <param name="object">A pointer to an existing PICTURE object</param>
		/// <param name="data">A pointer to the data to set</param>
		/// <param name="length">The length of data in bytes</param>
		/// <param name="copy">See above</param>
		/// <returns>False if copy is true and memory allocation fails, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_Picture_Set_Data(Flac__StreamMetadata @object, Flac__byte[] data, uint32_t length, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Picture);
			Debug.Assert(((data != null) && (length > 0)) || ((data == null) && (length == 0) && (copy == false)));

			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)@object.Data;

			// Do the copy first so that if we fail we leave the object untouched
			if (copy)
			{
				if (!Copy_Bytes(ref metaPicture.Data, data, length))
					return false;
			}
			else
				metaPicture.Data = data;

			@object.Length -= metaPicture.Data_Length;
			metaPicture.Data_Length = length;
			@object.Length += length;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Check a PICTURE block to see if it conforms to the FLAC
		/// specification. See the format specification for limits on the
		/// contents of the PICTURE block
		/// <param name="object">A pointer to an existing PICTURE object to be checked</param>
		/// <param name="violation">If there is a violation, a string explanation of the violation will be returned here</param>
		/// <returns>False if PICTURE block is illegal, else true</returns>
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Metadata_Object_Picture_Is_Legal(Flac__StreamMetadata @object, out string violation)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Picture);

			return Format.Flac__Format_Picture_Is_Legal((Flac__StreamMetadata_Picture)@object.Data, out violation);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Copy bytes
		/// </summary>
		/********************************************************************/
		private static Flac__bool Copy_Bytes(ref Flac__byte[] to, Flac__byte[] from, uint32_t bytes)
		{
			if ((bytes > 0) && (from != null))
			{
				Flac__byte[] x = new Flac__byte[bytes];
				Array.Copy(from, x, Math.Min(from.Length, bytes));
				to = x;
			}
			else
				to = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Copy bytes
		/// </summary>
		/********************************************************************/
		private static Flac__bool Ensure_Null_Terminated(ref Flac__byte[] entry, uint32_t length)
		{
			Flac__byte[] x = Alloc.Safe_Realloc_Add_2Op(entry, length, 1);
			if (x != null)
			{
				x[length] = 0x00;
				entry = x;

				return true;
			}
			else
				return false;
		}



		/********************************************************************/
		/// <summary>
		/// Copies the string 'from' to 'to', leaving 'to' unchanged if new
		/// fails, freeing the original 'to' if it succeeds and the original
		/// 'to' was not null
		/// </summary>
		/********************************************************************/
		private static Flac__bool Copy_CString(Encoding encoder, ref Flac__byte[] to, string from)
		{
			Flac__byte[] encodedBytes = encoder.GetBytes(from);
			Flac__byte[] copy = new Flac__byte[encodedBytes.Length + 1];
			Array.Copy(encodedBytes, copy, encodedBytes.Length);

			to = copy;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Copy_VCEntry(Flac__StreamMetadata_VorbisComment_Entry to, Flac__StreamMetadata_VorbisComment_Entry from)
		{
			to.Length = from.Length;

			if (from.Entry == null)
			{
				Debug.Assert(from.Length == 0);
				to.Entry = null;
			}
			else
			{
				Debug.Assert(from.Length > 0);

				Flac__byte[] x = Alloc.Safe_MAlloc_Add_2Op<Flac__byte>(from.Length, 1);
				if (x == null)
					return false;

				Array.Copy(from.Entry, x, from.Length);
				x[from.Length] = 0;

				to.Entry = x;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool Copy_Track(Flac__StreamMetadata_CueSheet_Track to, Flac__StreamMetadata_CueSheet_Track from)
		{
			to.Offset = from.Offset;
			to.Number = from.Number;
			to.Type = from.Type;
			to.Pre_Emphasis = from.Pre_Emphasis;
			to.Num_Indices = from.Num_Indices;

			Array.Copy(from.Isrc, to.Isrc, from.Isrc.Length);

			if (from.Indices == null)
				Debug.Assert(from.Num_Indices == 0);
			else
			{
				Debug.Assert(from.Num_Indices > 0);

				Flac__StreamMetadata_CueSheet_Index[] x = Alloc.Safe_MAlloc_Mul_2Op_P<Flac__StreamMetadata_CueSheet_Index>(from.Num_Indices, 1);
				if (x == null)
					return false;

				for (uint32_t i = 0; i < from.Num_Indices; i++)
				{
					x[i].Offset = from.Indices[i].Offset;
					x[i].Number = from.Indices[i].Number;
				}

				to.Indices = x;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void SeekTable_Calculate_Length(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.SeekTable);

			@object.Length = ((Flac__StreamMetadata_SeekTable)@object.Data).Num_Points * Constants.Flac__Stream_Metadata_SeekPoint_Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_SeekPoint[] SeekPoint_Array_New(uint32_t num_Points)
		{
			Debug.Assert(num_Points > 0);

			Flac__StreamMetadata_SeekPoint[] object_Array = Alloc.Safe_MAlloc_Mul_2Op_P<Flac__StreamMetadata_SeekPoint>(num_Points, 1);

			if (object_Array != null)
			{
				for (uint32_t i = 0; i < num_Points; i++)
				{
					object_Array[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
					object_Array[i].Stream_Offset = 0;
					object_Array[i].Frame_Samples = 0;
				}
			}

			return object_Array;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void VorbisComment_Calculate_Length(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

			@object.Length = Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8;
			@object.Length += metaVorbisComment.Vendor_String.Length;
			@object.Length += Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len / 8;

			for (uint32_t i = 0; i < metaVorbisComment.Num_Comments; i++)
			{
				@object.Length += Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8;
				@object.Length += metaVorbisComment.Comments[i].Length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_VorbisComment_Entry[] VorbisComment_Entry_Array_New(uint32_t num_Comments)
		{
			Debug.Assert(num_Comments > 0);

			return Alloc.Safe_CAlloc<Flac__StreamMetadata_VorbisComment_Entry>(num_Comments);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void VorbisComment_Entry_Array_Delete(Flac__StreamMetadata_VorbisComment_Entry[] object_Array, uint32_t num_Comments)
		{
			Debug.Assert((object_Array != null) && (num_Comments > 0));

			for (uint32_t i = 0; i < num_Comments; i++)
				object_Array[i].Entry = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_VorbisComment_Entry[] VorbisComment_Entry_Array_Copy(Flac__StreamMetadata_VorbisComment_Entry[] object_Array, uint32_t num_Comments)
		{
			Debug.Assert(object_Array != null);
			Debug.Assert(num_Comments > 1);

			Flac__StreamMetadata_VorbisComment_Entry[] return_Array = VorbisComment_Entry_Array_New(num_Comments);

			if (return_Array != null)
			{
				for (uint32_t i = 0; i < num_Comments; i++)
				{
					if (!Copy_VCEntry(return_Array[i], object_Array[i]))
					{
						VorbisComment_Entry_Array_Delete(return_Array, num_Comments);
						return null;
					}
				}
			}

			return return_Array;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool VorbisComment_Set_Entry(Flac__StreamMetadata @object, Flac__StreamMetadata_VorbisComment_Entry dest, Flac__StreamMetadata_VorbisComment_Entry src, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(dest != null);
			Debug.Assert(src != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);
			Debug.Assert(((src.Entry != null) && (src.Length > 0)) || ((src.Entry == null) && (src.Length == 0)));

			if (src.Entry != null)
			{
				if (copy)
				{
					// Do the copy first so that if we fail, we leave the dest object untouched
					if (!Copy_VCEntry(dest, src))
						return false;
				}
				else
				{
					// We have to make sure that the string we're taking over is null-terminated
					if (!Ensure_Null_Terminated(ref src.Entry, src.Length))
						return false;

					dest.Entry = src.Entry;
					dest.Length = src.Length;
				}
			}
			else
			{
				// The src is null
				dest.Entry = src.Entry;
				dest.Length = src.Length;
			}

			VorbisComment_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int VorbisComment_Find_Entry_From(Flac__StreamMetadata @object, uint32_t offset, string field_Name)
		{
			Debug.Assert(@object != null);
			Debug.Assert(@object.Type == Flac__MetadataType.Vorbis_Comment);
			Debug.Assert(field_Name != null);

			Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

			for (uint32_t i = offset; i < metaVorbisComment.Num_Comments; i++)
			{
				if (Flac__Metadata_Object_VorbisComment_Entry_Matches(metaVorbisComment.Comments[i], field_Name))
					return (int)i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void CueSheet_Calculate_Length(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);

			Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)@object.Data;

			@object.Length =
			(
				Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len
			) / 8;

			@object.Length += metaCueSheet.Num_Tracks *
			(
				Constants.Flac__Stream_Metadata_CueSheet_Track_Offset_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Number_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Type_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Reserved_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len
			) / 8;

			for (uint32_t i = 0; i < metaCueSheet.Num_Tracks; i++)
			{
				@object.Length += metaCueSheet.Tracks[i].Num_Indices *
				(
					Constants.Flac__Stream_Metadata_CueSheet_Index_Offset_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Index_Number_Len +
					Constants.Flac__Stream_Metadata_CueSheet_Index_Reserved_Len
				) / 8;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void CueSheet_Track_Array_Delete(Flac__StreamMetadata_CueSheet_Track[] object_Array, uint32_t num_Tracks)
		{
			Debug.Assert((object_Array != null) && (num_Tracks > 0));

			for (uint32_t i = 0; i < num_Tracks; i++)
			{
				if (object_Array[i].Indices != null)
				{
					Debug.Assert(object_Array[i].Num_Indices > 0);
					object_Array[i].Indices = null;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_CueSheet_Index[] CueSheet_Track_Index_Array_New(uint32_t num_Indices)
		{
			Debug.Assert(num_Indices > 0);

			return Alloc.Safe_CAlloc<Flac__StreamMetadata_CueSheet_Index>(num_Indices);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_CueSheet_Track[] CueSheet_Track_Array_New(uint32_t num_Tracks)
		{
			Debug.Assert(num_Tracks > 0);

			return Alloc.Safe_CAlloc<Flac__StreamMetadata_CueSheet_Track>(num_Tracks);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__StreamMetadata_CueSheet_Track[] CueSheet_Track_Array_Copy(Flac__StreamMetadata_CueSheet_Track[] object_Array, uint32_t num_Tracks)
		{
			Debug.Assert(object_Array != null);
			Debug.Assert(num_Tracks > 0);

			Flac__StreamMetadata_CueSheet_Track[] return_Array = CueSheet_Track_Array_New(num_Tracks);

			if (return_Array != null)
			{
				for (uint32_t i = 0; i < num_Tracks; i++)
				{
					if (!Copy_Track(return_Array[i], object_Array[i]))
					{
						CueSheet_Track_Array_Delete(return_Array, num_Tracks);
						return null;
					}
				}
			}

			return return_Array;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CueSheet_Set_Track(Flac__StreamMetadata @object, Flac__StreamMetadata_CueSheet_Track dest, Flac__StreamMetadata_CueSheet_Track src, Flac__bool copy)
		{
			Debug.Assert(@object != null);
			Debug.Assert(dest != null);
			Debug.Assert(src != null);
			Debug.Assert(@object.Type == Flac__MetadataType.CueSheet);
			Debug.Assert(((src.Indices != null) && (src.Num_Indices > 0)) || ((src.Indices == null) && (src.Num_Indices == 0)));

			if (copy)
			{
				// Do the copy first so that if we fail, we leave the dest object untouched
				if (!Copy_Track(dest, src))
					return false;
			}
			else
			{
				dest.Offset = src.Offset;
				dest.Number = src.Number;
				dest.Isrc = src.Isrc;
				dest.Type = src.Type;
				dest.Pre_Emphasis = src.Pre_Emphasis;
				dest.Num_Indices = src.Num_Indices;
				dest.Indices = src.Indices;
			}

			CueSheet_Calculate_Length(@object);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flac__Metadata_Object_Delete_Data(Flac__StreamMetadata @object)
		{
			Debug.Assert(@object != null);

			switch (@object.Type)
			{
				case Flac__MetadataType.StreamInfo:
				case Flac__MetadataType.Padding:
				{
					@object.Data = null;
					break;
				}

				case Flac__MetadataType.Application:
				{
					Flac__StreamMetadata_Application metaApplication = (Flac__StreamMetadata_Application)@object.Data;

					if (metaApplication.Data != null)
						metaApplication.Data = null;

					@object.Data = null;
					break;
				}

				case Flac__MetadataType.SeekTable:
				{
					Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)@object.Data;

					if (metaSeekTable.Points != null)
						metaSeekTable.Points = null;

					@object.Data = null;
					break;
				}

				case Flac__MetadataType.Vorbis_Comment:
				{
					Flac__StreamMetadata_VorbisComment metaVorbisComment = (Flac__StreamMetadata_VorbisComment)@object.Data;

					if (metaVorbisComment.Vendor_String.Entry != null)
						metaVorbisComment.Vendor_String.Entry = null;

					if (metaVorbisComment.Comments != null)
					{
						Debug.Assert(metaVorbisComment.Num_Comments > 0);

						VorbisComment_Entry_Array_Delete(metaVorbisComment.Comments, metaVorbisComment.Num_Comments);
						metaVorbisComment.Comments = null;
						metaVorbisComment.Num_Comments = 0;
					}

					@object.Data = null;
					break;
				}

				case Flac__MetadataType.CueSheet:
				{
					Flac__StreamMetadata_CueSheet metaCueSheet = (Flac__StreamMetadata_CueSheet)@object.Data;

					if (metaCueSheet.Tracks != null)
					{
						Debug.Assert(metaCueSheet.Num_Tracks > 0);

						CueSheet_Track_Array_Delete(metaCueSheet.Tracks, metaCueSheet.Num_Tracks);
						metaCueSheet.Tracks = null;
						metaCueSheet.Num_Tracks = 0;
					}

					@object.Data = null;
					break;
				}

				case Flac__MetadataType.Picture:
				{
					Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)@object.Data;

					if (metaPicture.Mime_Type != null)
						metaPicture.Mime_Type = null;

					if (metaPicture.Description != null)
						metaPicture.Description = null;

					if (metaPicture.Data != null)
						metaPicture.Data = null;

					@object.Data = null;
					break;
				}

				default:
				{
					Flac__StreamMetadata_Unknown metaUnknown = (Flac__StreamMetadata_Unknown)@object.Data;

					if (metaUnknown.Data != null)
						metaUnknown.Data = null;

					@object.Data = null;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneStreamInfo(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_StreamInfo fromData = (Flac__StreamMetadata_StreamInfo)from.Data;
			Flac__StreamMetadata_StreamInfo toData = (Flac__StreamMetadata_StreamInfo)to.Data;

			toData.Min_BlockSize = fromData.Min_BlockSize;
			toData.Max_BlockSize = fromData.Max_BlockSize;
			toData.Min_FrameSize = fromData.Min_FrameSize;
			toData.Max_FrameSize = fromData.Max_FrameSize;
			toData.Sample_Rate = fromData.Sample_Rate;
			toData.Channels = fromData.Channels;
			toData.Bits_Per_Sample = fromData.Bits_Per_Sample;
			toData.Total_Samples = fromData.Total_Samples;

			Array.Copy(fromData.Md5Sum, toData.Md5Sum, fromData.Md5Sum.Length);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneApplication(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_Application fromData = (Flac__StreamMetadata_Application)from.Data;
			Flac__StreamMetadata_Application toData = (Flac__StreamMetadata_Application)to.Data;

			if (to.Length < (Constants.Flac__Stream_Metadata_Application_Id_Len / 8))
			{
				// Underflow check
				return false;
			}

			Array.Copy(fromData.Id, toData.Id, Constants.Flac__Stream_Metadata_Application_Id_Len / 8);

			if (!Copy_Bytes(ref toData.Data, fromData.Data, from.Length - (Constants.Flac__Stream_Metadata_Application_Id_Len / 8)))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneSeekTable(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_SeekTable fromData = (Flac__StreamMetadata_SeekTable)from.Data;
			Flac__StreamMetadata_SeekTable toData = (Flac__StreamMetadata_SeekTable)to.Data;

			toData.Num_Points = fromData.Num_Points;

			if ((fromData.Num_Points > 0) && (fromData.Points != null))
			{
				Flac__StreamMetadata_SeekPoint[] x = new Flac__StreamMetadata_SeekPoint[fromData.Num_Points];

				for (int i = 0; i < x.Length; i++)
				{
					x[i] = new Flac__StreamMetadata_SeekPoint();
					x[i].Sample_Number = fromData.Points[i].Sample_Number;
					x[i].Stream_Offset = fromData.Points[i].Stream_Offset;
					x[i].Frame_Samples = fromData.Points[i].Frame_Samples;
				}

				toData.Points = x;
			}
			else
				toData.Points = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneVorbisComment(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_VorbisComment fromData = (Flac__StreamMetadata_VorbisComment)from.Data;
			Flac__StreamMetadata_VorbisComment toData = (Flac__StreamMetadata_VorbisComment)to.Data;

			if (toData.Vendor_String.Entry != null)
				toData.Vendor_String.Entry = null;

			if (!Copy_VCEntry(toData.Vendor_String, fromData.Vendor_String))
				return false;

			if (fromData.Num_Comments == 0)
				toData.Num_Comments = 0;
			else
			{
				toData.Comments = VorbisComment_Entry_Array_Copy(fromData.Comments, fromData.Num_Comments);
				if (toData.Comments == null)
				{
					toData.Num_Comments = 0;
					return false;
				}
			}

			toData.Num_Comments = fromData.Num_Comments;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneCueSheet(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_CueSheet fromData = (Flac__StreamMetadata_CueSheet)from.Data;
			Flac__StreamMetadata_CueSheet toData = (Flac__StreamMetadata_CueSheet)to.Data;

			Array.Copy(fromData.Media_Catalog_Number, toData.Media_Catalog_Number, fromData.Media_Catalog_Number.Length);

			toData.Lead_In = fromData.Lead_In;
			toData.Is_Cd = fromData.Is_Cd;
			toData.Num_Tracks = fromData.Num_Tracks;

			if (fromData.Num_Tracks == 0)
				Debug.Assert(fromData.Tracks == null);
			else
			{
				Debug.Assert(fromData.Tracks != null);

				toData.Tracks = CueSheet_Track_Array_Copy(fromData.Tracks, fromData.Num_Tracks);
				if (toData.Tracks == null)
					return false;
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool ClonePicture(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_Picture fromData = (Flac__StreamMetadata_Picture)from.Data;
			Flac__StreamMetadata_Picture toData = (Flac__StreamMetadata_Picture)to.Data;

			toData.Type = fromData.Type;

			if (!Copy_Bytes(ref toData.Mime_Type, fromData.Mime_Type, (uint32_t)fromData.Mime_Type.Length))
				return false;

			if (!Copy_Bytes(ref toData.Description, fromData.Description, (uint32_t)fromData.Description.Length))
				return false;

			toData.Width = fromData.Width;
			toData.Height = fromData.Height;
			toData.Depth = fromData.Depth;
			toData.Colors = fromData.Colors;
			toData.Data_Length = fromData.Data_Length;

			if (!Copy_Bytes(ref toData.Data, fromData.Data, fromData.Data_Length))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static Flac__bool CloneUnknown(Flac__StreamMetadata from, Flac__StreamMetadata to)
		{
			Flac__StreamMetadata_Unknown fromData = (Flac__StreamMetadata_Unknown)from.Data;
			Flac__StreamMetadata_Unknown toData = (Flac__StreamMetadata_Unknown)to.Data;

			if (!Copy_Bytes(ref toData.Data, fromData.Data, from.Length))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Flac__Metadata_Object_CueSheet_Track_Delete_Data(Flac__StreamMetadata_CueSheet_Track @object)
		{
			Debug.Assert(@object != null);

			if (@object.Indices != null)
			{
				Debug.Assert(@object.Num_Indices > 0);
				@object.Indices = null;
			}
		}
		#endregion
	}
}
