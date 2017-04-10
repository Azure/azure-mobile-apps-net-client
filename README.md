# Azure Mobile Apps: .NET Client SDK

With Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes. 

## Getting Started

If you are new to Azure Mobile Apps, you can get started by following our tutorials for connecting to your hosted cloud backend with a [Xamarin.Forms client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-xamarin-forms-get-started/) or [Windows client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-windows-store-dotnet-get-started/).

## Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-apps-net-client.git
    cd ./azure-mobile-apps-net-client/
    git submodule init
    git submodule update

## Supported platforms

* .NET Standard 1.1
* .NET Standard 1.4
* Xamarin Android releases for API 19 through 24 (KitKat through Nougat)
* Xamarin iOS releases for iOS versions 8.0 through 10.0
* Universal Windows Platform

Other versions may work.  We do not test them and thus cannot support them.

## Change log
- [Managed SDK](CHANGELOG.md)

## Managed Windows Client SDK

Our managed portable library for Window and Xamarin makes it easy to use Azure Mobile Apps from your managed client applications. The [Azure Mobile Client SDK](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client/) is available as a NuGet package or you can download the source using the instructions above. The managed portable library also supports .NET Standard 1.1 and .NET Standard 1.4.

To learn more about the client library, see [How to use the managed client for Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-dotnet-how-to-use-client-library/).

### Prerequisites

The SDK requires Visual Studio 2017.

### Building and Referencing the SDK

1. Open the ```Microsoft.Azure.Mobile.Client.sln``` solution file in Visual Studio 2017.
2. Ensure you have connected an iOS Build Agent prior to building
2. Use Solution -> Restore NuGet Packages...
3. Press F6 to build the solution.

### Running the Unit Tests

The following test suites under the 'unittest' directory contain the unit tests:

* Microsoft.WindowsAzure.Mobile._platform_.Test
* Microsoft.WindowsAzure.Mobile.SQLiteStore._platform_.Test

Mark the appropriate project as the Startup project and run it.  The UI will open and then you can run the tests.  If in doubt,
run the Win8 platform locally as a minimal unit test.

### Running the E2E Tests

You must have a working test endpoint to run the tests.  The test endpoint is not included and may take some time to set up.  If
you need a working test endpoint, please reach out to us on Twitter or via the GitHub Issues.  The Azure Mobile Apps team will run
the E2E tests prior to publication.

If you are an Azure Mobile Apps team member, you can set the appropriate e2etest project as active, build, and use a configured
endpoint.

## Useful Resources

* [Quickstarts](https://github.com/Azure/azure-mobile-apps-quickstarts)
* [E2E Test Suite](e2etest)
* [Samples](https://azure.microsoft.com/en-us/documentation/samples/?service=app-service&term=mobile)
* Tutorials and product overview are available at [Azure Mobile Apps Developer Center](http://azure.microsoft.com/en-us/develop/mobile).
* Our product team actively monitors the [Mobile Apps Developer Forum](http://social.msdn.microsoft.com/Forums/en-US/azuremobile/) to assist you with any troubles and the StackOverflow tag [azure-mobile-services](http://stackoverflow.com/questions/tagged/azure-mobile-services).
* Our product team publishes symbols to SymbolSource for an improved debugging experience. Instructions on enabling VisualStudio to load symbols from SymbolSource [here](http://www.symbolsource.org/Public/Wiki/Using)

## Contribute Code or Provide Feedback

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/Azure/azure-mobile-apps-net-client/issues) section of the project.
