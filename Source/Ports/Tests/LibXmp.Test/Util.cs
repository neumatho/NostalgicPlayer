/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Util
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Read_File_To_Memory(string path, string fileName, out byte[] _buffer, out c_long _size)
		{
			using (Stream s = new FileStream(Path.Combine(path, fileName), FileMode.Open, FileAccess.Read))
			{
				Hio f = Hio.Hio_Open_File(s);

				_buffer = null;
				_size = 0;

				if (f == null)
					return;

				c_long size = f.Hio_Size();
				if (size <= 0)
				{
					f.Hio_Close();
					return;
				}

				byte[] buffer = new byte[size];
				if (buffer == null)
				{
					f.Hio_Close();
					return;
				}

				if (f.Hio_Read(buffer, 1, (size_t)size) != (size_t)size)
				{
					f.Hio_Close();
					return;
				}

				f.Hio_Close();

				_buffer = buffer;
				_size = size;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Check_Randomness(c_int[] array, c_int offset, c_int size, c_double sDev)
		{
			c_double avg = 0.0;
			c_double dev = 0.0;

			for (c_int i = 0; i < size; i++)
				avg += array[offset + i];

			avg /= size;

			for (c_int i = 0; i < size; i++)
				dev += Math.Pow(avg - array[offset + i], 2);

			dev = Math.Sqrt(dev / size);

			return dev > sDev;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Check_Md5(uint8[] buffer, c_int length, string digest)
		{
			using (MD5 md5 = MD5.Create())
			{
				byte[] checksum = md5.ComputeHash(buffer, 0, length);

				string d = Helpers.ToHex(checksum);
				return string.CompareOrdinal(d, digest);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Map_Channel(Player_Data p, c_int chn)
		{
			if ((uint32)chn >= p.Virt.Virt_Channels)
				return -1;

			c_int voc = p.Virt.Virt_Channel[chn].Map;

			if ((uint32)voc >= p.Virt.MaxVoc)
				return -1;

			return voc;
		}



		/********************************************************************/
		/// <summary>
		/// Convert little-endian 16 bit samples to big-endian
		/// </summary>
		/********************************************************************/
		public static void Convert_Endian(uint8[] p, c_int offset, c_int l)
		{
			c_int off = offset;

			for (c_int i = 0; i < l; i++)
			{
				uint8 b = p[off];
				p[off] = p[off + 1];
				p[off + 1] = b;

				off += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Compare_Module(Xmp_Module mod, Stream f)
		{
			using (StreamReader sr = new StreamReader(f))
			{
				// Check title and format
				string line = sr.ReadLine();
				Assert.AreEqual(line, mod.Name, "Module name");
				line = sr.ReadLine();
				Assert.AreEqual(line, mod.Type, "Module type");

				// Check module attributes
				line = sr.ReadLine();
				int x = GetNextNumber(line, out string s);
				Assert.AreEqual(x, mod.Pat, "Number of patterns");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Trk, "Number of tracks");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Chn, "Number of channels");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Ins, "Number of instruments");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Smp, "Number of samples");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Spd, "Initial speed");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Bpm, "Initial tempo");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Len, "Module length");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Rst, "Restart position");
				x = GetNextNumber(s, out s);
				Assert.AreEqual(x, mod.Gvl, "Global volume");

				// Check orders
				if (mod.Len > 0)
				{
					line = sr.ReadLine();
					s = line;

					for (c_int i = 0; i < mod.Len; i++)
					{
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, mod.Xxo[i], "Orders");
					}
				}

				// Check instruments
				for (c_int i = 0; i < mod.Ins; i++)
				{
					Xmp_Instrument xxi = mod.Xxi[i];

					line = sr.ReadLine();
					x = GetNextNumber(line, out s);
					Assert.AreEqual(x, xxi.Vol, "Instrument volume");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, xxi.Nsm, "Number of subinstruments");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, xxi.Rls, "Instrument release");
					Assert.AreEqual(s.Substring(1), xxi.Name, "Instrument name");

					// Check envelopes
					Check_Envelope(xxi.Aei, sr);
					Check_Envelope(xxi.Fei, sr);
					Check_Envelope(xxi.Pei, sr);

					// Check mapping
					line = sr.ReadLine();
					s = line;

					for (c_int j = 0; j < Constants.Xmp_Max_Keys; j++)
					{
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, xxi.Map[j].Ins, "Instrument map");
					}

					line = sr.ReadLine();
					s = line;

					for (c_int j = 0; j < Constants.Xmp_Max_Keys; j++)
					{
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, xxi.Map[j].Xpo, "Transpose map");
					}

					// Check subinstruments
					for (c_int j = 0; j < xxi.Nsm; j++)
					{
						Xmp_SubInstrument sub = xxi.Sub[j];

						line = sr.ReadLine();
						x = GetNextNumber(line, out s);
						Assert.AreEqual(x, sub.Vol, "Subinst volume");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Gvl, "Subinst gl volume");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Pan, "Subinst pan");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Xpo, "Subinst transpose");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Fin, "Subinst finetune");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Vwf, "Subinst vibr wf");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Vde, "Subinst vibr depth");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Vra, "Subinst vibr rate");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Vsw, "Subinst vibr sweep");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Rvv, "Subinst vol var");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Sid, "Subinst sample nr");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, (int)sub.Nna, "Subinst NNA");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, (int)sub.Dct, "Subinst DCT");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, (int)sub.Dca, "Subinst DCA");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Ifc, "Subinst cutoff");
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, sub.Ifr, "Subinst resonance");
					}
				}

				// Check patterns
				for (c_int i = 0; i < mod.Pat; i++)
				{
					Xmp_Pattern xxp = mod.Xxp[i];

					line = sr.ReadLine();
					x = GetNextNumber(line, out s);
					Assert.AreEqual(x, xxp.Rows, "Pattern rows");

					for (c_int j = 0; j < mod.Chn; j++)
					{
						x = GetNextNumber(s, out s);
						Assert.AreEqual(x, xxp.Index[j], "Pattern index");
					}
				}

				// Check tracks
				for (c_int i = 0; i < mod.Trk; i++)
				{
					Xmp_Track xxt = mod.Xxt[i];

					line = sr.ReadLine();
					x = GetNextNumber(line, out s);
					Assert.AreEqual(x, xxt.Rows, "Track rows");

					using (MD5 md5 = MD5.Create())
					{
						byte[] checksum = md5.ComputeHash(GetEventBytes(xxt.Event, xxt.Rows));
						string d = Helpers.ToHex(checksum);
						Assert.AreEqual(s.Substring(1).ToUpper(), d, "Track data");
					}
				}

				// Check samples
				for (c_int i = 0; i < mod.Smp; i++)
				{
					Xmp_Sample xxs = mod.Xxs[i];
					c_int len = xxs.Len;

					if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
					{
						len *= 2;

						// Normalize data to little endian for the hash
						if (Test.Is_Big_Endian())
                            Convert_Endian(xxs.Data, xxs.DataOffset, xxs.Len);
					}

					line = sr.ReadLine();
					x = GetNextNumber(line, out s);
					Assert.AreEqual(x, xxs.Len, "Sample length");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, xxs.Lps, "Sample loop start");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, xxs.Lpe, "Sample loop end");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, (int)xxs.Flg, "Sample flags");

					s = s.Substring(1);
					if ((len > 0) && (xxs.Data != null))
					{
						using (MD5 md5 = MD5.Create())
						{
							byte[] checksum = md5.ComputeHash(xxs.Data, xxs.DataOffset, len);
							string d = Helpers.ToHex(checksum);
							Assert.AreEqual(s.Substring(0, 32).ToUpper(), d, "Sample data");
						}
					}

					s = s.Substring(32);
					Assert.AreEqual(s.Substring(1), xxs.Name, "Sample name");
				}

				// Check channels
				for (c_int i = 0; i < mod.Chn; i++)
				{
					Xmp_Channel xxc = mod.Xxc[i];

					line = sr.ReadLine();
					x = GetNextNumber(line, out s);
					Assert.AreEqual(x, xxc.Pan, "Channel pan");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, xxc.Vol, "Channel volume");
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, (int)xxc.Flg, "Channel flag");
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Compare_Playback(string path, string fileName, Playback_Sequence[] sequence, c_int rate, Xmp_Format flags, Xmp_Interp interp)
		{
			c_int ret;

			Ports.LibXmp.LibXmp opaque = Ports.LibXmp.LibXmp.Xmp_Create_Context();
			Assert.IsNotNull(opaque, "Create context");

			using (FileStream fs = new FileStream(Path.Combine(path, fileName), FileMode.Open, FileAccess.Read))
			{
				ret = opaque.Xmp_Load_Module_From_File(fs);
				Assert.AreEqual(0, ret, "Module load");
			}

			ret = opaque.Xmp_Start_Player(rate, flags);
			Assert.AreEqual(0, ret, "Start player");

			ret = opaque.Xmp_Set_Player(Xmp_Player.Interp, (c_int)interp);
			Assert.AreEqual(0, ret, "Set interp");

			c_int seqIndex = 0;
			while (sequence[seqIndex].Action != Playback_Action.Play_End)
			{
				switch (sequence[seqIndex].Action)
				{
					case Playback_Action.Play_End:	// Silence warning
						break;

					case Playback_Action.Play_Frames:
					{
						for (c_int count = sequence[seqIndex].Value; count > 0; count--)
							ret = opaque.Xmp_Play_Frame();

						Assert.AreEqual(sequence[seqIndex].Result, ret, "Play frames");
						break;
					}
				}

				seqIndex++;
			}

			opaque.Xmp_Free_Context();
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static int GetNextNumber(string str, out string newStr)
		{
			str = str.TrimStart();

			StringBuilder sb = new StringBuilder();

			int i;
			for (i = 0; i < str.Length; i++)
			{
				if (((str[i] >= '0') && (str[i] <= '9')) || (str[i] == '-'))
					sb.Append(str[i]);
				else
					break;
			}

			newStr = str.Substring(i);

			return int.Parse(sb.ToString());
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Check_Envelope(Xmp_Envelope env, StreamReader sr)
		{
			// Read envelope parameters
			string line = sr.ReadLine();
			int x = GetNextNumber(line, out string s);
			Assert.AreEqual(x, (int)env.Flg, "Envelope flags");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Npt, "Envelope number of points");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Scl, "Envelope scaling");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Sus, "Envelope sustain start");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Sue, "Envelope sustain end");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Lps, "Envelope loop start");
			x = GetNextNumber(s, out s);
			Assert.AreEqual(x, env.Lpe, "Envelope loop end");

			if (env.Npt > 0)
			{
				line = sr.ReadLine();
				s = line;

				for (c_int i = 0; i < env.Npt * 2; i++)
				{
					x = GetNextNumber(s, out s);
					Assert.AreEqual(x, env.Data[i], "Envelope point");
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static byte[] GetEventBytes(Xmp_Event[] evt, int evtCount)
		{
			byte[] ret = new byte[8 * evtCount];

			for (int i = 0; i < evtCount; i++)
			{
				Xmp_Event e = evt[i];
				int off = i * 8;

				ret[off + 0] = e.Note;
				ret[off + 1] = e.Ins;
				ret[off + 2] = e.Vol;
				ret[off + 3] = e.FxT;
				ret[off + 4] = e.FxP;
				ret[off + 5] = e.F2T;
				ret[off + 6] = e.F2P;
				ret[off + 7] = e._Flag;
			}

			return ret;
		}
		#endregion
	}
}
