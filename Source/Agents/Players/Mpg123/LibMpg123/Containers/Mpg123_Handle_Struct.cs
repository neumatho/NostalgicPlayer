/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Main handle structure
	/// </summary>
	internal class Mpg123_Handle_Struct
	{
		public bool Fresh;				// To be moved into flags
		public bool New_Format;
		public readonly Real[][][] Hybrid_Block = ArrayHelper.Initialize3Arrays<Real>(2, 2, Constant.SBLimit * Constant.SSLimit);
		public readonly c_int[] Hybrid_Blc = new c_int[2];

		// The scratch vars for the decoders, sometimes real, sometimes short... sometimes int/long
		public Memory<c_short>[][] Short_Buffs = ArrayHelper.Initialize2Arrays<Memory<c_short>>(2, 2);
		public Memory<Real>[][] Real_Buffs = ArrayHelper.Initialize2Arrays<Memory<Real>>(2, 2);
		public c_uchar[] RawBuffs;
		public c_int RawBuffSS;
		public c_int Bo;				// Just have it always here
		public c_uchar[] RawDecWin;		// The block with all decwins
		public c_int RawDecWins;		// Size of rawdecwin memory
		public Memory<Real> DecWin;		// _The_ decode table

		// For halfspeed mode
		public readonly c_uchar[] SSave = new c_uchar[34];
		public c_int HalfPhase;

		// There's some possible memory saving for stuff that is not _really_ dynamic

		// Layer 3
		public readonly c_int[,] LongLimit = new c_int[9, 23];
		public readonly c_int[,] ShortLimit = new c_int[9, 14];
		public readonly Real[] GainPow2 = new Real[256 + 118 + 4];	// Not really dynamic, just different for mmx

		// Layer 2
		public Real[][] Muls = ArrayHelper.Initialize2Arrays<Real>(27, 64);	// Also used by layer 1

		// Decode ntom
		public readonly c_ulong[] NToM_Val = new c_ulong[2];
		public c_ulong NToM_Step;

		public Synth_S Synths = new Synth_S();
		public (
			OptDec Type,
			OptCla Class
		) Cpu_Opts;

		public Al_Table[] alloc;

		// The runtime-chosen decoding, based on input and output format
		public Synth_S.Func_Synth Synth;
		public Synth_S.Func_Synth_Stereo Synth_Stereo;
		public Synth_S.Func_Synth_Mono Synth_Mono;

		// Yes, this function is runtime-switched, too
		public Action<Mpg123_Handle> Make_Decode_Tables;

		public c_int Stereo;			// I _think_ 1 for mono and 2 for stereo
		public c_int JsBound;
		public Single Single;
		public c_int II_SbLimit;
		public c_int Down_Sample_SbLimit;
		public c_int Lsf;				// 0: MPEG 1.0; 1: MPEG 2.0/2.5 -- Both used as bool and array index!

		// Many flags in disguise as integers... wasting bytes
		public bool Mpeg25;
		public c_int Down_Sample;
		public c_int Header_Change;
		public c_int Lay;
		public c_long Spf;				// Cached count of samples per frame
		public Func<Mpg123_Handle, c_int> Do_Layer;
		public bool Error_Protection;
		public c_int Bitrate_Index;
		public c_int Sampling_Frequency;
		public c_int Padding;
		public bool Extension;
		public Mode Mode;
		public c_int Mode_Ext;
		public bool Copyright;
		public bool Original;
		public c_int Emphasis;
		public c_int FrameSize;			// Computed frame size
		public c_int FreeSize;			// Free format frame size
		public Mpg123_Vbr Vbr;
		public off_t Num;				// Frame offset ...
		public off_t Input_Offset;		// Byte offset of this frame in input stream
		public off_t PlayNum;			// Playback offset... includes repetitions, reset at seeks
		public off_t Audio_Start;		// The byte offset in the file where audio data begins
		public Frame_State_Flags State_Flags;
		public c_char Silent_Resync;	// Do not complain for the next n resyncs
		public c_uchar[] Xing_Toc;		// The seek TOC from Xing header
		public bool FreeFormat;
		public c_long FreeFormat_FrameSize;

		// Bitstream info; bsi
		public c_int BitIndex;
		public c_long Bits_Avail;
		public c_uchar[] WordPointer;
		public int WordPointerIndex;

		// Temporary storage for getbits stuff
		public c_ulong UlTmp;
		public c_uchar UcTmp;

		// Rva data, used in common.c, set in id3.c
//		public c_double MaxOutBurst;	// The maximum amplitude in current sample represenation
		public c_double LastScale;
		public (
			c_int[] Level,
			c_float[] Gain,
			c_float[] Peak
		) Rva = ( new c_int[2], new c_float[2], new c_float[2] );

		// Input data
		public off_t Track_Frames;
		public off_t Track_Samples;
		public c_double Mean_FrameSize;
		public off_t Mean_Frames;
		public c_int FSizeOld;
		public c_int SSize;
		public c_uint BitReservoir;
		public readonly c_uchar[][] BsSpace = { new c_uchar[Constant.MaxFrameSize + 512 + 4], new c_uchar[Constant.MaxFrameSize + 512 + 4] };	// MAXFRAMESIZE
		public c_uchar[] BsBuf;
		public int BsBufIndex;
		public c_uchar[] BsBufOld;
		public int BsBufOldIndex;
		public c_int BsNum;

		// That is the header matching the last read frame body
		public c_ulong OldHead;

		// That is the header that is supposedly the first of the stream
		public c_ulong FirstHead;
		public c_int Abr_Rate;
		public readonly Frame_Index Index = new Frame_Index();

		// Output data
		public readonly OutBuffer Buffer = new OutBuffer();
		public readonly AudioFormat Af = new AudioFormat();
		public bool Own_Buffer;
		public size_t OutBlock;			// Number of bytes that this frame produces (upper bound)
		public bool To_Decode;			// This frame holds data to be decoded
		public bool To_Ignore;			// The same, somehow
		public off_t FirstFrame;		// Start decoding from here
		public off_t LastFrame;			// Last frame to decode (for gapless or num_frames limit)
		public off_t IgnoreFrame;		// Frames to decode but discard before firstframe
		public c_uint Crc;				// Well, I need a safe 16bit type, actually. But wider doesn't hurt
		public Reader Rd;				// Pointer to the reading functions
		public readonly Reader_Data RDat = new Reader_Data();	// Reader data and state info
		public readonly Mpg123_Pars_Struct P = new Mpg123_Pars_Struct();
		public Mpg123_Errors Err;
 		public bool Decoder_Change;
//		public bool Delayed_Change;
		public c_long Clip;

		// The meta crap
		public Mpg123_MetaFlags MetaFlags;
		public readonly c_uchar[] Id3Buf = new c_uchar[128];
		public readonly Mpg123_Id3V2 Id3V2 = new Mpg123_Id3V2();
		public c_uchar[] Id3V2_Raw;
		public size_t Id3V2_Size;
		public readonly Icy_Meta Icy = new Icy_Meta();

		// Those layer-specific structs could actually share memory, as they are not
		// in use simultaneously. One might allocate on decoder switch, too.
		// They all reside in one lump of memory (after each other), allocated to layerscratch
		public Real[] LayerScratch;
		public Memory<Real> Layer1;		// [2][SBLIMIT]
		public Memory<Real> Layer2;		// [2][4][SBLIMIT]

		// These are significant chunks of memory already...
		public (
			Memory<Real> Hybrid_In,		// [2][SBLIMIT][SSLIMIT]
			Memory<Real> Hybrid_Out		// [2][SSLIMIT][SBLIMIT]
		) Layer3;

		// A place for storing additional data for the large file wrapper. This is cruft!
		public Wrap_Data WrapperData;

		// A callback used to properly destruct the wrapper data
		public Action<Wrap_Data> WrapperClean;
		public c_int Enc_Delay;
		public c_int Enc_Padding;
		public Mpg123_MoreInfo PInfo;
	}
}
