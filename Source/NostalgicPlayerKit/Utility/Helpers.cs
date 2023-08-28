/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Different helper methods
	/// </summary>
	public static class Helpers
	{
		private static readonly Random rand = new Random();

		/********************************************************************/
		/// <summary>
		/// Copy length bytes from one stream to another. If getting out of
		/// data from the source stream, fill up the rest with zeros
		/// </summary>
		/********************************************************************/
		public static void CopyDataForceLength(Stream source, Stream destination, int length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);

				if (len < 1024)
					Array.Clear(buf, len, 1024 - len);

				destination.Write(buf, 0, 1024);

				length -= 1024;
			}

			if (length > 0)
			{
				int len = source.Read(buf, 0, length);
				if (len < length)
					Array.Clear(buf, len, length - len);

				destination.Write(buf, 0, length);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Copy length bytes from one stream to another. If getting out of
		/// data from the source stream, the copying is stopped
		/// </summary>
		/********************************************************************/
		public static void CopyData(Stream source, Stream destination, int length)
		{
			byte[] buf = new byte[1024];

			while (length >= 1024)
			{
				int len = source.Read(buf, 0, 1024);
				if (len == 0)
					return;

				destination.Write(buf, 0, len);

				length -= len;
			}

			if (length > 0)
			{
				int len = source.Read(buf, 0, length);
				if (len == 0)
					return;

				destination.Write(buf, 0, len);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Convert a byte array to a hexadecimal string
		/// </summary>
		/********************************************************************/
		public static string ToHex(byte[] data)
		{
			char[] result = new char[data.Length * 2];

			for (int y = 0, x = 0; y < data.Length; y++)
			{
				byte b = (byte)(data[y] >> 4);
				result[x++] = (char)(b > 9 ? b + 0x37 : b + 0x30);

				b = (byte)(data[y] & 0x0f);
				result[x++] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}

			return new string(result);
		}



		/********************************************************************/
		/// <summary>
		/// Parse an integer from a string and ignoring any trailing garbage
		/// </summary>
		/********************************************************************/
		public static int ParseInt(string s)
		{
			Match match = Regex.Match(s, @"^\s*[\+\-0-9][0-9]*");
			if (match.Success)
				return int.Parse(match.Value);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Parse a float from a string and ignoring any trailing garbage
		/// </summary>
		/********************************************************************/
		public static float ParseFloat(string s)
		{
			Match match = Regex.Match(s, @"^\s*[\+\-0-9e]+");
			if (match.Success)
				return float.Parse(match.Value);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between 0 and int.MaxValue
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber()
		{
			lock (rand)
			{
				return rand.Next();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between 0 and maxValue (exclusive)
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber(int maxValue)
		{
			lock (rand)
			{
				return rand.Next(maxValue);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return a random number between minValue (inclusive) and
		/// maxValue (exclusive)
		/// </summary>
		/********************************************************************/
		public static int GetRandomNumber(int minValue, int maxValue)
		{
			lock (rand)
			{
				return rand.Next(minValue, maxValue);
			}
		}
	}
}
