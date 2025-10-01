namespace TangerineBlazorApp.Models
{
    /// <summary>
    /// Backend API configuration settings
    /// </summary>
    public class BackendApiSettings
    {
        /// <summary>
        /// gRPC endpoint URL for the backend service
        /// </summary>
        public string GrpcEndpoint { get; set; } = "https://localhost:7197";
    }
}
