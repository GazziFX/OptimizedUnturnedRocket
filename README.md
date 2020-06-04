# Rocket.Unturned
 Fork of https://github.com/RocketMod/Rocket.Unturned

# What is changed?

* Most Linq expressions changed to more performant code
* Most ignore-case string comparsions changed from ToLower() to Equals(string, StringComparison.OrdinalIgnoreCase)
* Equals and GetHashCode overrides for UnturnedPlayer and ConsolePlayer
* Some classes not inherited from MonoBehaviour anymore
* Some obsolete Unturned events replaced with new
* In-built options to Hide Plugins. Option to display the server in the browser if MaxPlayer is greater than 24 (Visual Clamp MaxPlayers to 24)
* Better GodMode
* Commands /exp /jump /tp wp
* Some changes from [Nelson's commits](https://github.com/SmartlyDressedGames/RocketMod/commits)

# License

MIT License

Copyright (c) 2018 Sven Mawby

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
