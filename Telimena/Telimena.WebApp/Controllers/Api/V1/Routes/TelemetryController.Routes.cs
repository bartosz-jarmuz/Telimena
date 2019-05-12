namespace Telimena.WebApp.Controllers.Api.V1
{
    public partial class TelemetryController
    {
#pragma warning disable 1591
        public static class Routes
        {
            public const string Initialize = nameof(TelemetryController) + "." + nameof(TelemetryController.Initialize);
            public const string Post = nameof(TelemetryController) + "." + nameof(TelemetryController.Post);
            public const string PostWithVariousKeys = nameof(TelemetryController) + "." + nameof(TelemetryController.Post) + "V2";
            public const string PostBasic = nameof(TelemetryController) + "." + nameof(TelemetryController.PostBasic);
            public const string ExecuteQuery = nameof(TelemetryController) + "." + nameof(TelemetryController.ExecuteQuery);
        }

    }
}