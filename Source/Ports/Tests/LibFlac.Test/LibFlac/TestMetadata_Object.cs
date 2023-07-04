/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Format;
using Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.Common;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestMetadata_Object
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_StreamInfo()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.StreamInfo);
			Assert.IsNotNull(block);
			Assert.AreEqual(Constants.Flac__Stream_Metadata_StreamInfo_Length, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_StreamInfo));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_StreamInfo));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Padding()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Padding);
			Assert.IsNotNull(block);
			Assert.AreEqual(0U, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_Padding));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Padding));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Application()
		{
			Flac__byte[] dummyData = { 0x61, 0x62, 0x63, 0x64 };

			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Application);
			Assert.IsNotNull(block);
			Assert.AreEqual(Constants.Flac__Stream_Metadata_Application_Id_Len / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_Application));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Application));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_Application_Set_Data(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Application_Set_Data(block, dummyData, (uint32_t)dummyData.Length, true));
			Assert.AreEqual((Constants.Flac__Stream_Metadata_Application_Id_Len / 8) + dummyData.Length, block.Length);
			CollectionAssert.AreEqual(dummyData, ((Flac__StreamMetadata_Application)block.Data).Data);

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Application));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_Application_Set_Data(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Application_Set_Data(block, Make_DummyData(dummyData, (uint32_t)dummyData.Length), (uint32_t)dummyData.Length, false));
			Assert.AreEqual((Constants.Flac__Stream_Metadata_Application_Id_Len / 8) + dummyData.Length, block.Length);
			CollectionAssert.AreEqual(dummyData, ((Flac__StreamMetadata_Application)block.Data).Data);

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Application));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_SeekTable()
		{
			Flac__StreamMetadata_SeekPoint[] seekPoint_Array = new Flac__StreamMetadata_SeekPoint[14];

			for (int i = 0; i < seekPoint_Array.Length; i++)
			{
				seekPoint_Array[i] = new Flac__StreamMetadata_SeekPoint();
				seekPoint_Array[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
				seekPoint_Array[i].Stream_Offset = 0;
				seekPoint_Array[i].Frame_Samples = 0;
			}

			uint32_t seekPoints = 0;

			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.SeekTable);
			Check_SeekTable(block, seekPoints, null);

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_SeekTable));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			seekPoints = 2;
			Console.WriteLine($"Testing Flac__Metadata_SeekTable_Resize_Points(grow to {seekPoints})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Resize_Points(block, seekPoints));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoints = 1;
			Console.WriteLine($"Testing Flac__Metadata_SeekTable_Resize_Points(shrink to {seekPoints})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Resize_Points(block, seekPoints));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Is_Legal()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Is_Legal(block));

			seekPoints = 0;
			Console.WriteLine($"Testing Flac__Metadata_SeekTable_Resize_Points(shrink to {seekPoints})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Resize_Points(block, seekPoints));
			Check_SeekTable(block, seekPoints, null);

			seekPoints++;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Insert_Point() on empty array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Insert_Point(block, 0, seekPoint_Array[0]));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[0].Sample_Number = 1;
			seekPoints++;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Insert_Point() on beginning of non-empty array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Insert_Point(block, 0, seekPoint_Array[0]));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[1].Sample_Number = 2;
			seekPoints++;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Insert_Point() on middle of non-empty array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Insert_Point(block, 1, seekPoint_Array[1]));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[3].Sample_Number = 3;
			seekPoints++;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Insert_Point() on end of non-empty array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Insert_Point(block, 3, seekPoint_Array[3]));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_SeekTable));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			seekPoint_Array[2].Sample_Number = seekPoint_Array[3].Sample_Number;
			seekPoints--;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Delete_Point() on middle of array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Delete_Point(block, 2));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoints--;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Delete_Point() on end of array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Delete_Point(block, 2));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoints--;
			Console.WriteLine("Testing Flac__Metadata_SeekTable_Delete_Point() on beginning of array");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Delete_Point(block, 0));
			Check_SeekTable(block, seekPoints, seekPoint_Array.AsSpan(1).ToArray());

			Console.WriteLine("Testing Flac__Metadata_SeekTable_Set_Point()");
			Metadata_Object.Flac__Metadata_Object_SeekTable_Set_Point(block, 0, seekPoint_Array[0]);
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_SeekTable_Template()
		{
			Flac__StreamMetadata_SeekPoint[] seekPoint_Array = new Flac__StreamMetadata_SeekPoint[14];

			for (int i = 0; i < seekPoint_Array.Length; i++)
			{
				seekPoint_Array[i] = new Flac__StreamMetadata_SeekPoint();
				seekPoint_Array[i].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
				seekPoint_Array[i].Stream_Offset = 0;
				seekPoint_Array[i].Frame_Samples = 0;
			}

			uint32_t seekPoints = 0;

			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.SeekTable);
			Check_SeekTable(block, seekPoints, null);

			seekPoints += 2;
			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Placeholders()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Placeholders(block, 2));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[seekPoints++].Sample_Number = 7;
			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Point()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Point(block, 7));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			{
				Flac__uint64[] nums = { 3, 7 };
				seekPoint_Array[seekPoints++].Sample_Number = nums[0];
				seekPoint_Array[seekPoints++].Sample_Number = nums[1];

				Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Points()");
				Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Points(block, nums, (uint32_t)nums.Length));
				Check_SeekTable(block, seekPoints, seekPoint_Array);
			}

			seekPoint_Array[seekPoints++].Sample_Number = 0;
			seekPoint_Array[seekPoints++].Sample_Number = 10;
			seekPoint_Array[seekPoints++].Sample_Number = 20;

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points(block, 3, 30));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoints--;
			seekPoint_Array[0].Sample_Number = 0;
			seekPoint_Array[1].Sample_Number = 3;
			seekPoint_Array[2].Sample_Number = 7;
			seekPoint_Array[3].Sample_Number = 10;
			seekPoint_Array[4].Sample_Number = 20;
			seekPoint_Array[5].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;
			seekPoint_Array[6].Sample_Number = Constants.Flac__Stream_Metadata_SeekPoint_Placeholder;

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Sort(compact=true)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Sort(block, true));
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Is_Legal(block));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Sort(compact=false)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Sort(block, false));
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Is_Legal(block));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[seekPoints++].Sample_Number = 0;
			seekPoint_Array[seekPoints++].Sample_Number = 10;
			seekPoint_Array[seekPoints++].Sample_Number = 20;

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points_By_Samples()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points_By_Samples(block, 10, 30));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			seekPoint_Array[seekPoints++].Sample_Number = 0;
			seekPoint_Array[seekPoints++].Sample_Number = 11;
			seekPoint_Array[seekPoints++].Sample_Number = 22;

			Console.WriteLine("Testing Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points_By_Samples()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_SeekTable_Template_Append_Spaced_Points_By_Samples(block, 11, 30));
			Check_SeekTable(block, seekPoints, seekPoint_Array);

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Vorbis_Comment_Entry()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Entry_From_Name_Value_Pair()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Entry_From_Name_Value_Pair(out Flac__StreamMetadata_VorbisComment_Entry entry, "name", "value"));
			Assert.AreEqual("name=value", Encoding.UTF8.GetString(entry.Entry, 0, entry.Entry.Length - 1));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Entry_To_Name_Value_Pair()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Entry_To_Name_Value_Pair(entry, out string field_Name, out string field_Value));
			Assert.AreEqual("name", field_Name);
			Assert.AreEqual("value", field_Value);

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Entry_Matches()");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Entry_Matches(entry, field_Name));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Entry_Matches()");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_VorbisComment_Entry_Matches(entry, "blah"));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Vorbis_Comment_Copy()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Vorbis_Comment);
			Assert.IsNotNull(block);
			Assert.AreEqual(Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8 + Format.Flac__Vendor_String.Length + Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_VorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata vorbisComment = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(vorbisComment);
			Assert.AreNotSame(block, vorbisComment);
			Assert.IsNotNull(vorbisComment.Data);
			Assert.AreNotSame(block.Data, vorbisComment.Data);
			Assert.IsInstanceOfType(vorbisComment.Data, typeof(Flac__StreamMetadata_VorbisComment));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Flac__StreamMetadata_VorbisComment metadata_VorbisComment = (Flac__StreamMetadata_VorbisComment)vorbisComment.Data;

			VC_Resize(vorbisComment, 2);

			Console.WriteLine($"Testing Flac__Metadata_Object_VorbisComment_Resize_Comments(grow to {metadata_VorbisComment.Num_Comments})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Resize_Comments(block, metadata_VorbisComment.Num_Comments));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			VC_Resize(vorbisComment, 1);

			Console.WriteLine($"Testing Flac__Metadata_Object_VorbisComment_Resize_Comments(shrink to {metadata_VorbisComment.Num_Comments})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Resize_Comments(block, metadata_VorbisComment.Num_Comments));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			VC_Resize(vorbisComment, 0);

			Console.WriteLine($"Testing Flac__Metadata_Object_VorbisComment_Resize_Comments(shrink to {metadata_VorbisComment.Num_Comments})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Resize_Comments(block, metadata_VorbisComment.Num_Comments));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(copy) on empty array");
			VC_Insert_New(out Flac__StreamMetadata_VorbisComment_Entry entry, vorbisComment, 0, "name1=field1");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(copy) on non-empty array");
			VC_Insert_New(out entry, vorbisComment, 1, "name2=field2");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			VC_Resize(vorbisComment, 0);

			Console.WriteLine($"Testing Flac__Metadata_Object_VorbisComment_Resize_Comments(shrink to {metadata_VorbisComment.Num_Comments})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Resize_Comments(block, metadata_VorbisComment.Num_Comments));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on empty array");
			VC_Insert_New(out entry, vorbisComment, 0, "name1=field1");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 0, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on beginning of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 0, "name2=field2");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 0, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on middle of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 1, "name3=field3");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 1, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 3, "name4=field4");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 3, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 4, "name3=field3dup1");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 4, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(copy) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 5, "name3=field3dup1");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 5, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			int j;

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(1, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, 0, "name3")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(4, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, (uint32_t)j + 1, "name3")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(5, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, (uint32_t)j + 1, "name3")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(0, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, 0, "name2")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(-1, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, (uint32_t)j + 1, "name2")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Find_Entry_From()");
			Assert.AreEqual(-1, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Find_Entry_From(block, 0, "blah")));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Replace_Comment(first, copy)");
			VC_Replace_New(out entry, vorbisComment, "name3=field3new1", false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Replace_Comment(block, entry, false, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));
			Assert.AreEqual(6U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Replace_Comment(all, copy)");
			VC_Replace_New(out entry, vorbisComment, "name3=field3new2", true);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Replace_Comment(block, entry, true, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));
			Assert.AreEqual(4U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_VorbisComment));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on middle of array");
			VC_Delete(vorbisComment, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on end of array");
			VC_Delete(vorbisComment, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on beginning of array");
			VC_Delete(vorbisComment, 0);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 0));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(copy) on non-empty array");
			VC_Insert_New(out entry, vorbisComment, 1, "rem0=val0");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(copy) on non-empty array");
			VC_Insert_New(out entry, vorbisComment, 2, "rem0=val1");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(copy) on non-empty array");
			VC_Insert_New(out entry, vorbisComment, 3, "rem0=val2");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Remove_Entry_Matching(\"blah\")");
			Assert.AreEqual(0, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Remove_Entry_Matching(block, "blah")));
			Assert.AreEqual(4U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Remove_Entry_Matching(\"rem0\")");
			VC_Delete(vorbisComment, 1);
			Assert.AreEqual(1, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Remove_Entry_Matching(block, "rem0")));
			Assert.AreEqual(3U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Remove_Entries_Matching(\"blah\")");
			Assert.AreEqual(0, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Remove_Entries_Matching(block, "blah")));
			Assert.AreEqual(3U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Remove_Entries_Matching(\"rem0\")");
			VC_Delete(vorbisComment, 1);
			VC_Delete(vorbisComment, 1);
			Assert.AreEqual(2, (j = Metadata_Object.Flac__Metadata_Object_VorbisComment_Remove_Entries_Matching(block, "rem0")));
			Assert.AreEqual(1U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Set_Comment(copy)");
			VC_Set_New(out entry, vorbisComment, 0, "name5=field5");
			Metadata_Object.Flac__Metadata_Object_VorbisComment_Set_Comment(block, 0, entry, true);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Set_Vendor_String(copy)");
			VC_Set_Vs_New(out entry, vorbisComment, "name6=field6");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Set_Vendor_String(block, entry, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(vorbisComment);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Vorbis_Comment_Own()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Vorbis_Comment);
			Assert.IsNotNull(block);
			Assert.AreEqual(Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8 + Format.Flac__Vendor_String.Length + Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_VorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata vorbisComment = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(vorbisComment);
			Assert.AreNotSame(block, vorbisComment);
			Assert.IsNotNull(vorbisComment.Data);
			Assert.AreNotSame(block.Data, vorbisComment.Data);
			Assert.IsInstanceOfType(vorbisComment.Data, typeof(Flac__StreamMetadata_VorbisComment));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(own) on empty array");
			VC_Insert_New(out Flac__StreamMetadata_VorbisComment_Entry entry, vorbisComment, 0, "name1=field1");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Append_Comment(own) on non-empty array");
			VC_Insert_New(out entry, vorbisComment, 1, "name2=field2");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Append_Comment(block, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(vorbisComment);
			Metadata_Object.Flac__Metadata_Object_Delete(block);

			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Vorbis_Comment);
			Assert.IsNotNull(block);
			Assert.AreEqual(Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8 + Format.Flac__Vendor_String.Length + Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_VorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			vorbisComment = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(vorbisComment);
			Assert.AreNotSame(block, vorbisComment);
			Assert.IsNotNull(vorbisComment.Data);
			Assert.AreNotSame(block.Data, vorbisComment.Data);
			Assert.IsInstanceOfType(vorbisComment.Data, typeof(Flac__StreamMetadata_VorbisComment));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on empty array");
			VC_Insert_New(out entry, vorbisComment, 0, "name1=field1");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 0, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on beginning of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 0, "name2=field2");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 0, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on middle of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 1, "name3=field3");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 1, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 3, "name4=field4");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 3, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 4, "name3=field3dup1");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 4, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Insert_Comment(own) on end of non-empty array");
			VC_Insert_New(out entry, vorbisComment, 5, "name3=field3dup1");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Insert_Comment(block, 5, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Replace_Comment(first, own)");
			VC_Replace_New(out entry, vorbisComment, "name3=field3new1", false);
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Replace_Comment(block, entry, false, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));
			Assert.AreEqual(6U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Replace_Comment(all, own)");
			VC_Replace_New(out entry, vorbisComment, "name3=field3new2", true);
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Replace_Comment(block, entry, true, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));
			Assert.AreEqual(4U, ((Flac__StreamMetadata_VorbisComment)block.Data).Num_Comments);

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on middle of array");
			VC_Delete(vorbisComment, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on end of array");
			VC_Delete(vorbisComment, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Delete_Comment() on beginning of array");
			VC_Delete(vorbisComment, 0);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Delete_Comment(block, 0));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Set_Comment(own)");
			VC_Set_New(out entry, vorbisComment, 0, "name5=field5");
			Entry_Clone(ref entry);
			Metadata_Object.Flac__Metadata_Object_VorbisComment_Set_Comment(block, 0, entry, false);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_VorbisComment_Set_Vendor_String(own)");
			VC_Set_Vs_New(out entry, vorbisComment, "name6=field6");
			Entry_Clone(ref entry);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_VorbisComment_Set_Vendor_String(block, entry, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, vorbisComment));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(vorbisComment);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_CueSheet_Track()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_New()");
			Flac__StreamMetadata_CueSheet_Track track = Metadata_Object.Flac__Metadata_Object_CueSheet_Track_New();
			Assert.IsNotNull(track);

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Clone()");
			Flac__StreamMetadata_CueSheet_Track trackCopy = Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Clone(track);
			Assert.IsNotNull(trackCopy);
			Assert.AreNotSame(track, trackCopy);
			Compare_Track(trackCopy, track);

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Delete()");
			Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete(trackCopy);
			Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete(track);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_CueSheet_Track_Copy()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.CueSheet);
			Assert.IsNotNull(block);
			Assert.AreEqual((Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len + Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len + Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len + Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len + Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len) / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_CueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata cueSheet = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(cueSheet);
			Assert.AreNotSame(block, cueSheet);
			Assert.IsNotNull(cueSheet.Data);
			Assert.AreNotSame(block.Data, cueSheet.Data);
			Assert.IsInstanceOfType(cueSheet.Data, typeof(Flac__StreamMetadata_CueSheet));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Flac__StreamMetadata_CueSheet metadata_CueSheet = (Flac__StreamMetadata_CueSheet)cueSheet.Data;

			CS_Resize(cueSheet, 2);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Resize_Tracks(grow to {metadata_CueSheet.Num_Tracks})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Resize_Tracks(block, metadata_CueSheet.Num_Tracks));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			CS_Resize(cueSheet, 1);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Resize_Tracks(shrink to {metadata_CueSheet.Num_Tracks})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Resize_Tracks(block, metadata_CueSheet.Num_Tracks));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			CS_Resize(cueSheet, 0);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Resize_Tracks(shrink to {metadata_CueSheet.Num_Tracks})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Resize_Tracks(block, metadata_CueSheet.Num_Tracks));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(copy) on empty array");
			CS_Insert_New(out Flac__StreamMetadata_CueSheet_Track track, cueSheet, 0, 0, 1, "ABCDE1234567", false, false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 0, track, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(copy) on beginning of non-empty array");
			CS_Insert_New(out track, cueSheet, 0, 10, 2, "BBCDE1234567", false, false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 0, track, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(copy) on middle of non-empty array");
			CS_Insert_New(out track, cueSheet, 1, 20, 3, "CBCDE1234567", false, false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 1, track, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(copy) on end of non-empty array");
			CS_Insert_New(out track, cueSheet, 3, 30, 4, "DBCDE1234567", false, false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 3, track, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Blank_Track() on end of non-empty array");
			CS_Insert_New(out track, cueSheet, 4, 0, 0, "\0\0\0\0\0\0\0\0\0\0\0\0", false, false);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Blank_Track(block, 4));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_CueSheet));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on end of array");
			CS_Delete(cueSheet, 4);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 4));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on middle of array");
			CS_Delete(cueSheet, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on end of array");
			CS_Delete(cueSheet, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on beginning of array");
			CS_Delete(cueSheet, 0);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 0));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Set_Track(copy)");
			CS_Set_New(out track, cueSheet, 0, 40, 5, "EBCDE1234567", false, false);
			Metadata_Object.Flac__Metadata_Object_CueSheet_Set_Track(block, 0, track, true);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			TR_Resize(cueSheet, 0, 2);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Track_Resize_Indices(grow to {metadata_CueSheet.Tracks[0].Num_Indices})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Resize_Indices(block, 0, metadata_CueSheet.Tracks[0].Num_Indices));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			TR_Resize(cueSheet, 0, 1);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Track_Resize_Indices(shrink to {metadata_CueSheet.Tracks[0].Num_Indices})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Resize_Indices(block, 0, metadata_CueSheet.Tracks[0].Num_Indices));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			TR_Resize(cueSheet, 0, 0);

			Console.WriteLine($"Testing Flac__Metadata_Object_CueSheet_Track_Resize_Indices(shrink to {metadata_CueSheet.Tracks[0].Num_Indices})");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Resize_Indices(block, 0, metadata_CueSheet.Tracks[0].Num_Indices));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Flac__StreamMetadata_CueSheet_Index indx = new Flac__StreamMetadata_CueSheet_Index();
			indx.Offset = 0;
			indx.Number = 1;

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Insert_Index() on empty array");
			TR_Insert_New(cueSheet, 0, 0, indx);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Insert_Index(block, 0, 0, indx));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			indx = new Flac__StreamMetadata_CueSheet_Index();
			indx.Offset = 10;
			indx.Number = 2;

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Insert_Index() on beginning of non-empty array");
			TR_Insert_New(cueSheet, 0, 0, indx);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Insert_Index(block, 0, 0, indx));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			indx = new Flac__StreamMetadata_CueSheet_Index();
			indx.Offset = 20;
			indx.Number = 3;

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Insert_Index() on middle of non-empty array");
			TR_Insert_New(cueSheet, 0, 1, indx);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Insert_Index(block, 0, 1, indx));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			indx = new Flac__StreamMetadata_CueSheet_Index();
			indx.Offset = 30;
			indx.Number = 4;

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Insert_Index() on end of non-empty array");
			TR_Insert_New(cueSheet, 0, 3, indx);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Insert_Index(block, 0, 3, indx));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			indx = new Flac__StreamMetadata_CueSheet_Index();
			indx.Offset = 0;
			indx.Number = 0;

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Insert_Blank_Index() on end of non-empty array");
			TR_Insert_New(cueSheet, 0, 4, indx);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Insert_Blank_Index(block, 0, 4));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_CueSheet));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Delete_Index() on end of array");
			TR_Delete(cueSheet, 0, 4);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete_Index(block, 0, 4));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Delete_Index() on middle of array");
			TR_Delete(cueSheet, 0, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete_Index(block, 0, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Delete_Index() on end of array");
			TR_Delete(cueSheet, 0, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete_Index(block, 0, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Track_Delete_Index() on beginning of array");
			TR_Delete(cueSheet, 0, 0);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Track_Delete_Index(block, 0, 0));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(cueSheet);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_CueSheet_Track_Own()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.CueSheet);
			Assert.IsNotNull(block);
			Assert.AreEqual((Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len + Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len + Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len + Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len + Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len) / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_CueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata cueSheet = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(cueSheet);
			Assert.AreNotSame(block, cueSheet);
			Assert.IsNotNull(cueSheet.Data);
			Assert.AreNotSame(block.Data, cueSheet.Data);
			Assert.IsInstanceOfType(cueSheet.Data, typeof(Flac__StreamMetadata_CueSheet));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(own) on empty array");
			CS_Insert_New(out Flac__StreamMetadata_CueSheet_Track track, cueSheet, 0, 60, 7, "GBCDE1234567", false, false);
			Track_Clone(ref track);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 0, track, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(own) on beginning of non-empty array");
			CS_Insert_New(out track, cueSheet, 0, 70, 8, "HBCDE1234567", false, false);
			Track_Clone(ref track);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 0, track, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(own) on middle of non-empty array");
			CS_Insert_New(out track, cueSheet, 1, 80, 9, "IBCDE1234567", false, false);
			Track_Clone(ref track);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 1, track, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Insert_Track(own) on end of non-empty array");
			CS_Insert_New(out track, cueSheet, 3, 90, 10, "JBCDE1234567", false, false);
			Track_Clone(ref track);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Insert_Track(block, 3, track, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on middle of array");
			CS_Delete(cueSheet, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on end of array");
			CS_Delete(cueSheet, 2);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 2));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Delete_Track() on beginning of array");
			CS_Delete(cueSheet, 0);
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_CueSheet_Delete_Track(block, 0));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Set_Track(own)");
			CS_Set_New(out track, cueSheet, 0, 100, 11, "KBCDE1234567", false, false);
			Track_Clone(ref track);
			Metadata_Object.Flac__Metadata_Object_CueSheet_Set_Track(block, 0, track, false);
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, cueSheet));

			Console.WriteLine("Testing Flac__Metadata_Object_CueSheet_Is_Legal");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_CueSheet_Is_Legal(block, true, out string violation));
			Console.WriteLine($"Returned false as expected, violation=\"{violation}\"");

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(cueSheet);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Picture_Copy()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Picture);
			Assert.IsNotNull(block);
			Assert.AreEqual((Constants.Flac__Stream_Metadata_Picture_Type_Len + Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len + Constants.Flac__Stream_Metadata_Picture_Description_Length_Len + Constants.Flac__Stream_Metadata_Picture_Width_Len + Constants.Flac__Stream_Metadata_Picture_Height_Len + Constants.Flac__Stream_Metadata_Picture_Depth_Len + Constants.Flac__Stream_Metadata_Picture_Colors_Len + Constants.Flac__Stream_Metadata_Picture_Data_Length_Len) / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_Picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata picture = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(picture);
			Assert.AreNotSame(block, picture);
			Assert.IsNotNull(picture.Data);
			Assert.AreNotSame(block.Data, picture.Data);
			Assert.IsInstanceOfType(picture.Data, typeof(Flac__StreamMetadata_Picture));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			PI_Set_Mime_Type(picture, "image/png\t");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Mime_Type(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Mime_Type(block, "image/png\t", true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out string violation));
			Console.WriteLine($"Returned false as expected, violation=\"{violation}\"");

			PI_Set_Mime_Type(picture, "image/png");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Mime_Type(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Mime_Type(block, "image/png", true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation), $"Failed with violation: {violation}");

			PI_Set_Description(picture, "DESCRIPTION", 0xff);

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Description(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Description(block, "DESCRIPTION ", true));
			((Flac__StreamMetadata_Picture)block.Data).Description[^2] = 0xff;
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation));
			Console.WriteLine($"Returned false as expected, violation=\"{violation}\"");

			PI_Set_Description(picture, "DESCRIPTION");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Description(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Description(block, "DESCRIPTION", true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation), $"Failed with violation: {violation}");

			PI_Set_Data(picture, Encoding.ASCII.GetBytes("PNGDATA"), 7);

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Data(copy)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Data(block, Encoding.ASCII.GetBytes("PNGDATA"), 7, true));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Picture));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(picture);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Metadata_Object_Picture_Own()
		{
			Console.WriteLine("Testing Flac__Metadata_Object_New()");
			Flac__StreamMetadata block = Metadata_Object.Flac__Metadata_Object_New(Flac__MetadataType.Picture);
			Assert.IsNotNull(block);
			Assert.AreEqual((Constants.Flac__Stream_Metadata_Picture_Type_Len + Constants.Flac__Stream_Metadata_Picture_Mime_Type_Length_Len + Constants.Flac__Stream_Metadata_Picture_Description_Length_Len + Constants.Flac__Stream_Metadata_Picture_Width_Len + Constants.Flac__Stream_Metadata_Picture_Height_Len + Constants.Flac__Stream_Metadata_Picture_Depth_Len + Constants.Flac__Stream_Metadata_Picture_Colors_Len + Constants.Flac__Stream_Metadata_Picture_Data_Length_Len) / 8, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_Picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata picture = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(picture);
			Assert.AreNotSame(block, picture);
			Assert.IsNotNull(picture.Data);
			Assert.AreNotSame(block.Data, picture.Data);
			Assert.IsInstanceOfType(picture.Data, typeof(Flac__StreamMetadata_Picture));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			PI_Set_Mime_Type(picture, "image/png\t");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Mime_Type(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Mime_Type(block, "image/png\t", false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out string violation));
			Console.WriteLine($"Returned false as expected, violation=\"{violation}\"");

			PI_Set_Mime_Type(picture, "image/png");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Mime_Type(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Mime_Type(block, "image/png", false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation), $"Failed with violation: {violation}");

			PI_Set_Description(picture, "DESCRIPTION", 0xff);

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Description(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Description(block, "DESCRIPTION ", false));
			((Flac__StreamMetadata_Picture)block.Data).Description[^2] = 0xff;
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsFalse(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation));
			Console.WriteLine($"Returned false as expected, violation=\"{violation}\"");

			PI_Set_Description(picture, "DESCRIPTION");

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Description(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Description(block, "DESCRIPTION", false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Is_Legal");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Is_Legal(block, out violation), $"Failed with violation: {violation}");

			PI_Set_Data(picture, Encoding.ASCII.GetBytes("PNGDATA"), 7);

			Console.WriteLine("Testing Flac__Metadata_Object_Picture_Set_Data(own)");
			Assert.IsTrue(Metadata_Object.Flac__Metadata_Object_Picture_Set_Data(block, Encoding.ASCII.GetBytes("PNGDATA"), 7, false));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, picture));

			Console.WriteLine("Testing Flac__Metadata_Object_Clone()");
			Flac__StreamMetadata blockCopy = Metadata_Object.Flac__Metadata_Object_Clone(block);
			Assert.IsNotNull(blockCopy);
			Assert.AreNotSame(block, blockCopy);
			Assert.IsNotNull(blockCopy.Data);
			Assert.AreNotSame(block.Data, blockCopy.Data);
			Assert.IsInstanceOfType(blockCopy.Data, typeof(Flac__StreamMetadata_Picture));
			Assert.IsTrue(Metadata_Utils.Compare_Block(block, blockCopy));

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(blockCopy);

			Console.WriteLine("Testing Flac__Metadata_Object_Delete()");
			Metadata_Object.Flac__Metadata_Object_Delete(picture);
			Metadata_Object.Flac__Metadata_Object_Delete(block);
		}

		#region Private method
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private Flac__byte[] Make_DummyData(Flac__byte[] dummyData, uint32_t len)
		{
			Flac__byte[] ret = new Flac__byte[len];
			Array.Copy(dummyData, ret, len);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Compare_Track(Flac__StreamMetadata_CueSheet_Track from, Flac__StreamMetadata_CueSheet_Track to)
		{
			Assert.AreEqual(to.Offset, from.Offset);
			Assert.AreEqual(to.Number, from.Number);
			CollectionAssert.AreEqual(to.Isrc, from.Isrc);
			Assert.AreEqual(to.Type, from.Type);
			Assert.AreEqual(to.Pre_Emphasis, from.Pre_Emphasis);
			Assert.AreEqual(to.Num_Indices, from.Num_Indices);

			if ((to.Indices == null) || (from.Indices == null))
				Assert.AreEqual(to.Indices, from.Indices);
			else
			{
				for (uint32_t i = 0; i < to.Num_Indices; i++)
				{
					Assert.AreEqual(to.Indices[i].Offset, from.Indices[i].Offset);
					Assert.AreEqual(to.Indices[i].Number, from.Indices[i].Number);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Compare_SeekPoint_Array(Flac__StreamMetadata_SeekPoint[] from, Flac__StreamMetadata_SeekPoint[] to, uint32_t n)
		{
			Assert.IsNotNull(from);
			Assert.IsNotNull(to);

			for (uint32_t i = 0; i < n; i++)
			{
				Assert.AreEqual(to[i].Sample_Number, from[i].Sample_Number);
				Assert.AreEqual(to[i].Stream_Offset, from[i].Stream_Offset);
				Assert.AreEqual(to[i].Frame_Samples, from[i].Frame_Samples);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Check_SeekTable(Flac__StreamMetadata block, uint32_t num_Points, Flac__StreamMetadata_SeekPoint[] array)
		{
			Assert.IsNotNull(block);
			Assert.AreEqual(num_Points * Constants.Flac__Stream_Metadata_SeekPoint_Length, block.Length);
			Assert.IsNotNull(block.Data);
			Assert.IsInstanceOfType(block.Data, typeof(Flac__StreamMetadata_SeekTable));

			Flac__StreamMetadata_SeekTable metaSeekTable = (Flac__StreamMetadata_SeekTable)block.Data;
			Assert.AreEqual(num_Points, metaSeekTable.Num_Points);

			if (array == null)
				Assert.IsNull(metaSeekTable.Points);
			else
				Compare_SeekPoint_Array(metaSeekTable.Points, array, num_Points);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Entry_New(out Flac__StreamMetadata_VorbisComment_Entry entry, string field)
		{
			entry = new Flac__StreamMetadata_VorbisComment_Entry();

			byte[] encodedBytes = Encoding.UTF8.GetBytes(field);

			entry.Length = (Flac__uint32)encodedBytes.Length;
			entry.Entry = new Flac__byte[entry.Length + 1];
			Array.Copy(encodedBytes, 0, entry.Entry, 0, entry.Length);
			entry.Entry[entry.Length] = 0x00;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Entry_Clone(ref Flac__StreamMetadata_VorbisComment_Entry entry)
		{
			byte[] x = new byte[entry.Length + 1];
			Array.Copy(entry.Entry, x, entry.Length);
			x[entry.Length] = 0x00;
			entry.Entry = x;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Calc_Len(Flac__StreamMetadata block)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			block.Length = Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8;
			block.Length += vc.Vendor_String.Length;
			block.Length += Constants.Flac__Stream_Metadata_Vorbis_Comment_Num_Comments_Len / 8;

			for (uint32_t i = 0; i < vc.Num_Comments; i++)
			{
				block.Length += Constants.Flac__Stream_Metadata_Vorbis_Comment_Entry_Length_Len / 8;
				block.Length += vc.Comments[i].Length;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Resize(Flac__StreamMetadata block, uint32_t num)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			if (vc.Num_Comments != 0)
			{
				Debug.Assert(vc.Comments != null);

				if (num < vc.Num_Comments)
				{
					for (uint32_t i = num; i < vc.Num_Comments; i++)
					{
						if (vc.Comments[i].Entry != null)
							vc.Comments[i].Entry = null;
					}
				}
			}

			if (num == 0)
			{
				if (vc.Comments != null)
					vc.Comments = null;
			}
			else
			{
				Array.Resize(ref vc.Comments, (int)num);

				if (num > vc.Num_Comments)
				{
					for (uint32_t i = vc.Num_Comments; i < num; i++)
						vc.Comments[i] = new Flac__StreamMetadata_VorbisComment_Entry();
				}
			}

			vc.Num_Comments = num;
			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int VC_Find_From(Flac__StreamMetadata block, string name, uint32_t start)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			uint32_t n = (uint32_t)name.Length;

			for (uint32_t i = start; i < vc.Num_Comments; i++)
			{
				Flac__StreamMetadata_VorbisComment_Entry entry = vc.Comments[i];

				if ((entry.Length > n) && (Encoding.ASCII.GetString(entry.Entry, 0, (int)n) == name) && (entry.Entry[n] == (byte)'='))
					return (int)i;
			}

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Set_Vs_New(out Flac__StreamMetadata_VorbisComment_Entry entry, Flac__StreamMetadata block, string field)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			Entry_New(out entry, field);
			vc.Vendor_String = entry;
			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Set_New(out Flac__StreamMetadata_VorbisComment_Entry entry, Flac__StreamMetadata block, uint32_t pos, string field)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			Entry_New(out entry, field);
			vc.Comments[pos] = entry;
			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Insert_New(out Flac__StreamMetadata_VorbisComment_Entry entry, Flac__StreamMetadata block, uint32_t pos, string field)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			VC_Resize(block, vc.Num_Comments + 1);
			Array.Copy(vc.Comments, pos, vc.Comments, pos + 1, vc.Num_Comments - 1 - pos);

			VC_Set_New(out entry, block, pos, field);
			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Delete(Flac__StreamMetadata block, uint32_t pos)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			Array.Copy(vc.Comments, pos + 1, vc.Comments, pos, vc.Num_Comments - pos - 1);

			vc.Comments[vc.Num_Comments - 1] = new Flac__StreamMetadata_VorbisComment_Entry();

			VC_Resize(block, vc.Num_Comments - 1);
			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void VC_Replace_New(out Flac__StreamMetadata_VorbisComment_Entry entry, Flac__StreamMetadata block, string field, Flac__bool all)
		{
			Flac__StreamMetadata_VorbisComment vc = (Flac__StreamMetadata_VorbisComment)block.Data;

			int eq = field.IndexOf('=');
			string field_Name = field.Substring(0, eq);

			int indx = VC_Find_From(block, field_Name, 0);
			if (indx < 0)
				VC_Insert_New(out entry, block, vc.Num_Comments, field);
			else
			{
				VC_Set_New(out entry, block, (uint32_t)indx, field);

				if (all)
				{
					for (indx = indx + 1; (indx >= 0) && ((uint32_t)indx < vc.Num_Comments); )
					{
						indx = VC_Find_From(block, field_Name, (uint32_t)indx);
						if (indx >= 0)
							VC_Delete(block, (uint32_t)indx);
					}
				}
			}

			VC_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Track_New(out Flac__StreamMetadata_CueSheet_Track track, Flac__uint64 offset, Flac__byte number, string isrc, Flac__bool data, Flac__bool pre_Em)
		{
			track = new Flac__StreamMetadata_CueSheet_Track();

			byte[] encodedBytes = Encoding.ASCII.GetBytes(isrc);

			track.Offset = offset;
			track.Number = number;
			Array.Copy(encodedBytes, 0, track.Isrc, 0, encodedBytes.Length);
			track.Type = data ? 1U : 0;
			track.Pre_Emphasis = pre_Em ? 1U : 0;
			track.Num_Indices = 0;
			track.Indices = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Track_Clone(ref Flac__StreamMetadata_CueSheet_Track track)
		{
			if (track.Num_Indices > 0)
			{
				Flac__StreamMetadata_CueSheet_Index[] x = new Flac__StreamMetadata_CueSheet_Index[track.Num_Indices];

				for (int i = 0; i < track.Num_Indices; i++)
				{
					x[i] = new Flac__StreamMetadata_CueSheet_Index
					{
						Offset = track.Indices[i].Offset,
						Number = track.Indices[i].Number
					};
				}

				track.Indices = x;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CS_Calc_Len(Flac__StreamMetadata block)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;

			block.Length =
			(
				Constants.Flac__Stream_Metadata_CueSheet_Media_Catalog_Number_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Lead_In_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Is_Cd_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Reserved_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Num_Tracks_Len
			) / 8;

			block.Length += cs.Num_Tracks *
			(
				Constants.Flac__Stream_Metadata_CueSheet_Track_Offset_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Number_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Isrc_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Type_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Pre_Emphasis_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Reserved_Len +
				Constants.Flac__Stream_Metadata_CueSheet_Track_Num_Indices_Len
			) / 8;

			for (uint32_t i = 0; i < cs.Num_Tracks; i++)
			{
				block.Length += cs.Tracks[i].Num_Indices *
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
		private void TR_Resize(Flac__StreamMetadata block, uint32_t track_Num, uint32_t num)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;

			Debug.Assert(track_Num < cs.Num_Tracks);

			Flac__StreamMetadata_CueSheet_Track tr = cs.Tracks[track_Num];

			if (tr.Num_Indices != 0)
				Debug.Assert(tr.Indices != null);

			if (num == 0)
			{
				if (tr.Indices != null)
					tr.Indices = null;
			}
			else
			{
				Array.Resize(ref tr.Indices, (int)num);

				if (num > tr.Num_Indices)
				{
					for (uint32_t i = tr.Num_Indices; i < num; i++)
						tr.Indices[i] = new Flac__StreamMetadata_CueSheet_Index();
				}
			}

			tr.Num_Indices = (Flac__byte)num;
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TR_Set_New(Flac__StreamMetadata block, uint32_t track_Num, uint32_t pos, Flac__StreamMetadata_CueSheet_Index indx)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;
			Debug.Assert(track_Num < cs.Num_Tracks);

			Flac__StreamMetadata_CueSheet_Track tr = cs.Tracks[track_Num];

			Debug.Assert(pos < tr.Num_Indices);

			tr.Indices[pos] = indx;

			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TR_Insert_New(Flac__StreamMetadata block, uint32_t track_Num, uint32_t pos, Flac__StreamMetadata_CueSheet_Index indx)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;
			Debug.Assert(track_Num < cs.Num_Tracks);

			Flac__StreamMetadata_CueSheet_Track tr = cs.Tracks[track_Num];

			Debug.Assert(pos <= tr.Num_Indices);

			TR_Resize(block, track_Num, (uint32_t)tr.Num_Indices + 1);
			Array.Copy(tr.Indices, pos, tr.Indices, pos + 1, tr.Num_Indices - 1 - pos);

			TR_Set_New(block, track_Num, pos, indx);
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TR_Delete(Flac__StreamMetadata block, uint32_t track_Num, uint32_t pos)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;
			Debug.Assert(track_Num < cs.Num_Tracks);

			Flac__StreamMetadata_CueSheet_Track tr = cs.Tracks[track_Num];

			Debug.Assert(pos <= tr.Num_Indices);

			Array.Copy(tr.Indices, pos + 1, tr.Indices, pos, tr.Num_Indices - pos - 1);

			TR_Resize(block, track_Num, (uint32_t)tr.Num_Indices - 1);
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CS_Resize(Flac__StreamMetadata block, uint32_t num)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;

			if (cs.Num_Tracks != 0)
			{
				Debug.Assert(cs.Tracks != null);

				if (num < cs.Num_Tracks)
				{
					for (uint32_t i = num; i < cs.Num_Tracks; i++)
					{
						if (cs.Tracks[i].Indices != null)
							cs.Tracks[i].Indices = null;
					}
				}
			}

			if (num == 0)
			{
				if (cs.Tracks != null)
					cs.Tracks = null;
			}
			else
			{
				Array.Resize(ref cs.Tracks, (int)num);

				if (num > cs.Num_Tracks)
				{
					for (uint32_t i = cs.Num_Tracks; i < num; i++)
						cs.Tracks[i] = new Flac__StreamMetadata_CueSheet_Track();
				}
			}

			cs.Num_Tracks = num;
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CS_Set_New(out Flac__StreamMetadata_CueSheet_Track track, Flac__StreamMetadata block, uint32_t pos, Flac__uint64 offset, Flac__byte number, string isrc, Flac__bool data, Flac__bool pre_Em)
		{
			Track_New(out track, offset, number, isrc, data, pre_Em);
			((Flac__StreamMetadata_CueSheet)block.Data).Tracks[pos] = track;
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CS_Insert_New(out Flac__StreamMetadata_CueSheet_Track track, Flac__StreamMetadata block, uint32_t pos, Flac__uint64 offset, Flac__byte number, string isrc, Flac__bool data, Flac__bool pre_Em)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;

			CS_Resize(block, cs.Num_Tracks + 1);
			Array.Copy(cs.Tracks, pos, cs.Tracks, pos + 1, cs.Num_Tracks - 1 - pos);
			CS_Set_New(out track, block, pos, offset, number, isrc, data, pre_Em);
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CS_Delete(Flac__StreamMetadata block, uint32_t pos)
		{
			Flac__StreamMetadata_CueSheet cs = (Flac__StreamMetadata_CueSheet)block.Data;

			if (cs.Tracks[pos].Indices != null)
				cs.Tracks[pos].Indices = null;

			Array.Copy(cs.Tracks, pos + 1, cs.Tracks, pos, cs.Num_Tracks - pos - 1);

			cs.Tracks[cs.Num_Tracks - 1] = new Flac__StreamMetadata_CueSheet_Track();

			CS_Resize(block, cs.Num_Tracks - 1);
			CS_Calc_Len(block);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PI_Set_Mime_Type(Flac__StreamMetadata block, string s)
		{
			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)block.Data;

			if (metaPicture.Mime_Type != null)
				block.Length -= (uint32_t)metaPicture.Mime_Type.Length - 1;

			byte[] encodedBytes = Encoding.ASCII.GetBytes(s);

			metaPicture.Mime_Type = new byte[encodedBytes.Length + 1];
			Array.Copy(encodedBytes, metaPicture.Mime_Type, encodedBytes.Length);
			block.Length += (uint32_t)metaPicture.Mime_Type.Length - 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PI_Set_Description(Flac__StreamMetadata block, string s, params byte[] extra)
		{
			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)block.Data;

			if (metaPicture.Description != null)
				block.Length -= (uint32_t)metaPicture.Description.Length - 1;

			byte[] encodedBytes = Encoding.UTF8.GetBytes(s);

			metaPicture.Description = new byte[encodedBytes.Length + extra.Length + 1];
			Array.Copy(encodedBytes, metaPicture.Description, encodedBytes.Length);
			Array.Copy(extra, 0, metaPicture.Description, encodedBytes.Length, extra.Length);
			block.Length += (uint32_t)metaPicture.Description.Length - 1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void PI_Set_Data(Flac__StreamMetadata block, Flac__byte[] data, Flac__uint32 len)
		{
			Flac__StreamMetadata_Picture metaPicture = (Flac__StreamMetadata_Picture)block.Data;

			if (metaPicture.Data != null)
				block.Length -= metaPicture.Data_Length;

			metaPicture.Data = new Flac__byte[len];
			Array.Copy(data, metaPicture.Data, len);

			metaPicture.Data_Length = len;
			block.Length += len;
		}
		#endregion
	}
}
