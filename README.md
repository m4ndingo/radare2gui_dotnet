# radare2gui_dotnet
Another radare2 gui for windows
## Screenshots
**Debugger View**

![r2pipe_dotnet_main](https://cloud.githubusercontent.com/assets/12532269/20451500/55a62ee0-adfa-11e6-937e-59a497026ecc.png)

**Hexadecimal View**

![r2pipe_dotnet_hexview](https://cloud.githubusercontent.com/assets/12532269/20447475/51ef978e-addf-11e6-87dc-ae4fd4fc4b8f.png)

**Sections**

![r2pipe_dotnet_sections](https://cloud.githubusercontent.com/assets/12532269/20448572/f699b27e-ade4-11e6-9aa3-ae690cd98905.png)

**Help**

![r2pipe_dotnet_help](https://cloud.githubusercontent.com/assets/12532269/20447502/87ab5a2a-addf-11e6-9cc4-7c34673e5f49.png)
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

![r2pipe_dotnet_sf2](https://cloud.githubusercontent.com/assets/12532269/20446745/854239ba-addb-11e6-81c4-7dd25c48e37f.png)

@m_ndingo
