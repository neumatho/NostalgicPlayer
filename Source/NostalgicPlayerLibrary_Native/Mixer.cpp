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

	#pragma region Mixers
	
	#pragma region 32 bit mixers (only used on 32-bit platforms)

	#ifdef MIX32
	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8MonoNormal32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel))
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
	EXPORTAPI(INT32, Mix8StereoNormal32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
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
	/// Mixes a 8 bit surround sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8SurroundNormal32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		if (lVolSel >= rVolSel)
		{
			while (todo--)
			{
				INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
				index += increment;

				*dest++ += lVolSel * sample;
				*dest++ -= lVolSel * sample;
			}
		}
		else
		{
			while (todo--)
			{
				INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
				index += increment;

				*dest++ -= rVolSel * sample;
				*dest++ += rVolSel * sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer with interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8MonoInterp32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32 *rampVol))
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
	EXPORTAPI(INT32, Mix8StereoInterp32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
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
	/// Mixes a 8 bit surround sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix8SurroundInterp32(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 oldVol, vol;
		INT32 sample;

		if (lVolSel >= rVolSel)
		{
			vol = lVolSel;
			oldVol = oldLVol;
		}
		else
		{
			vol = rVolSel;
			oldVol = oldRVol;
		}

		if (ramVol != 0)
		{
			oldVol -= vol;

			while (todo--)
			{
				sample = (static_cast<INT32>(source[index >> FRACBITS]) << 8) +
					(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 8) -
						(static_cast<INT32>(source[index >> FRACBITS]) << 8)) *
						(index & FRACMASK) >> FRACBITS);

				index += increment;

				sample = ((vol << CLICK_SHIFT) + oldVol * ramVol) * sample >> CLICK_SHIFT;
				*dest++ += sample;
				*dest++ -= sample;

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

			*dest++ += vol * sample;
			*dest++ -= vol * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16MonoNormal32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = source[index >> FRACBITS];
			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16StereoNormal32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = source[index >> FRACBITS];
			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit surround sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16SurroundNormal32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		if (lVolSel >= rVolSel)
		{
			while (todo--)
			{
				INT16 sample = source[index >> FRACBITS];
				index += increment;

				*dest++ += lVolSel * sample;
				*dest++ -= lVolSel * sample;
			}
		}
		else
		{
			while (todo--)
			{
				INT16 sample = source[index >> FRACBITS];
				index += increment;

				*dest++ -= rVolSel * sample;
				*dest++ += rVolSel * sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a mono output buffer with interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16MonoInterp32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 sample;

		if (ramVol != 0)
		{
			oldLVol -= lVolSel;

			while (todo--)
			{
				sample = static_cast<INT32>(source[index >> FRACBITS]) +
					((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT32>(source[index >> FRACBITS])) *
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
			sample = static_cast<INT32>(source[index >> FRACBITS]) +
				((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT32>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16StereoInterp32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
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
				sample = static_cast<INT32>(source[index >> FRACBITS]) +
					((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT32>(source[index >> FRACBITS])) *
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
			sample = static_cast<INT32>(source[index >> FRACBITS]) +
				((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT32>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit surround sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, Mix16SurroundInterp32(const INT16* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 oldVol, vol;
		INT32 sample;

		if (lVolSel >= rVolSel)
		{
			vol = lVolSel;
			oldVol = oldLVol;
		}
		else
		{
			vol = rVolSel;
			oldVol = oldRVol;
		}
		
		if (ramVol != 0)
		{
			oldVol -= vol;

			while (todo--)
			{
				sample = static_cast<INT32>(source[index >> FRACBITS]) +
					((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT32>(source[index >> FRACBITS])) *
						(index & FRACMASK) >> FRACBITS);

				index += increment;

				sample = ((lVolSel << CLICK_SHIFT) + oldLVol * ramVol) * sample >> CLICK_SHIFT;
				*dest++ += sample;
				*dest++ -= sample;

				if (--ramVol == 0)
					break;
			}

			*rampVol = ramVol;
			if (todo < 0)
				return index;
		}

		while (todo--)
		{
			sample = static_cast<INT32>(source[index >> FRACBITS]) +
				((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT32>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += vol * sample;
			*dest++ -= vol * sample;
		}

		return index;
	}
	#endif

	#pragma endregion

	#pragma region 64 bit mixers (used on both platforms)
	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix8MonoNormal64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel))
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
	EXPORTAPI(INT64, Mix8StereoNormal64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
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
	/// Mixes a 8 bit surround sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix8SurroundNormal64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		if (lVolSel >= rVolSel)
		{
			while (todo--)
			{
				INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
				index += increment;

				*dest++ += lVolSel * sample;
				*dest++ -= lVolSel * sample;
			}
		}
		else
		{
			while (todo--)
			{
				INT16 sample = (static_cast<INT16>(source[index >> FRACBITS])) << 8;
				index += increment;

				*dest++ -= rVolSel * sample;
				*dest++ += rVolSel * sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit sample into a mono output buffer with interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix8MonoInterp64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 sample;

		if (ramVol != 0)
		{
			oldLVol -= lVolSel;

			while (todo--)
			{
				sample = static_cast<INT32>((static_cast<INT64>(source[index >> FRACBITS]) << 7) +
					(((static_cast<INT64>(source[(index >> FRACBITS) + 1]) << 7) -
						(static_cast<INT64>(source[index >> FRACBITS]) << 7)) *
						(index & FRACMASK) >> FRACBITS));

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
			sample = static_cast<INT32>((static_cast<INT64>(source[index >> FRACBITS]) << 7) +
				(((static_cast<INT64>(source[(index >> FRACBITS) + 1]) << 7) -
					(static_cast<INT64>(source[index >> FRACBITS]) << 7)) *
					(index & FRACMASK) >> FRACBITS));

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
	EXPORTAPI(INT64, Mix8StereoInterp64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
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
				sample = static_cast<INT32>((static_cast<INT64>(source[index >> FRACBITS]) << 8) +
					(((static_cast<INT64>(source[(index >> FRACBITS) + 1]) << 8) -
						(static_cast<INT64>(source[index >> FRACBITS]) << 8)) *
						(index & FRACMASK) >> FRACBITS));

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
			sample = static_cast<INT32>((static_cast<INT64>(source[index >> FRACBITS]) << 8) +
				(((static_cast<INT64>(source[(index >> FRACBITS) + 1]) << 8) -
					(static_cast<INT64>(source[index >> FRACBITS]) << 8)) *
					(index & FRACMASK) >> FRACBITS));

			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 8 bit surround sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix8SurroundInterp64(const INT8* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 oldVol, vol;
		INT32 sample;

		if (lVolSel >= rVolSel)
		{
			vol = lVolSel;
			oldVol = oldLVol;
		}
		else
		{
			vol = rVolSel;
			oldVol = oldRVol;
		}

		if (ramVol != 0)
		{
			oldVol -= vol;

			while (todo--)
			{
				sample = (static_cast<INT32>(source[index >> FRACBITS]) << 8) +
					(((static_cast<INT32>(source[(index >> FRACBITS) + 1]) << 8) -
						(static_cast<INT32>(source[index >> FRACBITS]) << 8)) *
						(index & FRACMASK) >> FRACBITS);

				index += increment;

				sample = ((vol << CLICK_SHIFT) + oldVol * ramVol) * sample >> CLICK_SHIFT;
				*dest++ += sample;
				*dest++ -= sample;

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

			*dest++ += vol * sample;
			*dest++ -= vol * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16MonoNormal64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = source[index >> FRACBITS];
			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16StereoNormal64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		while (todo--)
		{
			INT16 sample = source[index >> FRACBITS];
			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit surround sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16SurroundNormal64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		dest += offset;

		if (lVolSel >= rVolSel)
		{
			while (todo--)
			{
				INT16 sample = source[index >> FRACBITS];
				index += increment;

				*dest++ += lVolSel * sample;
				*dest++ -= lVolSel * sample;
			}
		}
		else
		{
			while (todo--)
			{
				INT16 sample = source[index >> FRACBITS];
				index += increment;

				*dest++ -= rVolSel * sample;
				*dest++ += rVolSel * sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a mono output buffer with interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16MonoInterp64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 sample;

		if (ramVol != 0)
		{
			oldLVol -= lVolSel;

			while (todo--)
			{
				sample = static_cast<INT32>(static_cast<INT64>(source[index >> FRACBITS]) +
					((static_cast<INT64>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT64>(source[index >> FRACBITS])) *
						(index & FRACMASK) >> FRACBITS));

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
			sample = static_cast<INT32>(static_cast<INT64>(source[index >> FRACBITS]) +
				((static_cast<INT64>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT64>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS));

			index += increment;

			*dest++ += lVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16StereoInterp64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
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
				sample = static_cast<INT32>(static_cast<INT64>(source[index >> FRACBITS]) +
					((static_cast<INT64>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT64>(source[index >> FRACBITS])) *
						(index & FRACMASK) >> FRACBITS));

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
			sample = static_cast<INT32>(static_cast<INT64>(source[index >> FRACBITS]) +
				((static_cast<INT64>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT64>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS));

			index += increment;

			*dest++ += lVolSel * sample;
			*dest++ += rVolSel * sample;
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Mixes a 16 bit surround sample into a stereo output buffer with
	/// interpolation
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT64, Mix16SurroundInterp64(const INT16* source, INT32* dest, INT32 offset, INT64 index, INT64 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32* rampVol))
	{
		dest += offset;

		INT32 ramVol = *rampVol;
		INT32 oldVol, vol;
		INT32 sample;

		if (lVolSel >= rVolSel)
		{
			vol = lVolSel;
			oldVol = oldLVol;
		}
		else
		{
			vol = rVolSel;
			oldVol = oldRVol;
		}

		if (ramVol != 0)
		{
			oldVol -= vol;

			while (todo--)
			{
				sample = static_cast<INT32>(source[index >> FRACBITS]) +
					((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
						static_cast<INT32>(source[index >> FRACBITS])) *
						(index & FRACMASK) >> FRACBITS);

				index += increment;

				sample = ((lVolSel << CLICK_SHIFT) + oldLVol * ramVol) * sample >> CLICK_SHIFT;
				*dest++ += sample;
				*dest++ -= sample;

				if (--ramVol == 0)
					break;
			}

			*rampVol = ramVol;
			if (todo < 0)
				return index;
		}

		while (todo--)
		{
			sample = static_cast<INT32>(source[index >> FRACBITS]) +
				((static_cast<INT32>(source[(index >> FRACBITS) + 1]) -
					static_cast<INT32>(source[index >> FRACBITS])) *
					(index & FRACMASK) >> FRACBITS);

			index += increment;

			*dest++ += vol * sample;
			*dest++ -= vol * sample;
		}

		return index;
	}
	#pragma endregion

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
	#define MIX_BITSHIFT_16					9
	#define MIX_EXTRACT_SAMPLE_16(var)		var = *source++ >> MIX_BITSHIFT_16
	#define MIX_CHECK_SAMPLE_16(var, bound)	var = (var >= bound) ? bound - 1 : (var < -bound) ? -bound : var
	#define MIX_PUT_SAMPLE_16(var)			*dest++ = (INT16)var

	EXPORTAPI(void, MixConvertTo16(INT16* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers))
	{
		INT32 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		if (swapSpeakers)
		{
			for (count >>= 2; count; count--)
			{
				MIX_EXTRACT_SAMPLE_16(x1);
				MIX_EXTRACT_SAMPLE_16(x2);
				MIX_EXTRACT_SAMPLE_16(x3);
				MIX_EXTRACT_SAMPLE_16(x4);

				MIX_CHECK_SAMPLE_16(x1, 32767);
				MIX_CHECK_SAMPLE_16(x2, 32767);
				MIX_CHECK_SAMPLE_16(x3, 32767);
				MIX_CHECK_SAMPLE_16(x4, 32767);

				MIX_PUT_SAMPLE_16(x2);
				MIX_PUT_SAMPLE_16(x1);
				MIX_PUT_SAMPLE_16(x4);
				MIX_PUT_SAMPLE_16(x3);
			}

			// We know it is always stereo samples when coming here
			while (remain > 0)
			{
				MIX_EXTRACT_SAMPLE_16(x1);
				MIX_EXTRACT_SAMPLE_16(x2);

				MIX_CHECK_SAMPLE_16(x1, 32767);
				MIX_CHECK_SAMPLE_16(x2, 32767);

				MIX_PUT_SAMPLE_16(x2);
				MIX_PUT_SAMPLE_16(x1);

				remain -= 2;
			}
		}
		else
		{
			for (count >>= 2; count; count--)
			{
				MIX_EXTRACT_SAMPLE_16(x1);
				MIX_EXTRACT_SAMPLE_16(x2);
				MIX_EXTRACT_SAMPLE_16(x3);
				MIX_EXTRACT_SAMPLE_16(x4);

				MIX_CHECK_SAMPLE_16(x1, 32767);
				MIX_CHECK_SAMPLE_16(x2, 32767);
				MIX_CHECK_SAMPLE_16(x3, 32767);
				MIX_CHECK_SAMPLE_16(x4, 32767);

				MIX_PUT_SAMPLE_16(x1);
				MIX_PUT_SAMPLE_16(x2);
				MIX_PUT_SAMPLE_16(x3);
				MIX_PUT_SAMPLE_16(x4);
			}

			while (remain--)
			{
				MIX_EXTRACT_SAMPLE_16(x1);
				MIX_CHECK_SAMPLE_16(x1, 32767);
				MIX_PUT_SAMPLE_16(x1);
			}
		}
	}



	/********************************************************************/
	/// <summary>
	/// Converts the mixed data to a 32 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define MIX_BITSHIFT_32					7
	#define MIX_EXTRACT_SAMPLE_32(var)		var = ((INT64)(*source++)) << MIX_BITSHIFT_32
	#define MIX_CHECK_SAMPLE_32(var, bound)	var = (var >= bound) ? bound - 1 : (var < -bound) ? -bound : var
	#define MIX_PUT_SAMPLE_32(var)			*dest++ = (INT32)var

	EXPORTAPI(void, MixConvertTo32(INT32* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers))
	{
		INT64 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		if (swapSpeakers)
		{
			for (count >>= 2; count; count--)
			{
				MIX_EXTRACT_SAMPLE_32(x1);
				MIX_EXTRACT_SAMPLE_32(x2);
				MIX_EXTRACT_SAMPLE_32(x3);
				MIX_EXTRACT_SAMPLE_32(x4);

				MIX_CHECK_SAMPLE_32(x1, 2147483647);
				MIX_CHECK_SAMPLE_32(x2, 2147483647);
				MIX_CHECK_SAMPLE_32(x3, 2147483647);
				MIX_CHECK_SAMPLE_32(x4, 2147483647);

				MIX_PUT_SAMPLE_32(x2);
				MIX_PUT_SAMPLE_32(x1);
				MIX_PUT_SAMPLE_32(x4);
				MIX_PUT_SAMPLE_32(x3);
			}

			// We know it is always stereo samples when coming here
			while (remain > 0)
			{
				MIX_EXTRACT_SAMPLE_32(x1);
				MIX_EXTRACT_SAMPLE_32(x2);

				MIX_CHECK_SAMPLE_32(x1, 2147483647);
				MIX_CHECK_SAMPLE_32(x2, 2147483647);

				MIX_PUT_SAMPLE_32(x2);
				MIX_PUT_SAMPLE_32(x1);

				remain -= 2;
			}
		}
		else
		{
			for (count >>= 2; count; count--)
			{
				MIX_EXTRACT_SAMPLE_32(x1);
				MIX_EXTRACT_SAMPLE_32(x2);
				MIX_EXTRACT_SAMPLE_32(x3);
				MIX_EXTRACT_SAMPLE_32(x4);

				MIX_CHECK_SAMPLE_32(x1, 2147483647);
				MIX_CHECK_SAMPLE_32(x2, 2147483647);
				MIX_CHECK_SAMPLE_32(x3, 2147483647);
				MIX_CHECK_SAMPLE_32(x4, 2147483647);

				MIX_PUT_SAMPLE_32(x1);
				MIX_PUT_SAMPLE_32(x2);
				MIX_PUT_SAMPLE_32(x3);
				MIX_PUT_SAMPLE_32(x4);
			}

			while (remain--)
			{
				MIX_EXTRACT_SAMPLE_32(x1);
				MIX_CHECK_SAMPLE_32(x1, 2147483647);
				MIX_PUT_SAMPLE_32(x1);
			}
		}
	}
	#pragma endregion

	#pragma region Resampler
	/********************************************************************/
	/// <summary>
	/// Resample a mono sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, ResampleMonoToMonoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 volSel))
	{
		INT32 sample;

		dest += offset;

		if (volSel == 256)
		{
			while (todo--)
			{
				sample = source[index >> FRACBITS];
				index += increment;

				*dest++ = sample;
			}
		}
		else
		{
			const float volDiv = 256.0f / static_cast<float>(volSel);

			while (todo--)
			{
				sample = source[index >> FRACBITS] / volDiv;
				index += increment;

				*dest++ = sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Resample a mono sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, ResampleMonoToStereoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 volSel))
	{
		INT32 sample;

		dest += offset;

		if (volSel == 256)
		{
			while (todo--)
			{
				sample = source[index >> FRACBITS];
				index += increment;

				*dest++ = sample;
				*dest++ = sample;
			}
		}
		else
		{
			const float volDiv = 256.0f / static_cast<float>(volSel);

			while (todo--)
			{
				sample = source[index >> FRACBITS] / volDiv;
				index += increment;

				*dest++ = sample;
				*dest++ = sample;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Resample a stereo sample into a mono output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, ResampleStereoToMonoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		INT64 sample1, sample2;

		dest += offset;

		if ((lVolSel == 256) && (rVolSel == 256))
		{
			while (todo--)
			{
				sample1 = source[(index >> FRACBITS) * 2];
				sample2 = source[(index >> FRACBITS) * 2 + 1];
				index += increment;

				*dest++ = (sample1 + sample2) / 2;
			}
		}
		else
		{
			const float lVolDiv = lVolSel == 0 ? 0 : 256.0f / static_cast<float>(lVolSel);
			const float rVolDiv = rVolSel == 0 ? 0 : 256.0f / static_cast<float>(rVolSel);

			while (todo--)
			{
				sample1 = lVolSel == 0 ? 0 : source[(index >> FRACBITS) * 2] / lVolDiv;
				sample2 = rVolSel == 0 ? 0 : source[(index >> FRACBITS) * 2 + 1] / rVolDiv;
				index += increment;

				*dest++ = (sample1 + sample2) / 2;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Resample a stereo sample into a stereo output buffer
	/// </summary>
	/********************************************************************/
	EXPORTAPI(INT32, ResampleStereoToStereoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel))
	{
		INT32 sample1, sample2;

		dest += offset;

		if ((lVolSel == 256) && (rVolSel == 256))
		{
			while (todo--)
			{
				sample1 = source[(index >> FRACBITS) * 2];
				sample2 = source[(index >> FRACBITS) * 2 + 1];
				index += increment;

				*dest++ = sample1;
				*dest++ = sample2;
			}
		}
		else
		{
			const float lVolDiv = lVolSel == 0 ? 0 : 256.0f / static_cast<float>(lVolSel);
			const float rVolDiv = rVolSel == 0 ? 0 : 256.0f / static_cast<float>(rVolSel);

			while (todo--)
			{
				sample1 = lVolSel == 0 ? 0 : source[(index >> FRACBITS) * 2] / lVolDiv;
				sample2 = rVolSel == 0 ? 0 : source[(index >> FRACBITS) * 2 + 1] / rVolDiv;
				index += increment;

				*dest++ = sample1;
				*dest++ = sample2;
			}
		}

		return index;
	}



	/********************************************************************/
	/// <summary>
	/// Converts the resampled data to a 16 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define BITSHIFT_16					16
	#define EXTRACT_SAMPLE_16(var)		var = *source++ >> BITSHIFT_16
	#define PUT_SAMPLE_16(var)			*dest++ = (INT16)var

	EXPORTAPI(void, ResampleConvertTo16(INT16* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers))
	{
		INT32 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		if (swapSpeakers)
		{
			for (count >>= 2; count; count--)
			{
				EXTRACT_SAMPLE_16(x1);
				EXTRACT_SAMPLE_16(x2);
				EXTRACT_SAMPLE_16(x3);
				EXTRACT_SAMPLE_16(x4);

				PUT_SAMPLE_16(x2);
				PUT_SAMPLE_16(x1);
				PUT_SAMPLE_16(x4);
				PUT_SAMPLE_16(x3);
			}

			// We know it is always stereo samples when coming here
			while (remain > 0)
			{
				EXTRACT_SAMPLE_16(x1);
				EXTRACT_SAMPLE_16(x2);

				PUT_SAMPLE_16(x2);
				PUT_SAMPLE_16(x1);

				remain -= 2;
			}
		}
		else
		{
			for (count >>= 2; count; count--)
			{
				EXTRACT_SAMPLE_16(x1);
				EXTRACT_SAMPLE_16(x2);
				EXTRACT_SAMPLE_16(x3);
				EXTRACT_SAMPLE_16(x4);

				PUT_SAMPLE_16(x1);
				PUT_SAMPLE_16(x2);
				PUT_SAMPLE_16(x3);
				PUT_SAMPLE_16(x4);
			}

			while (remain--)
			{
				EXTRACT_SAMPLE_16(x1);
				PUT_SAMPLE_16(x1);
			}
		}
	}



	/********************************************************************/
	/// <summary>
	/// Converts the resampled data to a 32 bit sample buffer
	/// </summary>
	/********************************************************************/
	#define EXTRACT_SAMPLE_32(var)		var = *source++
	#define PUT_SAMPLE_32(var)			*dest++ = (INT32)var

	EXPORTAPI(void, ResampleConvertTo32(INT32* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers))
	{
		INT32 x1, x2, x3, x4;

		dest += offset;
		INT32 remain = count & 3;

		if (swapSpeakers)
		{
			for (count >>= 2; count; count--)
			{
				EXTRACT_SAMPLE_32(x1);
				EXTRACT_SAMPLE_32(x2);
				EXTRACT_SAMPLE_32(x3);
				EXTRACT_SAMPLE_32(x4);

				PUT_SAMPLE_32(x2);
				PUT_SAMPLE_32(x1);
				PUT_SAMPLE_32(x4);
				PUT_SAMPLE_32(x3);
			}

			// We know it is always stereo samples when coming here
			while (remain > 0)
			{
				EXTRACT_SAMPLE_32(x1);
				EXTRACT_SAMPLE_32(x2);

				PUT_SAMPLE_32(x2);
				PUT_SAMPLE_32(x1);

				remain -= 2;
			}
		}
		else
		{
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
	#pragma endregion
}