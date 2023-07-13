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
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;
using Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestEncoder
	{
		private enum Layer
		{
			/// <summary>
			/// Flac__Stream_Encoder_Init_Stream() without seeking
			/// </summary>
			Stream,

			/// <summary>
			/// Flac__Stream_Encoder_Init_Stream() with seeking
			/// </summary>
			Seekable_Stream,

			/// <summary>
			/// Flac__Stream_Encoder_Init_File() (.NET stream)
			/// </summary>
			File,

			/// <summary>
			/// Flac__Stream_Encoder_Init_File()
			/// </summary>
			FileName
		}

		private Flac__StreamMetadata streamInfo;
		private Flac__StreamMetadata padding;
		private Flac__StreamMetadata seekTable;
		private Flac__StreamMetadata application1;
		private Flac__StreamMetadata application2;
		private Flac__StreamMetadata vorbisComment;
		private Flac__StreamMetadata cueSheet;
		private Flac__StreamMetadata picture;
		private Flac__StreamMetadata unknown;

		private Flac__StreamMetadata[] metadata_Sequence;
		private uint32_t num_Metadata;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestInitialize]
		public void Initialize()
		{
			Init_Metadata_Blocks();

			metadata_Sequence = new[]
			{
				vorbisComment, padding, seekTable, application1, application2, cueSheet, picture, unknown
			};
			num_Metadata = (uint32_t)metadata_Sequence.Length;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Encoder_Encode_Stream()
		{
			Test_Stream_Encoder(Layer.Stream);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Encoder_Encode_SeekableStream()
		{
			Test_Stream_Encoder(Layer.Seekable_Stream);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Encoder_Encode_File()
		{
			Test_Stream_Encoder(Layer.File);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Stream_Encoder_Encode_FileName()
		{
			Test_Stream_Encoder(Layer.FileName);
		}

		#region Private methods
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
				return Path.Combine(location, "metadata_out.flac");
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Init_Metadata_Blocks()
		{
			Metadata_Utils.Init_Metadata_Blocks(out streamInfo, out padding, out seekTable, out application1, out application2, out vorbisComment, out cueSheet, out picture, out unknown);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Test_Stream_Encoder(Layer layer)
		{
			Console.WriteLine("Testing Flac__Stream_Encoder_New()");
			Stream_Encoder encoder = Stream_Encoder.Flac__Stream_Encoder_New();
			Assert.IsNotNull(encoder);

			Flac__StreamMetadata_StreamInfo metaStreamInfo = (Flac__StreamMetadata_StreamInfo)streamInfo.Data;

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Streamable_Subset()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Streamable_Subset(true));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Channels()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Channels(metaStreamInfo.Channels));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Bits_Per_Sample()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Bits_Per_Sample(metaStreamInfo.Bits_Per_Sample));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Sample_Rate()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Sample_Rate(metaStreamInfo.Sample_Rate));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Compression_Level()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Compression_Level(uint32_t.MaxValue));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_BlockSize()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_BlockSize(metaStreamInfo.Min_BlockSize));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Do_Mid_Side_Stereo(false));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Loose_Mid_Side_Stereo(false));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Max_Lpc_Order()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Max_Lpc_Order(0));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Qlp_Coeff_Precision()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Qlp_Coeff_Precision(0));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Do_Qlp_Coeff_Prec_Search(false));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Do_Exhaustive_Model_Search(false));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Min_Residual_Partition_Order()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Min_Residual_Partition_Order(0));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Max_Residual_Partition_Order()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Max_Residual_Partition_Order(0));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Total_Samples_Estimate()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Total_Samples_Estimate(metaStreamInfo.Total_Samples));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Metadata()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Metadata(metadata_Sequence, num_Metadata));

			Console.WriteLine("Testing Flac__Stream_Encoder_Set_Limit_Min_Bitrate()");
			Assert.IsTrue(encoder.Flac__Stream_Encoder_Set_Limit_Min_Bitrate(true));

			FileStream file = null;

			if (layer != Layer.FileName)
			{
				Console.WriteLine("Open file for FLAC output");
				file = File.Create(FlacFileName);
			}

			try
			{
				Flac__StreamEncoderInitStatus init_Status;

				switch (layer)
				{
					case Layer.Stream:
					{
						Console.WriteLine("Testing Flac__Stream_Encoder_Init_Stream()");
						init_Status = encoder.Flac__Stream_Encoder_Init_Stream(Stream_Encoder_Write_Callback, null, null, Stream_Encoder_Metadata_Callback, file);
						break;
					}

					case Layer.Seekable_Stream:
					{
						Console.WriteLine("Testing Flac__Stream_Encoder_Init_Stream()");
						init_Status = encoder.Flac__Stream_Encoder_Init_Stream(Stream_Encoder_Write_Callback, Stream_Encoder_Seek_Callback, Stream_Encoder_Tell_Callback, null, file);
						break;
					}

					case Layer.File:
					{
						Console.WriteLine("Testing Flac__Stream_Encoder_Init_File()");
						init_Status = encoder.Flac__Stream_Encoder_Init_File(file, false, Stream_Encoder_Progress_Callback, null);
						break;
					}

					case Layer.FileName:
					{
						Console.WriteLine("Testing Flac__Stream_Encoder_Init_File()");
						init_Status = encoder.Flac__Stream_Encoder_Init_File(FlacFileName, Stream_Encoder_Progress_Callback, null);
						break;
					}

					default:
						throw new NotImplementedException();
				}

				Assert.AreEqual(Flac__StreamEncoderInitStatus.Ok, init_Status);

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_State()");
				Assert.AreEqual(Flac__StreamEncoderState.Ok, encoder.Flac__Stream_Encoder_Get_State());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Streamable_Subset()");
				Assert.IsTrue(encoder.Flac__Stream_Encoder_Get_Streamable_Subset());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Do_Mid_Side_Stereo()");
				Assert.IsFalse(encoder.Flac__Stream_Encoder_Get_Do_Mid_Side_Stereo());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Loose_Mid_Side_Stereo()");
				Assert.IsFalse(encoder.Flac__Stream_Encoder_Get_Loose_Mid_Side_Stereo());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Channels()");
				Assert.AreEqual(metaStreamInfo.Channels, encoder.Flac__Stream_Encoder_Get_Channels());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Bits_Per_Sample()");
				Assert.AreEqual(metaStreamInfo.Bits_Per_Sample, encoder.Flac__Stream_Encoder_Get_Bits_Per_Sample());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Sample_Rate()");
				Assert.AreEqual(metaStreamInfo.Sample_Rate, encoder.Flac__Stream_Encoder_Get_Sample_Rate());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_BlockSize()");
				Assert.AreEqual(metaStreamInfo.Min_BlockSize, encoder.Flac__Stream_Encoder_Get_BlockSize());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Max_Lpc_Order()");
				Assert.AreEqual(0U, encoder.Flac__Stream_Encoder_Get_Max_Lpc_Order());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Qlp_Coeff_Precision()");
				encoder.Flac__Stream_Encoder_Get_Qlp_Coeff_Precision();		// We asked the encoder to auto select this so we accept anything

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Do_Qlp_Coeff_Prec_Search()");
				Assert.IsFalse(encoder.Flac__Stream_Encoder_Get_Do_Qlp_Coeff_Prec_Search());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Do_Exhaustive_Model_Search()");
				Assert.IsFalse(encoder.Flac__Stream_Encoder_Get_Do_Exhaustive_Model_Search());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Min_Residual_Partition_Order()");
				Assert.AreEqual(0U, encoder.Flac__Stream_Encoder_Get_Min_Residual_Partition_Order());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Max_Residual_Partition_Order()");
				Assert.AreEqual(0U, encoder.Flac__Stream_Encoder_Get_Max_Residual_Partition_Order());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Total_Samples_Estimate()");
				Assert.AreEqual(metaStreamInfo.Total_Samples, encoder.Flac__Stream_Encoder_Get_Total_Samples_Estimate());

				Console.WriteLine("Testing Flac__Stream_Encoder_Get_Limit_Min_Bitrate()");
				Assert.AreEqual(true, encoder.Flac__Stream_Encoder_Get_Limit_Min_Bitrate());

				// Init the dummy sample buffer
				Flac__int32[] samples = new Flac__int32[1024];
				Flac__int32[][] samples_Array = new Flac__int32[][] { samples };

				for (uint32_t i = 0; i < samples.Length; i++)
					samples[i] = (Flac__int32)(i & 7);

				Console.WriteLine("Testing Flac__Stream_Encoder_Process()");
				Assert.IsTrue(encoder.Flac__Stream_Encoder_Process(samples_Array, (uint32_t)samples.Length));

				Console.WriteLine("Testing Flac__Stream_Encoder_Process_Interleaved()");
				Assert.IsTrue(encoder.Flac__Stream_Encoder_Process_Interleaved(samples, (uint32_t)samples.Length));

				Console.WriteLine("Testing Flac__Stream_Encoder_Finish()");
				Assert.IsTrue(encoder.Flac__Stream_Encoder_Finish());
			}
			finally
			{
				if (file != null)
					file.Dispose();
			}

			Console.WriteLine("Testing Flac__Stream_Encoder_Delete()");
			encoder.Flac__Stream_Encoder_Delete();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderWriteStatus Stream_Encoder_Write_Callback(Stream_Encoder _, Span<Flac__byte> buffer, size_t bytes, uint32_t samples, uint32_t current_Frame, object client_Data)
		{
			Stream f = (Stream)client_Data;

			f.Write(buffer.Slice(0, (int)bytes));

			return Flac__StreamEncoderWriteStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderSeekStatus Stream_Encoder_Seek_Callback(Stream_Encoder _, Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			Stream f = (Stream)client_Data;

			try
			{
				f.Seek((long)absolute_Byte_Offset, SeekOrigin.Begin);
			}
			catch(Exception)
			{
				return Flac__StreamEncoderSeekStatus.Error;
			}

			return Flac__StreamEncoderSeekStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__StreamEncoderTellStatus Stream_Encoder_Tell_Callback(Stream_Encoder _, out Flac__uint64 absolute_Byte_Offset, object client_Data)
		{
			Stream f = (Stream)client_Data;

			try
			{
				absolute_Byte_Offset = (Flac__uint64)f.Position;
			}
			catch(Exception)
			{
				absolute_Byte_Offset = 0;
				return Flac__StreamEncoderTellStatus.Error;
			}

			return Flac__StreamEncoderTellStatus.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Encoder_Metadata_Callback(Stream_Encoder encoder, Flac__StreamMetadata metadata, object client_Data)
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Stream_Encoder_Progress_Callback(Stream_Encoder encoder, Flac__uint64 bytes_Written, Flac__uint64 samples_Written, uint32_t frames_Written, uint32_t total_Frame_Estimate, object client_Data)
		{
		}
		#endregion
	}
}
