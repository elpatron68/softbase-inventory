# SoftBase Inventory

[![Build status](https://elpatron68.visualstudio.com/SoftBase/_apis/build/status/SoftBase-.NET%20Desktop-CI%20(Auto))](https://elpatron68.visualstudio.com/SoftBase/_build/latest?definitionId=-1)

SoftBase Inventory is a simple tool to inventory installed programs of one or multiple Windows devices.

![Screenshot](https://elpatron68.visualstudio.com/fdbd236e-0de8-4396-a9eb-0c2992bb587f/_apis/git/repositories/76be9487-fa9c-4089-aa5f-2dfa7d3a5c74/Items?path=%2FSoftBase%2Fimg%2FScreenshot_SoftBase+Inventory.png&versionDescriptor%5BversionOptions%5D=0&versionDescriptor%5BversionType%5D=0&versionDescriptor%5Bversion%5D=master&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1)

## Features

- Read list of installed programs from WMI (Windows Management Interface)
- Automatically save results to a Sqlite database
- Export reports as PDF or Excel file
- Manage inventories of multiple devices with different timestamps
- No limitaions, no nag screens, Free Open Source Software (FOSS)
- Clean and simple user interface
- No analytics, no tracking, maximum privacy
- Modern windows application based on state-of-the-art techiques

## Requirements

- Microsoft Windows 7 and newer (32 and 64 bit)
- Microsoft .NET Framework 4.7.2 [Web-Installer](http://go.microsoft.com/fwlink/?linkid=863262)

## Download

### Chocolatey Install

A [Chocolatey](https://chocolatey.org/) package for installation and easy updating will be added in the near future.

### Manual Download

Download the latest version from [here](https://butenostfreesen.de/softbase/setup.exe)

## Usage

Start the program, select a (new) database file to store your inventories in the first dialog and start inventoring your devices. 

Click `Export` Buttons to export the results to a PDF- or Excel file.

Select a device and a corresponding snapshot to view (or export) saved inventories from the database.

That´s all (for now).

## License

Before you ask...
Yes, you can use this for any commercial project.

SoftBase is Free Open Source Software licensed under the [MIT license](https://opensource.org/licenses/MIT).

Although you can do quite everything you like with SoftBase, I ask you not to use this software in a military environment.

All third party licenses are linked in the menu `Help` - `Licenses` - `3rd party licenses`.

## Help Wanted

If you have an idea how to translate WPF applications in a cofortable way to other languages: 
Let me know (or fork this project and create a pull request).

## Author

[Markus Busche](mailto:elpatron@mailbox.org?subject=SoftBase)  
Kiel, Germany  

## Donation

Although this is just a small program, many hours of development have been spent to make it a useful and stable tool.

If you like it and want to support the further development, feel free to donate a small amout of money to the author.
Just click on the `Donate` button in the main window or see `File`- `Donate` for a Paypal QR code.
