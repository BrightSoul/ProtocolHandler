# Protocol handler
This is a .NET 6 application for Windows which will install a [custom protocol handler](https://medium.com/swlh/custom-protocol-handling-how-to-8ac41ff651eb) in the Windows Registry. Whenever the user visits a link using that custom protocol, the application will handle it by running a configured executable.

## Getting started
Edit the `appsettings.json` file to customize the protocol name and its handlers.
```json
{
    "Protocol": "appexec",
    "Handlers":
    {
        "chrome": {
            "Executable": "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
            "Arguments": "--start-fullscreen {website}"
        },
        "firefox": {
            "Executable": "C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe",
            "Arguments": "-kiosk {website}"
        }
    }
}
```

This configuration defined `appexec` as then custom protocol. The application will then handle links like these.

 - [appexec://chrome?website=www.youtube.com](appexec://chrome?website=www.youtube.com)
 - [appexec://firefox?website=www.youtube.com](appexec://firefox?website=www.youtube.com)

The hostname part of the URL (e.g. `firefox` and `chrome` in these cases) will be searched in the `Handlers` section of the `appsettings.json` file. If found, then the `Executable` will be run with its `Parameters`. Named placeholders such as `{website}` will be replaced with the value found in the querystring key by the same name (eg. `www.youtube.com` in these cases).

## 
Publish the application by running the following command in the project directory:
```
dotnet publish
```
You'll find the output in the `bin\Debug\net6.0-windows\win-x64\publish` directory. Feel free to move it to a more permanent location on your PC (e.g in The `C:\Program Files\ProtocolHandler` directory). It is necessary to run the `ProtocolHandler.exe` at least one time, so it will register the custom protocol. An error will be reported but just because the application is not meant to be run this way. Instead, it will be run by Windows whenever a link using the custom handler is visited.