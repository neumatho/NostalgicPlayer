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

#define EXPORTAPI(returnType, methodSignature) __declspec(dllexport) returnType WINAPI methodSignature
#define OUTARG *
