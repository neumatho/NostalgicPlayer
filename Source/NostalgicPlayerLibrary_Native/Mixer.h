/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
#pragma once

#include "stdafx.h"
#include "Export.h"

extern "C"
{
	EXPORTAPI(INT32, Mix8MonoNormal(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel));
	EXPORTAPI(INT32, Mix8StereoNormal(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel));

	EXPORTAPI(INT32, Mix8MonoInterp(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 oldLVol, INT32* rampVol));
	EXPORTAPI(INT32, Mix8StereoInterp(const INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel, INT32 oldLVol, INT32 oldRVol, INT32 *rampVol));

	EXPORTAPI(void, AddAmigaFilter(bool stereo, INT32* dest, INT32 todo, INT32* filterPrevLeft, INT32* filterPrevRight));

	EXPORTAPI(void, MixConvertTo16(INT16* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers));
	EXPORTAPI(void, MixConvertTo32(INT32* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers));

	EXPORTAPI(INT32, ResampleMonoToMonoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 volSel));
	EXPORTAPI(INT32, ResampleMonoToStereoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 volSel));

	EXPORTAPI(INT32, ResampleStereoToMonoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel));
	EXPORTAPI(INT32, ResampleStereoToStereoNormal(const INT32* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel));

	EXPORTAPI(void, ResampleConvertTo16(INT16* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers));
	EXPORTAPI(void, ResampleConvertTo32(INT32* dest, INT32 offset, const INT32* source, INT32 count, BOOL swapSpeakers));
}