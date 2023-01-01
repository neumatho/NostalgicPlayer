/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.AudioIff
{
	/// <summary>
	/// Helper class for IEEE extended (80-bit) conversion
	/// </summary>
	internal static class IeeeExtended
	{
		/******************************************************************************/
		/* C O N V E R T   F R O M   I E E E   E X T E N D E D                        */
		/*                                                                            */
		/* Copyright (C) 1988-1991 Apple Computer, Inc.                               */
		/* All rights reserved.                                                       */
		/*                                                                            */
		/* Machine-independent I/O routines for IEEE floating-point numbers.          */
		/*                                                                            */
		/* NaN's and infinities are converted to HUGE_VAL or HUGE, which              */
		/* happens to be infinity on IEEE machines.  Unfortunately, it is             */
		/* impossible to preserve NaN's in a machine-independent way.                 */
		/* Infinities are, however, preserved on IEEE machines.                       */
		/*                                                                            */
		/* These routines have been tested on the following machines:                 */
		/*    Apple Macintosh, MPW 3.1 C compiler                                     */
		/*    Apple Macintosh, THINK C compiler                                       */
		/*    Silicon Graphics IRIS, MIPS compiler                                    */
		/*    Cray X/MP and Y/MP                                                      */
		/*    Digital Equipment VAX                                                   */
		/*                                                                            */
		/*                                                                            */
		/* Implemented by Malcolm Slaney and Ken Turkowski.                           */
		/*                                                                            */
		/* Malcolm Slaney contributions during 1988-1990 include big- and little-     */
		/* endian file I/O, conversion to and from Motorola's extended 80-bit         */
		/* floating-point format, and conversions to and from IEEE single-            */
		/* precision floating-point format.                                           */
		/*                                                                            */
		/* In 1991, Ken Turkowski implemented the conversions to and from             */
		/* IEEE double-precision format, added more precision to the extended         */
		/* conversions, and accommodated conversions involving +/- infinity,          */
		/* NaN's, and denormalized numbers.                                           */
		/******************************************************************************/
		public static double ConvertFromIeeeExtended(byte[] bytes)
		{
			double f;

			int expon = ((bytes[0] & 0x7f) << 8) | (bytes[1] & 0xff);
			ulong hiMant = ((ulong)(bytes[2] & 0xff) << 24) |
						   ((ulong)(bytes[3] & 0xff) << 16) |
						   ((ulong)(bytes[4] & 0xff) << 8) |
						   ((ulong)(bytes[5] & 0xff) << 0);
			ulong loMant = ((ulong)(bytes[6] & 0xff) << 24) |
						   ((ulong)(bytes[7] & 0xff) << 16) |
						   ((ulong)(bytes[8] & 0xff) << 8) |
						   ((ulong)(bytes[9] & 0xff) << 0);

			if ((expon == 0) && (hiMant == 0) && (loMant == 0))
				f = 0;
			else
			{
				if (expon == 0x7fff)
				{
					// Infinity or NaN
					f = double.NaN;
				}
				else
				{
					expon -= 16383;
					f = C.math.ldexp(UnsignedToFloat(hiMant), expon -= 31);
					f += C.math.ldexp(UnsignedToFloat(loMant), expon -= 32);
				}
			}

			if ((bytes[0] & 0x80) != 0)
				return -f;

			return f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double UnsignedToFloat(ulong u)
		{
			return ((long)(u - 2147483647 - 1)) + 2147483648.0;
		}



		/******************************************************************************/
		/* C O N V E R T   T O   I E E E   E X T E N D E D                            */
		/*                                                                            */
		/* Copyright (C) 1988-1991 Apple Computer, Inc.                               */
		/* All rights reserved.                                                       */
		/*                                                                            */
		/* Machine-independent I/O routines for IEEE floating-point numbers.          */
		/*                                                                            */
		/* NaN's and infinities are converted to HUGE_VAL or HUGE, which happens to   */
		/* be infinity on IEEE machines.  Unfortunately, it is impossible to preserve */
		/* NaN's in a machine-independent way.                                        */
		/* Infinities are, however, preserved on IEEE machines.                       */
		/*                                                                            */
		/* These routines have been tested on the following machines:                 */
		/*    Apple Macintosh, MPW 3.1 C compiler                                     */
		/*    Apple Macintosh, THINK C compiler                                       */
		/*    Silicon Graphics IRIS, MIPS compiler                                    */
		/*    Cray X/MP and Y/MP                                                      */
		/*    Digital Equipment VAX                                                   */
		/*                                                                            */
		/*                                                                            */
		/* Implemented by Malcolm Slaney and Ken Turkowski.                           */
		/*                                                                            */
		/* Malcolm Slaney contributions during 1988-1990 include big- and little-     */
		/* endian file I/O, conversion to and from Motorola's extended 80-bit         */
		/* floating-point format, and conversions to and from IEEE single-            */
		/* precision floating-point format.                                           */
		/*                                                                            */
		/* In 1991, Ken Turkowski implemented the conversions to and from             */
		/* IEEE double-precision format, added more precision to the extended         */
		/* conversions, and accommodated conversions involving +/- infinity,          */
		/* NaN's, and denormalized numbers.                                           */
		/******************************************************************************/
		public static byte[] ConvertToIeeeExtended(double num)
		{
			byte[] bytes = new byte[10];

			int sign;

			if (num < 0)
			{
				sign = 0x8000;
				num *= -1.0;
			}
			else
				sign = 0;

			int expon = 0;
			ulong hiMant, loMant;

			if (num == 0.0)
			{
				hiMant = 0;
				loMant = 0;
			}
			else
			{
				double fMant = C.math.frexp(num, out expon);
				if ((expon > 16384) || !(fMant < 1.0))
				{
					// Infinity or NaN
					expon = sign | 0x7fff;
					hiMant = 0;
					loMant = 0;		// Infinity
				}
				else
				{
					// Finite
					expon += 16382;
					if (expon < 0)
					{
						// Denormalized
						fMant = C.math.ldexp(fMant, expon);
						expon = 0;
					}

					expon |= sign;
					fMant = C.math.ldexp(fMant, 32);
					double fsMant = Math.Floor(fMant);
					hiMant = FloatToUnsigned(fsMant);
					fMant = C.math.ldexp(fMant - fsMant, 32);
					fsMant = Math.Floor(fMant);
					loMant = FloatToUnsigned(fsMant);
				}
			}

			bytes[0] = (byte)(expon >> 8);
			bytes[1] = (byte)expon;
			bytes[2] = (byte)(hiMant >> 24);
			bytes[3] = (byte)(hiMant >> 16);
			bytes[4] = (byte)(hiMant >> 8);
			bytes[5] = (byte)hiMant;
			bytes[6] = (byte)(loMant >> 24);
			bytes[7] = (byte)(loMant >> 16);
			bytes[8] = (byte)(loMant >> 8);
			bytes[9] = (byte)loMant;

			return bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong FloatToUnsigned(double f)
		{
			return (ulong)(((long)(f - 2147483648.0)) + 2147483647L) + 1;
		}
	}
}
