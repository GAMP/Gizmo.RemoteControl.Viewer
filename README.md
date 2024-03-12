# REMOTE CONTROL VIEWER

## This is a Blazor library for the client part of the remote control system.

### This client can be used either with the remote control system's server or as a standalone Blazor web application.

`The client sends commands to the server and displays the result as a screencast.`

## How to use

### 1. Add libraries to your project as a git submodule

#### &emsp;1.1. Open a command line in your repository root

## FOR STANDALONE BLAZOR APPLICATIONS

#### &emsp;1.2. Run the following commands

```bash
git submodule add https://github.com/GAMP/Gizmo.RemoteControl.Shared.git Submodules/Gizmo.RemoteControl.Shared
git submodule add https://github.com/GAMP/Gizmo.RemoteControl.Viewer.git Submodules/Gizmo.RemoteControl.Viewer
```

#### &emsp;1.3. Add the libraries to your project

### 2. In the startup of your Blazor application

#### &emsp;2.1. Add the following using statement

```csharp
using Gizmo.RemoteControl.Viewer;
```

#### &emsp;2.2. Add the following code to where you configure your services

```csharp
services.AddRemoteControlViewer();
```

### 3. In the \_imports.razor file

#### &emsp;3.1. Add the following line

```csharp
@using Gizmo.RemoteControl.Viewer
```

### 4. In the Router component

#### &emsp;4.1. Add the following attribute to the Router component

```csharp
<Router AppAssembly="your assembly" AdditionalAssemblies="new[] { typeof(Gizmo.RemoteControl.Viewer.App_Wasm).Assembly }">
```

### 5. In the Blazor .csproj file

#### &emsp;5.1. Add the following line

```xml
  <Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <PropertyGroup>
      <!-- this path is dependent on the host project -->
      <ViewerSourcePath>../Submodules/Gizmo.RemoteControl.Viewer</ViewerSourcePath>
      <!-- this path is dependent on viewer package.json path. Here is should be the wwwroot folder of the your web app -->
      <ViewerOutputPath>../../../Gizmo.Web.Manager.UI/wwwroot</ViewerOutputPath>
    </PropertyGroup>

    <Exec Command="npm install" WorkingDirectory="$(ViewerSourcePath)" />
    <Exec Command="set VIEWER_OUTPUT_PATH=$(ViewerOutputPath)&amp;&amp;npm run build_dev" WorkingDirectory="$(ViewerSourcePath)" Condition="'$(Configuration)' == 'Debug'" />
    <Exec Command="set VIEWER_OUTPUT_PATH=$(ViewerOutputPath)&amp;&amp;npm run build_prod" WorkingDirectory="$(ViewerSourcePath)" Condition="'$(Configuration)' == 'Release'" />
  </Target>
```

### 6. In the index.html file

#### &emsp;6.1. Add the following script tag

```html
<script src="your wwwroot path/remotecontrol_viewer/remotecontrol_code.js"></script>
<script src="your wwwroot path/remotecontrol_viewer/remotecontrol_style.js"></script>
```

## FOR REMOTE CONTROL SYSTEM'S SERVER (ONLY .NET 8 AND LATER)

#### &emsp;1.2. Run the following commands

```bash
git submodule add https://github.com/GAMP/Gizmo.RemoteControl.Viewer.git Submodules/Gizmo.RemoteControl.Viewer
```

#### &emsp;1.3. Add the library to your project

### 2. In the startup of your ASP NET application

#### &emsp;2.1. Add the following using statement

```csharp
using Gizmo.RemoteControl.Viewer;
```

#### &emsp;2.2. Add the following code to where you configure your services

```csharp
services
    .AddRemoteControlViewer()
    .AddRazorComponents()
    .AddInteractiveServerComponents();
```

#### &emsp;2.3. Add the following code to where you configure your application.

#### Note:

- _This should be added after the `app.UseRouting();` line._
- _This should be added before the `app.UseRemoteControlServer()` line._

```csharp
app
    .UseAntiforgery()
    .UseEndpoints(viewer =>
    {
        viewer
            .MapRazorComponents<RemoteControl.Viewer.App_Server>()
            .AddInteractiveServerRenderMode();
    });
```

### 3. In the Server .csproj file

#### &emsp;3.1. Add the following line

```xml
  <Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <PropertyGroup>
      <!-- this path is dependent on the host project -->
      <ViewerSourcePath>../Submodules/Gizmo.RemoteControl.Viewer</ViewerSourcePath>
      <!-- this path is dependent on viewer package.json path. Here is should be the wwwroot folder of the your server's wwwroot path -->
      <ViewerOutputPath>../../../Gizmo.Server/wwwroot</ViewerOutputPath>
    </PropertyGroup>

    <Exec Command="npm install" WorkingDirectory="$(ViewerSourcePath)" />
    <Exec Command="set VIEWER_OUTPUT_PATH=$(ViewerOutputPath)&amp;&amp;npm run build_dev" WorkingDirectory="$(ViewerSourcePath)" Condition="'$(Configuration)' == 'Debug'" />
    <Exec Command="set VIEWER_OUTPUT_PATH=$(ViewerOutputPath)&amp;&amp;npm run build_prod" WorkingDirectory="$(ViewerSourcePath)" Condition="'$(Configuration)' == 'Release'" />
  </Target>
```
