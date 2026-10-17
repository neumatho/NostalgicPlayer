/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Streams;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.FileFormat_Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Io;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Alternative sample tuning collection class
	/// </summary>
	internal class CTuningCollection
	{
		private readonly vector<CTuning> m_Tunings = new vector<CTuning>();

		/// <summary>
		/// OpenMPT ‹= 1.26 had to following limits:
		///  *  255 built-in tunings (only 2 were ever actually provided)
		///  *  255 local tunings
		///  *  255 tune-specific tunings
		/// As 1.27 copies all used tunings into the module, the limit of 255 is no
		/// longer sufficient. In the worst case scenario, the module contains 255
		/// unused tunings and uses 255 local ones. In addition to that, allow the
		/// user to additionally import both built-in tunings.
		/// Older OpenMPT versions will silently skip loading tunings beyond index
		/// 255
		/// </summary>
		public size_t s_nMaxTuningCount = 255 + 255 + 2;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CTuning GetTuning(string name)//XX 61
		{
			for (size_t i = 0; i < m_Tunings.size(); i++)
			{
				if (m_Tunings[i].GetName() == name)
					return m_Tunings[i];
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public SerializationResult Deserialize(ReaderStream iStrm, ref string name, Encoding defaultCharset)//XX 127
		{
			long startPos = iStrm.Position;

			SerializationResult oldLoadingResult = DeserializeOld(iStrm, ref name, defaultCharset);

			if (oldLoadingResult == SerializationResult.NoMagic)
			{
				// An old version was not recognised - trying new version
				iStrm.Seek(startPos, SeekOrigin.Begin);

				SsbRead ssb = new SsbRead(iStrm);
				ssb.BeginRead("TC", 3);	// Version

				int8 use_Utf8 = 0;
				ssb.ReadItem(ref use_Utf8, "UTF8");
				Encoding charset = use_Utf8 != 0 ? Encoding.UTF8 : defaultCharset;

				ReadIterator iterBeg = ssb.GetReadBegin();
				ReadIterator iterEnd = ssb.GetReadEnd();

				for (ReadIterator iter = iterBeg; iter != iterEnd; iter++)
				{
					uint16 dummyEditMask = 0xffff;

					if (ssb.MatchesId(iter, "0"))
						ssb.ReadIterItem(iter, ref name, (strm, ref ustr, dummy) => CTuningS11n.ReadStr(strm, ref ustr, dummy, charset));
					else if (ssb.MatchesId(iter, "1"))
						ssb.ReadIterItem(iter, ref dummyEditMask);
					else if (ssb.MatchesId(iter, "2"))
					{
						CTuningCollection tuningCollection = this;
						ssb.ReadIterItem(iter, ref tuningCollection, (strm, ref tc, dummy) => ReadTuning(strm, tc, dummy, charset));
					}
				}

				if (ssb.HasFailed())
					return SerializationResult.Failure;
				else
					return SerializationResult.Success;
			}
			else
				return oldLoadingResult;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CTuning AddTuning(CTuning pT)//XX 259
		{
			if (m_Tunings.size() >= s_nMaxTuningCount)
				return null;

			if (pT == null)
				return null;

			CTuning result = pT;
			m_Tunings.push_back(Utility.move(pT));

			return result;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public CTuning AddTuning(ReaderStream inStrm, Encoding defaultCharset)//XX 275
		{
			if (m_Tunings.size() >= s_nMaxTuningCount)
				return null;

			if (inStrm.EndOfStream)
				return null;

			CTuning pT = CTuning.CreateDeserializeOld(inStrm, defaultCharset);

			if (pT == null)
				pT = CTuning.CreateDeserialize(inStrm, defaultCharset);

			if (pT == null)
				return null;

			CTuning result = pT;
			m_Tunings.push_back(Utility.move(pT));

			return result;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadTuning(ReaderStream iStrm, CTuningCollection tc, size_t dummy, Encoding defaultCharset)//XX 49
		{
			tc.AddTuning(iStrm, defaultCharset);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private SerializationResult DeserializeOld(ReaderStream inStrm, ref string uName, Encoding defaultCharset)//XX 168
		{
			// 1. Begin marker:
			Io_.ReadIntLE(inStrm, out uint32 beginMarker);

			if (beginMarker != Magic.MagicBE("TCSH"))	// Magic is reversed in file, hence BE
				return SerializationResult.NoMagic;

			// 2. Version
			Io_.ReadIntLE(inStrm, out int32 version);

			if ((version > 2) || (version < 1))
				return SerializationResult.Failure;

			// 3. Name
			if (version < 2)
			{
				if (!Io_.ReadSizedStringLE<uint32>(inStrm, out StdString name, 256))
					return SerializationResult.Failure;

				uName = defaultCharset.GetString(name.Span());
			}
			else
			{
				if (!Io_.ReadSizedStringLE<uint8>(inStrm, out StdString name))
					return SerializationResult.Failure;

				uName = defaultCharset.GetString(name.Span());
			}

			// 4. Editmask
			Io_.ReadIntLE(inStrm, out int16 em);

			// Not assigning the value yet, for if it sets some property const,
			// further loading might fail

			// 5. Tunings
			{
				Io_.ReadIntLE(inStrm, out uint32 s);

				if (s > 50)
					return SerializationResult.Failure;

				for (size_t i = 0; i < s; i++)
				{
					if (AddTuning(inStrm, defaultCharset) == null)
						return SerializationResult.Failure;
				}
			}

			// 6. End marker
			Io_.ReadIntLE(inStrm, out uint32 endMarker);

			if (endMarker != Magic.MagicBE("TCSF"))		// Magic is reversed in file, hence BE
				return SerializationResult.Failure;

			return SerializationResult.Success;
		}
		#endregion
	}
}
