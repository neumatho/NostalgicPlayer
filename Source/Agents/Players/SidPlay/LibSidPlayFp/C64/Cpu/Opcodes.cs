/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cpu
{
	/// <summary>
	/// Holds different opcode constants
	/// </summary>
	internal static class Opcodes
	{
		public const int OpcodeMax = 0x100;

		// HLT
		//  case 0x02: case 0x12: case 0x22: case 0x32: case 0x42: case 0x52:
		//  case 0x62: case 0x72: case 0x92: case 0xb2: case 0xd2: case 0xf2:

		public const byte BRKn = 0x00;
		public const byte JSRw = 0x20;
		public const byte RTIn = 0x40;
		public const byte RTSn = 0x60;
		public const byte NOPb = 0x80;
		public const byte NOPb1 = 0x82;
		public const byte NOPb2 = 0xc2;
		public const byte NOPb3 = 0xe2;
		public const byte NOPb4 = 0x89;
		public const byte LDYb = 0xa0;
		public const byte CPYb = 0xc0;
		public const byte CPXb = 0xe0;

		public const byte ORAix = 0x01;
		public const byte ANDix = 0x21;
		public const byte EORix = 0x41;
		public const byte ADCix = 0x61;
		public const byte STAix = 0x81;
		public const byte LDAix = 0xa1;
		public const byte CMPix = 0xc1;
		public const byte SBCix = 0xe1;

		public const byte LDXb = 0xa2;

		public const byte SLOix = 0x03;
		public const byte RLAix = 0x23;
		public const byte SREix = 0x43;
		public const byte RRAix = 0x63;
		public const byte SAXix = 0x83;
		public const byte LAXix = 0xa3;
		public const byte DCPix = 0xc3;
		public const byte ISBix = 0xe3;

		public const byte NOPz = 0x04;
		public const byte NOPz1 = 0x44;
		public const byte NOPz2 = 0x64;
		public const byte BITz = 0x24;
		public const byte STYz = 0x84;
		public const byte LDYz = 0xa4;
		public const byte CPYz = 0xc4;
		public const byte CPXz = 0xe4;

		public const byte ORAz = 0x05;
		public const byte ANDz = 0x25;
		public const byte EORz = 0x45;
		public const byte ADCz = 0x65;
		public const byte STAz = 0x85;
		public const byte LDAz = 0xa5;
		public const byte CMPz = 0xc5;
		public const byte SBCz = 0xe5;

		public const byte ASLz = 0x06;
		public const byte ROLz = 0x26;
		public const byte LSRz = 0x46;
		public const byte RORz = 0x66;
		public const byte STXz = 0x86;
		public const byte LDXz = 0xa6;
		public const byte DECz = 0xc6;
		public const byte INCz = 0xe6;

		public const byte SLOz = 0x07;
		public const byte RLAz = 0x27;
		public const byte SREz = 0x47;
		public const byte RRAz = 0x67;
		public const byte SAXz = 0x87;
		public const byte LAXz = 0xa7;
		public const byte DCPz = 0xc7;
		public const byte ISBz = 0xe7;

		public const byte PHPn = 0x08;
		public const byte PLPn = 0x28;
		public const byte PHAn = 0x48;
		public const byte PLAn = 0x68;
		public const byte DEYn = 0x88;
		public const byte TAYn = 0xa8;
		public const byte INYn = 0xc8;
		public const byte INXn = 0xe8;

		public const byte ORAb = 0x09;
		public const byte ANDb = 0x29;
		public const byte EORb = 0x49;
		public const byte ADCb = 0x69;
		public const byte LDAb = 0xa9;
		public const byte CMPb = 0xc9;
		public const byte SBCb = 0xe9;
		public const byte SBCb1 = 0xeb;

		public const byte ASLn = 0x0a;
		public const byte ROLn = 0x2a;
		public const byte LSRn = 0x4a;
		public const byte RORn = 0x6a;
		public const byte TXAn = 0x8a;
		public const byte TAXn = 0xaa;
		public const byte DEXn = 0xca;
		public const byte NOPn = 0xea;
		public const byte NOPn1 = 0x1a;
		public const byte NOPn2 = 0x3a;
		public const byte NOPn3 = 0x5a;
		public const byte NOPn4 = 0x7a;
		public const byte NOPn5 = 0xda;
		public const byte NOPn6 = 0xfa;

		public const byte ANCb = 0x0b;
		public const byte ANCb1 = 0x2b;
		public const byte ASRb = 0x4b;
		public const byte ARRb = 0x6b;
		public const byte ANEb = 0x8b;
		public const byte XAAb = 0x8b;
		public const byte LXAb = 0xab;
		public const byte SBXb = 0xcb;

		public const byte NOPa = 0x0c;
		public const byte BITa = 0x2c;
		public const byte JMPw = 0x4c;
		public const byte JMPi = 0x6c;
		public const byte STYa = 0x8c;
		public const byte LDYa = 0xac;
		public const byte CPYa = 0xcc;
		public const byte CPXa = 0xec;

		public const byte ORAa = 0x0d;
		public const byte ANDa = 0x2d;
		public const byte EORa = 0x4d;
		public const byte ADCa = 0x6d;
		public const byte STAa = 0x8d;
		public const byte LDAa = 0xad;
		public const byte CMPa = 0xcd;
		public const byte SBCa = 0xed;

		public const byte ASLa = 0x0e;
		public const byte ROLa = 0x2e;
		public const byte LSRa = 0x4e;
		public const byte RORa = 0x6e;
		public const byte STXa = 0x8e;
		public const byte LDXa = 0xae;
		public const byte DECa = 0xce;
		public const byte INCa = 0xee;

		public const byte SLOa = 0x0f;
		public const byte RLAa = 0x2f;
		public const byte SREa = 0x4f;
		public const byte RRAa = 0x6f;
		public const byte SAXa = 0x8f;
		public const byte LAXa = 0xaf;
		public const byte DCPa = 0xcf;
		public const byte ISBa = 0xef;

		public const byte BPLr = 0x10;
		public const byte BMIr = 0x30;
		public const byte BVCr = 0x50;
		public const byte BVSr = 0x70;
		public const byte BCCr = 0x90;
		public const byte BCSr = 0xb0;
		public const byte BNEr = 0xd0;
		public const byte BEQr = 0xf0;

		public const byte ORAiy = 0x11;
		public const byte ANDiy = 0x31;
		public const byte EORiy = 0x51;
		public const byte ADCiy = 0x71;
		public const byte STAiy = 0x91;
		public const byte LDAiy = 0xb1;
		public const byte CMPiy = 0xd1;
		public const byte SBCiy = 0xf1;

		public const byte SLOiy = 0x13;
		public const byte RLAiy = 0x33;
		public const byte SREiy = 0x53;
		public const byte RRAiy = 0x73;
		public const byte SHAiy = 0x93;
		public const byte LAXiy = 0xb3;
		public const byte DCPiy = 0xd3;
		public const byte ISBiy = 0xf3;

		public const byte NOPzx = 0x14;
		public const byte NOPzx1 = 0x34;
		public const byte NOPzx2 = 0x54;
		public const byte NOPzx3 = 0x74;
		public const byte NOPzx4 = 0xd4;
		public const byte NOPzx5 = 0xf4;
		public const byte STYzx = 0x94;
		public const byte LDYzx = 0xb4;

		public const byte ORAzx = 0x15;
		public const byte ANDzx = 0x35;
		public const byte EORzx = 0x55;
		public const byte ADCzx = 0x75;
		public const byte STAzx = 0x95;
		public const byte LDAzx = 0xb5;
		public const byte CMPzx = 0xd5;
		public const byte SBCzx = 0xf5;

		public const byte ASLzx = 0x16;
		public const byte ROLzx = 0x36;
		public const byte LSRzx = 0x56;
		public const byte RORzx = 0x76;
		public const byte STXzy = 0x96;
		public const byte LDXzy = 0xb6;
		public const byte DECzx = 0xd6;
		public const byte INCzx = 0xf6;

		public const byte SLOzx = 0x17;
		public const byte RLAzx = 0x37;
		public const byte SREzx = 0x57;
		public const byte RRAzx = 0x77;
		public const byte SAXzy = 0x97;
		public const byte LAXzy = 0xb7;
		public const byte DCPzx = 0xd7;
		public const byte ISBzx = 0xf7;

		public const byte CLCn = 0x18;
		public const byte SECn = 0x38;
		public const byte CLIn = 0x58;
		public const byte SEIn = 0x78;
		public const byte TYAn = 0x98;
		public const byte CLVn = 0xb8;
		public const byte CLDn = 0xd8;
		public const byte SEDn = 0xf8;

		public const byte ORAay = 0x19;
		public const byte ANDay = 0x39;
		public const byte EORay = 0x59;
		public const byte ADCay = 0x79;
		public const byte STAay = 0x99;
		public const byte LDAay = 0xb9;
		public const byte CMPay = 0xd9;
		public const byte SBCay = 0xf9;

		public const byte TXSn = 0x9a;
		public const byte TSXn = 0xba;

		public const byte SLOay = 0x1b;
		public const byte RLAay = 0x3b;
		public const byte SREay = 0x5b;
		public const byte RRAay = 0x7b;
		public const byte SHSay = 0x9b;
		public const byte TASay = 0x9b;
		public const byte LASay = 0xbb;
		public const byte DCPay = 0xdb;
		public const byte ISBay = 0xfb;

		public const byte NOPax = 0x1c;
		public const byte NOPax1 = 0x3c;
		public const byte NOPax2 = 0x5c;
		public const byte NOPax3 = 0x7c;
		public const byte NOPax4 = 0xdc;
		public const byte NOPax5 = 0xfc;
		public const byte SHYax = 0x9c;
		public const byte LDYax = 0xbc;

		public const byte ORAax = 0x1d;
		public const byte ANDax = 0x3d;
		public const byte EORax = 0x5d;
		public const byte ADCax = 0x7d;
		public const byte STAax = 0x9d;
		public const byte LDAax = 0xbd;
		public const byte CMPax = 0xdd;
		public const byte SBCax = 0xfd;

		public const byte ASLax = 0x1e;
		public const byte ROLax = 0x3e;
		public const byte LSRax = 0x5e;
		public const byte RORax = 0x7e;
		public const byte SHXay = 0x9e;
		public const byte LDXay = 0xbe;
		public const byte DECax = 0xde;
		public const byte INCax = 0xfe;

		public const byte SLOax = 0x1f;
		public const byte RLAax = 0x3f;
		public const byte SREax = 0x5f;
		public const byte RRAax = 0x7f;
		public const byte SHAay = 0x9f;
		public const byte LAXay = 0xbf;
		public const byte DCPax = 0xdf;
		public const byte ISBax = 0xff;

		// Instruction aliases
		public const byte ASOix = SLOix;
		public const byte LSEix = SREix;
		public const byte AXSix = SAXix;
		public const byte DCMix = DCPix;
		public const byte INSix = ISBix;
		public const byte ASOz = SLOz;
		public const byte LSEz = SREz;
		public const byte AXSz = SAXz;
		public const byte DCMz = DCPz;
		public const byte INSz = ISBz;
		public const byte ALRb = ASRb;
		public const byte OALb = LXAb;
		public const byte ASOa = SLOa;
		public const byte LSEa = SREa;
		public const byte AXSa = SAXa;
		public const byte DCMa = DCPa;
		public const byte INSa = ISBa;
		public const byte ASOiy = SLOiy;
		public const byte LSEiy = SREiy;
		public const byte AXSiy = SHAiy;
		public const byte DCMiy = DCPiy;
		public const byte INSiy = ISBiy;
		public const byte ASOzx = SLOzx;
		public const byte LSEzx = SREzx;
		public const byte AXSzy = SAXzy;
		public const byte DCMzx = DCPzx;
		public const byte INSzx = ISBzx;
		public const byte ASOay = SLOay;
		public const byte LSEay = SREay;
		public const byte DCMay = DCPay;
		public const byte INSay = ISBay;
		public const byte SAYax = SHYax;
		public const byte XASay = SHXay;
		public const byte ASOax = SLOax;
		public const byte LSEax = SREax;
		public const byte AXAay = SHAay;
		public const byte DCMax = DCPax;
		public const byte INSax = ISBax;
		public const byte SKBn = NOPb;
		public const byte SKWn = NOPa;
	}
}
