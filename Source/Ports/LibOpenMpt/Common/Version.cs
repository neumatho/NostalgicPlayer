/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.C.Std.Exceptions;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Parse;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common
{
	/// <summary>
	/// OpenMPT version handling
	/// </summary>
	internal class Version : IEquatable<Version>
	{
		#region LiteralParser class
		/// <summary>
		/// TNE: This is the port of the _LiteralVersionImpl literal operator
		/// that MPT_V expands to. It is internal rather than private, so the
		/// unit test can check the parsing itself the same way the original
		/// test does
		/// </summary>
		internal static class LiteralParser
		{
			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public static Version Parse(string str)
			{
				size_t len = (size_t)str.Length;

				// 0123456789
				// 1.23.45.67
				uint8[] v = [ 0, 0, 0, 0 ];
				size_t field = 0;
				size_t fieldLen = 0;

				for (size_t i = 0; i < len; ++i)
				{
					char c = str[(int)i];

					if (c == '.')
					{
						if (field >= 3)
							throw new invalid_argument();

						if (fieldLen == 0)
							throw new invalid_argument();

						field++;
						fieldLen = 0;
					}
					else if ((('0' <= c) && (c <= '9')) || (('a' <= c) && (c <= 'z')) || (('A' <= c) && (c <= 'Z')))
					{
						fieldLen++;

						if (fieldLen > 2)
							throw new invalid_argument();

						v[field] <<= 4;
						v[field] |= NibbleFromChar(c);
					}
					else
						throw new invalid_argument();
				}

				if (fieldLen == 0)
					throw new invalid_argument();

				return new Version(v[0], v[1], v[2], v[3]);
			}



			/********************************************************************/
			/// <summary>
			///
			/// </summary>
			/********************************************************************/
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static uint8 NibbleFromChar(char x)
			{
				return ('0' <= x) && (x <= '9') ? (uint8)(x - '0' + 0) :
					   ('a' <= x) && (x <= 'z') ? (uint8)(x - 'a' + 10) :
					   ('A' <= x) && (x <= 'Z') ? (uint8)(x - 'A' + 10) : throw new invalid_argument();
			}
		}
		#endregion

		public enum Field
		{
			Major,
			Minor,
			Patch,
			Test
		}

		/// <summary>
		/// e.g. 0x01170208
		/// </summary>
		private readonly uint32 m_Version;

		private static readonly Version Version_Current = Make_Version_Numeric(VersionNumber.MajorMajor, VersionNumber.Major, VersionNumber.Minor, VersionNumber.MinorMinor);

		#region MPT_V versions
		public static class MPT_V
		{
			public static readonly Version _1_17_00_00 = LiteralParser.Parse("1.17.00.00");
			public static readonly Version _1_17_02_46 = LiteralParser.Parse("1.17.02.46");
			public static readonly Version _1_17_02_49 = LiteralParser.Parse("1.17.02.49");
			public static readonly Version _1_17_02_50 = LiteralParser.Parse("1.17.02.50");
			public static readonly Version _1_17_02_51 = LiteralParser.Parse("1.17.02.51");
			public static readonly Version _1_17_02_52 = LiteralParser.Parse("1.17.02.52");
			public static readonly Version _1_17_02_54 = LiteralParser.Parse("1.17.02.54");
			public static readonly Version _1_17_03_02 = LiteralParser.Parse("1.17.03.02");
			public static readonly Version _1_18_00_00 = LiteralParser.Parse("1.18.00.00");
			public static readonly Version _1_18_00_01 = LiteralParser.Parse("1.18.00.01");
			public static readonly Version _1_18_01_00 = LiteralParser.Parse("1.18.01.00");
			public static readonly Version _1_18_02_00 = LiteralParser.Parse("1.18.02.00");
			public static readonly Version _1_18_02_01 = LiteralParser.Parse("1.18.02.01");
			public static readonly Version _1_19_00_00 = LiteralParser.Parse("1.19.00.00");
			public static readonly Version _1_19_00_01 = LiteralParser.Parse("1.19.00.01");
			public static readonly Version _1_19_00_21 = LiteralParser.Parse("1.19.00.21");
			public static readonly Version _1_19_00_30 = LiteralParser.Parse("1.19.00.30");
			public static readonly Version _1_20_00_00 = LiteralParser.Parse("1.20.00.00");
			public static readonly Version _1_20_00_14 = LiteralParser.Parse("1.20.00.14");
			public static readonly Version _1_20_00_35 = LiteralParser.Parse("1.20.00.35");
			public static readonly Version _1_20_00_36 = LiteralParser.Parse("1.20.00.36");
			public static readonly Version _1_20_00_40 = LiteralParser.Parse("1.20.00.40");
			public static readonly Version _1_20_00_53 = LiteralParser.Parse("1.20.00.53");
			public static readonly Version _1_20_00_54 = LiteralParser.Parse("1.20.00.54");
			public static readonly Version _1_20_00_56 = LiteralParser.Parse("1.20.00.56");
			public static readonly Version _1_20_00_62 = LiteralParser.Parse("1.20.00.62");
			public static readonly Version _1_20_00_69 = LiteralParser.Parse("1.20.00.69");
			public static readonly Version _1_20_00_76 = LiteralParser.Parse("1.20.00.76");
			public static readonly Version _1_20_00_77 = LiteralParser.Parse("1.20.00.77");
			public static readonly Version _1_20_01_03 = LiteralParser.Parse("1.20.01.03");
			public static readonly Version _1_20_01_10 = LiteralParser.Parse("1.20.01.10");
			public static readonly Version _1_20_01_11 = LiteralParser.Parse("1.20.01.11");
			public static readonly Version _1_20_02_02 = LiteralParser.Parse("1.20.02.02");
			public static readonly Version _1_20_02_06 = LiteralParser.Parse("1.20.02.06");
			public static readonly Version _1_20_02_10 = LiteralParser.Parse("1.20.02.10");
			public static readonly Version _1_21_01_16 = LiteralParser.Parse("1.21.01.16");
			public static readonly Version _1_21_01_25 = LiteralParser.Parse("1.21.01.25");
			public static readonly Version _1_22_00_00 = LiteralParser.Parse("1.22.00.00");
			public static readonly Version _1_22_01_04 = LiteralParser.Parse("1.22.01.04");
			public static readonly Version _1_22_03_01 = LiteralParser.Parse("1.22.03.01");
			public static readonly Version _1_22_03_02 = LiteralParser.Parse("1.22.03.02");
			public static readonly Version _1_22_03_12 = LiteralParser.Parse("1.22.03.12");
			public static readonly Version _1_22_07_09 = LiteralParser.Parse("1.22.07.09");
			public static readonly Version _1_22_07_19 = LiteralParser.Parse("1.22.07.19");
			public static readonly Version _1_23_01_02 = LiteralParser.Parse("1.23.01.02");
			public static readonly Version _1_23_01_04 = LiteralParser.Parse("1.23.01.04");
			public static readonly Version _1_23_04_03 = LiteralParser.Parse("1.23.04.03");
			public static readonly Version _1_24_00_00 = LiteralParser.Parse("1.24.00.00");
			public static readonly Version _1_24_01_06 = LiteralParser.Parse("1.24.01.06");
			public static readonly Version _1_24_02_02 = LiteralParser.Parse("1.24.02.02");
			public static readonly Version _1_25_00_00 = LiteralParser.Parse("1.25.00.00");
			public static readonly Version _1_25_00_07 = LiteralParser.Parse("1.25.00.07");
			public static readonly Version _1_25_00_19 = LiteralParser.Parse("1.25.00.19");
			public static readonly Version _1_26_00_00 = LiteralParser.Parse("1.26.00.00");
			public static readonly Version _1_26_00_01 = LiteralParser.Parse("1.26.00.01");
			public static readonly Version _1_27_00_00 = LiteralParser.Parse("1.27.00.00");
			public static readonly Version _1_27_00_27 = LiteralParser.Parse("1.27.00.27");
			public static readonly Version _1_27_00_37 = LiteralParser.Parse("1.27.00.37");
			public static readonly Version _1_27_00_49 = LiteralParser.Parse("1.27.00.49");
			public static readonly Version _1_28_00_00 = LiteralParser.Parse("1.28.00.00");
			public static readonly Version _1_28_00_09 = LiteralParser.Parse("1.28.00.09");
			public static readonly Version _1_28_00_12 = LiteralParser.Parse("1.28.00.12");
			public static readonly Version _1_28_00_20 = LiteralParser.Parse("1.28.00.20");
			public static readonly Version _1_28_00_43 = LiteralParser.Parse("1.28.00.43");
			public static readonly Version _1_28_00_44 = LiteralParser.Parse("1.28.00.44");
			public static readonly Version _1_28_02_06 = LiteralParser.Parse("1.28.02.06");
			public static readonly Version _1_28_03_04 = LiteralParser.Parse("1.28.03.04");
			public static readonly Version _1_29 = LiteralParser.Parse("1.29");
			public static readonly Version _1_29_00_00 = LiteralParser.Parse("1.29.00.00");
			public static readonly Version _1_29_00_22 = LiteralParser.Parse("1.29.00.22");
			public static readonly Version _1_29_00_32 = LiteralParser.Parse("1.29.00.32");
			public static readonly Version _1_29_00_34 = LiteralParser.Parse("1.29.00.34");
			public static readonly Version _1_29_00_55 = LiteralParser.Parse("1.29.00.55");
			public static readonly Version _1_29_00_57 = LiteralParser.Parse("1.29.00.57");
			public static readonly Version _1_29_12_02 = LiteralParser.Parse("1.29.12.02");
			public static readonly Version _1_30 = LiteralParser.Parse("1.30");
			public static readonly Version _1_30_00_00 = LiteralParser.Parse("1.30.00.00");
			public static readonly Version _1_30_00_14 = LiteralParser.Parse("1.30.00.14");
			public static readonly Version _1_30_00_34 = LiteralParser.Parse("1.30.00.34");
			public static readonly Version _1_30_00_36 = LiteralParser.Parse("1.30.00.36");
			public static readonly Version _1_30_00_40 = LiteralParser.Parse("1.30.00.40");
			public static readonly Version _1_30_00_45 = LiteralParser.Parse("1.30.00.45");
			public static readonly Version _1_30_00_53 = LiteralParser.Parse("1.30.00.53");
			public static readonly Version _1_30_00_54 = LiteralParser.Parse("1.30.00.54");
			public static readonly Version _1_30_08_02 = LiteralParser.Parse("1.30.08.02");
			public static readonly Version _1_31_00_13 = LiteralParser.Parse("1.31.00.13");
			public static readonly Version _1_31_00_25 = LiteralParser.Parse("1.31.00.25");
			public static readonly Version _1_32_00_13 = LiteralParser.Parse("1.32.00.13");
			public static readonly Version _1_32_00_15 = LiteralParser.Parse("1.32.00.15");
			public static readonly Version _1_32_00_27 = LiteralParser.Parse("1.32.00.27");
			public static readonly Version _1_32_00_29 = LiteralParser.Parse("1.32.00.29");
			public static readonly Version _1_32_00_40 = LiteralParser.Parse("1.32.00.40");
			public static readonly Version _1_32_00_43 = LiteralParser.Parse("1.32.00.43");
			public static readonly Version _1_32_01_02 = LiteralParser.Parse("1.32.01.02");
			public static readonly Version _1_32_02_03 = LiteralParser.Parse("1.32.02.03");
			public static readonly Version _1_32_03_04 = LiteralParser.Parse("1.32.03.04");
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Version()
		{
			m_Version = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Version(uint32 version)
		{
			m_Version = version;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Version(uint8 v1, uint8 v2, uint8 v3, uint8 v4)
		{
			m_Version = ((uint32)v1 << 24) | ((uint32)v2 << 16) | ((uint32)v3 << 8) | ((uint32)v4 << 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 GetRawVersion()
		{
			return m_Version;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Version Masked(uint32 mask)
		{
			return new Version(m_Version & mask);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static Version Make_Version_Numeric(uint8 v0, uint8 v1, uint8 v2, uint8 v3)
		{
			return new Version(v0, v1, v2, v3);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Version Current()
		{
			return Version_Current;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Version Parse(string s)
		{
			uint32 result = 0;
			vector<string> numbers = new vector<string>(s.Split('.'));

			for (size_t i = 0; (i < numbers.size()) && (i < 4); ++i)
				result |= (Parse_.Parse_Hex(numbers[i]) & 0xff) << (int)((3 - i) * 8);

			return new Version(result);
		}



		/********************************************************************/
		/// <summary>
		/// Return a version without build number (the last number in the
		/// version). The current versioning scheme uses this number only for
		/// test builds, and it should be 00 for official builds. So
		/// sometimes it might be wanted to do comparisons without the build
		/// number
		/// </summary>
		/********************************************************************/
		public Version WithoutTestNumber()
		{
			return new Version(m_Version & 0xffffff00U);
		}



		/********************************************************************/
		/// <summary>
		/// Returns true if a given version number is from a test build,
		/// false if it's a release build
		/// </summary>
		/********************************************************************/
		public bool IsTestVersion()
		{
			return (
				// Legacy
				((this > MPT_V._1_17_02_54) && (this < MPT_V._1_18_02_00) && (this != MPT_V._1_18_00_00))
				||
				// Test builds have non-zero VER_MINORMINOR
				((this > MPT_V._1_18_02_00) && ((m_Version & 0xffffff00U) != m_Version))
				);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public override string ToString()//XX 66
		{
			uint32 v = m_Version;

			if (v == 0)
			{
				// Unknown version
				return "Unknown";
			}
			else if ((v & 0xffff) == 0)
			{
				// Only parts of the version number are known (e.g. when reading the version from the IT or S3M file header)
				return string.Format("{0:X}.{1:X2}", (v >> 24) & 0xff, (v >> 16) & 0xff);
			}
			else
			{
				// Full version info available
				return string.Format("{0:X}.{1:X2}.{2:X2}.{3:X2}", (v >> 24) & 0xff, (v >> 16) & 0xff, (v >> 8) & 0xff, v & 0xff);
			}
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator true(Version v)
		{
			return v.m_Version != 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator false(Version v)
		{
			return v.m_Version == 0;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator == (Version me, Version other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return me.GetRawVersion() == other.GetRawVersion();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator != (Version me, Version other)
		{
			return !(me == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator >= (Version me, Version other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return me.GetRawVersion() >= other.GetRawVersion();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator <= (Version me, Version other)
		{
			if (ReferenceEquals(me, other))
				return true;

			if ((me is null) || (other is null))
				return false;

			return me.GetRawVersion() <= other.GetRawVersion();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator > (Version me, Version other)
		{
			if ((me is null) || (other is null))
				return false;

			return me.GetRawVersion() > other.GetRawVersion();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public static bool operator < (Version me, Version other)
		{
			if ((me is null) || (other is null))
				return false;

			return me.GetRawVersion() < other.GetRawVersion();
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override bool Equals(object obj)
		{
			return (obj is Version other) && (this == other);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Version other)
		{
			return this == other;
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public override int GetHashCode()
		{
			return m_Version.GetHashCode();
		}
	}
}
