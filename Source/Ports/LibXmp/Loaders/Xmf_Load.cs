/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// Loader for Astroidea XMF, used by Imperium Galactica and some other modules.
	/// This format is completely unrelated to the MIDI XMF format
	/// </summary>
	internal class Xmf_Load : IFormatLoader
	{
		private const int Xmf_Sample_Array_Size = 16 * 256;

		private readonly LibXmp lib;
		private readonly Encoding encoder;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Xmf = new Format_Loader
		{
			Id = Guid.Parse("D89EE906-0815-4C82-9102-7BE3B9405EDC"),
			Name = "Astroidea XMF",
			Description = "Loader for Astroidea XMF, used by Imperium Galactica and some other modules. This format is completely unrelated to the MIDI XMF format.",
			Create = Create
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Xmf_Load(LibXmp libXmp)
		{
			lib = libXmp;
			encoder = EncoderCollection.Dos;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Xmf_Load(libXmp);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			t = null;

			uint8[] buf = new uint8[Xmf_Sample_Array_Size];
			uint32 samples_Length = 0;

			// This value is 0x03 for all Imperium Galactica modules.
			// The demo "Prostate 666" and all other XMFs use 0x04 instead
			c_int xmf_Type = f.Hio_Read8();
			if ((xmf_Type != 0x03) && (xmf_Type != 0x04))
				return -1;

			if (f.Hio_Read(buf, 1, Xmf_Sample_Array_Size) < Xmf_Sample_Array_Size)
				return -1;

			// Test instruments
			CPointer<uint8> pos = buf;
			c_int num_Ins = 0;

			for (c_int i = 0; i < 256; i++)
			{
				uint32 loopStart = DataIo.ReadMem24L(pos + 0);
				uint32 loopEnd = DataIo.ReadMem24L(pos + 3);
				uint32 dataStart = DataIo.ReadMem24L(pos + 6);
				uint32 dataEnd = DataIo.ReadMem24L(pos + 9);
				uint8 flags = pos[13];
				uint16 sRate = DataIo.ReadMem16L(pos + 14);
				uint32 len = dataEnd - dataStart;
				pos += 16;

				if ((flags & ~(0x04 | 0x08 | 0x10)) != 0)
					return -1;

				// If ping-pong loop flag is enabled, normal loop flag should be enabled too
				if ((flags & (0x08 | 0x10)) == 0x10)
					return -1;

				// If loop flag is enabled, the loop should have a valid end point
				if (((flags & 0x80) != 0) && (loopEnd == 0))
					return -1;

				// A 16-bit sample should have an even number of bytes
				if (((flags & 0x04) != 0) && ((len & 1) != 0))
					return -1;

				// If this slot contains a valid sample, it should have a somewhat
				// realistic middle-c frequency
				if ((len != 0) && (sRate < 100))
					return -1;

				// Despite the data start and end values, samples are stored
				// sequentially after the pattern data. These fields are still
				// required to calculate the sample length
				if (dataStart > dataEnd)
					return -1;

				samples_Length += len;

				// All known XMFs have well-formed loops
				if ((loopEnd != 0) && ((loopStart >= len) || (loopEnd > len) || (loopStart > loopEnd)))
					return -1;

				if (len > 0)
					num_Ins = i + 1;
			}

			if (num_Ins > Constants.Max_Instruments)
				return -1;

			// Get pattern data size
			if (f.Hio_Read(buf, 1, 258) < 258)
				return -1;

			c_int num_Channels = buf[256] + 1;
			c_int num_Patterns = buf[257] + 1;

			if (num_Channels > Constants.Xmp_Max_Channels)
				return -1;

			// Test total module length
			c_int samples_Start = 0x1103 + num_Channels + num_Patterns * num_Channels * 64 * 6;
			c_int length = f.Hio_Size();

			if ((length < samples_Start) || (((size_t)(length - samples_Start)) < samples_Length))
				return -1;

			lib.common.LibXmp_Read_Title(f, out t, 0, encoder);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Loader(Module_Data m, Hio f, c_int start)
		{
			Xmp_Module mod = m.Mod;
			c_int i;

			// Imperium Galactica uses 0x03, other Astroidea tracks use 0x04
			c_int xmf_Type = f.Hio_Read8();
			if (xmf_Type == 0x03)
				lib.common.LibXmp_Set_Type(m, "Imperium Galactica XMF");
			else
				lib.common.LibXmp_Set_Type(m, "Astroidea XMF");

			CPointer<uint8> buf = CMemory.MAlloc<uint8>(Xmf_Sample_Array_Size);
			if (buf.IsNull)
				return -1;

			// Count instruments
			if (f.Hio_Read(buf, 1, Xmf_Sample_Array_Size) < Xmf_Sample_Array_Size)
				goto Err;

			mod.Ins = 0;
			CPointer<uint8> pos = buf;

			for (i = 0; i < 256; i++, pos += 16)
			{
				if (DataIo.ReadMem24L(pos + 9) > DataIo.ReadMem24L(pos + 6))
					mod.Ins = i;
			}

			mod.Ins++;
			mod.Smp = mod.Ins;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				goto Err;

			// Instruments
			pos = buf;

			for (i = 0; i < mod.Ins; i++, pos += 16)
			{
				Extra_Sample_Data xtra = m.Xtra[i];
				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_Sample xxs = mod.Xxs[i];

				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					goto Err;

				Xmp_SubInstrument sub = xxi.Sub[0];

				xxs.Len = (c_int)(DataIo.ReadMem24L(pos + 9) - DataIo.ReadMem24L(pos + 6));
				xxs.Lps = (c_int)DataIo.ReadMem24L(pos + 0);
				xxs.Lpe = (c_int)DataIo.ReadMem24L(pos + 3);
				xtra.C5Spd = DataIo.ReadMem16L(pos + 14);
				sub.Vol = pos[12];
				sub.Sid = i;

				// The Sound Blaster driver will only loop if both the
				// loop start and loop end are non-zero. The Sound Blaster
				// driver does not support 16-bit samples or bidirectional
				// looping, and plays these as regular 8-bit looped samples.
				// 
				// GUS: 16-bit samples are loaded as 8-bit but play as 16-bit.
				// If the first sample is 16-bit it will partly work (due to
				// having a GUS RAM address of 0?). Other 16-bit samples will
				// read from silence, garbage, or other samples

				if ((pos[13] & 0x04) != 0)	// GUS 16-bit flag
				{
					xxs.Flg |= Xmp_Sample_Flag._16Bit;
					xxs.Len >>= 1;
				}

				if ((pos[13] & 0x08) != 0)	// GUS loop enabled
					xxs.Flg |= Xmp_Sample_Flag.Loop;

				if ((pos[13] & 0x10) != 0)	// GUS reverse flag
					xxs.Flg |= Xmp_Sample_Flag.Loop_BiDir;

				if (xxs.Len > 0)
					xxi.Nsm = 1;
			}

			// Sequence
			if (f.Hio_Read(mod.Xxo, 1, 256) < 256)
				return -1;

			mod.Chn = f.Hio_Read8() + 1;
			mod.Pat = f.Hio_Read8() + 1;
			mod.Trk = mod.Chn * mod.Pat;

			for (i = 0; i < 256; i++)
			{
				if (mod.Xxo[i] == 0xff)
					break;
			}

			mod.Len = i;

			// Panning table (supported by the Gravis UltraSound driver only)
			if (f.Hio_Read(buf, 1, (size_t)mod.Chn) < (size_t)mod.Chn)
				goto Err;

			for (i = 0; i < mod.Chn; i++)
			{
				mod.Xxc[i].Pan = 0x80 + (buf[i] - 7) * 16;

				if (mod.Xxc[i].Pan > 255)
					mod.Xxc[i].Pan = 255;
			}

			size_t pat_Sz = (size_t)mod.Chn * 6 * 64;
			if (pat_Sz > Xmf_Sample_Array_Size)
			{
				pos = CMemory.Realloc(buf, (int)pat_Sz);
				if (pos.IsNull)
					goto Err;

				buf = pos;
			}

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				goto Err;

			// Patterns
			for (i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					goto Err;

				if (f.Hio_Read(buf, 1, pat_Sz) < pat_Sz)
					goto Err;

				pos = buf;

				for (c_int j = 0; j < 64; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						if (pos[0] > 0)
							@event.Note = (byte)(pos[0] + 36);

						@event.Ins = pos[1];

						Xmf_Translate_Effect(@event, pos[2], pos[5], 0);
						Xmf_Translate_Effect(@event, pos[3], pos[4], 1);

						pos += 6;
					}
				}
			}

			CMemory.Free(buf);

			// Sample data
			//
			// Despite the GUS sample start and end pointers saved in the file,
			// these are actually just loaded sequentially
			for (i = 0; i < mod.Ins; i++)
			{
				if (Sample.LibXmp_Load_Sample(m, f, Sample_Flag.None, mod.Xxs[i], null, i) != 0)
					return -1;
			}

			// With the Sound Blaster driver, full volume samples have a -0dB mix.
			// Doing this in libxmp (x4 mvolbase) clips a little bit, so use a
			// slightly lower level (x3 mvolbase, ~192 in IT terms).
			// 
			// This only applies to the Imperium Galactica tracks; the tracks with
			// 0x04 use the full GUS volume range
			m.VolBase = 0xff;
			m.MVolBase = 48;
			m.MVol = (xmf_Type == 0x03) ? m.MVolBase * 3 : m.MVolBase;

			return 0;

			Err:
			CMemory.Free(buf);

			return -1;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xmf_Insert_Effect(Xmp_Event @event, uint8 fxt, uint8 fxp, c_int chn)
		{
			// TODO: Command pages would be nice, but no official modules rely on 5xy/6xy
			if (chn == 0)
			{
				@event.FxT = fxt;
				@event.FxP = fxp;
			}
			else
			{
				@event.F2T = fxt;
				@event.F2P = fxp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Xmf_Translate_Effect(Xmp_Event @event, uint8 effect, uint8 param, c_int chn)
		{
			// Most effects are Protracker compatible. Only the effects actually
			// implemented by Imperium Galactica are handled here
			switch (effect)
			{
				// None/arpeggio
				// Portamento up
				// Portamento down
				// Set speed + set BPM
				case 0x00:
				case 0x01:
				case 0x02:
				case 0x0f:
				{
					if (param != 0)
						Xmf_Insert_Effect(@event, effect, param, chn);

					break;
				}

				// Tone portamento
				// Vibrato
				// Set volume
				// Break
				case 0x03:
				case 0x04:
				case 0x0c:
				case 0x0d:
				{
					Xmf_Insert_Effect(@event, effect, param, chn);
					break;
				}

				// Volume slide + tone portamento
				// Volume slide + vibrato
				case 0x05:
				case 0x06:
				{
					if (effect == 0x05)
						Xmf_Insert_Effect(@event, Effects.Fx_TonePorta, 0, chn ^ 1);

					if (effect == 0x06)
						Xmf_Insert_Effect(@event, Effects.Fx_Vibrato, 0, chn ^ 1);

					// Fall-through
					goto case 0x0a;
				}

				// Volume slide
				case 0x0a:
				{
					if ((param & 0x0f) != 0)
					{
						// Down takes precedence and uses the full param
						Xmf_Insert_Effect(@event, Effects.Fx_VolSlide_Dn, (uint8)(param << 2), chn);
					}
					else if ((param & 0xf0) != 0)
						Xmf_Insert_Effect(@event, Effects.Fx_VolSlide_Up, (uint8)(param >> 2), chn);

					break;
				}

				// Pattern jump (jumps to xx + 1)
				case 0x0b:
				{
					if (param < 255)
						Xmf_Insert_Effect(@event, Effects.Fx_Jump, (uint8)(param + 1), chn);

					break;
				}

				// Extended
				case 0x0e:
				{
					switch (param >> 4)
					{
						// Fine slide up
						// Fine slide down
						// Note retrigger (TODO: only once)
						// Note cut
						// Note delay
						// Pattern delay
						case 0x01:
						case 0x02:
						case 0x09:
						case 0x0c:
						case 0x0d:
						case 0x0e:
						{
							if ((param & 0x0f) != 0)
								Xmf_Insert_Effect(@event, effect, param, chn);

							break;
						}

						// Vibrato waveform
						case 0x04:
						{
							param &= 3;
							param = (uint8)(param < 3 ? param : 2);

							Xmf_Insert_Effect(@event, effect, param, chn);
							break;
						}

						// Pattern loop
						case 0x06:
						{
							Xmf_Insert_Effect(@event, effect, param, chn);
							break;
						}

						// Fine volume slide up
						case 0x0a:
						{
							if ((param & 0x0f) != 0)
								Xmf_Insert_Effect(@event, Effects.Fx_F_VSlide_Up, (uint8)((param & 0x0f) << 2), chn);

							break;
						}

						// Fine volume slide down
						case 0x0b:
						{
							if ((param & 0x0f) != 0)
								Xmf_Insert_Effect(@event, Effects.Fx_F_VSlide_Dn, (uint8)((param & 0x0f) << 2), chn);

							break;
						}
					}
					break;
				}

				// Panning (4-bit, GUS driver only)
				case 0x10:
				{
					param &= 0x0f;
					param |= (uint8)(param << 4);

					Xmf_Insert_Effect(@event, Effects.Fx_SetPan, param, chn);
					break;
				}

				// Ultra Tracker retrigger
				case 0x11:
				{
					// TODO: Should support the full param range, needs testing
					Xmf_Insert_Effect(@event, Effects.Fx_Extended, (uint8)((Effects.Ex_Retrig << 4) | (param & 0x0f)), chn);
					break;
				}
			}
		}
		#endregion
	}
}
