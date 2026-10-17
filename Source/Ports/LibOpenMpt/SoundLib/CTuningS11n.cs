/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// 
	/// </summary>
	internal static class CTuningS11n
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadNoteMap(Stream iStrm, map<TuningNoteIndexType, string> m, size_t dummy, Encoding charset)//XX 952
		{
			Io_.ReadAdaptiveInt64LE(iStrm, out uint64 val);
			OpenMpt.LimitMax(ref val, (uint64)256);	// Read 256 at max

			for (size_t i = 0; i < val; i++)
			{
				Io_.ReadIntLE(iStrm, out int16 key);
				Io_.ReadSizedStringLE<uint8>(iStrm, out StdString str);
				m[key] = charset.GetString(str.Span());
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadRatioTable(Stream iStrm, ref vector<TuningRatioType> v, size_t _)//XX 969
		{
			Io_.ReadAdaptiveInt64LE(iStrm, out uint64 val);
			v.resize(Math.Min(size_t.CreateSaturating(val), 256));	// Read 256 vals at max

			for (size_t i = 0; i < v.size(); i++)
			{
				Ieee754Binary32LE tmp = (Ieee754Binary32LE)0.0f;
				Io_.Read(iStrm, ref tmp);
				v[i] = tmp;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void ReadStr(Stream iStrm, ref string ustr, size_t dummy, Encoding charset)//XX 983
		{
			StdString str = new StdString();

			Io_.ReadAdaptiveInt64LE(iStrm, out uint64 val);
			size_t nSize = val > 255 ? 255 : val;	// Read 255 characters at max

			str.clear();
			str.resize(nSize);

			for (size_t i = 0; i < nSize; i++)
				Io_.ReadIntLE(iStrm, out str[i]);

			if (str.find_first_of("\0") != StdString.npos)
			{
				// Trim \0 at the end
				str.resize(str.find_first_of("\0"));
			}

			ustr = charset.GetString(str.Span());
		}
	}
}
