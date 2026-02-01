/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;
using Buffer = Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Buffer;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// EXIF metadata parser
	/// </summary>
	public static class Exif
	{
		private const uint32_t Exif_II_Long = 0x49492a00;
		private const uint32_t Exif_MM_Long = 0x4d4d002a;

		private const c_int Base_Tag_Size = 12;
		private const c_int Ifd_Extra_Size = 6;

		private const uint16_t MakerNote_Tag = 0x927c;

		#region Exif_MakerNote_Data class
		private class Exif_MakerNote_Data
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Exif_MakerNote_Data(uint8_t[] h, c_int r)
			{
				Header = h;
				Header_Size = (size_t)h.Length;
				Result = r;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public CPointer<uint8_t> Header { get; }



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public size_t Header_Size { get; }



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public c_int Result { get; }
		}
		#endregion

		#region Exif_Tag class
		private class Exif_Tag
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Exif_Tag(string name, uint16_t id)
			{
				Name = name.ToCharPointer();
				Id = id;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public CPointer<char> Name { get; }



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public uint16_t Id { get; }
		}
		#endregion

		private static readonly uint8_t[] aoc_Header = [ (uint8_t)'A', (uint8_t)'O', (uint8_t)'C', 0 ];
		private static readonly uint8_t[] casio_Header = [ (uint8_t)'Q', (uint8_t)'V', (uint8_t)'C', 0, 0, 0 ];
		private static readonly uint8_t[] foveon_Header = [ (uint8_t)'F', (uint8_t)'O', (uint8_t)'V', (uint8_t)'E', (uint8_t)'O', (uint8_t)'N', 0, 0 ];
		private static readonly uint8_t[] fuji_Header = [ (uint8_t)'F', (uint8_t)'U', (uint8_t)'J', (uint8_t)'I' ];
		private static readonly uint8_t[] nikon_Header = [ (uint8_t)'N', (uint8_t)'i', (uint8_t)'k', (uint8_t)'o', (uint8_t)'n', 0 ];
		private static readonly uint8_t[] olympus1_Header = [ (uint8_t)'O', (uint8_t)'L', (uint8_t)'Y', (uint8_t)'M', (uint8_t)'P', 0 ];
		private static readonly uint8_t[] olympus2_Header = [ (uint8_t)'O', (uint8_t)'L', (uint8_t)'Y', (uint8_t)'M', (uint8_t)'P', (uint8_t)'U', (uint8_t)'S', 0, (uint8_t)'I', (uint8_t)'I' ];
		private static readonly uint8_t[] panasonic_Header = [ (uint8_t)'P', (uint8_t)'a', (uint8_t)'n', (uint8_t)'a', (uint8_t)'s', (uint8_t)'o', (uint8_t)'n', (uint8_t)'i', (uint8_t)'c', 0, 0, 0 ];
		private static readonly uint8_t[] sigma_Header = [ (uint8_t)'S', (uint8_t)'I', (uint8_t)'G', (uint8_t)'M', (uint8_t)'A', 0, 0, 0 ];
		private static readonly uint8_t[] sony_Header = [ (uint8_t)'S', (uint8_t)'O', (uint8_t)'N', (uint8_t)'Y', (uint8_t)' ', (uint8_t)'D', (uint8_t)'S', (uint8_t)'C', (uint8_t)' ', 0, 0, 0 ];

		private static readonly Exif_MakerNote_Data[] makerNote_Data =
		[
			new Exif_MakerNote_Data(aoc_Header, 6),
			new Exif_MakerNote_Data(casio_Header, -1),
			new Exif_MakerNote_Data(foveon_Header, 10),
			new Exif_MakerNote_Data(fuji_Header, -1),
			new Exif_MakerNote_Data(olympus1_Header, 8),
			new Exif_MakerNote_Data(olympus2_Header, -1),
			new Exif_MakerNote_Data(panasonic_Header, 12),
			new Exif_MakerNote_Data(sigma_Header, 10),
			new Exif_MakerNote_Data(sony_Header, 12)
		];

		private static readonly Exif_Tag[] tag_List =						// JEITA CP-3451 EXIF specification:
		[
			new Exif_Tag("GPSVersionID", 0x00),						// Table 12 GPS Attribute Information
			new Exif_Tag("GPSLatitudeRef", 0x01),
			new Exif_Tag("GPSLatitude", 0x02),
			new Exif_Tag("GPSLongitudeRef", 0x03),
			new Exif_Tag("GPSLongitude", 0x04),
			new Exif_Tag("GPSAltitudeRef", 0x05),
			new Exif_Tag("GPSAltitude", 0x06),
			new Exif_Tag("GPSTimeStamp", 0x07),
			new Exif_Tag("GPSSatellites", 0x08),
			new Exif_Tag("GPSStatus", 0x09),
			new Exif_Tag("GPSMeasureMode", 0x0A),
			new Exif_Tag("GPSDOP", 0x0B),
			new Exif_Tag("GPSSpeedRef", 0x0C),
			new Exif_Tag("GPSSpeed", 0x0D),
			new Exif_Tag("GPSTrackRef", 0x0E),
			new Exif_Tag("GPSTrack", 0x0F),
			new Exif_Tag("GPSImgDirectionRef", 0x10),
			new Exif_Tag("GPSImgDirection", 0x11),
			new Exif_Tag("GPSMapDatum", 0x12),
			new Exif_Tag("GPSDestLatitudeRef", 0x13),
			new Exif_Tag("GPSDestLatitude", 0x14),
			new Exif_Tag("GPSDestLongitudeRef", 0x15),
			new Exif_Tag("GPSDestLongitude", 0x16),
			new Exif_Tag("GPSDestBearingRef", 0x17),
			new Exif_Tag("GPSDestBearing", 0x18),
			new Exif_Tag("GPSDestDistanceRef", 0x19),
			new Exif_Tag("GPSDestDistance", 0x1A),
			new Exif_Tag("GPSProcessingMethod", 0x1B),
			new Exif_Tag("GPSAreaInformation", 0x1C),
			new Exif_Tag("GPSDateStamp", 0x1D),
			new Exif_Tag("GPSDifferential", 0x1E),
			new Exif_Tag("ImageWidth", 0x100),						// Table 3 TIFF Rev. 6.0 Attribute Information Used in Exif
			new Exif_Tag("ImageLength", 0x101),
			new Exif_Tag("BitsPerSample", 0x102),
			new Exif_Tag("Compression", 0x103),
			new Exif_Tag("PhotometricInterpretation", 0x106),
			new Exif_Tag("Orientation", 0x112),
			new Exif_Tag("SamplesPerPixel", 0x115),
			new Exif_Tag("PlanarConfiguration", 0x11C),
			new Exif_Tag("YCbCrSubSampling", 0x212),
			new Exif_Tag("YCbCrPositioning", 0x213),
			new Exif_Tag("XResolution", 0x11A),
			new Exif_Tag("YResolution", 0x11B),
			new Exif_Tag("ResolutionUnit", 0x128),
			new Exif_Tag("StripOffsets", 0x111),
			new Exif_Tag("RowsPerStrip", 0x116),
			new Exif_Tag("StripByteCounts", 0x117),
			new Exif_Tag("JPEGInterchangeFormat", 0x201),
			new Exif_Tag("JPEGInterchangeFormatLength", 0x202),
			new Exif_Tag("TransferFunction", 0x12D),
			new Exif_Tag("WhitePoint", 0x13E),
			new Exif_Tag("PrimaryChromaticities", 0x13F),
			new Exif_Tag("YCbCrCoefficients", 0x211),
			new Exif_Tag("ReferenceBlackWhite", 0x214),
			new Exif_Tag("DateTime", 0x132),
			new Exif_Tag("ImageDescription", 0x10E),
			new Exif_Tag("Make", 0x10F),
			new Exif_Tag("Model", 0x110),
			new Exif_Tag("Software", 0x131),
			new Exif_Tag("Artist", 0x13B),
			new Exif_Tag("Copyright", 0x8298),
			new Exif_Tag("ExifVersion", 0x9000),						// Table 4 Exif IFD Attribute Information (1)
			new Exif_Tag("FlashpixVersion", 0xA000),
			new Exif_Tag("ColorSpace", 0xA001),
			new Exif_Tag("ComponentsConfiguration", 0x9101),
			new Exif_Tag("CompressedBitsPerPixel", 0x9102),
			new Exif_Tag("PixelXDimension", 0xA002),
			new Exif_Tag("PixelYDimension", 0xA003),
			new Exif_Tag("MakerNote", 0x927C),
			new Exif_Tag("UserComment", 0x9286),
			new Exif_Tag("RelatedSoundFile", 0xA004),
			new Exif_Tag("DateTimeOriginal", 0x9003),
			new Exif_Tag("DateTimeDigitized", 0x9004),
			new Exif_Tag("SubSecTime", 0x9290),
			new Exif_Tag("SubSecTimeOriginal", 0x9291),
			new Exif_Tag("SubSecTimeDigitized", 0x9292),
			new Exif_Tag("ImageUniqueID", 0xA420),
			new Exif_Tag("ExposureTime", 0x829A),					// Table 5 Exif IFD Attribute Information (2)
			new Exif_Tag("FNumber", 0x829D),
			new Exif_Tag("ExposureProgram", 0x8822),
			new Exif_Tag("SpectralSensitivity", 0x8824),
			new Exif_Tag("ISOSpeedRatings", 0x8827),
			new Exif_Tag("OECF", 0x8828),
			new Exif_Tag("ShutterSpeedValue", 0x9201),
			new Exif_Tag("ApertureValue", 0x9202),
			new Exif_Tag("BrightnessValue", 0x9203),
			new Exif_Tag("ExposureBiasValue", 0x9204),
			new Exif_Tag("MaxApertureValue", 0x9205),
			new Exif_Tag("SubjectDistance", 0x9206),
			new Exif_Tag("MeteringMode", 0x9207),
			new Exif_Tag("LightSource", 0x9208),
			new Exif_Tag("Flash", 0x9209),
			new Exif_Tag("FocalLength", 0x920A),
			new Exif_Tag("SubjectArea", 0x9214),
			new Exif_Tag("FlashEnergy", 0xA20B),
			new Exif_Tag("SpatialFrequencyResponse", 0xA20C),
			new Exif_Tag("FocalPlaneXResolution", 0xA20E),
			new Exif_Tag("FocalPlaneYResolution", 0xA20F),
			new Exif_Tag("FocalPlaneResolutionUnit", 0xA210),
			new Exif_Tag("SubjectLocation", 0xA214),
			new Exif_Tag("ExposureIndex", 0xA215),
			new Exif_Tag("SensingMethod", 0xA217),
			new Exif_Tag("FileSource", 0xA300),
			new Exif_Tag("SceneType", 0xA301),
			new Exif_Tag("CFAPattern", 0xA302),
			new Exif_Tag("CustomRendered", 0xA401),
			new Exif_Tag("ExposureMode", 0xA402),
			new Exif_Tag("WhiteBalance", 0xA403),
			new Exif_Tag("DigitalZoomRatio", 0xA404),
			new Exif_Tag("FocalLengthIn35mmFilm", 0xA405),
			new Exif_Tag("SceneCaptureType", 0xA406),
			new Exif_Tag("GainControl", 0xA407),
			new Exif_Tag("Contrast", 0xA408),
			new Exif_Tag("Saturation", 0xA409),
			new Exif_Tag("Sharpness", 0xA40A),
			new Exif_Tag("DeviceSettingDescription", 0xA40B),
			new Exif_Tag("SubjectDistanceRange", 0xA40C),

			// InteropIFD tags
			new Exif_Tag("RelatedImageFileFormat", 0x1000),
			new Exif_Tag("RelatedImageWidth", 0x1001),
			new Exif_Tag("RelatedImageLength", 0x1002),

			// Private EXIF tags
			new Exif_Tag("PrintImageMatching", 0xC4A5),				// Undocumented meaning

			// IFD tags
			new Exif_Tag("ExifIFD", 0x8769),							// An IFD pointing to standard Exif metadata
			new Exif_Tag("GPSInfo", 0x8825),							// An IFD pointing to GPS Exif Metadata
			new Exif_Tag("InteropIFD", 0xA005),						// Table 13 Interoperability IFD Attribute Information
			new Exif_Tag("GlobalParametersIFD", 0x0190),
			new Exif_Tag("ProfileIFD", 0xc6f5)
		];

		private static readonly size_t[] exif_Sizes = Initialize_Exif_Sizes();

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static size_t[] Initialize_Exif_Sizes()
		{
			size_t[] result = new size_t[(c_int)AvTiffDataType.Ifd];

			result[0] = 0;
			result[(c_int)AvTiffDataType.Byte] = 1;
			result[(c_int)AvTiffDataType.String] = 1;
			result[(c_int)AvTiffDataType.Short] = 2;
			result[(c_int)AvTiffDataType.Long] = 4;
			result[(c_int)AvTiffDataType.Rational] = 8;
			result[(c_int)AvTiffDataType.SByte] = 1;
			result[(c_int)AvTiffDataType.Undefined] = 1;
			result[(c_int)AvTiffDataType.SShort] = 2;
			result[(c_int)AvTiffDataType.SLong] = 4;
			result[(c_int)AvTiffDataType.SRational] = 8;
			result[(c_int)AvTiffDataType.Float] = 4;
			result[(c_int)AvTiffDataType.Double] = 8;
			result[(c_int)AvTiffDataType.Ifd] = 4;

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the tag name associated with the provided tag ID. If
		/// the tag ID is unknown, NULL is returned.
		///
		/// For example, av_exif_get_tag_name(0x112) returns "Orientation"
		/// </summary>
		/********************************************************************/
		public static CPointer<char> Av_Exif_Get_Tag_Name(uint16_t id)//XX 215
		{
			for (size_t i = 0; i < Macros.FF_Array_Elems(tag_List); i++)
			{
				if (tag_List[i].Id == id)
					return tag_List[i].Name;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieves the tag ID associated with the provided tag string
		/// name. If the tag name is unknown, a negative number is returned.
		/// Otherwise it always fits inside a uint16_t integer
		/// </summary>
		/********************************************************************/
		public static int32_t Av_Exif_Get_Tag_Id(CPointer<char> name)//XX 225
		{
			if (name.IsNull)
				return -1;

			for (size_t i = 0; i < Macros.FF_Array_Elems(tag_List); i++)
			{
				if (CString.strcmp(tag_List[i].Name, name) == 0)
					return tag_List[i].Id;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Frees all resources associated with the given EXIF metadata
		/// struct. Does not free the pointer passed itself, in case it is
		/// stack-allocated.
		/// The pointer passed to this function must be freed by the caller,
		/// if it is heap-allocated. Passing NULL is permitted
		/// </summary>
		/********************************************************************/
		public static void Av_Exif_Free(AvExifMetadata ifd)//XX 609
		{
			if (ifd == null)
				return;

			if (ifd.Entries.IsNull)
			{
				ifd.Count = 0;
				ifd.Size = 0;

				return;
			}

			for (size_t i = 0; i < ifd.Count; i++)
			{
				AvExifEntry entry = ifd.Entries[i];
				Exif_Free_Entry(entry);
			}

			Mem.Av_FreeP(ref ifd.Entries);

			ifd.Count = 0;
			ifd.Size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates a buffer using av_malloc of an appropriate size and
		/// writes the EXIF data represented by ifd into that buffer.
		///
		/// Upon error, *buffer will be NULL. The buffer becomes owned by the
		/// caller upon success. The *buffer argument must be NULL before
		/// calling
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Write(IClass logCtx, AvExifMetadata ifd, out AvBufferRef buffer, AvExifHeaderMode header_Mode)//XX 703
		{
			buffer = null;

			size_t headSize = 8;
			c_int off = 0;

			c_int le = 1;

			size_t size = Exif_Get_Ifd_Size(ifd);

			switch (header_Mode)
			{
				case AvExifHeaderMode.Exif00:
				{
					off = 6;
					break;
				}

				case AvExifHeaderMode.T_Off:
				{
					off = 4;
					break;
				}

				case AvExifHeaderMode.Assume_Be:
				{
					le = 0;
					headSize = 0;
					break;
				}

				case AvExifHeaderMode.Assume_Le:
				{
					le = 1;
					headSize = 0;
					break;
				}
			}

			AvBufferRef buf = Buffer.Av_Buffer_Alloc(size + (size_t)off + headSize);

			if (buf == null)
				return Error.ENOMEM;

			DataBufferContext dataBuffer = (DataBufferContext)buf.Data;

			if (header_Mode == AvExifHeaderMode.Exif00)
			{
				IntReadWrite.Av_WL32(dataBuffer.Data, Macros.MkTag('E', 'x', 'i', 'f'));
				IntReadWrite.Av_WN16(dataBuffer.Data + 4, 0);
			}
			else if (header_Mode == AvExifHeaderMode.T_Off)
				IntReadWrite.Av_WN32(dataBuffer.Data, 0);

			ByteStream.ByteStream2_Init_Writer(out PutByteContext pb, dataBuffer.Data + off, (c_int)(dataBuffer.Size - (size_t)off));

			if ((header_Mode != AvExifHeaderMode.Assume_Be) && (header_Mode != AvExifHeaderMode.Assume_Le))
			{
				// These constants are be32 in both cases
				// le == 1 always in this case
				ByteStream.ByteStream2_Put_BE32(pb, Exif_II_Long);
				TPut32(pb, le, 8);
			}

			c_int ret = Exif_Write_Ifd(logCtx, pb, le, 0, ifd);

			if (ret < 0)
			{
				Buffer.Av_Buffer_Unref(ref buf);

				Log.Av_Log(logCtx, Log.Av_Log_Error, "error writing EXIF data: %s\n", Error.Av_Err2Str(ret));

				return ret;
			}

			buffer = buf;

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decodes the EXIF data provided in the buffer and writes it into
		/// the struct *ifd. If this function succeeds, the IFD is owned by
		/// the caller and must be cleared after use by calling
		/// av_exif_free(); If this function fails and returns a negative
		/// value, it will call av_exif_free(ifd) before returning
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Parse_Buffer(IClass logCtx, CPointer<uint8_t> buf, size_t size, out AvExifMetadata ifd, AvExifHeaderMode header_Mode)//XX 764
		{
			ifd = null;

			GetByteContext gBytes;
			c_int ret, le;

			if (size > c_int.MaxValue)
				return Error.EINVAL;

			size_t off = 0;

			switch (header_Mode)
			{
				case AvExifHeaderMode.Exif00:
				{
					if (size < 6)
						return Error.InvalidData;

					off = 6;

					goto case AvExifHeaderMode.T_Off;
				}

				case AvExifHeaderMode.T_Off:
				{
					if (size < 4)
						return Error.InvalidData;

					if (off != 0)
						off = IntReadWrite.Av_RB32(buf) + 4;

					goto case AvExifHeaderMode.Tiff_Header;
				}

				case AvExifHeaderMode.Tiff_Header:
				{
					if (size <= off)
						return Error.InvalidData;

					ByteStream.ByteStream2_Init(out gBytes, buf + off, (c_int)(size - off));

					// Read TIFF header
					ret = Tiff_Common.FF_TDecode_Header(gBytes, out le, out c_int ifd_Offset);

					if (ret < 0)
					{
						Log.Av_Log(logCtx, Log.Av_Log_Error, "invalid TIFF header in EXIF data: %s\n", Error.Av_Err2Str(ret));

						return ret;
					}

					ByteStream.ByteStream2_Seek(gBytes, ifd_Offset, SeekOrigin.Begin);
					break;
				}

				case AvExifHeaderMode.Assume_Le:
				{
					le = 1;

					ByteStream.ByteStream2_Init(out gBytes, buf, (c_int)size);
					break;
				}

				case AvExifHeaderMode.Assume_Be:
				{
					le = 0;

					ByteStream.ByteStream2_Init(out gBytes, buf, (c_int)size);
					break;
				}

				default:
					return Error.EINVAL;
			}

			// Parse IFD0 here. If the return value is positive that tells us
			// there is subimage metadata, but we don't parse that IFD here
			ret = Exif_Parse_Ifd_List(logCtx, gBytes, le, 0, out ifd);

			if (ret < 0)
			{
				Av_Exif_Free(ifd);

				Log.Av_Log(logCtx, Log.Av_Log_Error, "error decoding EXIF data: %s\n", Error.Av_Err2Str(ret));

				return ret;
			}

			return ByteStream.ByteStream2_Tell(gBytes);
		}



		/********************************************************************/
		/// <summary>
		/// Recursively reads all tags from the IFD and stores them in the
		/// provided metadata dictionary
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Ifd_To_Dict(IClass logCtx, AvExifMetadata ifd, ref AvDictionary metadata)//XX 914
		{
			return Exif_Ifd_To_Dict(logCtx, CString.Empty, ifd, ref metadata);
		}



		/********************************************************************/
		/// <summary>
		/// Get an entry with the tagged ID from the EXIF metadata struct. A
		/// pointer to the entry will be written into *value.
		///
		/// If the entry was present and returned successfully, a positive
		/// number is returned.
		/// If the entry was not found, *value is left untouched and zero is
		/// returned.
		/// If an error occurred, a negative number is returned.
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Get_Entry(IClass logCtx, AvExifMetadata ifd, uint16_t id, AvExifFlag flags, out AvExifEntry value)//XX 1051
		{
			return Exif_Get_Entry(logCtx, ifd, id, (flags & AvExifFlag.Recursive) != 0 ? 0 : c_int.MaxValue, out value);
		}



		/********************************************************************/
		/// <summary>
		/// Remove an entry from the provided EXIF metadata struct.
		///
		/// If the entry was present and removed successfully, a positive
		/// number is returned.
		/// If the entry was not found, zero is returned.
		/// If an error occurred, a negative number is returned
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Remove_Entry(IClass logCtx, AvExifMetadata ifd, uint16_t id, AvExifFlag flags)//XX 1140
		{
			return Exif_Remove_Entry(logCtx, ifd, id, (flags & AvExifFlag.Recursive) != 0 ? 0 : c_int.MaxValue);
		}



		/********************************************************************/
		/// <summary>
		/// Convert an orientation constant used by EXIF's orientation tag
		/// into a display matrix used by AV_FRAME_DATA_DISPLAYMATRIX.
		///
		/// Returns 0 on success and negative if the orientation is invalid,
		/// i.e. not between 1 and 8 (inclusive)
		/// </summary>
		/********************************************************************/
		public static c_int Av_Exif_Orientation_To_Matrix(CPointer<int32_t> matrix, c_int orientation)//XX 1194
		{
			switch (orientation)
			{
				case 1:
				{
					Display.Av_Display_Rotation_Set(matrix, 0.0);
					break;
				}

				case 2:
				{
					Display.Av_Display_Rotation_Set(matrix, 0.0);
					Display.Av_Display_Matrix_Flip(matrix, 1, 0);
					break;
				}

				case 3:
				{
					Display.Av_Display_Rotation_Set(matrix, 180.0);
					break;
				}

				case 4:
				{
					Display.Av_Display_Rotation_Set(matrix, 180.0);
					Display.Av_Display_Matrix_Flip(matrix, 1, 0);
					break;
				}

				case 5:
				{
					Display.Av_Display_Rotation_Set(matrix, 90.0);
					Display.Av_Display_Matrix_Flip(matrix, 1, 0);
					break;
				}

				case 6:
				{
					Display.Av_Display_Rotation_Set(matrix, 90.0);
					break;
				}

				case 7:
				{
					Display.Av_Display_Rotation_Set(matrix, -90.0);
					Display.Av_Display_Matrix_Flip(matrix, 1, 0);
					break;
				}

				case 8:
				{
					Display.Av_Display_Rotation_Set(matrix, -90.0);
					break;
				}

				default:
					return Error.EINVAL;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TPut16(PutByteContext pb, c_int le, uint16_t value)//XX 238
		{
			if (le != 0)
				ByteStream.ByteStream2_Put_LE16(pb, value);
			else
				ByteStream.ByteStream2_Put_BE16(pb, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TPut32(PutByteContext pb, c_int le, uint32_t value)//XX 243
		{
			if (le != 0)
				ByteStream.ByteStream2_Put_LE32(pb, value);
			else
				ByteStream.ByteStream2_Put_BE32(pb, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void TPut64(PutByteContext pb, c_int le, uint64_t value)//XX 248
		{
			if (le != 0)
				ByteStream.ByteStream2_Put_LE64(pb, value);
			else
				ByteStream.ByteStream2_Put_BE64(pb, value);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Read_Values(IClass logCtx, GetByteContext gb, c_int le, AvExifEntry entry)//XX 253
		{
			switch (entry.Type)
			{
				case AvTiffDataType.Short:
				case AvTiffDataType.Long:
				{
					entry.Value.UInt = Mem.Av_CAlloc<uint64_t>(entry.Count);
					break;
				}

				case AvTiffDataType.SShort:
				case AvTiffDataType.SLong:
				{
					entry.Value.SInt = Mem.Av_CAlloc<int64_t>(entry.Count);
					break;
				}

				case AvTiffDataType.Double:
				case AvTiffDataType.Float:
				{
					entry.Value.Dbl = Mem.Av_CAlloc<c_double>(entry.Count);
					break;
				}

				case AvTiffDataType.Rational:
				case AvTiffDataType.SRational:
				{
					entry.Value.Rat = Mem.Av_CAlloc<AvRational>(entry.Count);
					break;
				}

				case AvTiffDataType.Undefined:
				case AvTiffDataType.Byte:
				{
					entry.Value.UBytes = Mem.Av_MAllocz<uint8_t>(entry.Count);
					break;
				}

				case AvTiffDataType.SByte:
				{
					entry.Value.SBytes = Mem.Av_MAllocz<int8_t>(entry.Count);
					break;
				}

				case AvTiffDataType.String:
				{
					entry.Value.Str = Mem.Av_MAllocz<char>(entry.Count + 1);
					break;
				}

				case AvTiffDataType.Ifd:
				{
					Log.Av_Log(logCtx, Log.Av_Log_Warning, "Bad IFD type for non-IFD tag\n");

					return Error.InvalidData;
				}
			}

//			if (entry.Value.Ptr.IsNull)
//				return Error.ENOMEM;

			switch (entry.Type)
			{
				case AvTiffDataType.Short:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.UInt[i] = Tiff_Common.FF_TGet_Short(gb, le);

					break;
				}

				case AvTiffDataType.Long:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.UInt[i] = Tiff_Common.FF_TGet_Long(gb, le);

					break;
				}

				case AvTiffDataType.SShort:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.SInt[i] = (int16_t)Tiff_Common.FF_TGet_Short(gb, le);

					break;
				}

				case AvTiffDataType.SLong:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.SInt[i] = (int32_t)Tiff_Common.FF_TGet_Long(gb, le);

					break;
				}

				case AvTiffDataType.Double:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.Dbl[i] = Tiff_Common.FF_TGet_Double(gb, le);

					break;
				}

				case AvTiffDataType.Float:
				{
					for (size_t i = 0; i < entry.Count; i++)
						entry.Value.Dbl[i] = BitConverter.UInt32BitsToSingle(Tiff_Common.FF_TGet_Long(gb, le));

					break;
				}

				case AvTiffDataType.Rational:
				case AvTiffDataType.SRational:
				{
					for (size_t i = 0; i < entry.Count; i++)
					{
						int32_t num = (int32_t)Tiff_Common.FF_TGet_Long(gb, le);
						int32_t den = (int32_t)Tiff_Common.FF_TGet_Long(gb, le);

						entry.Value.Rat[i] = Rational.Av_Make_Q(num, den);
					}

					break;
				}

				case AvTiffDataType.Undefined:
				case AvTiffDataType.Byte:
				{
					ByteStream.ByteStream2_Get_Buffer(gb, entry.Value.UBytes, entry.Count);
					break;
				}

				case AvTiffDataType.SByte:
				{
					ByteStream.ByteStream2_Get_Buffer(gb, entry.Value.SBytes, entry.Count);
					break;
				}

				case AvTiffDataType.String:
				{
					ByteStream.ByteStream2_Get_Buffer(gb, entry.Value.Str, entry.Count);
					break;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Exif_Write_Values(PutByteContext pb, c_int le, AvExifEntry entry)//XX 343
		{
			switch (entry.Type)
			{
				case AvTiffDataType.Short:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut16(pb, le, (uint16_t)entry.Value.UInt[i]);

					break;
				}

				case AvTiffDataType.Long:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut32(pb, le, (uint32_t)entry.Value.UInt[i]);

					break;
				}

				case AvTiffDataType.SShort:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut16(pb, le, (uint16_t)entry.Value.SInt[i]);

					break;
				}

				case AvTiffDataType.SLong:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut32(pb, le, (uint32_t)entry.Value.SInt[i]);

					break;
				}

				case AvTiffDataType.Double:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut64(pb, le, BitConverter.DoubleToUInt64Bits(entry.Value.Dbl[i]));

					break;
				}

				case AvTiffDataType.Float:
				{
					for (size_t i = 0; i < entry.Count; i++)
						TPut32(pb, le, BitConverter.SingleToUInt32Bits((c_float)entry.Value.Dbl[i]));

					break;
				}

				case AvTiffDataType.Rational:
				case AvTiffDataType.SRational:
				{
					for (size_t i = 0; i < entry.Count; i++)
					{
						TPut32(pb, le, (uint32_t)entry.Value.Rat[i].Num);
						TPut32(pb, le, (uint32_t)entry.Value.Rat[i].Den);
					}

					break;
				}

				case AvTiffDataType.Undefined:
				case AvTiffDataType.Byte:
				{
					ByteStream.ByteStream2_Put_Buffer(pb, entry.Value.UBytes, entry.Count);
					break;
				}

				case AvTiffDataType.SByte:
				{
					ByteStream.ByteStream2_Put_Buffer(pb, entry.Value.SBytes, entry.Count);
					break;
				}

				case AvTiffDataType.String:
				{
					ByteStream.ByteStream2_Put_Buffer(pb, entry.Value.Str, entry.Count);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Derived from Exiv2 MakerNote's article
		/// https://exiv2.org/makernote.html or archived at
		/// https://web.archive.org/web/20250311155857/https://exiv2.org/makernote.html
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Get_MakerNote_Offset(GetByteContext gb)//XX 434
		{
			if (ByteStream.ByteStream2_Get_Bytes_Left(gb) < Base_Tag_Size)
				return -1;

			for (c_int i = 0; i < (c_int)Macros.FF_Array_Elems(makerNote_Data); i++)
			{
				if (CMemory.memcmp(gb.Buffer, makerNote_Data[i].Header, makerNote_Data[i].Header_Size) == 0)
					return makerNote_Data[i].Result;
			}

			if (CMemory.memcmp(gb.Buffer, nikon_Header, (size_t)nikon_Header.Length) == 0)
			{
				if (ByteStream.ByteStream2_Get_Bytes_Left(gb) < 14)
					return -1;
				else if ((IntReadWrite.Av_RB32(gb.Buffer + 10) == Exif_MM_Long) || (IntReadWrite.Av_RB32(gb.Buffer + 10) == Exif_II_Long))
					return -1;

				return 8;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Decode_Tag(IClass logCtx, GetByteContext gb, c_int le, c_int depth, AvExifEntry entry)//XX 458
		{
			c_int ret = 0, makerNote_Offset = -1;

			// Safety check to prevent infinite recursion on malicious IFDs
			if (depth > 3)
				return Error.InvalidData;

			c_int tell = ByteStream.ByteStream2_Tell(gb);

			entry.Id = (uint16_t)Tiff_Common.FF_TGet_Short(gb, le);

			AvTiffDataType type = (AvTiffDataType)Tiff_Common.FF_TGet_Short(gb, le);
			c_int count = (c_int)Tiff_Common.FF_TGet_Long(gb, le);
			uint32_t payload = Tiff_Common.FF_TGet_Long(gb, le);

			Log.Av_Log(logCtx, Log.Av_Log_Debug, "TIFF Tag: id: 0x%04x, type: %d, count: %u, offset: %d, payload: %u\n", entry.Id, type, count, tell, payload);

			// AV_TIFF_IFD is the largest, numerically
			if ((type > AvTiffDataType.Ifd) || (count >= (c_int.MaxValue / 8U)))
				return Error.InvalidData;

			bool is_Ifd = (type == AvTiffDataType.Ifd) || (Tiff_Common.FF_Tis_Ifd(entry.Id) != 0) || (entry.Id == MakerNote_Tag);

			if (is_Ifd)
			{
				if (payload == 0)
					goto End;

				ByteStream.ByteStream2_Seek(gb, (c_int)payload, SeekOrigin.Begin);
			}

			if (entry.Id == MakerNote_Tag)
			{
				makerNote_Offset = Exif_Get_MakerNote_Offset(gb);

				if (makerNote_Offset < 0)
					is_Ifd = false;
			}

			if (is_Ifd)
			{
				entry.Type = AvTiffDataType.Ifd;
				entry.Count = 1;
				entry.Ifd_Offset = makerNote_Offset > 0 ? (c_uint)makerNote_Offset : 0U;

				if (entry.Ifd_Offset != 0)
				{
					entry.Ifd_Lead = Mem.Av_MAlloc<uint8_t>(entry.Ifd_Offset);

					if (entry.Ifd_Lead.IsNull)
						return Error.ENOMEM;

					ByteStream.ByteStream2_Get_Buffer(gb, entry.Ifd_Lead, entry.Ifd_Offset);
				}

				ret = Exif_Parse_Ifd_List(logCtx, gb, le, depth + 1, out entry.Value.Ifd);

				if ((ret < 0) && (entry.Id == MakerNote_Tag))
				{
					// We guessed that MakerNote was an IFD
					// but we were probably incorrect at this
					// point so we try again as a binary blob
					Av_Exif_Free(entry.Value.Ifd);

					Log.Av_Log(logCtx, Log.Av_Log_Debug, "unrecognized MarkerNote IFD, retrying as blob\n");

					is_Ifd = false;
				}
			}

			// Inverted condition instead of else so we can fall through from above
			if (!is_Ifd)
			{
				entry.Type = type == AvTiffDataType.Ifd ? AvTiffDataType.Undefined : type;
				entry.Count = (uint32_t)count;

				ByteStream.ByteStream2_Seek(gb, ((size_t)count * exif_Sizes[(c_int)type]) > 4 ? (c_int)payload : tell + 8, SeekOrigin.Begin);

				ret = Exif_Read_Values(logCtx, gb, le, entry);
			}

			End:
			ByteStream.ByteStream2_Seek(gb, tell + Base_Tag_Size, SeekOrigin.Begin);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Parse_Ifd_List(IClass logCtx, GetByteContext gb, c_int le, c_int depth, out AvExifMetadata ifd)//XX 534
		{
			ifd = null;

			Log.Av_Log(logCtx, Log.Av_Log_Debug, "parsing IFD list at offset: %d\n", ByteStream.ByteStream2_Tell(gb));

			if (ByteStream.ByteStream2_Get_Bytes_Left(gb) < 2)
			{
				Log.Av_Log(logCtx, Log.Av_Log_Error, "not enough bytes remaining in EXIF buffer: 2 required\n");

				return Error.InvalidData;
			}

			uint32_t entries = Tiff_Common.FF_TGet_Short(gb, le);

			if (ByteStream.ByteStream2_Get_Bytes_Left(gb) < (entries * Base_Tag_Size))
			{
				Log.Av_Log(logCtx, Log.Av_Log_Error, "not enough bytes remaining in EXIF buffer. entries: %u\n", entries);

				return Error.InvalidData;
			}

			if (entries > 4096)
			{
				// This is a lot of entries, probably an error
				Log.Av_Log(logCtx, Log.Av_Log_Error, "too many entries: %u\n", entries);

				return Error.InvalidData;
			}

			ifd = new AvExifMetadata();
			ifd.Count = entries;

			Log.Av_Log(logCtx, Log.Av_Log_Debug, "entry count for IFD: %u\n", ifd.Count);

			// Empty IFD is technically legal but equivalent to no metadata present
			if (ifd.Count == 0)
				goto End;

			size_t required_Size = ifd.Count;
			CPointer<AvExifEntry> temp = Mem.Av_Fast_ReallocObj(ifd.Entries, ref ifd.Size, required_Size);

			if (temp.IsNull)
			{
				Mem.Av_FreeP(ref ifd.Entries);

				return Error.ENOMEM;
			}

			ifd.Entries = temp;

			// Entries have pointers in them which can cause issues if
			// they are freed or realloc'd when garbage
			ifd.Entries.Clear((c_int)required_Size);

			for (uint32_t i = 0; i < entries; i++)
			{
				c_int ret = Exif_Decode_Tag(logCtx, gb, le, depth, ifd.Entries[i]);

				if (ret < 0)
					return ret;
			}

			End:
			// At the end of an IFD is an pointer to the next IFD
			// or zero if there are no more IFDs, which is usually the case
			return (c_int)Tiff_Common.FF_TGet_Long(gb, le);
		}



		/********************************************************************/
		/// <summary>
		/// Note that this function does not free the entry pointer itself
		/// because it's probably part of a larger array that should be freed
		/// all at once
		/// </summary>
		/********************************************************************/
		private static void Exif_Free_Entry(AvExifEntry entry)//XX 598
		{
			if (entry == null)
				return;

			if (entry.Type == AvTiffDataType.Ifd)
				Av_Exif_Free(entry.Value.Ifd);
//			else
//				Mem.Av_FreeP(ref entry.Value.Ptr);

			Mem.Av_FreeP(ref entry.Ifd_Lead);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static size_t Exif_Get_Ifd_Size(AvExifMetadata ifd)//XX 627
		{
			// 6 == 4 + 2; 2-byte entry-count at the beginning
			// plus 4-byte next-IFD pointer at the end
			size_t total_Size = Ifd_Extra_Size;

			for (size_t i = 0; i < ifd.Count; i++)
			{
				AvExifEntry entry = ifd.Entries[i];

				if (entry.Type == AvTiffDataType.Ifd)
					total_Size += Base_Tag_Size + Exif_Get_Ifd_Size(entry.Value.Ifd) + entry.Ifd_Offset;
				else
				{
					size_t payload_Size = entry.Count * exif_Sizes[(c_int)entry.Type];
					total_Size += Base_Tag_Size + (payload_Size > 4 ? payload_Size : 0);
				}
			}

			return total_Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Write_Ifd(IClass logCtx, PutByteContext pb, c_int le, c_int depth, AvExifMetadata ifd)//XX 644
		{
			c_int ret, tell2;

			c_int tell = ByteStream.ByteStream2_Tell_P(pb);

			TPut16(pb, le, (uint16_t)ifd.Count);

			c_int offset = (c_int)(tell + Ifd_Extra_Size + (Base_Tag_Size * ifd.Count));

			Log.Av_Log(logCtx, Log.Av_Log_Debug, "writing IFD with %u entries and initial offset %d\n", ifd.Count, offset);

			for (size_t i = 0; i < ifd.Count; i++)
			{
				AvExifEntry entry = ifd.Entries[i];

				Log.Av_Log(logCtx, Log.Av_Log_Debug, "writing TIFF entry: id: 0x%04x type: %d, count: %u, offset: %d, offset value: %d\n", entry.Id, entry.Type, entry.Count, ByteStream.ByteStream2_Tell_P(pb), offset);

				TPut16(pb, le, entry.Id);

				if ((entry.Id == MakerNote_Tag) && (entry.Type == AvTiffDataType.Ifd))
				{
					size_t ifd_Size = Exif_Get_Ifd_Size(entry.Value.Ifd);

					TPut16(pb, le, (uint16_t)AvTiffDataType.Undefined);
					TPut32(pb, le, (uint32_t)ifd_Size);
				}
				else
				{
					TPut16(pb, le, (uint16_t)entry.Type);
					TPut32(pb, le, entry.Count);
				}

				if (entry.Type == AvTiffDataType.Ifd)
				{
					TPut32(pb, le, (uint32_t)offset);

					tell2 = ByteStream.ByteStream2_Tell_P(pb);
					ByteStream.ByteStream2_Seek_P(pb, offset, SeekOrigin.Begin);

					if (entry.Ifd_Offset != 0)
						ByteStream.ByteStream2_Put_Buffer(pb, entry.Ifd_Lead, entry.Ifd_Offset);

					ret = Exif_Write_Ifd(logCtx, pb, le, depth + 1, entry.Value.Ifd);

					if (ret < 0)
						return ret;

					offset += (c_int)(ret + entry.Ifd_Offset);
					ByteStream.ByteStream2_Seek_P(pb, tell2, SeekOrigin.Begin);
				}
				else
				{
					size_t payload_Size = entry.Count * exif_Sizes[(c_int)entry.Type];

					if (payload_Size > 4)
					{
						TPut32(pb, le, (uint32_t)offset);

						tell2 = ByteStream.ByteStream2_Tell_P(pb);
						ByteStream.ByteStream2_Seek_P(pb, offset, SeekOrigin.Begin);

						Exif_Write_Values(pb, le, entry);

						offset += (c_int)payload_Size;
						ByteStream.ByteStream2_Seek_P(pb, tell2, SeekOrigin.Begin);
					}
					else
					{
						// Zero uninitialized excess payload values
						IntReadWrite.Av_WN32(pb.Buffer, 0);

						Exif_Write_Values(pb, le, entry);

						ByteStream.ByteStream2_Seek_P(pb, (c_int)(4 - payload_Size), SeekOrigin.Current);
					}
				}
			}

			// We write 0 if this is the top-level exif IFD
			// indicating that there are no more IFD pointers
			TPut32(pb, le, (uint32_t)(depth != 0 ? offset : 0));

			return offset - tell;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static string Column_Sep(uint32_t i, c_int c)//XX 824
		{
			return i != 0 ? (i % c) != 0 ? ", " : "\n" : string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Ifd_To_Dict(IClass logCtx, CPointer<char> prefix, AvExifMetadata ifd, ref AvDictionary metadata)//XX 826
		{
			c_int ret = 0;
			CPointer<char> key = null;
			CPointer<char> value = null;

			if (prefix.IsNull)
				prefix = CString.Empty;

			for (uint16_t i = 0; i < ifd.Count; i++)
			{
				AvExifEntry entry = ifd.Entries[i];
				CPointer<char> name = Av_Exif_Get_Tag_Name(entry.Id);

				BPrint.Av_BPrint_Init(out AVBPrint bp, entry.Count * 10, BPrint.Av_BPrint_Size_Unlimited);

				if (prefix[0] != '\0')
					BPrint.Av_BPrintf(bp, "%s/", prefix);

				if (name.IsNotNull)
					BPrint.Av_BPrintf(bp, "%s", name);
				else
					BPrint.Av_BPrintf(bp, "0x%04X", entry.Id);

				ret = BPrint.Av_BPrint_Finalize(bp, out key);

				if (ret < 0)
					goto End;

				BPrint.Av_BPrint_Init(out bp, entry.Count * 10, BPrint.Av_BPrint_Size_Unlimited);

				switch (entry.Type)
				{
					case AvTiffDataType.Ifd:
					{
						ret = Exif_Ifd_To_Dict(logCtx, key, entry.Value.Ifd, ref metadata);

						if (ret < 0)
							goto End;

						break;
					}

					case AvTiffDataType.Short:
					case AvTiffDataType.Long:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%7u", Column_Sep(j, 8), (uint32_t)entry.Value.UInt[j]);

						break;
					}

					case AvTiffDataType.SShort:
					case AvTiffDataType.SLong:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%7d", Column_Sep(j, 8), (int32_t)entry.Value.SInt[j]);

						break;
					}

					case AvTiffDataType.Rational:
					case AvTiffDataType.SRational:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%7i:%-7i", Column_Sep(j, 4), entry.Value.Rat[j].Num, entry.Value.Rat[j].Den);

						break;
					}

					case AvTiffDataType.Double:
					case AvTiffDataType.Float:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%.15g", Column_Sep(j, 4), entry.Value.Dbl[j]);

						break;
					}

					case AvTiffDataType.String:
					{
						BPrint.Av_BPrintf(bp, "%s", entry.Value.Str);
						break;
					}

					case AvTiffDataType.Undefined:
					case AvTiffDataType.Byte:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%3i", Column_Sep(j, 16), entry.Value.UBytes[j]);

						break;
					}

					case AvTiffDataType.SByte:
					{
						for (uint32_t j = 0; j < entry.Count; j++)
							BPrint.Av_BPrintf(bp, "%s%3i", Column_Sep(j, 16), entry.Value.SBytes[j]);

						break;
					}
				}

				if (entry.Type != AvTiffDataType.Ifd)
				{
					if (BPrint.Av_BPrint_Is_Complete(bp) == 0)
					{
						BPrint.Av_BPrint_Finalize(bp, out _);

						ret = Error.ENOMEM;

						goto End;
					}

					ret = BPrint.Av_BPrint_Finalize(bp, out value);

					if (ret < 0)
						goto End;

					ret = Dict.Av_Dict_Set(ref metadata, key, value, AvDict.Dont_Strdup_Key | AvDict.Dont_Strdup_Val);

					key.SetToNull();
					value.SetToNull();

					if (ret < 0)
						goto End;
				}
				else
					Mem.Av_FreeP(ref key);
			}

			End:
			Mem.Av_FreeP(ref key);
			Mem.Av_FreeP(ref value);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Get_Entry(IClass logCtx, AvExifMetadata ifd, uint16_t id, c_int depth, out AvExifEntry value)//XX 1026
		{
			value = null;

			c_int offset = 1;

			if ((ifd == null) || (ifd.Count != 0) && ifd.Entries.IsNull)
				return Error.EINVAL;

			for (size_t i = 0; i < ifd.Count; i++)
			{
				if (ifd.Entries[i].Id == id)
				{
					value = ifd.Entries[i];

					return (c_int)i + offset;
				}

				if (ifd.Entries[i].Type == AvTiffDataType.Ifd)
				{
					if (depth < 3)
					{
						c_int ret = Exif_Get_Entry(logCtx, ifd.Entries[i].Value.Ifd, id, depth + 1, out value);

						if (ret != 0)
							return ret < 0 ? ret : ret + offset;
					}

					offset += (c_int)ifd.Entries[i].Value.Ifd.Count;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Exif_Remove_Entry(IClass logCtx, AvExifMetadata ifd, uint16_t id, c_int depth)//XX 1105
		{
			int32_t index = -1;
			c_int ret = 0;

			if ((ifd == null) || (ifd.Count != 0) && ifd.Entries.IsNull)
				return Error.EINVAL;

			for (size_t i = 0; i < ifd.Count; i++)
			{
				if (ifd.Entries[i].Id == id)
				{
					index = (int32_t)i;
					break;
				}

				if ((ifd.Entries[i].Type == AvTiffDataType.Ifd) && (depth < 3))
				{
					ret = Exif_Remove_Entry(logCtx, ifd.Entries[i].Value.Ifd, id, depth + 1);

					if (ret != 0)
						return ret;
				}
			}

			if (index < 0)
				return 0;

			Exif_Free_Entry(ifd.Entries[index]);

			if (index == --ifd.Count)
			{
				if (index == 0)
					Mem.Av_FreeP(ref ifd.Entries);

				return 1;
			}

			CMemory.memmove(ifd.Entries + index, ifd.Entries + index + 1, (size_t)(ifd.Count - index));

			return (c_int)(1 + ifd.Count - index);
		}
		#endregion
	}
}
