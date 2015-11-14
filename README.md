# Microsoft Azure Mobile Apps: .NET Client SDK

With Microsoft Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes. To learn more, visit our [Developer Center](http://azure.microsoft.com/en-us/develop/mobile).

## Getting Started

If you are new to Mobile Services, you can get started by following our tutorials for connecting your Mobile
Services cloud backend to [Windows Store apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-windows-store-get-started/),
and [Windows Phone 8 apps](http://azure.microsoft.com/en-us/documentation/articles/mobile-services-windows-phone-get-started/).

## Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-apps-net-client
    cd ./azure-mobile-apps-net-client/

## Reference Documentation

## Change log
- [Managed SDK](CHANGELOG.md)

## Managed Windows Client SDK

Our managed portable library for Windows 8, Windows Phone 8, Windows Phone 8.1, and Windows Runtime Universal C# Client SDK makes it incredibly easy to use Mobile Services from your Windows applications. The [Microsoft Azure Mobile Services SDK](http://nuget.org/packages/WindowsAzure.MobileServices/) is available
as a Nuget package or you can download the source using the instructions above. The managed portable library also supports the full .NET 4.5 platform.

### Prerequisites

The SDK requires Visual Studio 2013.

###Building and Referencing the SDK

The managed portable library solution includes a core portable assembly and platform-specific assemblies for each of the supported platforms: Windows 8,
Windows Phone 8 and .NET 4.5. The core portable platform project is ```Microsoft.WindowsAzure.Mobile```. The platform-specific assembly projects are
named using a ```Microsoft.WindowsAzure.Mobile.Ext.<Platform>``` convention. The Windows Phone 8 platform also
include a ```Microsoft.WindowsAzure.Mobile.UI.<Platform>``` project that contain UI components. To build the Managed Portable Libray:

1. Open the ```sdk\Managed\Microsoft.WindowsAzure.Mobile.Managed.sln``` solution file in Visual Studio 2012.
2. Press F6 to build the solution.

### Running the Tests

The managed portable library ```Microsoft.WindowsAzure.Mobile.Managed.sln``` has a test application for each of the supported platforms: Windows 8,
Windows Phone 8 and .NET 4.5.

1. Open the ```sdk\Managed\Microsoft.WindowsAzure.Mobile.Managed.sln``` solution file in Visual Studio 2012.
2. Right-click on the test project for a given platform in the Solution Explorer and select ```Set as StartUp Project```.
3. Press F5 to run the application in debug mode.
4. An application will appear with a prompt for a runtime Uri and Tags. You can safely ignore this prompt and just click the Start button.
5. The test suite will run and display the results.

## Useful Resources

* [Quickstarts](https://github.com/Azure/azure-mobile-services-quickstarts)
* [E2E Test Suite](https://github.com/Azure/azure-mobile-services-test)
* [Samples](https://github.com/Azure/mobile-services-samples)
* Tutorials and product overview are available at [Microsoft Azure Mobile Services Developer Center](http://azure.microsoft.com/en-us/develop/mobile).
* Our product team actively monitors the [Mobile Services Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) to assist you with any troubles.

## Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-services/issues) section of the project.
