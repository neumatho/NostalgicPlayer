/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Identify the syntax and semantics of the bitstream.
	/// The principle is roughly:
	/// Two decoders with the same ID can decode the same streams.
	/// Two encoders with the same ID can encode compatible streams.
	/// There may be slight deviations from the principle due to implementation
	/// details.
	///
	/// If you add a codec ID to this list, add it so that
	///  1. no value of an existing codec ID changes (that would break ABI),
	///  2. it is as close as possible to similar codecs
	///
	/// After adding new codec IDs, do not forget to add an entry to the codec
	/// descriptor list and bump libavcodec minor version
	/// </summary>
	public enum AvCodecId
	{
		/// <summary></summary>
		None,

		//
		// Video codecs
		//

		/// <summary></summary>
		Mpeg1Video,
		/// <summary></summary>
		Mpeg2Video,		// Preferred ID for MPEG-1/2 video decoding
		/// <summary></summary>
		H261,
		/// <summary></summary>
		H263,
		/// <summary></summary>
		Rv10,
		/// <summary></summary>
		Rv20,
		/// <summary></summary>
		MJpeg,
		/// <summary></summary>
		MJpegB,
		/// <summary></summary>
		LJpeg,
		/// <summary></summary>
		SP5X,
		/// <summary></summary>
		JpegLs,
		/// <summary></summary>
		Mpeg4,
		/// <summary></summary>
		RawVideo,
		/// <summary></summary>
		MsMpeg4v1,
		/// <summary></summary>
		MsMpeg4v2,
		/// <summary></summary>
		MsMpeg4v3,
		/// <summary></summary>
		Wmv1,
		/// <summary></summary>
		Wmv2,
		/// <summary></summary>
		H263P,
		/// <summary></summary>
		H263I,
		/// <summary></summary>
		Flv1,
		/// <summary></summary>
		Svq1,
		/// <summary></summary>
		Svq3,
		/// <summary></summary>
		DvVideo,
		/// <summary></summary>
		HuffYuv,
		/// <summary></summary>
		Cyuv,
		/// <summary></summary>
		H264,
		/// <summary></summary>
		Indeo3,
		/// <summary></summary>
		Vp3,
		/// <summary></summary>
		Theora,
		/// <summary></summary>
		Asv1,
		/// <summary></summary>
		Asv2,
		/// <summary></summary>
		Ffv1,
		/// <summary></summary>
		_4Xm,
		/// <summary></summary>
		Vcr1,
		/// <summary></summary>
		Cljr,
		/// <summary></summary>
		Mdec,
		/// <summary></summary>
		Roq,
		/// <summary></summary>
		Interplay_Video,
		/// <summary></summary>
		Xan_Wc3,
		/// <summary></summary>
		Xan_Wc4,
		/// <summary></summary>
		Rpza,
		/// <summary></summary>
		Cinepak,
		/// <summary></summary>
		Ws_Vqa,
		/// <summary></summary>
		MsRle,
		/// <summary></summary>
		MsVideo1,
		/// <summary></summary>
		Idcin,
		/// <summary></summary>
		_8BPS,
		/// <summary></summary>
		Smc,
		/// <summary></summary>
		Flic,
		/// <summary></summary>
		TrueMotion1,
		/// <summary></summary>
		VmdVideo,
		/// <summary></summary>
		Mszh,
		/// <summary></summary>
		ZLib,
		/// <summary></summary>
		QtRle,
		/// <summary></summary>
		Tscc,
		/// <summary></summary>
		Ulti,
		/// <summary></summary>
		QDraw,
		/// <summary></summary>
		Vixl,
		/// <summary></summary>
		Qpeg,
		/// <summary></summary>
		Png,
		/// <summary></summary>
		Ppm,
		/// <summary></summary>
		Pbm,
		/// <summary></summary>
		Pgm,
		/// <summary></summary>
		PgmYUV,
		/// <summary></summary>
		Pam,
		/// <summary></summary>
		FfvHuff,
		/// <summary></summary>
		Rv30,
		/// <summary></summary>
		Rv40,
		/// <summary></summary>
		Vc1,
		/// <summary></summary>
		Wmv3,
		/// <summary></summary>
		Loco,
		/// <summary></summary>
		Wnv1,
		/// <summary></summary>
		Aasc,
		/// <summary></summary>
		Indeo2,
		/// <summary></summary>
		Fraps,
		/// <summary></summary>
		TrueMotion2,
		/// <summary></summary>
		Bmp,
		/// <summary></summary>
		Cscd,
		/// <summary></summary>
		MmVideo,
		/// <summary></summary>
		Zmbv,
		/// <summary></summary>
		Avs,
		/// <summary></summary>
		SmackVideo,
		/// <summary></summary>
		Nuv,
		/// <summary></summary>
		Kmvc,
		/// <summary></summary>
		FlashSv,
		/// <summary></summary>
		Cavs,
		/// <summary></summary>
		Jpeg2000,
		/// <summary></summary>
		Vmnc,
		/// <summary></summary>
		Vp5,
		/// <summary></summary>
		Vp6,
		/// <summary></summary>
		Vp6F,
		/// <summary></summary>
		Targa,
		/// <summary></summary>
		DsicinVideo,
		/// <summary></summary>
		TierTexSeqVideo,
		/// <summary></summary>
		Tiff,
		/// <summary></summary>
		Gif,
		/// <summary></summary>
		Dxa,
		/// <summary></summary>
		DnxHd,
		/// <summary></summary>
		Thp,
		/// <summary></summary>
		Sgi,
		/// <summary></summary>
		C93,
		/// <summary></summary>
		BethSoftVid,
		/// <summary></summary>
		Ptx,
		/// <summary></summary>
		Txd,
		/// <summary></summary>
		Vp6A,
		/// <summary></summary>
		Amv,
		/// <summary></summary>
		Vb,
		/// <summary></summary>
		Pcx,
		/// <summary></summary>
		Sunrast,
		/// <summary></summary>
		Indeo4,
		/// <summary></summary>
		Indeo5,
		/// <summary></summary>
		Mimic,
		/// <summary></summary>
		Rl2,
		/// <summary></summary>
		Escape124,
		/// <summary></summary>
		Dirac,
		/// <summary></summary>
		Bfi,
		/// <summary></summary>
		Cmv,
		/// <summary></summary>
		MotionPixels,
		/// <summary></summary>
		Tgv,
		/// <summary></summary>
		Tgq,
		/// <summary></summary>
		Tqi,
		/// <summary></summary>
		Aura,
		/// <summary></summary>
		Aura2,
		/// <summary></summary>
		V210X,
		/// <summary></summary>
		Tmv,
		/// <summary></summary>
		V210,
		/// <summary></summary>
		Dpx,
		/// <summary></summary>
		Mad,
		/// <summary></summary>
		Frwu,
		/// <summary></summary>
		FlashSv2,
		/// <summary></summary>
		CdGraphics,
		/// <summary></summary>
		R210,
		/// <summary></summary>
		Anm,
		/// <summary></summary>
		BinkVideo,
		/// <summary></summary>
		Iff_Ilbm,
		/// <summary></summary>
		Iff_ByteRun1 = Iff_Ilbm,
		/// <summary></summary>
		Kgv1,
		/// <summary></summary>
		Yop,
		/// <summary></summary>
		Vp8,
		/// <summary></summary>
		Pictor,
		/// <summary></summary>
		Ansi,
		/// <summary></summary>
		A64_Multi,
		/// <summary></summary>
		A64_Multi5,
		/// <summary></summary>
		R10K,
		/// <summary></summary>
		Mxpeg,
		/// <summary></summary>
		Lagarith,
		/// <summary></summary>
		Prores,
		/// <summary></summary>
		Jv,
		/// <summary></summary>
		Dfa,
		/// <summary></summary>
		Wmv3Image,
		/// <summary></summary>
		Vc1Image,
		/// <summary></summary>
		UtVideo,
		/// <summary></summary>
		Bmv_Video,
		/// <summary></summary>
		Vble,
		/// <summary></summary>
		Dxtory,
		/// <summary></summary>
		Xwd,
		/// <summary></summary>
		Cdxl,
		/// <summary></summary>
		Xbm,
		/// <summary></summary>
		ZeroCodec,
		/// <summary></summary>
		Mss1,
		/// <summary></summary>
		Msa1,
		/// <summary></summary>
		Tscc2,
		/// <summary></summary>
		Mts2,
		/// <summary></summary>
		Cllc,
		/// <summary></summary>
		Mss2,
		/// <summary></summary>
		Vp9,
		/// <summary></summary>
		Aic,
		/// <summary></summary>
		Escape130,
		/// <summary></summary>
		G2M,
		/// <summary></summary>
		Webp,
		/// <summary></summary>
		Hnm4_Video,
		/// <summary></summary>
		Hevc,
		/// <summary></summary>
		H265 = Hevc,
		/// <summary></summary>
		Fic,
		/// <summary></summary>
		Alias_Pix,
		/// <summary></summary>
		Brender_Pix,
		/// <summary></summary>
		Paf_Video,
		/// <summary></summary>
		/// <summary></summary>
		Exr,
		/// <summary></summary>
		Vp7,
		/// <summary></summary>
		Sanm,
		/// <summary></summary>
		/// <summary></summary>
		SgiRle,
		/// <summary></summary>
		Mvc1,
		/// <summary></summary>
		/// <summary></summary>
		Mvc2,
		/// <summary></summary>
		Hqx,
		/// <summary></summary>
		/// <summary></summary>
		Tdsc,
		/// <summary></summary>
		Hq_Hqa,
		/// <summary></summary>
		Hap,
		/// <summary></summary>
		Dds,
		/// <summary></summary>
		Dxv,
		/// <summary></summary>
		ScreenPresso,
		/// <summary></summary>
		Rscc,
		/// <summary></summary>
		Avs2,
		/// <summary></summary>
		Pgx,
		/// <summary></summary>
		Avs3,
		/// <summary></summary>
		Msp2,
		/// <summary></summary>
		Vvc,
		/// <summary></summary>
		H266 = Vvc,
		/// <summary></summary>
		Y41P,
		/// <summary></summary>
		Avrp,
		/// <summary></summary>
		_012V,
		/// <summary></summary>
		Avui,
		/// <summary></summary>
		Targa_Y216,
		/// <summary></summary>
		YUV4,
		/// <summary></summary>
		Avrn,
		/// <summary></summary>
		Cpia,
		/// <summary></summary>
		XFace,
		/// <summary></summary>
		Snow,
		/// <summary></summary>
		SmvJpeg,
		/// <summary></summary>
		APng,
		/// <summary></summary>
		Daala,
		/// <summary></summary>
		Cfhd,
		/// <summary></summary>
		TrueMotion2Rt,
		/// <summary></summary>
		M101,
		/// <summary></summary>
		MagicYuv,
		/// <summary></summary>
		SheerVideo,
		/// <summary></summary>
		Ylc,
		/// <summary></summary>
		Psd,
		/// <summary></summary>
		Pixlet,
		/// <summary></summary>
		SpeedHq,
		/// <summary></summary>
		Fmvc,
		/// <summary></summary>
		Scpr,
		/// <summary></summary>
		ClearVideo,
		/// <summary></summary>
		Xpm,
		/// <summary></summary>
		Av1,
		/// <summary></summary>
		Bitpacked,
		/// <summary></summary>
		Mscc,
		/// <summary></summary>
		Srgc,
		/// <summary></summary>
		Svg,
		/// <summary></summary>
		Gdv,
		/// <summary></summary>
		Fits,
		/// <summary></summary>
		Imm4,
		/// <summary></summary>
		Prosumer,
		/// <summary></summary>
		Mwsc,
		/// <summary></summary>
		Wcmv,
		/// <summary></summary>
		Rasc,
		/// <summary></summary>
		Hymt,
		/// <summary></summary>
		Arbc,
		/// <summary></summary>
		Agm,
		/// <summary></summary>
		Lscr,
		/// <summary></summary>
		Vp4,
		/// <summary></summary>
		Imm5,
		/// <summary></summary>
		Mvdv,
		/// <summary></summary>
		Mvha,
		/// <summary></summary>
		CdToons,
		/// <summary></summary>
		Mv30,
		/// <summary></summary>
		Notchlc,
		/// <summary></summary>
		Pfm,
		/// <summary></summary>
		MobiClip,
		/// <summary></summary>
		PhotoCd,
		/// <summary></summary>
		Ipu,
		/// <summary></summary>
		Argo,
		/// <summary></summary>
		Cri,
		/// <summary></summary>
		Simbiosis_Imx,
		/// <summary></summary>
		Sga_Video,
		/// <summary></summary>
		Gem,
		/// <summary></summary>
		Vbn,
		/// <summary></summary>
		JpegXl,
		/// <summary></summary>
		Qoi,
		/// <summary></summary>
		Phm,
		/// <summary></summary>
		Radiance_Hdr,
		/// <summary></summary>
		Wbmp,
		/// <summary></summary>
		Media100,
		/// <summary></summary>
		Vqc,
		/// <summary></summary>
		Pdv,
		/// <summary></summary>
		Evc,
		/// <summary></summary>
		Rtv1,
		/// <summary></summary>
		Vmix,
		/// <summary></summary>
		Lead,
		/// <summary></summary>
		Dnxuc,
		/// <summary></summary>
		Rv60,
		/// <summary></summary>
		JpegXl_Anim,
		/// <summary></summary>
		Apv,
		/// <summary></summary>
		Prores_Raw,

		//
		// Various PCM "codecs"
		//

		/// <summary></summary>
		First_Audio = 0x10000,		// A dummy id pointing at the start of audio codecs
		/// <summary></summary>
		Pcm_S16Le = 0x10000,
		/// <summary></summary>
		Pcm_S16Be,
		/// <summary></summary>
		Pcm_U16Le,
		/// <summary></summary>
		Pcm_U16Be,
		/// <summary></summary>
		Pcm_S8,
		/// <summary></summary>
		Pcm_U8,
		/// <summary></summary>
		Pcm_Mulaw,
		/// <summary></summary>
		Pcm_Alaw,
		/// <summary></summary>
		Pcm_S32Le,
		/// <summary></summary>
		Pcm_S32Be,
		/// <summary></summary>
		Pcm_U32Le,
		/// <summary></summary>
		Pcm_U32Be,
		/// <summary></summary>
		Pcm_S24Le,
		/// <summary></summary>
		Pcm_S24Be,
		/// <summary></summary>
		Pcm_U24Le,
		/// <summary></summary>
		Pcm_U24Be,
		/// <summary></summary>
		Pcm_S24Daud,
		/// <summary></summary>
		Pcm_Zork,
		/// <summary></summary>
		Pcm_S16Le_Planar,
		/// <summary></summary>
		Pcm_Dvd,
		/// <summary></summary>
		Pcm_F32Be,
		/// <summary></summary>
		Pcm_F32Le,
		/// <summary></summary>
		Pcm_F64Be,
		/// <summary></summary>
		Pcm_F64Le,
		/// <summary></summary>
		Pcm_Bluray,
		/// <summary></summary>
		Pcm_Lxf,
		/// <summary></summary>
		S302M,
		/// <summary></summary>
		Pcm_S8_Planar,
		/// <summary></summary>
		Pcm_S24Le_Planar,
		/// <summary></summary>
		Pcm_S32Le_Planar,
		/// <summary></summary>
		Pcm_S16Be_Planar,
		/// <summary></summary>
		Pcm_S64Le,
		/// <summary></summary>
		Pcm_S64Be,
		/// <summary></summary>
		Pcm_F16Le,
		/// <summary></summary>
		Pcm_F24Le,
		/// <summary></summary>
		Pcm_Vidc,
		/// <summary></summary>
		Pcm_Sga,

		//
		// Various ADPCM codecs
		//

		/// <summary></summary>
		Adpcm_Ima_Qt = 0x11000,
		/// <summary></summary>
		Adpcm_Ima_Wav,
		/// <summary></summary>
		Adpcm_Ima_Dk3,
		/// <summary></summary>
		Adpcm_Ima_Dk4,
		/// <summary></summary>
		Adpcm_Ima_Ws,
		/// <summary></summary>
		Adpcm_Ima_SmJpeg,
		/// <summary></summary>
		Adpcm_Ms,
		/// <summary></summary>
		Adpcm_4Xm,
		/// <summary></summary>
		Adpcm_Xa,
		/// <summary></summary>
		Adpcm_Adx,
		/// <summary></summary>
		Adpcm_Ea,
		/// <summary></summary>
		Adpcm_G726,
		/// <summary></summary>
		Adpcm_Ct,
		/// <summary></summary>
		Adpcm_Swf,
		/// <summary></summary>
		/// <summary></summary>
		Adpcm_Yamaha,
		/// <summary></summary>
		Adpcm_SbPro_4,
		/// <summary></summary>
		Adpcm_SbPro_3,
		/// <summary></summary>
		Adpcm_SbPro_2,
		/// <summary></summary>
		Adpcm_Thp,
		/// <summary></summary>
		Adpcm_Ima_Amv,
		/// <summary></summary>
		Adpcm_Ea_R1,
		/// <summary></summary>
		Adpcm_Ea_R3,
		/// <summary></summary>
		Adpcm_Ea_R2,
		/// <summary></summary>
		Adpcm_Ima_Ea_Sead,
		/// <summary></summary>
		Adpcm_IMA_Ee_Eacs,
		/// <summary></summary>
		Adpcm_Ea_Xas,
		/// <summary></summary>
		Adpcm_Ea_Maxis_Xa,
		/// <summary></summary>
		Adpcm_Ima_Iss,
		/// <summary></summary>
		Adpcm_G722,
		/// <summary></summary>
		Adpcm_Ima_Apc,
		/// <summary></summary>
		Adpcm_Vima,
		/// <summary></summary>
		Adpcm_Afc,
		/// <summary></summary>
		Adpcm_Ima_Oki,
		/// <summary></summary>
		Adpcm_Dtk,
		/// <summary></summary>
		Adpcm_Ima_Rad,
		/// <summary></summary>
		Adpcm_G726Le,
		/// <summary></summary>
		Adpcm_Thp_Le,
		/// <summary></summary>
		Adpcm_Psx,
		/// <summary></summary>
		Adpcm_Aica,
		/// <summary></summary>
		Adpcm_Ima_Dat4,
		/// <summary></summary>
		Adpcm_Mtaf,
		/// <summary></summary>
		Adpcm_Agm,
		/// <summary></summary>
		Adpcm_Argo,
		/// <summary></summary>
		Adpcm_Ima_Ssi,
		/// <summary></summary>
		Adpcm_Zork,
		/// <summary></summary>
		Adpcm_Ima_Apm,
		/// <summary></summary>
		Adpcm_Ima_Alp,
		/// <summary></summary>
		Adpcm_Ima_Mtf,
		/// <summary></summary>
		Adpcm_Ima_Cunning,
		/// <summary></summary>
		Adpcm_Ima_Moflex,
		/// <summary></summary>
		Adpcm_Ima_Acorn,
		/// <summary></summary>
		Adpcm_Xmd,
		/// <summary></summary>
		Adpcm_Ima_Xbox,
		/// <summary></summary>
		Adpcm_Sanyo,
		/// <summary></summary>
		Adpcm_Ima_Hvqm4,
		/// <summary></summary>
		Adpcm_Ima_Pda,
		/// <summary></summary>
		Adpcm_N64,
		/// <summary></summary>
		Adpcm_Ima_Hvqm2,
		/// <summary></summary>
		Adpcm_Ima_Magix,
		/// <summary></summary>
		Adpcm_Psxc,
		/// <summary></summary>
		Adpcm_Circus,
		/// <summary></summary>
		Adpcm_Ima_Escape,

		//
		// AMR
		//

		/// <summary></summary>
		Amr_Nb = 0x12000,
		/// <summary></summary>
		Amr_Wb,

		//
		// RealAudio codecs
		//

		/// <summary></summary>
		Ra_144 = 0x13000,
		/// <summary></summary>
		Ra_288,

		//
		// Various DPCM codecs
		//

		/// <summary></summary>
		Roq_Dpcm = 0x14000,
		/// <summary></summary>
		Interplay_Dpcm,
		/// <summary></summary>
		Xan_Dpcm,
		/// <summary></summary>
		Sol_Dpcm,
		/// <summary></summary>
		Sdx2_Dpcm,
		/// <summary></summary>
		Gremlin_Dpcm,
		/// <summary></summary>
		Derf_Dpcm,
		/// <summary></summary>
		Wady_Dpcm,
		/// <summary></summary>
		Cbd2_Dpcm,

		//
		// Audio codecs
		//

		/// <summary></summary>
		Mp2 = 0x15000,
		/// <summary></summary>
		Mp3,			// Preferred ID for decoding MPEG audio layer 1, 2 or 3
		/// <summary></summary>
		Aac,
		/// <summary></summary>
		Ac3,
		/// <summary></summary>
		Dts,
		/// <summary></summary>
		Vorbis,
		/// <summary></summary>
		DvAudio,
		/// <summary></summary>
		WmaV1,
		/// <summary></summary>
		WmaV2,
		/// <summary></summary>
		Mace3,
		/// <summary></summary>
		Mace6,
		/// <summary></summary>
		VmdAudio,
		/// <summary></summary>
		Flac,
		/// <summary></summary>
		Mp3Adu,
		/// <summary></summary>
		Mp3On4,
		/// <summary></summary>
		Shorten,
		/// <summary></summary>
		Alac,
		/// <summary></summary>
		Westwood_Snd1,
		/// <summary></summary>
		Gsm,		// As in Berlin toast format
		/// <summary></summary>
		Qdm2,
		/// <summary></summary>
		Cook,
		/// <summary></summary>
		TrueSpeech,
		/// <summary></summary>
		Tta,
		/// <summary></summary>
		SmackAudio,
		/// <summary></summary>
		Qcelp,
		/// <summary></summary>
		WavPack,
		/// <summary></summary>
		DsicinAudio,
		/// <summary></summary>
		Imc,
		/// <summary></summary>
		MusePack7,
		/// <summary></summary>
		Mlp,
		/// <summary></summary>
		Gsm_Ms, /* as found in WAV */
		/// <summary></summary>
		Atrac3,
		/// <summary></summary>
		Ape,
		/// <summary></summary>
		Nellymoser,
		/// <summary></summary>
		MusePack8,
		/// <summary></summary>
		Speex,
		/// <summary></summary>
		WmaVoice,
		/// <summary></summary>
		WmaPro,
		/// <summary></summary>
		WmaLossless,
		/// <summary></summary>
		Atrac3P,
		/// <summary></summary>
		Eac3,
		/// <summary></summary>
		Sipr,
		/// <summary></summary>
		Mp1,
		/// <summary></summary>
		TwinVq,
		/// <summary></summary>
		TrueHd,
		/// <summary></summary>
		Mp4Als,
		/// <summary></summary>
		Atrac1,
		/// <summary></summary>
		BinkAudio_Rdft,
		/// <summary></summary>
		BinkAudio_Dct,
		/// <summary></summary>
		Aac_Latm,
		/// <summary></summary>
		Qdmc,
		/// <summary></summary>
		Celt,
		/// <summary></summary>
		G723_1,
		/// <summary></summary>
		G729,
		/// <summary></summary>
		_8Svx_Exp,
		/// <summary></summary>
		_8Svx_Fib,
		/// <summary></summary>
		Bmv_Audio,
		/// <summary></summary>
		Ralf,
		/// <summary></summary>
		Iac,
		/// <summary></summary>
		Ilbc,
		/// <summary></summary>
		Opus,
		/// <summary></summary>
		Comfort_Noise,
		/// <summary></summary>
		Tak,
		/// <summary></summary>
		Metasound,
		/// <summary></summary>
		Paf_Audio,
		/// <summary></summary>
		On2Avc,
		/// <summary></summary>
		Dss_Sp,
		/// <summary></summary>
		Codec2,
		/// <summary></summary>
		FFWavesynth,
		/// <summary></summary>
		Sonic,
		/// <summary></summary>
		Sonic_Ls,
		/// <summary></summary>
		Evrc,
		/// <summary></summary>
		Smv,
		/// <summary></summary>
		Dsd_Lsbf,
		/// <summary></summary>
		Dsd_Msbf,
		/// <summary></summary>
		Dsd_Lsbf_Planar,
		/// <summary></summary>
		Dsd_Msbf_Planar,
		/// <summary></summary>
		_4Gv,
		/// <summary></summary>
		Interplay_Acm,
		/// <summary></summary>
		Xma1,
		/// <summary></summary>
		Xma2,
		/// <summary></summary>
		Dst,
		/// <summary></summary>
		Atrac3Al,
		/// <summary></summary>
		Atrac3Pal,
		/// <summary></summary>
		Dolby_E,
		/// <summary></summary>
		Aptx,
		/// <summary></summary>
		Aptx_Hd,
		/// <summary></summary>
		Sbc,
		/// <summary></summary>
		Atrac9,
		/// <summary></summary>
		Hcom,
		/// <summary></summary>
		Acelp_Kelvin,
		/// <summary></summary>
		Mpegh_3D_Audio,
		/// <summary></summary>
		Siren,
		/// <summary></summary>
		Hca,
		/// <summary></summary>
		FastAudio,
		/// <summary></summary>
		MsnSiren,
		/// <summary></summary>
		Dfpwm,
		/// <summary></summary>
		Bonk,
		/// <summary></summary>
		Misc4,
		/// <summary></summary>
		Apac,
		/// <summary></summary>
		Ftr,
		/// <summary></summary>
		WavArc,
		/// <summary></summary>
		Rka,
		/// <summary></summary>
		Ac4,
		/// <summary></summary>
		Osq,
		/// <summary></summary>
		Qoa,
		/// <summary></summary>
		Lc3,
		/// <summary></summary>
		G728,
		/// <summary></summary>
		Ahx,

		//
		// Subtitle codecs
		//

		/// <summary></summary>
		First_Subtitle = 0x17000,		// A dummy ID pointing at the start of subtitle codecs
		/// <summary></summary>
		Dvd_Subtitle = 0x17000,
		/// <summary></summary>
		Dvb_Subtitle,
		/// <summary></summary>
		Text,							// Raw UTF-8 text
		/// <summary></summary>
		Xsub,
		/// <summary></summary>
		Ssa,
		/// <summary></summary>
		Mov_Text,
		/// <summary></summary>
		Hdmv_Pgs_Subtitle,
		/// <summary></summary>
		Dvb_TeleText,
		/// <summary></summary>
		Srt,
		/// <summary></summary>
		MicroDvd,
		/// <summary></summary>
		Eia_608,
		/// <summary></summary>
		JacoSub,
		/// <summary></summary>
		Sami,
		/// <summary></summary>
		RealText,
		/// <summary></summary>
		Stl,
		/// <summary></summary>
		SubViewer1,
		/// <summary></summary>
		SubViewer,
		/// <summary></summary>
		SubRip,
		/// <summary></summary>
		WebTT,
		/// <summary></summary>
		Mpl2,
		/// <summary></summary>
		VPlayer,
		/// <summary></summary>
		Pjs,
		/// <summary></summary>
		Ass,
		/// <summary></summary>
		Hdmv_Text_Subtitle,
		/// <summary></summary>
		Ttml,
		/// <summary></summary>
		Arib_Caption,
		/// <summary></summary>
		Ivtv_Vbi,

		//
		// Other specific kind of codecs (generally used for attachments)
		//

		/// <summary></summary>
		First_Unknown = 0x18000,		// A dummy ID pointing at the start of various fake codecs
		/// <summary></summary>
		Ttf = 0x18000,

		/// <summary></summary>
		Scte_35,						// Contain timestamp estimated through PCR of program stream
		/// <summary></summary>
		Epg,
		/// <summary></summary>
		BinText,
		/// <summary></summary>
		Xbin,
		/// <summary></summary>
		Idf,
		/// <summary></summary>
		Otf,
		/// <summary></summary>
		Smpte_Klv,
		/// <summary></summary>
		Dvd_Nav,
		/// <summary></summary>
		Timed_Id3,
		/// <summary></summary>
		Bin_Data,
		/// <summary></summary>
		Smpte_2038,
		/// <summary></summary>
		Lcevc,
		/// <summary></summary>
		Smpte_436M_Anc,

		/// <summary></summary>
		Probe = 0x19000,				// codec_id is not known (like AV_CODEC_ID_NONE) but lavf should attempt to identify it

		/// <summary></summary>
		Mpeg2TS = 0x20000,				// _FAKE_ codec to indicate a raw MPEG-2 TS stream (only used by libavformat)
		/// <summary></summary>
		Mpeg4Systems = 0x20001,			// _FAKE_ codec to indicate a MPEG-4 Systems stream (only used by libavformat)
		/// <summary></summary>
		FFMetadata = 0x21000,			// Dummy codec for streams containing only metadata information
		/// <summary></summary>
		Wrapped_AvFrame = 0x21001,		// Passthrough codec, AVFrames wrapped in AVPacket

		/// <summary>
		/// Dummy null video codec, useful mainly for development and debugging.
		/// Null encoder/decoder discard all input and never return any output
		/// </summary>
		VNull,
		/// <summary>
		/// Dummy null audio codec, useful mainly for development and debugging.
		/// Null encoder/decoder discard all input and never return any output
		/// </summary>
		ANull
	}
}
