/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.LibMpg123.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibMpg123
{
	/// <summary>
	/// Heap of routines dealing with the core mpg123 data structure
	/// </summary>
	internal class Frame
	{
		private readonly LibMpg123 lib;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Frame(LibMpg123 libMpg123)
		{
			lib = libMpg123;
		}



		/********************************************************************/
		/// <summary>
		/// Create a handle with preset parameters
		/// </summary>
		/********************************************************************/
		public void Frame_Init_Par(Mpg123_Handle fr, Mpg123_Pars mp)
		{
			fr.Own_Buffer = true;
			fr.Buffer.Data = null;
			fr.Buffer.RData = null;
			fr.Buffer.Fill = 0;
			fr.Buffer.Size = 0;
			fr.RawBuffs = null;
			fr.RawBuffSS = 0;
			fr.RawDecWin = null;
			fr.RawDecWins = 0;
			fr.LayerScratch = null;
			fr.Xing_Toc = null;
			fr.Cpu_Opts.Type = lib.optimize.DefDec();
			fr.Cpu_Opts.Class = lib.optimize.DecClass(fr.Cpu_Opts.Type);

			// These two look unnecessary, check guarantee for synth_ntom_set_step (in control_generic, even)!
			fr.NToM_Val[0] = Constant.NToM_Mul >> 1;
			fr.NToM_Val[1] = Constant.NToM_Mul >> 1;
			fr.NToM_Step = Constant.NToM_Mul;

			// Unnecessary: fr->buffer.size = fr->buffer.fill = 0
			lib.Mpg123_Reset_Eq();

			lib.icy.Init_Icy(fr.Icy);
			lib.id3.Init_Id3(fr);

			// Frame_outbuffer is missing...
			// frame_buffers is missing... that one needs cpu opt setting!
			// after these... frame_reset is needed before starting full decode
			lib.format.Invalidate_Format(fr.Af);

			fr.RDat.R_Read = null;
			fr.RDat.R_LSeek = null;
			fr.RDat.IOHandle = null;
			fr.RDat.R_Read_Handle = null;
			fr.RDat.R_LSeek_Handle = null;
			fr.RDat.Cleanup_Handle = null;
			fr.WrapperData = null;
			fr.WrapperClean = null;
			fr.Decoder_Change = true;
			fr.Err = Mpg123_Errors.Ok;

			if (mp == null)
				Frame_Default_Pars(fr.P);
			else
				mp.Copy(fr.P);

			lib.readers.Bc_Prepare(fr.RDat.Buffer, (size_t)fr.P.FeedPool, (size_t)fr.P.FeedBuffer);

			fr.Down_Sample = 0;		// Initialize to silence harmless errors when debugging
			fr.Id3V2_Raw = null;

			Frame_Fixed_Reset(fr);	// Reset only the fixed data, dynamic buffers are not there yet!

			fr.Synth = null;
			fr.Synth_Mono = null;
			fr.Make_Decode_Tables = null;

			lib.index.Fi_Init(fr.Index);
			Frame_Index_Setup(fr);	// Apply the size setting

			fr.PInfo = null;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the 32 Band Audio Equalizer settings to flat
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Mpg123_Reset_Eq(Mpg123_Handle mh)
		{
			if (mh == null)
				return Mpg123_Errors.Bad_Handle;

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Frame_OutBuffer(Mpg123_Handle fr)
		{
			size_t size = fr.OutBlock;

			if (!fr.Own_Buffer)
			{
				if (fr.Buffer.Size < size)
				{
					fr.Err = Mpg123_Errors.Bad_Buffer;
					return Mpg123_Errors.Err;
				}
			}

			if ((fr.Buffer.RData != null) && (fr.Buffer.Size != size))
				fr.Buffer.RData = null;

			fr.Buffer.Size = size;
			fr.Buffer.Data = null;

			if (fr.Buffer.RData == null)
				fr.Buffer.RData = new byte[fr.Buffer.Size + 15];

			if (fr.Buffer.RData == null)
			{
				fr.Err = Mpg123_Errors.Out_Of_Mem;
				return Mpg123_Errors.Err;
			}

			fr.Buffer.Data = fr.Buffer.RData;
			fr.Own_Buffer = true;
			fr.Buffer.Fill = 0;

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Frame_Index_Setup(Mpg123_Handle fr)
		{
			Mpg123_Errors ret = Mpg123_Errors.Err;

			if (fr.P.Index_Size >= 0)
			{
				// Simple fixed index
				fr.Index.Grow_Size = 0;
				ret = lib.index.Fi_Resize(fr.Index, (size_t)fr.P.Index_Size);
			}
			else
			{
				// A growing index. We give it a start, though
				fr.Index.Grow_Size = (size_t)(-fr.P.Index_Size);

				if (fr.Index.Size < fr.Index.Grow_Size)
					ret = lib.index.Fi_Resize(fr.Index, fr.Index.Grow_Size);
				else
					ret = Mpg123_Errors.Ok;		// We have minimal size already... and since growing is OK
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Frame_Buffers(Mpg123_Handle fr)
		{
			c_int buffSSize = 0;

			// The used-to-be-static buffer of the synth functions, has some subtly different types/sizes
			//
			// 2to1, 4to1, ntom, generic, i386: real[2][2][0x110]
			// mmx, sse: short[2][2][0x110]
			// i586(_dither): 4352 bytes; int/long[2][2][0x110]
			// i486: int[2][2][17*FIR_BUFFER_SIZE]
			// altivec: static real __attribute__ ((aligned (16))) buffs[4][4][0x110]
			//
			// Huh, altivec looks like fun. Well, let it be large... then, the 16 byte alignment seems to be implicit on MacOSX malloc anyway.
			// Let's make a reasonable attempt to allocate enough memory...
			// Keep in mind: biggest ones are i486 and altivec (mutually exclusive!), then follows i586 and normal real.
			// mmx/sse use short but also real for resampling.
			// Thus, minimum is 2*2*0x110*sizeof(real).

			if (fr.Cpu_Opts.Type == OptDec.AltiVec)
				buffSSize = 4 * 4 * 0x110 * sizeof(Real);
			else if ((fr.Cpu_Opts.Type == OptDec.IFuenf) || (fr.Cpu_Opts.Type == OptDec.IFuenf_Dither) || (fr.Cpu_Opts.Type == OptDec.DreiDNow))
				buffSSize = 2 * 2 * 0x110 * 4;	// Don't rely on type real, we need 4352 bytes

			if (2 * 2 * 0x110 * sizeof(Real) > buffSSize)
				buffSSize = 2 * 2 * 0x110 * sizeof(Real);

			buffSSize += 15;	// For 16-byte alignment (SSE likes that)

			if ((fr.RawBuffs != null) && (fr.RawBuffSS != buffSSize))
				fr.RawBuffs = null;

			if (fr.RawBuffs == null)
				fr.RawBuffs = new byte[buffSSize];

			if (fr.RawBuffs == null)
				return -1;

			Memory<byte> mem = fr.RawBuffs;
			fr.RawBuffSS = buffSSize;

			c_int resizeFacor = sizeof(c_short) / sizeof(c_uchar);
			fr.Short_Buffs[0][0] = Unsafe.As<Memory<c_uchar>, Memory<c_short>>(ref mem).Slice(0, mem.Length / resizeFacor);
			fr.Short_Buffs[0][1] = fr.Short_Buffs[0][0].Slice(0x110);
			fr.Short_Buffs[1][0] = fr.Short_Buffs[0][1].Slice(0x110);
			fr.Short_Buffs[1][1] = fr.Short_Buffs[1][0].Slice(0x110);

			resizeFacor = sizeof(Real) / sizeof(c_uchar);
			fr.Real_Buffs[0][0] = Unsafe.As<Memory<c_uchar>, Memory<Real>>(ref mem).Slice(0, mem.Length / resizeFacor);
			fr.Real_Buffs[0][1] = fr.Real_Buffs[0][0].Slice(0x110);
			fr.Real_Buffs[1][0] = fr.Real_Buffs[0][1].Slice(0x110);
			fr.Real_Buffs[1][1] = fr.Real_Buffs[1][0].Slice(0x110);

			// Now the different decwins... all of the same size, actually.
			// The MMX ones want 32byte alignment, which I'll try to ensure manually
			{
				c_int decWin_Size = (512 + 32) * sizeof(Real);

				// Hm, that's basically realloc() ...
				if ((fr.RawDecWin != null) && (fr.RawDecWins != decWin_Size))
					fr.RawDecWin = null;

				if (fr.RawDecWin == null)
					fr.RawDecWin = new c_uchar[decWin_Size];

				if (fr.RawDecWin == null)
					return -1;

				resizeFacor = sizeof(Real) / sizeof(c_uchar);
				mem = fr.RawDecWin.AsMemory();

				fr.RawDecWins = decWin_Size;
				fr.DecWin = Unsafe.As<Memory<c_uchar>, Memory<Real>>(ref mem).Slice(0, mem.Length / resizeFacor);
			}

			// Layer scratch buffers are of compile-time fixed size, so allocate only once
			if (fr.LayerScratch == null)
			{
				// Allocate specific layer1/2/3 buffers, so that we know they'll work for SSE
				size_t scratchSize = 0;

				scratchSize += sizeof(Real) * 2 * Constant.SBLimit;			// Layer 1
				scratchSize += sizeof(Real) * 2 * 4 * Constant.SBLimit;		// Layer 2
				scratchSize += sizeof(Real) * 2 * Constant.SBLimit * Constant.SSLimit;// Layer 3
				scratchSize += sizeof(Real) * 2 * Constant.SSLimit * Constant.SBLimit;

				fr.LayerScratch = new Real[scratchSize + 63];
				if (fr.LayerScratch == null)
					return -1;

				Memory<Real> scratcher = fr.LayerScratch.AsMemory();

				fr.Layer1 = scratcher;
				scratcher = scratcher.Slice(2 * Constant.SBLimit);

				fr.Layer2 = scratcher;
				scratcher = scratcher.Slice(2 * 4 * Constant.SBLimit);

				fr.Layer3.Hybrid_In = scratcher;
				scratcher = scratcher.Slice(2 * Constant.SBLimit * Constant.SSLimit);
				fr.Layer3.Hybrid_Out = scratcher;
				scratcher = scratcher.Slice(2 * Constant.SSLimit * Constant.SBLimit);
			}

			// Only reset the buffers we created just now
			Frame_Decode_Buffers_Reset(fr);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Frame_Buffers_Reset(Mpg123_Handle fr)
		{
			fr.Buffer.Fill = 0;		// Hm, reset buffer fill... did we do a flush?
			fr.BsNum = 0;

			// Wondering: could it be actually _wanted_ to retain buffer contents over
			// different files? (special gapless / cut stuff)
			fr.BsBuf = fr.BsSpace[1];
			fr.BsBufIndex = 0;
			fr.BsBufOld = fr.BsBuf;
			fr.BsBufOldIndex = fr.BsBufIndex;
			fr.BitReservoir = 0;

			Frame_Decode_Buffers_Reset(fr);
			Array.Clear(fr.BsSpace[0]);
			Array.Clear(fr.BsSpace[1]);
			Array.Clear(fr.SSave);

			fr.Hybrid_Blc[0] = fr.Hybrid_Blc[1] = 0;
			Array.Clear(fr.Hybrid_Blc);

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Just copy the Xing TOC over...
		/// </summary>
		/********************************************************************/
		public bool Frame_Fill_Toc(Mpg123_Handle fr, Memory<c_uchar> in_)
		{
			if (fr.Xing_Toc == null)
				fr.Xing_Toc = new byte[100];

			if (fr.Xing_Toc != null)
			{
				in_.Slice(0, 100).CopyTo(fr.Xing_Toc.AsMemory());

				return true;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare the handle for a new track.
		/// Reset variables, buffers...
		/// </summary>
		/********************************************************************/
		public Mpg123_Errors Frame_Reset(Mpg123_Handle fr)
		{
			Frame_Buffers_Reset(fr);
			Frame_Fixed_Reset(fr);
			Frame_Free_Toc(fr);
			lib.index.Fi_Reset(fr.Index);

			return Mpg123_Errors.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Frame_Exit(Mpg123_Handle fr)
		{
			fr.Buffer.RData = null;

			Frame_Free_Buffers(fr);
			Frame_Free_Toc(fr);
			lib.index.Fi_Exit(fr.Index);
			lib.id3.Exit_Id3(fr);
			lib.icy.Clear_Icy(fr.Icy);

			// Cleanup possible mess from LFS wrapper
			if (fr.WrapperClean != null)
			{
				fr.WrapperClean(fr.WrapperData);
				fr.WrapperData = null;
			}

			lib.readers.Bc_Cleanup(fr.RDat.Buffer);
		}



		/********************************************************************/
		/// <summary>
		/// Find the best frame in index just before the wanted one, seek to
		/// there then step to just before wanted one with read_frame. Do
		/// not care tabout the stuff that was in buffer but not played back
		/// everything that left the decoder is counted as played.
		///
		/// Decide if you want low latency reaction and accurate timing info
		/// or stable long-time playback with buffer!
		/// </summary>
		/********************************************************************/
		public off_t Frame_Index_Find(Mpg123_Handle fr, off_t want_Frame, out off_t get_Frame)
		{
			// Default is file start if no index position
			off_t goPos = 0;
			get_Frame = 0;

			// Possibly use VBRI index, too? I'd need an example for this...
			if (fr.Index.Fill != 0)
			{
				// Find in index
				//
				// At index fi there is frame step*fi...
				size_t fi = (size_t)(want_Frame / fr.Index.Step);

				if (fi >= fr.Index.Fill)	// If we are beyond the end of frame index...
				{
					// When fuzzy seek is allowed, we have some limited tolerance for the
					// frames we want to read rather then jump over
					if (((fr.P.Flags & Mpg123_Param_Flags.Fuzzy) != 0) && ((want_Frame - ((off_t)fr.Index.Fill - 1) * fr.Index.Step) > 10))
					{
						goPos = Frame_Fuzzy_Find(fr, want_Frame, out get_Frame);

						if (goPos > fr.Audio_Start)
							return goPos;		// Only in that case, we have a useful guess

						// Else... just continue, fuzzyness didn't help
					}

					// Use the last available position, slowly advancing from that one
					fi = fr.Index.Fill - 1;
				}

				// We have index position, that yields frame and byte offsets
				get_Frame = (off_t)fi * fr.Index.Step;
				goPos = fr.Index.Data[fi];
				fr.State_Flags |= Frame_State_Flags.Accurate;	// When using the frame index, we are accurate
			}
			else
			{
				if ((fr.P.Flags & Mpg123_Param_Flags.Fuzzy) != 0)
					return Frame_Fuzzy_Find(fr, want_Frame, out get_Frame);

				// A bit hackish here... but we need to be fresh when looking for the first header again
				fr.FirstHead = 0;
				fr.OldHead = 0;
			}

			return goPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public off_t Frame_Ins2Outs(Mpg123_Handle fr, off_t ins)
		{
			off_t outs = 0;

			switch (fr.Down_Sample)
			{
				case 0:
				case 1:
				case 2:
				{
					outs = ins >> fr.Down_Sample;
					break;
				}

				case 3:
				{
					outs = lib.nToM.NToM_Ins2Outs(fr, ins);
					break;
				}
			}

			return outs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public off_t Frame_Outs(Mpg123_Handle fr, off_t num)
		{
			off_t outs = 0;

			switch (fr.Down_Sample)
			{
				case 0:
				case 1:
				case 2:
				{
					outs = (fr.Spf >> fr.Down_Sample) * num;
					break;
				}

				case 3:
				{
					outs = lib.nToM.NToM_FrmOuts(fr, num);
					break;
				}
			}

			return outs;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the number of output samples we expect from this frame.
		/// This is either simple spf() or a tad more elaborate for ntom
		/// </summary>
		/********************************************************************/
		public off_t Frame_Expect_OutSamples(Mpg123_Handle fr)
		{
			off_t outs = 0;

			switch (fr.Down_Sample)
			{
				case 0:
				case 1:
				case 2:
				{
					outs = fr.Spf >> fr.Down_Sample;
					break;
				}

				case 3:
				{
					outs = lib.nToM.NToM_Frame_OutSamples(fr);
					break;
				}
			}

			return outs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Frame_Skip(Mpg123_Handle fr)
		{
			if (fr.Lay == 3)
				lib.parse.Set_Pointer(fr, true, 512);
		}



		/********************************************************************/
		/// <summary>
		/// Sample accurate seek prepare for decoder.
		/// This gets unadjusted output samples and takes resampling into
		/// account
		/// </summary>
		/********************************************************************/
		public void Frame_Set_Seek(Mpg123_Handle fr, off_t sp)
		{
			fr.FirstFrame = Frame_Offset(fr, sp);

			if (fr.Down_Sample == 3)
				lib.nToM.NToM_Set_NToM(fr, fr.FirstFrame);

			fr.IgnoreFrame = IgnoreFrame(fr);
		}



		/********************************************************************/
		/// <summary>
		/// Adjust the volume, taking both fr.outscale and rva values into
		/// account
		/// </summary>
		/********************************************************************/
		public void Do_Rva(Mpg123_Handle fr)
		{
			c_double rvaFact = 1;

			if (Get_Rva(fr, out c_double peak, out c_double gain))
				rvaFact = Math.Pow(10, gain / 20);

			c_double newScale = fr.P.OutScale * rvaFact;

			// If peak is unknown (== 0) this check won't hurt
			if ((peak * newScale) > 1.0)
				newScale = 1.0 / peak;

			// First rva setting is forced with fr.lastscale < 0
			if ((newScale != fr.LastScale) || fr.Decoder_Change)
			{
				fr.LastScale = newScale;

				// It may be too early, actually
				if (fr.Make_Decode_Tables != null)
					fr.Make_Decode_Tables(fr);		// The actual work
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Frame_Default_Pars(Mpg123_Pars mp)
		{
			mp.OutScale = 1.0;
			mp.Flags = Mpg123_Param_Flags.None;
			mp.Flags |= Mpg123_Param_Flags.Auto_Resample | Mpg123_Param_Flags.Float_Fallback;
			mp.Force_Rate = 0;
			mp.Down_Sample = 0;
			mp.Rva = 0;
			mp.HalfSpeed = 0;
			mp.DoubleSpeed = 0;
			mp.Verbose = 0;
			mp.Icy_Interval = 0;
			mp.Timeout = 0;
			mp.Resync_Limit = 1024;
			mp.Index_Size = Constant.Index_Size;
			mp.PreFrames = 4;		// That's good for layer 3 ISO compliance bitstream

			lib.Mpg123_Fmt_All(mp);

			// Default of keeping some 4K buffers at hand, should cover the "usual" use
			// case (using 16K pipe buffers as role model)
			mp.FeedPool = 5;
			mp.FeedBuffer = 4096;
			mp.FreeFormat_FrameSize = -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Frame_Decode_Buffers_Reset(Mpg123_Handle fr)
		{
			if (fr.RawBuffs != null)
				Array.Clear(fr.RawBuffs, 0, fr.RawBuffSS);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Frame_Icy_Reset(Mpg123_Handle fr)
		{
			fr.Icy.Data = null;
			fr.Icy.Interval = 0;
			fr.Icy.Next = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Frame_Free_Toc(Mpg123_Handle fr)
		{
			fr.Xing_Toc = null;
		}



		/********************************************************************/
		/// <summary>
		/// Reset everything except dynamic memory
		/// </summary>
		/********************************************************************/
		private void Frame_Fixed_Reset(Mpg123_Handle fr)
		{
			Frame_Icy_Reset(fr);
			lib.readers.Open_Bad(fr);

			fr.To_Decode = false;
			fr.To_Ignore = false;
			fr.MetaFlags = 0;
			fr.OutBlock = 0;		// This will be reset before decoding!
			fr.Num = -1;
			fr.Input_Offset = -1;
			fr.PlayNum = -1;
			fr.State_Flags = Frame_State_Flags.Accurate;
			fr.Silent_Resync = 0;
			fr.Audio_Start = 0;
			fr.Clip = 0;
			fr.OldHead = 0;
			fr.FirstHead = 0;
			fr.Lay = 0;
			fr.Vbr = Mpg123_Vbr.Cbr;
			fr.Abr_Rate = 0;
			fr.Track_Frames = 0;
			fr.Track_Samples = -1;
			fr.FrameSize = 0;
			fr.Mean_Frames = 0;
			fr.Mean_FrameSize = 0;
			fr.FreeSize = 0;
			fr.LastScale = -1;
			fr.Rva.Level[0] = -1;
			fr.Rva.Level[1] = -1;
			fr.Rva.Gain[0] = 0;
			fr.Rva.Gain[1] = 0;
			fr.Rva.Peak[0] = 0;
			fr.Rva.Gain[1] = 0;
			fr.FSizeOld = 0;
			fr.FirstFrame = 0;
			fr.IgnoreFrame = fr.FirstFrame - fr.P.PreFrames;
			fr.Header_Change = 0;
			fr.LastFrame = -1;
			fr.Fresh = true;
			fr.New_Format = false;
			fr.Bo = 1;				// The usual bo

			lib.id3.Reset_Id3(fr);
			lib.icy.Reset_Icy(fr.Icy);

			fr.Icy.Interval = 0;
			fr.Icy.Next = 0;
			fr.HalfPhase = 0;		// Here or indeed only on first-time init?
			fr.Error_Protection = false;
			fr.FreeFormat_FrameSize = fr.P.FreeFormat_FrameSize;
			fr.Enc_Delay = -1;
			fr.Enc_Padding = -1;

			Array.Clear(fr.Id3Buf);

			fr.Id3V2_Raw = null;
			fr.Id3V2_Size = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Frame_Free_Buffers(Mpg123_Handle fr)
		{
			fr.RawBuffs = null;
			fr.RawBuffSS = 0;
			fr.RawDecWin = null;
			fr.RawDecWins = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Fuzzy frame offset searching (guessing).
		/// When we don't have an accurate position, we may use an inaccurate
		/// one.
		/// Possibilities:
		///   - Use approximate positions from Xing TOC (not yet parsed)
		///   - Guess wildly from mean framesize and offset of first frame /
		///     beginning of file
		/// </summary>
		/********************************************************************/
		private off_t Frame_Fuzzy_Find(Mpg123_Handle fr, off_t want_Frame, out off_t get_Frame)
		{
			// Default is to go to the beginning
			off_t ret = fr.Audio_Start;
			get_Frame = 0;

			// But we try to find something better.
			// Xing VBR TOC works with relative positions, both in terms of audio frames and stream bytes.
			// Thus, it only works when we know the length of things.
			// Oh... I assume the offsets are relative to the _total_ file length
			if ((fr.Xing_Toc != null) && (fr.Track_Frames > 0) && (fr.RDat.FileLen > 0))
			{
				// One could round...
				c_int toc_Entry = (c_int)((c_double)want_Frame * 100.0f / fr.Track_Frames);

				// It is an index in the 100-entry table
				if (toc_Entry < 0)
					toc_Entry = 0;

				if (toc_Entry > 99)
					toc_Entry = 99;

				// Now estimate back what frame we get
				get_Frame = (off_t)((c_double)toc_Entry * 100.0f * fr.Track_Frames);
				fr.State_Flags &= ~Frame_State_Flags.Accurate;
				fr.Silent_Resync = 1;

				// Question: Is the TOC for whole file size (with/without ID3) or the "real" audio data only?
				// ID3v1 info could also matter
				ret = (off_t)((c_double)fr.Xing_Toc[toc_Entry] / 256.0f * fr.RDat.FileLen);
			}
			else if (fr.Mean_FrameSize > 0)
			{
				// Just guess with mean framesize (may be exact with CBR files).
				// Query filelen here or not?
				fr.State_Flags &= ~Frame_State_Flags.Accurate;	// Fuzzy!
				fr.Silent_Resync = 1;
				get_Frame = want_Frame;

				ret = (off_t)(fr.Audio_Start + fr.Mean_FrameSize * want_Frame);
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private off_t Frame_Offset(Mpg123_Handle fr, off_t outs)
		{
			off_t num = 0;

			switch (fr.Down_Sample)
			{
				case 0:
				case 1:
				case 2:
				{
					num = outs / (fr.Spf >> fr.Down_Sample);
					break;
				}

				case 3:
				{
					num = lib.nToM.NToM_FrameOff(fr, outs);
					break;
				}
			}

			return num;
		}



		/********************************************************************/
		/// <summary>
		/// Compute the needed frame to ignore from, for getting accurate/
		/// consistent output for intended firstframe
		/// </summary>
		/********************************************************************/
		private off_t IgnoreFrame(Mpg123_Handle fr)
		{
			off_t preShift = fr.P.PreFrames;

			// Layer 3 _really_ needs at least one frame before
			if ((fr.Lay == 3) && (preShift < 1))
				preShift = 1;

			// Layer 1 & 2 reall do not need more than 2
			if ((fr.Lay != 3) && (preShift > 2))
				preShift = 2;

			return fr.FirstFrame - preShift;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Get_Rva(Mpg123_Handle fr, out c_double peak, out c_double gain)
		{
			c_double p = -1;
			c_double g = 0;
			bool ret = false;

			if (fr.P.Rva != 0)
			{
				c_int rt = 0;

				// Should one assume a zero RVA as no RVA?
				if ((fr.P.Rva == Mpg123_Param_Rva.Rva_Album) && (fr.Rva.Level[1] != -1))
					rt = 1;

				if (fr.Rva.Level[rt] != -1)
				{
					p = fr.Rva.Peak[rt];
					g = fr.Rva.Gain[rt];
					ret = true;		// Success
				}
			}

			peak = p;
			gain = g;

			return ret;
		}
		#endregion
	}
}
