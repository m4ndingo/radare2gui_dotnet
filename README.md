# radare2gui_dotnet
Another radare2 gui for windows
## Screenshots
**Debugger View**
![r2pipe_dotnet_classic](https://cloud.githubusercontent.com/assets/12532269/20447430/0a9937e6-addf-11e6-966f-f62c5616f3f6.png)

**Hexadecimal View**
![r2pipe_dotnet_hexview](https://cloud.githubusercontent.com/assets/12532269/20447475/51ef978e-addf-11e6-87dc-ae4fd4fc4b8f.png)

**Sections**
![r2pipe_dotnet_sections](https://cloud.githubusercontent.com/assets/12532269/20448471/717bdafe-ade4-11e6-8097-8c7006e7f5c1.png)

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
