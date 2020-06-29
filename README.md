# Azure Mobile Apps: .NET Client SDK

With Azure Mobile Apps you can add a scalable backend to your connected client applications in minutes.

## Getting Started

The Azure Mobile Apps .NET Client code is part of Azure Mobile Apps - an offline capable data service.  To use, add the [Microsoft.Azure.Mobile.Client](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client/) package, and optionally, the [Microsoft.Azure.Mobile.Client.SQLiteStore](https://www.nuget.org/packages/Microsoft.Azure.Mobile.Client.SQLiteStore) packages to your project.

If you are new to Azure Mobile Apps, you can get started by following our tutorials for connecting to your hosted cloud backend with a [Xamarin.Forms client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-xamarin-forms-get-started/) or [Windows client](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-windows-store-dotnet-get-started/).  To learn more about the client library, see [How to use the managed client for Azure Mobile Apps](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-dotnet-how-to-use-client-library/).

## Supported platforms

* .NET Standard 2.0

With the release of v5.0.0 of the libraries, we only support .NET Standard 2.0.  This includes the latest versions of Xamarin Android, Xamarin iOS, Xamarin Forms, and UWP.  

## Building the Library

### Prerequisites

The SDK requires Visual Studio 2019.

### Download Source Code

To get the source code of our SDKs and samples via **git** just type:

    git clone https://github.com/Azure/azure-mobile-apps-net-client.git
    cd ./azure-mobile-apps-net-client


### Building and Referencing the SDK

1. Open the ```Microsoft.Azure.Mobile.Client.sln``` solution file in Visual Studio 2019.
2. Use Solution -> Restore NuGet Packages...
3. Press F6 to build the solution.

### Running the Unit Tests

The following test suites under the 'test' directory contain the unit tests:

* Microsoft.WindowsAzure.Mobile._platform_.Test
* Microsoft.WindowsAzure.Mobile.SQLiteStore._platform_.Test

You can run the unit tests using the MSTest test runner.  Ensure you run the unit tests prior to submitting a PR.

### Running the E2E Tests

> **TODO** Describe how to set up the E2E endpoint and run the live test suite.

## Future of Azure Mobile Apps

Microsoft is committed to fully supporting Azure Mobile Apps, including **support for the latest OS release, bug fixes, documentation improvements, and community PR reviews**. Please note that the product team is **not currently investing in any new feature work** for Azure Mobile Apps. We highly appreciate community contributions to all areas of Azure Mobile Apps.

## Useful Resources

* [Quickstarts](https://github.com/Azure/azure-mobile-apps-quickstarts)
* [Samples](https://azure.microsoft.com/en-us/documentation/samples/?service=app-service&term=mobile)
* [Azure Mobile Developer Center](http://azure.microsoft.com/en-us/develop/mobile).
* StackOverflow: tag [azure-mobile-services](http://stackoverflow.com/questions/tagged/azure-mobile-services).
* [Instructions on enabling VisualStudio to load symbols from SymbolSource](http://www.symbolsource.org/Public/Wiki/Using)

## Contribute Code or Provide Feedback

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

If you would like to become an active contributor to this project please follow the instructions provided in [Microsoft Azure Projects Contribution Guidelines](http://azure.github.com/guidelines.html).

If you encounter any bugs with the library, please file an issue in the [Issues](https://github.com/Azure/azure-mobile-apps-net-client/issues) section of the project.
