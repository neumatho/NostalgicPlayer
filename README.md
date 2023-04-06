# NostalgicPlayer
Modules are relatively small files which contains various sound samples and information on how they should be played. This way of making music goes a long way back, and even at the time of the Commodore 64 the concept was used. But it was on the Amiga computer it really started off. The original type of modules were made with a program called SoundTracker, but since then a lot of different module types have shown up, all of them offering different features - such as more sound channels, more samples, realtime echo and so on.

NostalgicPlayer is a program that can play these modules. NostalgicPlayer is a Windows version of the APlayer, which was started on the Amiga computer back in 1993, later continued on BeOS, and was a reaction to the lack of good module players for the Amiga.

Please enjoy this program, and support the development by giving any feedback (bug reports and flattering comments :-))

# Download

If you want to try it out, you can install the player from Microsoft Store <a href="https://apps.microsoft.com/store/detail/nostalgicplayer/9N1TNGT8PKC8">here</a>. If you don't have any modules by yourself, check out my <a href="https://nostalgicplayer.dk">homepage</a>. There you can find my collection of modules in all supported formats. When new formats are supported, the module list will grow.

# License

NostalgicPlayer is licensed under the <a href="https://github.com/neumatho/NostalgicPlayer/blob/main/LICENSE">Apache-2.0 License</a>. SidPlayFp, ReSIDfp, MikMod player and converter are licensed under <a href="https://github.com/neumatho/NostalgicPlayer/blob/main/Source/Agents/Players/MikMod/LICENSE">GNU Lesser General Public License</a>. Ancient decruncher is licensed under the <a href="https://github.com/neumatho/NostalgicPlayer/blob/main/Source/Agents/Decrunchers/AncientDecruncher/LICENSE">BSD 2-Clause License</a>.

# Structure

The player is structured, so it is easy to create your own user interface around the logic. All the main logic is in the *NostalgicPlayerLibrary* and *NostalgicPlayerKit*. The later is used by the agents to communicate with the player.

Agents can be anything, e.g. players, converters, visuals etc. You only need to include the agents you want to use, if you write your own player around the library. Just remember the credits. All agents are loaded dynamic, but you still need to have a reference to the ones you want to use. The reason for this, is to make sure that any packages an agent need is copied to your bin folder.

In the Clients folder, you can find two different clients that use the player. *NostalgicPlayerConsole* is a simple player that loads a single module and play it. It will then write some information to the console. *NostalgicPlayer* is a full blown player using Windows Forms and is the one that is released to the public.

# Visual Studio

To load the solution and project files, you need to use at least Visual Studio Community 2022 (it is free) and .NET 7 framework.

# Formats supported

Modules in all supported formats can be found on my homepage at https://nostalgicplayer.dk. Note that the modules on my homepage will first appear, after an official release of the player that can handle them. For a description of the module formats, see the documentation.

