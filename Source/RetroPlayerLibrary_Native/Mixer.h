/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
#pragma once

#include "stdafx.h"
#include "Export.h"

#define FRACBITS 11

extern "C"
{
	EXPORTAPI(INT32, Mix8MonoNormal(INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel));
	EXPORTAPI(INT32, Mix8StereoNormal(INT8* source, INT32* dest, INT32 offset, INT32 index, INT32 increment, INT32 todo, INT32 lVolSel, INT32 rVolSel));

	EXPORTAPI(void, ConvertTo16(INT16* dest, INT32 offset, INT32* source, INT32 count));
	EXPORTAPI(void, ConvertTo32(INT32* dest, INT32 offset, INT32* source, INT32 count));
}