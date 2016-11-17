# radare2gui_dotnet
Another radare2 gui for windows
![r2pipeguidotnet](https://cloud.githubusercontent.com/assets/12532269/20358089/cb65b5e6-ac2a-11e6-9b8b-4a073e7d5106.png)
# Hexview
![r2pipeguidotnet_hexview](https://cloud.githubusercontent.com/assets/12532269/20358143/f7cef1f6-ac2a-11e6-8430-95e4da91f062.png)
# Compilation
## Packages required

Nuget console:
```
PM> Install-Package Newtonsoft.Json
PM> Install-Package r2pipe
```
## Framework 4.5.1
Since Nuget installs r2pipe compiled with the .net framework 4.5.1, you have to configure the project for this version; also you need the framework (downloadable from Microsoft)

## Compile r2pipe (from sources) for other .net frameworks
You can download r2pipe from https://github.com/radare/radare2-r2pipe/ and compile against other framework versions. Open the solution (.sln) and compile the dll, then add to this project if needed :) 

## radare2 binaries for Windows
By the moment, follow these instructions: https://radare.gitbooks.io/radare2book/content/introduction/windows_compilation.html

<a href="http://twitter.com/m_ndingo" target="_blank">@m_ndingo</a>
