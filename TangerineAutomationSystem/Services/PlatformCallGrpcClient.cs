using System.Threading.Tasks;

namespace TangerineAutomationSystem.Services
{
    public class PlatformCallGrpcClient
    {
        private readonly string _endpoint;
        public PlatformCallGrpcClient(string endpoint = "https://localhost:5001")
        {
            _endpoint = endpoint;
        }

        public async Task<bool> CallExecutePlatformTaskAsync(string platformId, string taskId, string payloadJson)
        {
            // TODO: Replace with real gRPC client call. This is a stub that simulates success.
            await Task.CompletedTask;
            return true;
        }
    }
}