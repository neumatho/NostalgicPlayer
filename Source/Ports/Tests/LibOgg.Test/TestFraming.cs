/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibOgg.Containers;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibOgg.Test
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestFraming
	{
		private OggStream osEn;
		private OggStream osDe;
		private OggSync oy;

		// 17 only
		private static readonly c_int[] head1_0 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x06,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0x15, 0xed, 0xec, 0x91,
			1,
			17
		];

		// 17, 254, 255, 256, 500, 510, 600 byte, pad
		private static readonly c_int[] head1_1 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0x59, 0x10, 0x6c, 0x2c,
			1,
			17
		];

		private static readonly c_int[] head2_1 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x04,
			0x07, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0x89, 0x33, 0x85, 0xce,
			13,
			254, 255, 0, 255, 1, 255, 245, 255, 255, 0,
			255, 255, 90
		];

		// Nil packets; beginning, middle, end
		private static readonly c_int[] head1_2 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_2 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x04,
			0x07, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0x5c, 0x3f, 0x66, 0xcb,
			17,
			17, 254, 255, 0, 0, 255, 1, 0, 255, 245, 255, 255, 0,
			255, 255, 90, 0
		];

		// Large initial packet
		private static readonly c_int[] head1_3 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0x01, 0x27, 0x31, 0xaa,
			18,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 10
		];

		private static readonly c_int[] head2_3 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x04,
			0x07, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0x7f, 0x4e, 0x8a, 0xd2,
			4,
			255, 4, 255, 0
		];

		// Continuing packet test
		private static readonly c_int[] head1_4 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_4 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x00,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0xf8, 0x3c, 0x19, 0x79,
			255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255
		];

		private static readonly c_int[] head3_4 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x05,
			0x07, 0x0c, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 2, 0, 0, 0,
			0x38, 0xe6, 0xb6, 0x28,
			6,
			255, 220, 255, 4, 255, 0
		];

		// Spill expansion test
		private static readonly c_int[] head1_4b =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_4b =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x00,
			0x07, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0xce, 0x8f, 0x17, 0x1a,
			23,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255, 255, 10, 255, 4, 255, 0, 0
		];

		private static readonly c_int[] head3_4b =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x04,
			0x07, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 2, 0, 0, 0,
			0x9b, 0xb2, 0x50, 0xa1,
			1,
			0
		];

		// Page with the 255 segment limit
		private static readonly c_int[] head1_5 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_5 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x00,
			0x07, 0xfc, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0xed, 0x2a, 0x2e, 0xa7,
			255,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10, 10,
			10, 10, 10, 10, 10, 10, 10
		];

		private static readonly c_int[] head3_5 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x04,
			0x07, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 2, 0, 0, 0,
			0x6c, 0x3b, 0x82, 0x3d,
			1,
			50
		];

		// Packet that overspans over an entire page
		private static readonly c_int[] head1_6 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_6 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x00,
			0x07, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0x68, 0x22, 0x7c, 0x3d,
			255,
			100,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255
		];

		private static readonly c_int[] head3_6 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x01,
			0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
			0x01, 0x02, 0x03, 0x04, 2, 0, 0, 0,
			0xf4, 0x87, 0xba, 0xf3,
			255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255
		];

		private static readonly c_int[] head4_6 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x05,
			0x07, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 3, 0, 0, 0,
			0xf7, 0x2f, 0x6c, 0x60,
			5,
			254, 255, 4, 255, 0
		];

		// Packet that overspans over an entire page
		private static readonly c_int[] head1_7 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x02,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 0, 0, 0, 0,
			0xff, 0x7b, 0x23, 0x17,
			1,
			0
		];

		private static readonly c_int[] head2_7 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x00,
			0x07, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 1, 0, 0, 0,
			0x68, 0x22, 0x7c, 0x3d,
			255,
			100,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255, 255, 255,
			255, 255, 255, 255, 255, 255
		];

		private static readonly c_int[] head3_7 =
		[
			0x4f, 0x67, 0x67, 0x53, 0, 0x05,
			0x07, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x01, 0x02, 0x03, 0x04, 2, 0, 0, 0,
			0xd4, 0xe0, 0x60, 0xe5,
			1,
			0
		];

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Framing()
		{
			OggStream.Init(out osEn, 0x04030201);
			OggStream.Init(out osDe, 0x04030201);
			OggSync.Init(out oy);

			// Exercise each code path in the framing code. Also verify that
			// the checksums are working

			{
				// 17 only
				c_int[] packets = [ 17, -1 ];
				c_int[][] headRet = [ head1_0, null ];

				Console.WriteLine("Testing single page encoding");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// 17, 254, 255, 256, 500, 510, 600 byte, pad
				c_int[] packets = [ 17, 254, 255, 256, 500, 510, 600, -1 ];
				c_int[][] headRet = [ head1_1, head2_1, null ];

				Console.WriteLine("Testing basic page encoding");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Nil packets; beginning, middle, end
				c_int[] packets = [ 0, 17, 254, 255, 0, 256, 0, 500, 510, 600, 0, -1 ];
				c_int[][] headRet = [ head1_2, head2_2, null ];

				Console.WriteLine("Testing basic nil packets");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Large initial packet
				c_int[] packets = [ 4345, 259, 255, -1 ];
				c_int[][] headRet = [ head1_3, head2_3, null ];

				Console.WriteLine("Testing initial-packet lacing > 4k");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Continuing packet test; with page spill expansion, we have to
				// overflow the lacing table
				c_int[] packets = [ 0, 65500, 259, 255, -1 ];
				c_int[][] headRet = [ head1_4, head2_4, head3_4, null ];

				Console.WriteLine("Testing single packet page span");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Spill expand packet test
				c_int[] packets = [ 0, 4345, 259, 255, 0, 0, -1 ];
				c_int[][] headRet = [ head1_4b, head2_4b, head3_4b, null ];

				Console.WriteLine("Testing page spill expansion");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Page with the 255 segment limit
				c_int[] packets =
				[
					0, 10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 10,
					10, 10, 10, 10, 10, 10, 10, 50, -1
				];
				c_int[][] headRet = [ head1_5, head2_5, head3_5, null ];

				Console.WriteLine("Testing max packet segments");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Packet that overspans over an entire page
				c_int[] packets = [ 0, 100, 130049, 259, 255, -1 ];
				c_int[][] headRet = [ head1_6, head2_6, head3_6, head4_6, null ];

				Console.WriteLine("Testing very large packets");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Test for the libogg 1.1.1 resync in large continuation bug
				// found by Josh Coalson
				c_int[] packets = [ 0, 100, 130049, 259, 255, -1 ];
				c_int[][] headRet = [ head1_6, head2_6, head3_6, head4_6, null ];

				Console.WriteLine("Testing continuation resync in very large packets");
				TestPack(packets, headRet, 100, 2, 3);
			}

			{
				// Term only page. Why not?
				c_int[] packets = [ 0, 100, 64770, -1 ];
				c_int[][] headRet = [ head1_7, head2_7, head3_7, null ];

				Console.WriteLine("Testing zero data page (1 nil packet)");
				TestPack(packets, headRet, 0, 0, 0);
			}

			{
				// Build a bunch of pages for testing
				CPointer<byte> data = Memory.Ogg_MAlloc<byte>(1024 * 1024);
				c_int[] pl = [ 0, 1,1,98,4079, 1,1,2954,2057, 76,34,912,0,234,1000,1000, 1000,300,-1 ];
				c_int inPtr = 0;
				OggPage[] og = new OggPage[5];

				osEn.Reset();

				for (c_int i = 0; pl[i] != -1; i++)
				{
					Ogg_Packet op = new Ogg_Packet();
					c_int len = pl[i];

					op.Packet = data + inPtr;
					op.Bytes = len;
					op.Eos = pl[i + 1] < 0;
					op.GranulePos = (i + 1) * 1000;

					for (c_int j = 0; j < len; j++)
						data[inPtr++] = (byte)(i + j);

					osEn.PacketIn(op);
				}

				Memory.Ogg_Free(data);

				// Retrieve finished pages
				for (c_int i = 0; i < 5; i++)
				{
					if (osEn.PageOut(out og[i]) == 0)
						Assert.Fail("Too few pages output building sync tests");

					CopyPage(og[i]);
				}

				// Test lost pages on pagein/packetout: no rollback
				{
					Console.WriteLine("Testing loss of pages");

					oy.Reset();
					osDe.Reset();

					for (c_int i = 0; i < 5; i++)
					{
						CMemory.MemCpy(oy.Buffer(og[i].Page.HeaderLen), og[i].Page.Header, og[i].Page.HeaderLen);
						oy.Wrote(og[i].Page.HeaderLen);

						CMemory.MemCpy(oy.Buffer(og[i].Page.BodyLen), og[i].Page.Body, og[i].Page.BodyLen);
						oy.Wrote(og[i].Page.BodyLen);
					}

					oy.PageOut(out OggPage temp);
					osDe.PageIn(temp);
					oy.PageOut(out temp);
					osDe.PageIn(temp);
					oy.PageOut(out temp);
					// Skip
					oy.PageOut(out temp);
					osDe.PageIn(temp);

					// Do we get the expected results/packets?
					if (osDe.PacketOut(out Ogg_Packet test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 0, 0, 0);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 1, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 2, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 98, 3, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 4079, 4, 5000);

					if (osDe.PacketOut(out test) != -1)
						Assert.Fail("Error: Loss of page did not return error");

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 76, 9, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 34, 10, -1);
				}

				// Test lost pages on pagein/packetout: rollback with continuation
				{
					Console.WriteLine("Testing loss of pages (rollback required)");

					oy.Reset();
					osDe.Reset();

					for (c_int i = 0; i < 5; i++)
					{
						CMemory.MemCpy(oy.Buffer(og[i].Page.HeaderLen), og[i].Page.Header, og[i].Page.HeaderLen);
						oy.Wrote(og[i].Page.HeaderLen);

						CMemory.MemCpy(oy.Buffer(og[i].Page.BodyLen), og[i].Page.Body, og[i].Page.BodyLen);
						oy.Wrote(og[i].Page.BodyLen);
					}

					oy.PageOut(out OggPage temp);
					osDe.PageIn(temp);
					oy.PageOut(out temp);
					osDe.PageIn(temp);
					oy.PageOut(out temp);
					osDe.PageIn(temp);
					oy.PageOut(out temp);
					// Skip
					oy.PageOut(out temp);
					osDe.PageIn(temp);

					// Do we get the expected results/packets?
					if (osDe.PacketOut(out Ogg_Packet test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 0, 0, 0);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 1, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 2, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 98, 3, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 4079, 4, 5000);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 5, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 1, 6, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 2954, 7, -1);

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 2057, 8, 9000);

					if (osDe.PacketOut(out test) != -1)
						Assert.Fail("Error: Loss of page did not return error");

					if (osDe.PacketOut(out test) != 1)
						Assert.Fail("Error");

					CheckPacket(test, 300, 17, 18000);
				}

				// The rest only test sync
				{
					// Test fractional page inputs: incomplete capture
					Console.WriteLine("Testing sync on partial inputs");

					oy.Reset();
					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header, 3);
					oy.Wrote(3);

					if (oy.PageOut(out OggPage ogDe) > 0)
						Assert.Fail("Error");

					// Test fractional page inputs: incomplete fixed header
					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header + 3, 20);
					oy.Wrote(20);

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					// Test fractional page inputs: incomplete header
					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header + 23, 5);
					oy.Wrote(5);

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					// Test fractional page inputs: incomplete body
					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header + 28, og[1].Page.HeaderLen - 28);
					oy.Wrote(og[1].Page.HeaderLen - 28);

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, 1000);
					oy.Wrote(1000);

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body + 1000, og[1].Page.BodyLen - 1000);
					oy.Wrote(og[1].Page.BodyLen - 1000);

					if (oy.PageOut(out ogDe) <= 0)
						Assert.Fail("Error");
				}

				// Test fractional page inputs: page + incomplete capture
				{
					Console.WriteLine("Testing sync on 1+partial inputs");

					oy.Reset();

					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header, og[1].Page.HeaderLen);
					oy.Wrote(og[1].Page.HeaderLen);

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, og[1].Page.BodyLen);
					oy.Wrote(og[1].Page.BodyLen);

					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header, 20);
					oy.Wrote(20);

					if (oy.PageOut(out OggPage ogDe) <= 0)
						Assert.Fail("Error");

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header + 20, og[1].Page.HeaderLen - 20);
					oy.Wrote(og[1].Page.HeaderLen - 20);

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, og[1].Page.BodyLen);
					oy.Wrote(og[1].Page.BodyLen);

					if (oy.PageOut(out ogDe) <= 0)
						Assert.Fail("Error");
				}

				// Test recapture: garbage + page
				{
					Console.WriteLine("Testing search for capture");

					oy.Reset();

					// Garbage
					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, og[1].Page.BodyLen);
					oy.Wrote(og[1].Page.BodyLen);

					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header, og[1].Page.HeaderLen);
					oy.Wrote(og[1].Page.HeaderLen);

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, og[1].Page.BodyLen);
					oy.Wrote(og[1].Page.BodyLen);

					CMemory.MemCpy(oy.Buffer(og[2].Page.HeaderLen), og[2].Page.Header, 20);
					oy.Wrote(20);

					if (oy.PageOut(out OggPage ogDe) > 0)
						Assert.Fail("Error");

					if (oy.PageOut(out ogDe) <= 0)
						Assert.Fail("Error");

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					CMemory.MemCpy(oy.Buffer(og[2].Page.HeaderLen), og[2].Page.Header + 20, og[2].Page.HeaderLen - 20);
					oy.Wrote(og[2].Page.HeaderLen - 20);

					CMemory.MemCpy(oy.Buffer(og[2].Page.BodyLen), og[2].Page.Body, og[2].Page.BodyLen);
					oy.Wrote(og[2].Page.BodyLen);

					if (oy.PageOut(out ogDe) <= 0)
						Assert.Fail("Error");
				}

				// Test recapture: page + garbage + page
				{
					Console.WriteLine("Testing recapture");

					oy.Reset();

					CMemory.MemCpy(oy.Buffer(og[1].Page.HeaderLen), og[1].Page.Header, og[1].Page.HeaderLen);
					oy.Wrote(og[1].Page.HeaderLen);

					CMemory.MemCpy(oy.Buffer(og[1].Page.BodyLen), og[1].Page.Body, og[1].Page.BodyLen);
					oy.Wrote(og[1].Page.BodyLen);

					CMemory.MemCpy(oy.Buffer(og[2].Page.HeaderLen), og[2].Page.Header, og[2].Page.HeaderLen);
					oy.Wrote(og[2].Page.HeaderLen);

					CMemory.MemCpy(oy.Buffer(og[2].Page.HeaderLen), og[2].Page.Header, og[2].Page.HeaderLen);
					oy.Wrote(og[2].Page.HeaderLen);

					if (oy.PageOut(out OggPage ogDe) <= 0)
						Assert.Fail("Error");

					CMemory.MemCpy(oy.Buffer(og[2].Page.BodyLen), og[2].Page.Body, og[2].Page.BodyLen - 5);
					oy.Wrote(og[2].Page.BodyLen - 5);

					CMemory.MemCpy(oy.Buffer(og[3].Page.HeaderLen), og[3].Page.Header, og[3].Page.HeaderLen);
					oy.Wrote(og[3].Page.HeaderLen);

					CMemory.MemCpy(oy.Buffer(og[3].Page.BodyLen), og[3].Page.Body, og[3].Page.BodyLen);
					oy.Wrote(og[3].Page.BodyLen);

					if (oy.PageOut(out ogDe) > 0)
						Assert.Fail("Error");

					if (oy.PageOut(out ogDe) <= 0)
						Assert.Fail("Error");
				}

				// Free page data that was previously copied
				{
					for (c_int i = 0; i < 5; i++)
						FreePage(og[i]);
				}
			}

			oy.Clear();
			osEn.Clear();
			osDe.Clear();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void TestPack(int[] pl, int[][] headers, c_int byteSkip, c_int pageSkip, c_int packetSkip)
		{
			CPointer<byte> data = Memory.Ogg_MAlloc<byte>(1024 * 1024);		// For scripted test cases only
			c_long inPtr = 0;
			c_long outPtr = 0;
			c_long dePtr = 0;
			c_long depacket = 0;
			c_long granulePos = 7, pageNo = 0;
			c_int packets;
			c_int pageOut = pageSkip;
			bool eosFlag = false;
			bool bosFlag = false;

			c_int byteSkipCount = 0;

			osEn.Reset();
			osDe.Reset();
			oy.Reset();

			for (packets = 0; packets < packetSkip; packets++)
				depacket += pl[packets];

			for (packets = 0; ; packets++)
			{
				if (pl[packets] == -1)
					break;
			}

			for (c_int i = 0; i < packets; i++)
			{
				// Construct a test packet
				Ogg_Packet op = new Ogg_Packet();
				c_int len = pl[i];

				op.Packet = data + inPtr;
				op.Bytes = len;
				op.Eos = pl[i + 1] < 0;
				op.GranulePos = granulePos;

				granulePos += 1024;

				for (c_int j = 0; j < len; j++)
					data[inPtr++] = (byte)(i + j);

				// Submit the test packet
				osEn.PacketIn(op);

				// Retrieve any finished pages
				{
					while (osEn.PageOut(out OggPage og) != 0)
					{
						// We have a page. Check it carefully
						Console.Write($"{pageNo}, ");

						if (headers[pageNo] == null)
							Assert.Fail("Coded too many pages");

						CheckPage(data + outPtr, headers[pageNo], og);

						outPtr += og.Page.BodyLen;
						pageNo++;

						if (pageSkip != 0)
						{
							bosFlag = true;
							pageSkip--;
							dePtr += og.Page.BodyLen;
						}

						// Have a complete page; submit it to sync/decode
						{
							CPointer<byte> buf = oy.Buffer(og.Page.HeaderLen + og.Page.BodyLen);
							CPointer<byte> next = buf;

							byteSkipCount += og.Page.HeaderLen;

							if (byteSkipCount > byteSkip)
							{
								CMemory.MemCpy(next, og.Page.Header, byteSkipCount - byteSkip);
								next += byteSkipCount - byteSkip;
								byteSkipCount = byteSkip;
							}

							byteSkipCount += og.Page.BodyLen;

							if (byteSkipCount > byteSkip)
							{
								CMemory.MemCpy(next, og.Page.Body, byteSkipCount - byteSkip);
								next += byteSkipCount - byteSkip;
								byteSkipCount = byteSkip;
							}

							oy.Wrote(next - buf);

							while (true)
							{
								c_int ret = oy.PageOut(out OggPage ogDe);
								if (ret == 0)
									break;

								if (ret < 0)
									continue;

								// Got a page. Happy happy. Verify that it's good
								Console.Write($"({pageOut}), ");

								CheckPage(data + dePtr, headers[pageOut], ogDe);
								dePtr += ogDe.Page.BodyLen;
								pageOut++;

								// Submit it to deconstitution
								osDe.PageIn(ogDe);

								// Packets out?
								while (osDe.PacketPeek(out Ogg_Packet opDe2) > 0)
								{
									osDe.PacketPeek(out _);
									osDe.PacketOut(out Ogg_Packet opDe);		// Just catching them all

									// Verify peek and out match
									ComparePacket(opDe, opDe2);

									// Verify the packet
									if (CMemory.MemCmp(data + depacket, opDe.Packet, opDe.Bytes) != 0)
										Assert.Fail("Packet data mismatch in decode");

									// Check bos flag
									if (!bosFlag && !opDe.Bos)
										Assert.Fail("Bos flag not set on packet");

									if (bosFlag && opDe.Bos)
										Assert.Fail("Bos flag incorrectly set on packet");

									bosFlag = true;
									depacket += opDe.Bytes;

									// Check eos flag
									if (eosFlag)
										Assert.Fail("Multiple decoded packets with eos flag");

									if (opDe.Eos)
										eosFlag = true;

									// Check granulepos flag
									if (opDe.GranulePos != -1)
										Console.Write($" Granule: {opDe.GranulePos} ");
								}
							}
						}
					}
				}
			}

			Memory.Ogg_Free(data);

			Assert.IsNull(headers[pageNo], "Did not write last page");
			Assert.IsNull(headers[pageOut], "Did not decode last page");
			Assert.AreEqual(outPtr, inPtr, "Encoded page data incomplete");
			Assert.AreEqual(dePtr, inPtr, "Decoded page data incomplete");
			Assert.AreEqual(depacket, inPtr, "Decoded packet data incomplete");
			Assert.IsTrue(eosFlag, "Never got a packet with EOS set");

			Console.WriteLine();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CheckPage(CPointer<byte> data, int[] header, OggPage og)
		{
			// Test data
			for (c_long j = 0; j < og.Page.BodyLen; j++)
				Assert.AreEqual(data[j], og.Page.Body[j], $"Body data mismatch (2) at pos {j}");

			// Test header
			for (c_long j = 0; j < og.Page.HeaderLen; j++)
				Assert.AreEqual(header[j], og.Page.Header[j], $"Header content mismatch at pos {j}");

			Assert.AreEqual(header[26] + 27, og.Page.HeaderLen, $"Header length incorrect");
		}

		private c_int sequence = 0;
		private c_int lastNo = 0;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CheckPacket(Ogg_Packet op, c_long len, c_int no, c_long pos)
		{
			Assert.AreEqual(len, op.Bytes, "Incorrect packet length");
			Assert.AreEqual(pos, op.GranulePos, "Incorrect packet granpos");

			// Packet number just follows sequence/gap; adjust the input number
			// for that
			if (no == 0)
				sequence = 0;
			else
			{
				sequence++;

				if (no > (lastNo + 1))
					sequence++;
			}

			lastNo = no;
			Assert.AreEqual(sequence, op.PacketNo, "Incorrect packet sequence");

			// Test data
			for (c_long j = 0; j < op.Bytes; j++)
				Assert.AreEqual((j + no) & 0xff, op.Packet[j], $"Body data mismatch (1) at pos {j}");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void ComparePacket(Ogg_Packet op1, Ogg_Packet op2)
		{
			Assert.AreEqual(op1.Packet, op2.Packet, "op1.packet != op2.packet");
			Assert.AreEqual(op1.Bytes, op2.Bytes, "op1.bytes != op2.bytes");
			Assert.AreEqual(op1.Bos, op2.Bos, "op1.bos != op2.bos");
			Assert.AreEqual(op1.Eos, op2.Eos, "op1.eos != op2.eos");
			Assert.AreEqual(op1.GranulePos, op2.GranulePos, "op1.granulepos != op2.granulepos");
			Assert.AreEqual(op1.PacketNo, op2.PacketNo, "op1.packetno != op2.packetno");
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void CopyPage(OggPage og)
		{
			CPointer<byte> temp = Memory.Ogg_MAlloc<byte>((size_t)og.Page.HeaderLen);
			CMemory.MemCpy(temp, og.Page.Header, og.Page.HeaderLen);
			og.Page.Header = temp;

			temp = Memory.Ogg_MAlloc<byte>((size_t)og.Page.BodyLen);
			CMemory.MemCpy(temp, og.Page.Body, og.Page.BodyLen);
			og.Page.Body = temp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void FreePage(OggPage og)
		{
			Memory.Ogg_Free(og.Page.Header);
			Memory.Ogg_Free(og.Page.Body);
		}
		#endregion
	}
}
