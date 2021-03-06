/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
#include "Mixer.h"

extern "C"
{
	#define FRACBITS		11
	#define FRACMASK		((1L << FRACBITS) - 1L)

	#define CLICK_SHIFT		6

	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8MonoNormal(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 7;
			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8StereoNormal(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer with interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8MonoInterp(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32 *rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 sample;

		if (ramVol != 0)
		{
			oldLVol -= lVolSel;

			while (todo--)
			{
				sample = (static_cast<INT32>(source[index >> FRACBITS]) << 7) +
							(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 7) -
							(static_cast<INT32>(source[index >> FRACBITS]) << 7)) *
							(index & FRACMASK) >> FRACBITS);

				index += increment;

				*dest++ += ((lVolSel << CLICK_SHIFT) + oldLVol * ramVol) * sample >> CLICK_SHIFT;

				if (--ramVol == 0)
					break;
			}

			*rampVol = ramVol;
			if (todo < 0)
				return index;
		}

		while (todo--)
		{
			sample = (static_cast<INT32>(source[index >> FRACBITS]) << 7) +
						(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 7) -
						(static_cast<INT32>(source[index >> FRACBITS]) << 7)) *
						(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8StereoInterp(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 sample;

		if (ramVol != 0)
		{
			oldLVol -= lVolSel;
			oldRVol -= rVolSel;

			while (todo--)
			{
				sample = (static_cast<INT32>(source[index >> FRACBITS]) << 8) +
					(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 8) -
						(static_cast<INT32>(source[index >> FRACBITS]) << 8)) *
						(index & FRACMASK) >> FRACBITS);

				index += increment;

				*dest++ += ((lVolSel << CLICK_SHIFT) + oldLVol * ramVol) * sample >> CLICK_SHIFT;
				*dest++ += ((rVolSel << CLICK_SHIFT) + oldRVol * ramVol) * sample >> CLICK_SHIFT;

				if (--ramVol == 0)
					break;
			}

			*rampVol = ramVol;
			if (todo < 0)
				return index;
		}

		while (todo--)
		{
			sample = (static_cast<INT32>(source[index >> FRACBITS]) << 8) +
				(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 8) -
					(static_cast<INT32>(source[index >> FRACBITS]) << 8)) *
					(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Adds the Amiga low-pass filter
	/// </summary>
	/********************************************************************/
	EXPORTAPI(void, AddAmigaFilter(bool stereo, INT32* dest, INT32 todo, INT32 *filterPrevLeft, INT32 *filterPrevRight))
	{
		INT32 i;

		if (stereo)
		{
			// Stereo buffer
			todo /= 2;

			for (i = 0; i < todo; i++)
			{
				*filterPrevLeft = ((*dest) + *filterPrevLeft * 3) >> 2;
				*dest++ = *filterPrevLeft;

				*filterPrevRight = ((*dest) + *filterPrevRight * 3) >> 2;
				*dest++ = *filterPrevRight;
			}
		}
		else
		{
			// Mono buffer
			for (i = 0; i < todo; i++)
			{
				*filterPrevLeft = ((*dest) + *filterPrevLeft * 3) >> 2;
				*dest++ = *filterPrevLeft;
			}
		}
	}



	/********************************************************************/
	/// <summary>
	/// Converts the mixed data to a 16 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define BITSHIFT_16					9
	#define EXTRACT_SAMPLE_16(var)		var = *source++ >> BITSHIFT_16
	#define CHECK_SAMPLE_16(var, bound)	var = (var >= bound) ? bound - 1 : (var < -bound) ? -bound : var
	#define PUT_SAMPLE_16(var)			*dest++ = (INT16)var

	EXPORTAPI(void, ConvertTo16(INT16* dest, INT32 offset, const INT32* source, INT32 count))
	{
		INT32 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		for (count >>= 2; count; count--)
		{
			EXTRACT_SAMPLE_16(x1);
			EXTRACT_SAMPLE_16(x2);
			EXTRACT_SAMPLE_16(x3);
			EXTRACT_SAMPLE_16(x4);

			CHECK_SAMPLE_16(x1, 32767);
			CHECK_SAMPLE_16(x2, 32767);
			CHECK_SAMPLE_16(x3, 32767);
			CHECK_SAMPLE_16(x4, 32767);

			PUT_SAMPLE_16(x1);
			PUT_SAMPLE_16(x2);
			PUT_SAMPLE_16(x3);
			PUT_SAMPLE_16(x4);
		}

		while (remain--)
		{
			EXTRACT_SAMPLE_16(x1);
			CHECK_SAMPLE_16(x1, 32767);
			PUT_SAMPLE_16(x1);
		}
	}



	/********************************************************************/
	/// <summary>
	/// Converts the mixed data to a 32 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define BITSHIFT_32					7
	#define EXTRACT_SAMPLE_32(var)		var = ((INT64)(*source++)) << BITSHIFT_32
	#define CHECK_SAMPLE_32(var, bound)	var = (var >= bound) ? bound - 1 : (var < -bound) ? -bound : var
	#define PUT_SAMPLE_32(var)			*dest++ = (INT32)var

	EXPORTAPI(void, ConvertTo32(INT32* dest, INT32 offset, const INT32* source, INT32 count))
	{
		INT64 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		for (count >>= 2; count; count--)
		{
			EXTRACT_SAMPLE_32(x1);
			EXTRACT_SAMPLE_32(x2);
			EXTRACT_SAMPLE_32(x3);
			EXTRACT_SAMPLE_32(x4);

			CHECK_SAMPLE_32(x1, 2147483647);
			CHECK_SAMPLE_32(x2, 2147483647);
			CHECK_SAMPLE_32(x3, 2147483647);
			CHECK_SAMPLE_32(x4, 2147483647);

			PUT_SAMPLE_32(x1);
			PUT_SAMPLE_32(x2);
			PUT_SAMPLE_32(x3);
			PUT_SAMPLE_32(x4);
		}

		while (remain--)
		{
			EXTRACT_SAMPLE_32(x1);
			CHECK_SAMPLE_32(x1, 2147483647);
			PUT_SAMPLE_32(x1);
		}
	}
}