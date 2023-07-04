/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Decoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;
using Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestDecoder
	{
		private enum Layer
		{
			/// <summary>
			/// Flac__Stream_Decoder_Init_Stream() without seeking
			/// </summary>
			Stream,

			/// <summary>
			/// Flac__Stream_Decoder_Init_Stream() with seeking
			/// </summary>
			Seekable_Stream,

			/// <summary>
			/// Flac__Stream_Decoder_Init_File() (.NET stream)
			/// </summary>
			File,

			/// <summary>
			/// Flac__Stream_Decoder_Init_File()
			/// </summary>
			FileName
		}

		private class StreamDecoderClientData
		{
			public Layer Layer;
			public Stream File;
			public uint32_t Current_Metadata_Number;
			public Flac__bool Ignore_Errors;
			public Flac__bool Error_Occurred;
		}

		private uint32_t num_Expected;
		private readonly Flac__StreamMetadata[] expected_Metadata_Sequence = new Flac__StreamMetadata[9];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_New_Delete()
		{
			Console.WriteLine("Testing Flac__Stream_Decoder_New()");
			Stream_Decoder decoder = Stream_Decoder.Flac__Stream_Decoder_New();
			Assert.IsNotNull(decoder);

			Console.WriteLine("Testing Flac__Stream_Decoder_Delete()");
			decoder.Flac__Stream_Decoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_New_Init_Delete_Stream()
		{
			Console.WriteLine("Testing Flac__Stream_Decoder_New()");
			Stream_Decoder decoder = Stream_Decoder.Flac__Stream_Decoder_New();
			Assert.IsNotNull(decoder);

			Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
			Flac__StreamDecoderInitStatus init_Status = decoder.Flac__Stream_Decoder_Init_Stream(null, null, null, null, null, null, null, null, null);
			Assert.AreEqual(Flac__StreamDecoderInitStatus.Invalid_Callbacks, init_Status);

			Console.WriteLine("Testing Flac__Stream_Decoder_Delete()");
			decoder.Flac__Stream_Decoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_New_Init_Delete_File_Stream()
		{
			Console.WriteLine("Testing Flac__Stream_Decoder_New()");
			Stream_Decoder decoder = Stream_Decoder.Flac__Stream_Decoder_New();
			Assert.IsNotNull(decoder);

			Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");

			using (MemoryStream ms = new MemoryStream())
			{
				Flac__StreamDecoderInitStatus init_Status = decoder.Flac__Stream_Decoder_Init_File(ms, true, null, null, null, null);
				Assert.AreEqual(Flac__StreamDecoderInitStatus.Invalid_Callbacks, init_Status);
			}

			Console.WriteLine("Testing Flac__Stream_Decoder_Delete()");
			decoder.Flac__Stream_Decoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_New_Init_Delete_File()
		{
			Console.WriteLine("Testing Flac__Stream_Decoder_New()");
			Stream_Decoder decoder = Stream_Decoder.Flac__Stream_Decoder_New();
			Assert.IsNotNull(decoder);

			Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
			Flac__StreamDecoderInitStatus init_Status = decoder.Flac__Stream_Decoder_Init_File(FlacFileName, null, null, null, null);
			Assert.AreEqual(Flac__StreamDecoderInitStatus.Invalid_Callbacks, init_Status);

			Console.WriteLine("Testing Flac__Stream_Decoder_Delete()");
			decoder.Flac__Stream_Decoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_Decode_Stream()
		{
			Test_Stream_Decoder(Layer.Stream);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_Decode_SeekableStream()
		{
			Test_Stream_Decoder(Layer.Seekable_Stream);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_Decode_File()
		{
			Test_Stream_Decoder(Layer.File);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Decoder_Decode_FileName()
		{
			Test_Stream_Decoder(Layer.FileName);
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Stream_Decoder(Layer layer)
		{
			Metadata_Utils.Init_Metadata_Blocks(out Flac__StreamMetadata streamInfo, out Flac__StreamMetadata padding, out Flac__StreamMetadata seekTable, out Flac__StreamMetadata application1, out Flac__StreamMetadata application2, out Flac__StreamMetadata vorbisComment, out Flac__StreamMetadata cueSheet, out Flac__StreamMetadata picture, out Flac__StreamMetadata unknown);

			num_Expected = 0;
			expected_Metadata_Sequence[num_Expected++] = streamInfo;

			Console.WriteLine("Testing Flac__Stream_Decoder_New()");
			Stream_Decoder decoder = Stream_Decoder.Flac__Stream_Decoder_New();
			Assert.IsNotNull(decoder);

			Console.WriteLine("Testing Flac__Stream_Decoder_Set_Md5_Checking()");
			Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Md5_Checking(true));

			StreamDecoderClientData decoder_Client_Data = new StreamDecoderClientData();
			decoder_Client_Data.Layer = layer;

			if (layer != Layer.FileName)
			{
				Console.WriteLine("Open FLAC file");
				Open_Test_File(decoder_Client_Data);
				Assert.IsNotNull(decoder_Client_Data.File);
			}

			try
			{
				Flac__StreamDecoderInitStatus init_Status;

				switch (layer)
				{
					case Layer.Stream:
					{
						Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
						init_Status = decoder.Flac__Stream_Decoder_Init_Stream(Stream_Decoder_Read_Callback, null, null, null, null, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, decoder_Client_Data);
						break;
					}

					case Layer.Seekable_Stream:
					{
						Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
						init_Status = decoder.Flac__Stream_Decoder_Init_Stream(Stream_Decoder_Read_Callback, Stream_Decoder_Seek_Callback, Stream_Decoder_Tell_Callback, Stream_Decoder_Length_Callback, Stream_Decoder_Eof_Callback, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, decoder_Client_Data);
						break;
					}

					case Layer.File:
					{
						Console.WriteLine("Testing Flac__Stream_Decoder_Init_File()");
						init_Status = decoder.Flac__Stream_Decoder_Init_File(decoder_Client_Data.File, false, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, decoder_Client_Data);
						break;
					}

					case Layer.FileName:
					{
						Console.WriteLine("Testing Flac__Stream_Decoder_Init_File()");
						init_Status = decoder.Flac__Stream_Decoder_Init_File(FlacFileName, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, decoder_Client_Data);
						break;
					}

					default:
						throw new NotImplementedException();
				}

				Assert.AreEqual(Flac__StreamDecoderInitStatus.Ok, init_Status);

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_State()");
				Assert.AreEqual(Flac__StreamDecoderState.Search_For_Metadata, decoder.Flac__Stream_Decoder_Get_State());

				decoder_Client_Data.Current_Metadata_Number = 0;
				decoder_Client_Data.Ignore_Errors = false;
				decoder_Client_Data.Error_Occurred = false;

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_Md5_Checking()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Get_Md5_Checking());

				Console.WriteLine("Testing Flac__Stream_Decoder_Process_Until_End_Of_Metadata()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Until_End_Of_Metadata());

				Console.WriteLine("Testing Flac__Stream_Decoder_Process_Single()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Single());

				Console.WriteLine("Testing Flac__Stream_Decoder_Skip_Single_Frame()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Skip_Single_Frame());

				if ((layer == Layer.Stream) || (layer == Layer.Seekable_Stream))
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Flush()");
					Assert.IsTrue(decoder.Flac__Stream_Decoder_Flush());

					decoder_Client_Data.Ignore_Errors = true;

					Console.WriteLine("Testing Flac__Stream_Decoder_Process_Single()");
					Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Single());

					decoder_Client_Data.Ignore_Errors = false;
				}

				Flac__bool expect = layer != Layer.Stream;
				Console.WriteLine("Testing Flac__Stream_Decoder_Seek_Absolute()");
				Assert.AreEqual(expect, decoder.Flac__Stream_Decoder_Seek_Absolute(0));

				Console.WriteLine("Testing Flac__Stream_Decoder_Process_Until_End_Of_Stream()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Until_End_Of_Stream());

				expect = layer != Layer.Stream;
				Console.WriteLine("Testing Flac__Stream_Decoder_Seek_Absolute()");
				Assert.AreEqual(expect, decoder.Flac__Stream_Decoder_Seek_Absolute(0));

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_Channels()");
				Assert.AreEqual(((Flac__StreamMetadata_StreamInfo)streamInfo.Data).Channels, decoder.Flac__Stream_Decoder_Get_Channels());

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_Bits_Per_Sample()");
				Assert.AreEqual(((Flac__StreamMetadata_StreamInfo)streamInfo.Data).Bits_Per_Sample, decoder.Flac__Stream_Decoder_Get_Bits_Per_Sample());

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_Sample_Rate()");
				Assert.AreEqual(((Flac__StreamMetadata_StreamInfo)streamInfo.Data).Sample_Rate, decoder.Flac__Stream_Decoder_Get_Sample_Rate());

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_BlockSize()");
				// Value could be anything since we're at the last block, so accept any reasonable answer
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Get_BlockSize() > 0);

				Console.WriteLine("Testing Flac__Stream_Decoder_Get_Channel_Assignment()");
				decoder.Flac__Stream_Decoder_Get_Channel_Assignment();

				if ((layer == Layer.Stream) || (layer == Layer.Seekable_Stream))
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Reset()");
					Assert.IsTrue(decoder.Flac__Stream_Decoder_Reset());

					if (layer == Layer.Stream)
					{
						// After a reset, we have to rewind the input ourselves
						Console.WriteLine("Rewinding input");
						decoder_Client_Data.File.Seek(0, SeekOrigin.Begin);
					}

					decoder_Client_Data.Current_Metadata_Number = 0;

					Console.WriteLine("Testing Flac__Stream_Decoder_Process_Until_End_Of_Stream()");
					Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Until_End_Of_Stream());
				}

				Console.WriteLine("Testing Flac__Stream_Decoder_Finish()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Finish());

				//
				// Respond all
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = application1;
				expected_Metadata_Sequence[num_Expected++] = application2;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				num_Expected = 0;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Respond all, ignore VORBIS_COMMENT
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore(VORBIS_COMMENT)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore(Flac__MetadataType.Vorbis_Comment));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = application1;
				expected_Metadata_Sequence[num_Expected++] = application2;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Respond all, ignore APPLICATION
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore(APPLICATION)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore(Flac__MetadataType.Application));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Respond all, ignore APPLICATION id of app#1
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = application2;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Respond all, ignore APPLICATION id of app#1 & app#2
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_Application(of app block #2)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_Application(((Flac__StreamMetadata_Application)application2.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all, respond VORBIS_COMMENT
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond(VORBIS_COMMENT)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType.Vorbis_Comment));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all, respond APPLICATION
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond(APPLICATION)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType.Application));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = application1;
				expected_Metadata_Sequence[num_Expected++] = application2;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all, respond APPLICATION id of app#1
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = application1;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all, respond APPLICATION id of app#1 & app#2
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_Application(of app block #2)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_Application(((Flac__StreamMetadata_Application)application2.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = application1;
				expected_Metadata_Sequence[num_Expected++] = application2;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Respond all, ignore APPLICATION, respond APPLICATION id of app#1
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore(APPLICATION)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore(Flac__MetadataType.Application));

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = streamInfo;
				expected_Metadata_Sequence[num_Expected++] = padding;
				expected_Metadata_Sequence[num_Expected++] = seekTable;
				expected_Metadata_Sequence[num_Expected++] = application1;
				expected_Metadata_Sequence[num_Expected++] = vorbisComment;
				expected_Metadata_Sequence[num_Expected++] = cueSheet;
				expected_Metadata_Sequence[num_Expected++] = picture;
				expected_Metadata_Sequence[num_Expected++] = unknown;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);

				//
				// Ignore all, respond APPLICATION, ignore APPLICATION id of app#1
				//
				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_All()");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_All());

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Respond(APPLICATION)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Respond(Flac__MetadataType.Application));

				Console.WriteLine("Testing Flac__Stream_Decoder_Set_Metadata_Ignore_Application(of app block #1)");
				Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Metadata_Ignore_Application(((Flac__StreamMetadata_Application)application1.Data).Id));

				num_Expected = 0;
				expected_Metadata_Sequence[num_Expected++] = application2;

				Stream_Decoder_Test_Respond(decoder, decoder_Client_Data);
			}
			finally
			{
				if ((layer == Layer.Stream) || (layer == Layer.Seekable_Stream))
					decoder_Client_Data.File.Dispose();
			}

			Console.WriteLine("Testing Flac__Stream_Decoder_Delete()");
			decoder.Flac__Stream_Decoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private string FlacFileName
		{
			get
			{
				string location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
				return Path.Combine(location, "TestData", "metadata.flac");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Open_Test_File(StreamDecoderClientData pdcd)
		{
			pdcd.File = File.OpenRead(FlacFileName);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderReadStatus Stream_Decoder_Read_Callback(Stream_Decoder _, Span<Flac__byte> buffer, ref size_t bytes, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;
			size_t requested_Bytes = bytes;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in read callback is NULL");
				return Flac__StreamDecoderReadStatus.Abort;
			}

			if (dcd.Error_Occurred)
				return Flac__StreamDecoderReadStatus.Abort;

			if (dcd.File.Position >= dcd.File.Length)
			{
				bytes = 0;
				return Flac__StreamDecoderReadStatus.End_Of_Stream;
			}
			else if (requested_Bytes > 0)
			{
				bytes = (size_t)dcd.File.Read(buffer.Slice(0, (int)requested_Bytes));
				if (bytes == 0)
				{
					if (dcd.File.Position >= dcd.File.Length)
						return Flac__StreamDecoderReadStatus.End_Of_Stream;
					else
						return Flac__StreamDecoderReadStatus.Abort;
				}
				else
					return Flac__StreamDecoderReadStatus.Continue;
			}
			else
				return Flac__StreamDecoderReadStatus.Abort;		// Abort to avoid a deadlock
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderSeekStatus Stream_Decoder_Seek_Callback(Stream_Decoder _, Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in seek callback is NULL");
				return Flac__StreamDecoderSeekStatus.Error;
			}

			if (dcd.Error_Occurred)
				return Flac__StreamDecoderSeekStatus.Error;

			try
			{
				dcd.File.Seek((long)absolute_Byte_Offset, SeekOrigin.Begin);
			}
			catch(Exception)
			{
				dcd.Error_Occurred = true;
				return Flac__StreamDecoderSeekStatus.Error;
			}

			return Flac__StreamDecoderSeekStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderTellStatus Stream_Decoder_Tell_Callback(Stream_Decoder _, out Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;
			absolute_Byte_Offset = 0;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in tell callback is NULL");
				return Flac__StreamDecoderTellStatus.Error;
			}

			if (dcd.Error_Occurred)
				return Flac__StreamDecoderTellStatus.Error;

			try
			{
				absolute_Byte_Offset = (Flac__uint64)dcd.File.Position;
			}
			catch(Exception)
			{
				dcd.Error_Occurred = true;
				return Flac__StreamDecoderTellStatus.Error;
			}

			return Flac__StreamDecoderTellStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderLengthStatus Stream_Decoder_Length_Callback(Stream_Decoder _, out Flac__uint64 stream_Length, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;
			stream_Length = 0;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in length callback is NULL");
				return Flac__StreamDecoderLengthStatus.Error;
			}

			if (dcd.Error_Occurred)
				return Flac__StreamDecoderLengthStatus.Error;

			try
			{
				stream_Length = (Flac__uint64)dcd.File.Length;
			}
			catch(Exception)
			{
				dcd.Error_Occurred = true;
				return Flac__StreamDecoderLengthStatus.Error;
			}

			return Flac__StreamDecoderLengthStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__bool Stream_Decoder_Eof_Callback(Stream_Decoder _, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in eof callback is NULL");
				return true;
			}

			if (dcd.Error_Occurred)
				return true;

			try
			{
				return dcd.File.Position >= dcd.File.Length;
			}
			catch(Exception)
			{
				dcd.Error_Occurred = true;
				return true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamDecoderWriteStatus Stream_Decoder_Write_Callback(Stream_Decoder _, Flac__Frame frame, Flac__int32[][] __, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in write callback is NULL");
				return Flac__StreamDecoderWriteStatus.Abort;
			}

			if (dcd.Error_Occurred)
				return Flac__StreamDecoderWriteStatus.Abort;

			if (((frame.Header.Number_Type == Flac__FrameNumberType.Frame_Number) && (frame.Header.Frame_Number == 0)) || ((frame.Header.Number_Type == Flac__FrameNumberType.Sample_Number) && (frame.Header.Sample_Number == 0)))
				Console.WriteLine("Content...");

			return Flac__StreamDecoderWriteStatus.Continue;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Decoder_Metadata_Callback(Stream_Decoder _, Flac__StreamMetadata metadata, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in metadata callback is NULL");
				return;
			}

			if (dcd.Error_Occurred)
				return;

			if (metadata.Type == Flac__MetadataType.Application)
			{
				Flac__StreamMetadata_Application appData = (Flac__StreamMetadata_Application)metadata.Data;
				Console.WriteLine("{0} ('{1}{2}{3}{4}')", dcd.Current_Metadata_Number, appData.Id[0], appData.Id[1], appData.Id[2], appData.Id[3]);
			}
			else
				Console.WriteLine(dcd.Current_Metadata_Number);

			if (dcd.Current_Metadata_Number >= num_Expected)
			{
				Console.WriteLine("Got more metadata blocks than expected");
				dcd.Error_Occurred = true;
			}
			else
			{
				if (!Metadata_Utils.Compare_Block(expected_Metadata_Sequence[dcd.Current_Metadata_Number], metadata))
				{
					Console.WriteLine("Metadata block mismatch");
					dcd.Error_Occurred = true;
				}
			}

			dcd.Current_Metadata_Number++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Decoder_Error_Callback(Stream_Decoder decoder, Flac__StreamDecoderErrorStatus status, object client_Data)
		{
			StreamDecoderClientData dcd = (StreamDecoderClientData)client_Data;

			if (dcd == null)
			{
				Console.WriteLine("ERROR: client_Data in error callback is NULL");
				return;
			}

			if (!dcd.Ignore_Errors)
			{
				Console.WriteLine($"ERROR: Got error callback: {status}");
				dcd.Error_Occurred = true;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Decoder_Test_Respond(Stream_Decoder decoder, StreamDecoderClientData dcd)
		{
			Assert.IsTrue(decoder.Flac__Stream_Decoder_Set_Md5_Checking(true));

			// For Flac__Stream_Decoder_Init_File, the Flac__Stream_Decoder_Finish() closes the file, so we have to keep re-opening
			if (dcd.Layer == Layer.File)
			{
				Console.WriteLine("Open FLAC file");
				Open_Test_File(dcd);
				Assert.IsNotNull(dcd.File);
			}

			Flac__StreamDecoderInitStatus init_Status;

			switch (dcd.Layer)
			{
				case Layer.Stream:
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
					init_Status = decoder.Flac__Stream_Decoder_Init_Stream(Stream_Decoder_Read_Callback, null, null, null, null, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, dcd);
					break;
				}

				case Layer.Seekable_Stream:
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Init_Stream()");
					init_Status = decoder.Flac__Stream_Decoder_Init_Stream(Stream_Decoder_Read_Callback, Stream_Decoder_Seek_Callback, Stream_Decoder_Tell_Callback, Stream_Decoder_Length_Callback, Stream_Decoder_Eof_Callback, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, dcd);
					break;
				}

				case Layer.File:
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Init_File()");
					init_Status = decoder.Flac__Stream_Decoder_Init_File(dcd.File, false, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, dcd);
					break;
				}

				case Layer.FileName:
				{
					Console.WriteLine("Testing Flac__Stream_Decoder_Init_File()");
					init_Status = decoder.Flac__Stream_Decoder_Init_File(FlacFileName, Stream_Decoder_Write_Callback, Stream_Decoder_Metadata_Callback, Stream_Decoder_Error_Callback, dcd);
					break;
				}

				default:
					throw new NotImplementedException();
			}

			Assert.AreEqual(Flac__StreamDecoderInitStatus.Ok, init_Status);

			dcd.Current_Metadata_Number = 0;

			if ((dcd.Layer == Layer.Stream) || (dcd.Layer == Layer.Seekable_Stream))
				dcd.File.Seek(0, SeekOrigin.Begin);

			Console.WriteLine("Testing Flac__Stream_Decoder_Process_Until_End_Of_Stream()");
			Assert.IsTrue(decoder.Flac__Stream_Decoder_Process_Until_End_Of_Stream());

			Console.WriteLine("Testing Flac__Stream_Decoder_Finish()");
			Assert.IsTrue(decoder.Flac__Stream_Decoder_Finish());
		}
		#endregion
	}
}
