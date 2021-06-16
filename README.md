# NostalgicPlayer
Modules are relatively small files which contains various sound samples and information on how they should be played. This way of making music goes a long way back, and even at the time of the Commodore 64 the concept was used. But it was on the Amiga computer it really started off. The original type of modules were made with a program called SoundTracker, but since then a lot of different module types have shown up, all of them offering different features - such as more sound channels, more samples, realtime echo and so on.

NostalgicPlayer is a program that can play these modules. NostalgicPlayer is a Windows version of the APlayer, which was started on the Amiga computer back in 1993, later continued on BeOS, and was a reaction to the lack of good module players for the Amiga.

Please enjoy this program, and support the development by giving any feedback (bug reports and flattering comments :-))

You can find an installation package on Microsoft Store, just search after NostalgicPlayer.

# License

NostalgicPlayer is licensed under the <a href="https://github.com/neumatho/NostalgicPlayer/blob/main/LICENSE">Apache-2.0 License</a>. The MikMod player and converter are licensed under <a href="http://www.gnu.org/licenses/lgpl-3.0.html">GNU Lesser General Public License</a>.

# Structure

The player is structured, so it is easy to create your own user interface around the logic. All the main logic is in the *NostalgicPlayerLibrary* and *NostalgicPlayerKit*. The later is used by the agents to communicate with the player.

Agents can be anything, e.g. players, converters, visuals etc. You only need to include the agents you want to use, if you write your own player around the library. Just remember the credits. All agents are loaded dynamic, but you still need to have a reference to the ones you want to use. The reason for this, is to make sure that any packages an agent need is copied to your bin folder.

In the Clients folder, you can find two different clients that use the player. *NostalgicPlayerConsole* is a simple player that loads a single module and play it. It will then write some information to the console. *NostalgicPlayer* is a full blown player using Windows Forms and is the one that is released to the public.

# Formats supported

Modules in all supported formats can be found on my homepage at https://www.nostalgicplayer.dk. Note that the modules on my homepage will first appear, after an official release of the player that can handle them. For a description of the module formats, see the documentation.

| Format | Extension | Converter | Player |
| ------ | -------------- | --------- | ------ |
| Asylum | .amf | MikMod Converter | MikMod |
| Atari Octalyser | .mod | | ModTracker |
| AudioIFF | .aiff / .aif | | Sample |
| Composer 669 | .669 | MikMod Converter | MikMod |
| Digital Sound and Music Interface | .amf | MikMod Converter | MikMod |
| Digital Sound Interface Kit | .dsm | MikMod Converter | MikMod |
| D.O.C. SoundTracker II | .mod | | ModTracker |
| D.O.C. SoundTracker VI | .mod | | ModTracker |
| D.O.C. SoundTracker IX | .mod | | ModTracker |
| Farandole Composer | .far | MikMod Converter | MikMod |
| Fast/TakeTracker | .mod | | ModTracker |
| FastTracker II | .xm | MikMod Converter | MikMod |
| Future Composer 1.0 - 1.3 | .fc / .fc13 / .smod | Module Converter | Future Composer |
| Future Composer 1.4 | .fc / .fc14 | | Future Composer |
| General DigiMusic | .gdm | MikMod Converter | MikMod |
| IFF-16SV (PCM) | .16sv | | Sample |
| IFF-8SVX (Fibonacci) | .8svx | | Sample |
| IFF-8SVX (PCM) | .8svx | | Sample |
| Imago Orpheus | .imf | MikMod Converter | MikMod |
| Impulse Tracker | .it | MikMod Converter | MikMod |
| JamCracker | .jam | | JamCracker |
| Master SoundTracker 1.0 | .mod | | ModTracker |
| MultiTracker | .mtm | | ModTracker |
| NoiseTracker | .mod | | ModTracker |
| ProTracker | .mod | | ModTracker |
| RIFF-WAVE (ADPCM) | .wav | | Sample |
| RIFF-WAVE (IEEE Float) | .wav | | Sample |
| RIFF-WAVE (PCM) | .wav | | Sample |
| Scream Tracker 2 | .stm | MikMod Converter | MikMod |
| Scream Tracker 3 | .s3m | MikMod Converter | MikMod |
| Scream Tracker Music Interface Kit | .stx | MikMod Converter | MikMod |
| SoundTracker 2.2 | .mod | | ModTracker |
| StarTrekker | .mod | | ModTracker |
| StarTrekker 8 voices | .mod | | ModTracker |
| Ultimate SoundTracker 1.0 - 1.21 | .mod | | ModTracker |
| Ultimate SoundTracker 1.8 - 2.0 | .mod | | ModTracker |
| UltraTracker | .ult | MikMod Converter | MikMod |
| UniMod | .uni | MikMod Converter | MikMod |
| Unis 669 | .669 | MikMod Converter | MikMod |
| Unreal Music File | .umx | MikMod Converter | MikMod |
