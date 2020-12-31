/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
#include "Mixer.h"

extern "C"
{
	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8MonoNormal(INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
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
	EXPORTAPI(INT32, Mix8StereoNormal(INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
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
	/// Converts the mixed data to a 16 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define BITSHIFT_16					9
	#define EXTRACT_SAMPLE_16(var)		var = *source++ >> BITSHIFT_16
	#define CHECK_SAMPLE_16(var, bound)	var = (var >= bound) ? bound - 1 : (var < -bound) ? -bound : var
	#define PUT_SAMPLE_16(var)			*dest++ = (INT16)var

	EXPORTAPI(void, ConvertTo16(INT16* dest, INT32 offset, INT32* source, INT32 count))
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
	#define EXTRACT_SAMPLE_32(var)		var = *source++ << BITSHIFT_32
	#define PUT_SAMPLE_32(var)			*dest++ = var

	EXPORTAPI(void, ConvertTo32(INT32* dest, INT32 offset, INT32* source, INT32 count))
	{
		INT32 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		for (count >>= 2; count; count--)
		{
			EXTRACT_SAMPLE_32(x1);
			EXTRACT_SAMPLE_32(x2);
			EXTRACT_SAMPLE_32(x3);
			EXTRACT_SAMPLE_32(x4);

			PUT_SAMPLE_32(x1);
			PUT_SAMPLE_32(x2);
			PUT_SAMPLE_32(x3);
			PUT_SAMPLE_32(x4);
		}

		while (remain--)
		{
			EXTRACT_SAMPLE_32(x1);
			PUT_SAMPLE_32(x1);
		}
	}
}