| Format | Extension | Converter | Player |
| ------ | -------------- | --------- | ------ |
| AC1D Packer | .ac1d / .ac1 | ProWizard | ModTracker |
| AHX 1.x | .ahx / .thx | | AHX |
| AHX 2.x | .ahx / .thx | | AHX |
| Asylum | .amf | MikMod Converter | MikMod |
| Atari Octalyser | .mod | | ModTracker |
| AudioIFF | .aiff / .aif | | Sample |
| Ben Replay | .ben | ProWizard | ModTracker |
| Binary Packer | .bnr | ProWizard | ModTracker |
| Channel Player 1 | .ch1 / .chn / .chan | ProWizard | ModTracker |
| Channel Player 2 | .ch2 / .chn / .chan | ProWizard | ModTracker |
| Channel Player 3 | .ch3 / .chn / .chan | ProWizard | ModTracker |
| ChipTracker | .chp / .chip / .krs / .kris | ProWizard | ModTracker |
| Composer 669 | .669 | MikMod Converter | MikMod |
| David Whittaker | .dw | | David Whittaker |
| Delta Music 1.0 | .dm1 | | Delta Music 1.0 |
| Delta Music 2.0 | .dm2 | | Delta Music 2.0 |
| Devils Replay | .dev | ProWizard | ModTracker |
| DigiBooster 1.x | .digi | | DigiBooster 1.x |
| DigiBooster 3.x | .dbm | | DigiBooster Pro |
| DigiBooster Pro 2.x | .dbm | | DigiBooster Pro |
| Digital Illusions | .di | ProWizard | ModTracker |
| Digital Sound and Music Interface | .amf | MikMod Converter | MikMod |
| Digital Sound Interface Kit | .dsm | MikMod Converter | MikMod |
| Digital Tracker MOD | .mod | | ModTracker |
| D.O.C. SoundTracker II | .mod | | ModTracker |
| D.O.C. SoundTracker VI | .mod | | ModTracker |
| D.O.C. SoundTracker IX | .mod | | ModTracker |
| Eureka Packer | .eureka / .eu | ProWizard | ModTracker |
| Farandole Composer | .far | MikMod Converter | MikMod |
| Fast/TakeTracker | .mod | | ModTracker |
| FastTracker II | .xm | MikMod Converter | MikMod |
| FC-M Packer | .fc-m / .fcm | ProWizard | ModTracker |
| FLAC | .flac | | Sample |
| Fred Editor | .frd / .fred | | Fred Editor |
| Fred Editor (Final) | .frd / .fred | Module Converter | Fred Editor |
| Fuchs Tracker | .ft | ProWizard | ModTracker |
| Future Composer 1.0 - 1.3 | .fc / .fc13 / .smod | Module Converter | Future Composer |
| Future Composer 1.4 | .fc / .fc14 | | Future Composer |
| Fuzzac Packer | .fuzzac / .fuz | ProWizard | ModTracker |
| Game Music Creator | .gmc | | Game Music Creator |
| General DigiMusic | .gdm | MikMod Converter | MikMod |
| GnoiPacker | .gnoi | ProWizard | ModTracker |
| GnuPlayer | .gnu | ProWizard | ModTracker |
| GPMO | .gpmo | ProWizard | ModTracker |
| HCD Protector | .hcd | ProWizard | ModTracker |
| Heatseeker mc1.0 | .hmc / .crb | ProWizard | ModTracker |
| His Master's Noise (Gnomie by Night) | .mod | | ModTracker |
| Hornet Packer | .hrt | ProWizard | ModTracker |
| IFF-16SV (PCM) | .16sv | | Sample |
| IFF-8SVX (Fibonacci) | .8svx | | Sample |
| IFF-8SVX (PCM) | .8svx | | Sample |
| Imago Orpheus | .imf | MikMod Converter | MikMod |
| Impulse Tracker | .it | MikMod Converter | MikMod |
| JamCracker | .jam | | JamCracker |
| Kefrens Sound Machine | .kms | ProWizard | ModTracker |
| Laxity Tracker | .lax / .unic2 | ProWizard | ModTracker |
| Master SoundTracker 1.0 | .mod | | ModTracker |
| MED 1.12 | .med | | MED |
| MED 2.00 | .med | | MED |
| MED 2.10 MED4 | .med | Module Converter | OctaMED |
| MED 2.10 MMD0 | .med / .mmd0 / .md0 | | OctaMED |
| MED Packer | .med / .mmdc | | OctaMED |
| Mod's Grave | .wow | | ModTracker |
| Module-Patterncompressor | .pmd3 | ProWizard | ModTracker |
| Module Protector | .mp | ProWizard | ModTracker |
| Mosh Player | .mosh | ProWizard | ModTracker |
| MPEG 1.0 | .mp1 / .mp2 / .mp3 / .m2a / .mpg | | Mpg123 |
| MPEG 2.0 | .mp1 / .mp2 / .mp3 / .m2a / .mpg | | Mpg123 |
| MPEG 2.5 | .mp1 / .mp2 / .mp3 / .m2a / .mpg | | Mpg123 |
| MultiTracker | .mtm | | ModTracker |
| Newtron Packer 1.0 | .nw1 | ProWizard | ModTracker |
| Newtron Packer 2.0 | .nw2 | ProWizard | ModTracker |
| NoisePacker 1 | .np1 | ProWizard | ModTracker |
| NoisePacker 2 | .np2 | ProWizard | ModTracker |
| NoisePacker 3 | .np3 | ProWizard | ModTracker |
| NoiseRunner | .nru | ProWizard | ModTracker |
| NoiseTracker | .mod | | ModTracker |
| NoiseTracker Compressed | .ntc | ProWizard | ModTracker |
| NovoTrade Packer | .ntp | ProWizard | ModTracker |
| OctaMED | .med / .mmd0 / .md0 / .omed | | OctaMED |
| OctaMED Professional 3.00 - 4.xx | .med / .mmd1 / .md1 / .omed | | OctaMED |
| OctaMED Professional 5.00 - 6.xx | .med / .mmd2 / .md2 / .omed | | OctaMED |
| OctaMED Soundstudio | .med / .mmd3 / .md3 / .omed / .ocss | | OctaMED |
| Ogg Vorbis | .ogg / oga | | Ogg Vorbis |
| Oktalyzer | .okt / .okta | | Oktalyzer |
| Perfect Song 1 | .pf1 | ProWizard | ModTracker |
| Perfect Song 2 | .pf2 | ProWizard | ModTracker |
| Pha Packer | .pha | ProWizard | ModTracker |
| Polka Packer | .ppk | ProWizard | ModTracker |
| Power Music | .pm | ProWizard | ModTracker |
| Promizer 0.1 | .pm0 / .pm01 | ProWizard | ModTracker |
| Promizer 1.0c | .pm1 / .pm10 | ProWizard | ModTracker |
| Promizer 1.8a | .pmz | ProWizard | ModTracker |
| Promizer 2.0 | .pm2 / .pm20 | ProWizard | ModTracker |
| Promizer 4.0 | .pm4 / .pm40 | ProWizard | ModTracker |
| ProPacker 1.0 | .p10 / .pp10 | ProWizard | ModTracker |
| ProPacker 2.1 | .p21 / .pp21 | ProWizard | ModTracker |
| ProPacker 3.0 | .p30 / .pp30 | ProWizard | ModTracker |
| ProRunner 1 | .pr1 / .pru1 | ProWizard | ModTracker |
| ProRunner 2 | .pr2 / .pru2 | ProWizard | ModTracker |
| ProTracker | .mod | | ModTracker |
| Pygmy Packer | .pyg | ProWizard | ModTracker |
| Quadra Composer | .emod | | Quadra Composer |
| RIFF-WAVE (ADPCM) | .wav | | Sample |
| RIFF-WAVE (IEEE Float) | .wav | | Sample |
| RIFF-WAVE (PCM) | .wav | | Sample |
| Sawteeth | .st | | Sawteeth |
| Scream Tracker 2 | .stm | MikMod Converter | MikMod |
| Scream Tracker 3 | .s3m | MikMod Converter | MikMod |
| Scream Tracker Music Interface Kit | .stx | MikMod Converter | MikMod |
| SidPlay | .sid / .c64 / .mus / .str / .prg | | SidPlay |
| SKYT Packer | .skt / .skyt | ProWizard | ModTracker |
| SoundFX 1.x | .sfx | Module Converter | SoundFX |
| SoundFX 2.0 | .sfx / .sfx2 | | SoundFX |
| SoundMon 1.1 | .bp / .bp2 | | SoundMon |
| SoundMon 2.2 | .bp / .bp3 | | SoundMon |
| SoundTracker 2.2 | .mod | | ModTracker |
| StarTrekker | .mod | | ModTracker |
| StarTrekker 8 voices | .mod | | ModTracker |
| StarTrekker Packer | .stp / .stpk | ProWizard | ModTracker |
| STIM (Slam Tilt) | .sti | ProWizard | ModTracker |
| TFMX 1.5 | .tfx / mdat. / .tfm | | TFMX |
| TFMX Professional | .tfx / mdat. / .tfm | | TFMX |
| TFMX 7 voices | .tfx / mdat. / .tfm | | TFMX |
| The Dark Demon | .tdd | ProWizard | ModTracker |
| The Player 2.2A | .p22a | ProWizard | ModTracker |
| The Player 3.0A | .p30a | ProWizard | ModTracker |
| The Player 4.0A | .p40a | ProWizard | ModTracker |
| The Player 4.0B | .p40b | ProWizard | ModTracker |
| The Player 4.1A | .p41a | ProWizard | ModTracker |
| The Player 5.0A | .p50a | ProWizard | ModTracker |
| The Player 6.0A | .p60a | ProWizard | ModTracker |
| The Player 6.1A | .p61a | ProWizard | ModTracker |
| Titanics Player | .tip | ProWizard | ModTracker |
| TMK Replay | .tmk | ProWizard | ModTracker |
| Tracker Packer 1 | .tp1 | ProWizard | ModTracker |
| Tracker Packer 2 | .tp2 | ProWizard | ModTracker |
| Tracker Packer 3 | .tp3 | ProWizard | ModTracker |
| Ultimate SoundTracker 1.0 - 1.21 | .mod | | ModTracker |
| Ultimate SoundTracker 1.8 - 2.0 | .mod | | ModTracker |
| UltraTracker | .ult | MikMod Converter | MikMod |
| Unic Tracker | .unic | ProWizard | ModTracker |
| UniMod | .uni | MikMod Converter | MikMod |
| Unis 669 | .669 | MikMod Converter | MikMod |
| Unreal Music File | .umx | MikMod Converter | MikMod |
| Wanton Packer | .wnp | ProWizard | ModTracker |
| Xann Packer | .xann | ProWizard | ModTracker |
| Zen Packer | .zen | ProWizard | ModTracker |
