/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibFlac.Flac;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibFlac.Test.LibFlac
{
	/// <summary>
	/// 
	/// </summary>
	[TestClass]
	public class TestFormat
	{
		private struct Sample_Rates
		{
			public uint32_t rate;
			public Flac__bool valid;
			public Flac__bool subset;
		}

		private static readonly Sample_Rates[] sampleRates =
		{
			new Sample_Rates { rate = 0      , valid = true , subset = true  },
			new Sample_Rates { rate = 1      , valid = true , subset = true  },
			new Sample_Rates { rate = 9      , valid = true , subset = true  },
			new Sample_Rates { rate = 10     , valid = true , subset = true  },
			new Sample_Rates { rate = 4000   , valid = true , subset = true  },
			new Sample_Rates { rate = 8000   , valid = true , subset = true  },
			new Sample_Rates { rate = 11025  , valid = true , subset = true  },
			new Sample_Rates { rate = 12000  , valid = true , subset = true  },
			new Sample_Rates { rate = 16000  , valid = true , subset = true  },
			new Sample_Rates { rate = 22050  , valid = true , subset = true  },
			new Sample_Rates { rate = 24000  , valid = true , subset = true  },
			new Sample_Rates { rate = 32000  , valid = true , subset = true  },
			new Sample_Rates { rate = 32768  , valid = true , subset = true  },
			new Sample_Rates { rate = 44100  , valid = true , subset = true  },
			new Sample_Rates { rate = 48000  , valid = true , subset = true  },
			new Sample_Rates { rate = 65000  , valid = true , subset = true  },
			new Sample_Rates { rate = 65535  , valid = true , subset = true  },
			new Sample_Rates { rate = 65536  , valid = true , subset = false },
			new Sample_Rates { rate = 65540  , valid = true , subset = true  },
			new Sample_Rates { rate = 65550  , valid = true , subset = true  },
			new Sample_Rates { rate = 65555  , valid = true , subset = false },
			new Sample_Rates { rate = 66000  , valid = true , subset = true  },
			new Sample_Rates { rate = 66001  , valid = true , subset = false },
			new Sample_Rates { rate = 96000  , valid = true , subset = true  },
			new Sample_Rates { rate = 100000 , valid = true , subset = true  },
			new Sample_Rates { rate = 100001 , valid = true , subset = false },
			new Sample_Rates { rate = 192000 , valid = true , subset = true  },
			new Sample_Rates { rate = 500000 , valid = true , subset = true  },
			new Sample_Rates { rate = 500001 , valid = true , subset = false },
			new Sample_Rates { rate = 500010 , valid = true , subset = true  },
			new Sample_Rates { rate = 655349 , valid = true , subset = false },
			new Sample_Rates { rate = 655350 , valid = true , subset = true  },
			new Sample_Rates { rate = 655351 , valid = true , subset = false },
			new Sample_Rates { rate = 655360 , valid = true , subset = false },
			new Sample_Rates { rate = 700000 , valid = true , subset = false },
			new Sample_Rates { rate = 700010 , valid = true , subset = false },
			new Sample_Rates { rate = 705600 , valid = true , subset = false },
			new Sample_Rates { rate = 768000 , valid = true , subset = false },
			new Sample_Rates { rate = 1000000, valid = true , subset = false },
			new Sample_Rates { rate = 1048575, valid = true , subset = false },
			new Sample_Rates { rate = 1100000, valid = false, subset = false }
		};

		private struct VCEntry_Names
		{
			public string @string;
			public Flac__bool valid;
		}

		private static readonly VCEntry_Names[] vcEntryNames =
		{
			new VCEntry_Names { @string = ""    , valid = true  },
			new VCEntry_Names { @string = "a"   , valid = true  },
			new VCEntry_Names { @string = "="   , valid = false },
			new VCEntry_Names { @string = "a="  , valid = false },
			new VCEntry_Names { @string = "\x01", valid = false },
			new VCEntry_Names { @string = "\x1f", valid = false },
			new VCEntry_Names { @string = "\x7d", valid = true  },
			new VCEntry_Names { @string = "\x7e", valid = false },
			new VCEntry_Names { @string = "\xff", valid = false }
		};

		private struct VCEntry_Values
		{
			public uint32_t length;
			public Flac__byte[] @string;
			public Flac__bool valid;
		}

		private static readonly VCEntry_Values[] vcEntryValues =
		{
			new VCEntry_Values { length = 0, @string = new Flac__byte[] { }                 , valid = true  },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { }                 , valid = true  },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0x01 }            , valid = true  },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0x7f }            , valid = true  },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0x80 }            , valid = false },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0x81 }            , valid = false },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0xc0 }            , valid = false },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0xe0 }            , valid = false },
			new VCEntry_Values { length = 1, @string = new Flac__byte[] { 0xf0 }            , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xc0, 0x41 }      , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xc1, 0x41 }      , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xc0, 0x85 }      , valid = false },	// Non-shortest form
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xc1, 0x85 }      , valid = false },	// Non-shortest form
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xc2, 0x85 }      , valid = true  },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xe0, 0x41 }      , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xe1, 0x41 }      , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xe0, 0x85 }      , valid = false },
			new VCEntry_Values { length = 2, @string = new Flac__byte[] { 0xe1, 0x85 }      , valid = false },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe0, 0x85, 0x41 }, valid = false },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe1, 0x85, 0x41 }, valid = false },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe0, 0x85, 0x80 }, valid = false },	// Non-shortest form
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe0, 0x95, 0x80 }, valid = false },	// Non-shortest form
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe0, 0xa5, 0x80 }, valid = true  },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe1, 0x85, 0x80 }, valid = true  },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe1, 0x95, 0x80 }, valid = true  },
			new VCEntry_Values { length = 3, @string = new Flac__byte[] { 0xe1, 0xa5, 0x80 }, valid = true  }
		};

		private struct VCEntry_Values_Nt
		{
			public Flac__byte[] @string;
			public Flac__bool valid;
		}

		private static readonly VCEntry_Values_Nt[] vcEntryValuesNt =
		{
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0x00 }                  , valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0x01, 0x00 }            , valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0x7f, 0x00 }            , valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0x80, 0x00 }            , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0x81, 0x00 }            , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc0, 0x00 }            , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x00 }            , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xf0, 0x00 }            , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc0, 0x41, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc1, 0x41, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc0, 0x85, 0x00 }      , valid = false },	// Non-shortest form
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc1, 0x85, 0x00 }      , valid = false },	// Non-shortest form
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xc2, 0x85, 0x00 }      , valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x41, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0x41, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x85, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0x85, 0x00 }      , valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x85, 0x41, 0x00 }, valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0x85, 0x41, 0x00 }, valid = false },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x85, 0x80, 0x00 }, valid = false },	// Non-shortest form
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0x95, 0x80, 0x00 }, valid = false },	// Non-shortest form
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe0, 0xa5, 0x80, 0x00 }, valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0x85, 0x80, 0x00 }, valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0x95, 0x80, 0x00 }, valid = true  },
			new VCEntry_Values_Nt { @string = new Flac__byte[] { 0xe1, 0xa5, 0x80, 0x00 }, valid = true  }
		};

		private struct VCEntries
		{
			public uint32_t length;
			public Flac__byte[] @string;
			public Flac__bool valid;
		}

		private static readonly VCEntries[] vcEntries =
		{
			new VCEntries { length = 0, @string = new Flac__byte[] { }                             , valid = false },
			new VCEntries { length = 1, @string = new Flac__byte[] { 0x61 }                        , valid = false },
			new VCEntries { length = 1, @string = new Flac__byte[] { 0x3d }                        , valid = true  },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0x61, 0x3d }                  , valid = true  },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0x01, 0x3d }                  , valid = false },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0x1f, 0x3d }                  , valid = false },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0x7d, 0x3d }                  , valid = true  },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0x7e, 0x3d }                  , valid = false },
			new VCEntries { length = 2, @string = new Flac__byte[] { 0xff, 0x3d }                  , valid = false },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0x01 }            , valid = true  },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0x7f }            , valid = true  },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0x80 }            , valid = false },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0x81 }            , valid = false },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0xc0 }            , valid = false },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0 }            , valid = false },
			new VCEntries { length = 3, @string = new Flac__byte[] { 0x61, 0x3d, 0xf0 }            , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xc0, 0x41 }      , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xc1, 0x41 }      , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xc0, 0x85 }      , valid = false },	// Non-shortest form
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xc1, 0x85 }      , valid = false },	// Non-shortest form
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xc2, 0x85 }      , valid = true  },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0x41 }      , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0x41 }      , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0x85 }      , valid = false },
			new VCEntries { length = 4, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0x85 }      , valid = false },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0x85, 0x41 }, valid = false },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0x85, 0x41 }, valid = false },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0x85, 0x80 }, valid = false },	// Non-shortest form
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0x95, 0x80 }, valid = false },	// Non-shortest form
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe0, 0xa5, 0x80 }, valid = true  },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0x85, 0x80 }, valid = true  },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0x95, 0x80 }, valid = true  },
			new VCEntries { length = 5, @string = new Flac__byte[] { 0x61, 0x3d, 0xe1, 0xa5, 0x80 }, valid = true  }
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_Sample_Rate()
		{
			for (uint32_t i = 0; i < sampleRates.Length; i++)
			{
				Console.WriteLine($"Testing {sampleRates[i].rate}");
				Assert.AreEqual(sampleRates[i].valid, Format.Flac__Format_Sample_Rate_Is_Valid(sampleRates[i].rate));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_Sample_Rate_Subset()
		{
			for (uint32_t i = 0; i < sampleRates.Length; i++)
			{
				Console.WriteLine($"Testing {sampleRates[i].rate}");
				Assert.AreEqual(sampleRates[i].subset, Format.Flac__Format_Sample_Rate_Is_Subset(sampleRates[i].rate));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_VorbisComment_Entry_Name()
		{
			for (uint32_t i = 0; i < vcEntryNames.Length; i++)
			{
				Console.WriteLine($"Testing {vcEntryNames[i].@string}");
				Assert.AreEqual(vcEntryNames[i].valid, Format.Flac__Format_VorbisComment_Entry_Name_Is_Legal(vcEntryNames[i].@string));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_VorbisComment_Entry_Value()
		{
			for (uint32_t i = 0; i < vcEntryValues.Length; i++)
			{
				Console.WriteLine($"Testing {BitConverter.ToString(vcEntryValues[i].@string)}");
				Assert.AreEqual(vcEntryValues[i].valid, Format.Flac__Format_VorbisComment_Entry_Value_Is_Legal(vcEntryValues[i].@string, vcEntryValues[i].length));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_VorbisComment_Entry_Value_Null_Terminated()
		{
			for (uint32_t i = 0; i < vcEntryValuesNt.Length; i++)
			{
				Console.WriteLine($"Testing {BitConverter.ToString(vcEntryValuesNt[i].@string)}");
				Assert.AreEqual(vcEntryValuesNt[i].valid, Format.Flac__Format_VorbisComment_Entry_Value_Is_Legal(vcEntryValuesNt[i].@string, uint32_t.MaxValue));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Format_VorbisComment_Entry()
		{
			for (uint32_t i = 0; i < vcEntries.Length; i++)
			{
				Console.WriteLine($"Testing {BitConverter.ToString(vcEntries[i].@string)}");
				Assert.AreEqual(vcEntries[i].valid, Format.Flac__Format_VorbisComment_Entry_Is_Legal(vcEntries[i].@string, vcEntries[i].length));
			}
		}
	}
}
