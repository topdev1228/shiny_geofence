using System;


namespace Samples
{
    static class Constants
    {
#if DEBUG
        public const string AppCenterTokens = "";
        public const string AnhListenerConnectionString = "Endpoint=sb://shinysamples.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=jI6ss5WOD//xPNuHFJmS7sWWzqndYQyz7wAVOMTZoLE=";
        public const string AnhHubName = "shinysamples";
#else
        public const string AppCenterTokens = "#{AppCenterTokens}#";
        public const string AnhListenerConnectionString = "#{AnhListenerConnectionString}";
        public const string AnhHubName = "#{AnhHubName}";
#endif
    }
}
