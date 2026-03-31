using System.Net;

using Grpc.Net.Client;

using Project;

Console.WriteLine("Testing gRPC connection to Project Service...");
var handler = new HttpClientHandler();
// handler.ServerCertificateCustomValidationCallback =
//     HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

var httpClient = new HttpClient(handler)
{
    DefaultRequestVersion = HttpVersion.Version11,
    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
};

GrpcChannel channel = GrpcChannel.ForAddress(
    "http://localhost:5006",
    new GrpcChannelOptions
    {
        HttpClient = httpClient
    });

var client = new ProjectService.ProjectServiceClient(channel);

try
{
    var request = new ProjectExistsRequest
    {
        ProjectId = "56082c59-2c01-4905-80dd-34056a742cba"
    };

    ProjectExistsResponse? response = await client.ProjectExistsAsync(request);
    Console.WriteLine($"Project exists: {response.Exists}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
}
