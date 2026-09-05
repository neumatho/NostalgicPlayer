/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using TuningNoteIndexType = System.Int16;	// Some signed integer-type
global using TuningUNoteIndexType = System.UInt16;	// Unsigned NOTEINDEXTYPE
global using TuningRatioType = System.Single;	// Some 'real figure' type able to present ratios. If changing RATIOTYPE, serialization methods may need modifications

// Counter of steps between notes. If there is no 'finetune'(finestepcount == 0),
// then 'step difference' between notes is the
// same as differences in NOTEINDEXTYPE. In a way similar to ticks and rows in pattern -
// ticks <-> STEPINDEX, rows <-> NOTEINDEX
global using TuningStepIndexType = System.Int32;
global using TuningUStepIndexType = System.UInt32;
