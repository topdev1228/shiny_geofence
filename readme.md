## Shiny Samples

Please note that this is always built against the dev version of Shiny libraries, thus some features may not be available yet on nuget.

Use Samples.sln if you want to build the project.  The -Debug.sln is for testing with the raw source code which requires special setup.

## Builds

OS|Status
--|------
Android|[![Build status](https://dev.azure.com/shinylib/Shiny/_apis/build/status/Android%20Sample)](https://dev.azure.com/shinylib/Shiny/_build/latest?definitionId=17)
iOS|[![Build status](https://dev.azure.com/shinylib/Shiny/_apis/build/status/iOS%20Sample)](https://dev.azure.com/shinylib/Shiny/_build/latest?definitionId=16)
Tizen|[![Build status](https://dev.azure.com/shinylib/Shiny/_apis/build/status/Tizen%20Mobile)](https://dev.azure.com/shinylib/Shiny/_build/latest?definitionId=12)
UWP|[![Build status](https://dev.azure.com/shinylib/Shiny/_apis/build/status/Sample%20UWP)](https://dev.azure.com/shinylib/Shiny/_build/latest?definitionId=9)

## AppCenter Test Releases

OS|Link
--|----
Android|[Link](https://install.appcenter.ms/orgs/shinyorg/apps/shiny/distribution_groups/all)
iOS|[Link](https://install.appcenter.ms/orgs/shinyorg/apps/shiny-1/distribution_groups/all)


## Compiling on iOS
NFC & Push Notifications are enabled in the info.plist which means you need a custom provisioning profile (or you have to disable these before deploying to your device)