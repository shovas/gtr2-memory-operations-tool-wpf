SHO GTR2 Memory Operations Tool

README

Overview

- Documentation
- Requirements
- How It Works
- Credits

Documentation

See Docs for Release Notes, Development Notes, Plans, License, and other information.

Requirements

- Steam GTR2.EXE SHA256: 0d355d96975d1748bdab216c340a7277d69f8faa92f677fbafea0a832f26550f
  - If you 4GB Patch'd it or did something else to the Steam GTR2.EXE it might still work
  - My GTR2.EXE hashes to ae0d573cd4a2567c76e3dc4dafaf79128febfbb817303607bae027b695023755 and still works with CCGEP
- .NET 10.0 Desktop Runtime
  - https://dotnet.microsoft.com/en-us/download/dotnet/10.0
  - Search Microsoft.com if the link breaks

How It Works

- The GTR Memory Operations tool is designed to read and write the memory of the GTR2 process, which allows it to modify game data in real-time and affect the game's behaviour.

- Windows lets processes of the same user, running with the same privileges, and not explicitly configured otherwise, to read/write each other's memory by default, so no special permissions are needed.

- The tool uses the Windows API functions OpenProcess, ReadProcessMemory, and WriteProcessMemory to read/write the memory of the GTR2 process.

- GTR2 already shares a region of memory with many useful pieces of data that can be read, things such as drivers, tire wear/temperature, fuel level, lap times, and more. But you can't write to this memory and even if you could the game wouldn't change it's behaviour based on it. So, we have to find the real data in other regions of memory and adjust those.

- This tool can read those non-shared memory regions which include all the rest of the data. The trick is to find the correct memory addresses for the data you want to read/write, which can be done using a memory scanner/debugger like Cheat Engine.

- Once the correct memory addresses are found, they can be read and new values written into them, effectively altering the behaviour of GTR2.

- For example, eventually I would like to monitor the Player and AI driver laptimes in real-time so that I can dynamically adjust the AI difficulty to the Player's laptimes, thereby eliminating the need to adjust the AI Difficulty level. You just put in laps and the AI will adjust to your level. The idea would be to find the right AI difficulty level during practice and qualifying and then leave it at that for the race.

  - There's more to it then that. Find out more here: https://www.simwiki.net/wiki/Automatic_AI_-_Performance-based_Dynamic_AI_Scaling

  Credits

  - T-Shirt for the original memory operations python scripts that provided much of the foundation for reading and writing memory and also mapping out many useful memory locations
  - The Iron Wolf for CCGEP and especially the open source CCGEP Monitor that had valuable shared memory reading code
  - The Simwiki Discord where many of us GTR2 fans congregate and share information about modding
  - The GTR2 forum communities such as Trackaholics, EEC, Evolution Modding, GT-IMT, Steam Forums, and Reddit r/GTR2 where we can post questions for the community