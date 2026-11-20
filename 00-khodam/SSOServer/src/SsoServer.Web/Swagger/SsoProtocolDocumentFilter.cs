using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SsoServer.Web.Swagger;

/// <summary>
/// Adds the machine-facing OAuth2/OpenID Connect endpoints and the basic /health
/// endpoint to the Swagger document.
///
/// These endpoints are implemented by OpenIddict and the MVC layer (form-encoded
/// requests, JSON responses), so the ApiExplorer cannot describe them automatically;
/// the controllers stay hidden ([ApiExplorerSettings(IgnoreApi = true)]) and this
/// filter documents them by hand, matching the real wire contract exactly:
/// connect/token, connect/userinfo, connect/introspect, connect/revoke and /health.
///
/// The browser endpoints (/connect/authorize, /connect/logout, /Account/Login)
/// return HTML forms and redirects - they serve the web version and are
/// deliberately excluded from the API document: an API explorer must not
/// advertise form-returning pages. Interactive login in Swagger UI is provided
/// by the "oauth2" password security scheme (see Program.cs), whose Authorize
/// dialog posts to /connect/token directly.
/// </summary>
public sealed partial class SsoProtocolDocumentFilter : IDocumentFilter
{
    public const string ProtocolTag = "OAuth2 / OpenID Connect";
    public const string HealthTag = "Health";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Paths ??= new OpenApiPaths();
        var paths = swaggerDoc.Paths;

        // Machine endpoints only. The browser endpoints (/connect/authorize,
        // /connect/logout, /Account/Login) serve HTML forms and redirects - they
        // belong to the web version and stay out of the API document.
        AddPath(paths, "/connect/token", ConnectToken());
        AddPath(paths, "/connect/userinfo", ConnectUserinfo());
        AddPath(paths, "/connect/introspect", ConnectIntrospect());
        AddPath(paths, "/connect/revoke", ConnectRevoke());
        AddPath(paths, "/health", BasicHealth());

        AddSharedSchemas(swaggerDoc);
        DescribeTags(swaggerDoc);
    }

    private static void AddPath(OpenApiPaths paths, string path, OpenApiPathItem item)
    {
        if (!paths.ContainsKey(path))
        {
            paths[path] = item;
        }
    }

    private static OpenApiPathItem NewPathItem(params (HttpMethod Method, OpenApiOperation Operation)[] operations)
    {
        var pathItem = new OpenApiPathItem();
        pathItem.Operations ??= new Dictionary<HttpMethod, OpenApiOperation>();
        foreach (var (method, operation) in operations)
        {
            pathItem.Operations[method] = operation;
        }

        return pathItem;
    }

    private static OpenApiOperation ProtocolOperation(
        string operationId,
        string summary,
        string description,
        IList<IOpenApiParameter>? parameters = null,
        OpenApiRequestBody? requestBody = null,
        bool anonymous = true,
        OpenApiResponses? responses = null)
    {
        var operation = new OpenApiOperation
        {
            OperationId = operationId,
            Summary = summary,
            Description = description,
        };

        operation.Tags ??= new HashSet<OpenApiTagReference>();
        operation.Tags.Add(new OpenApiTagReference(ProtocolTag));

        if (parameters is not null)
        {
            operation.Parameters = parameters;
        }

        if (requestBody is not null)
        {
            operation.RequestBody = requestBody;
        }

        if (anonymous)
        {
            // security: [{}] — overrides the document-wide Bearer requirement.
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement());
        }

        if (responses is not null)
        {
            operation.Responses = responses;
        }

        return operation;
    }

    private static void AddTag(OpenApiOperation operation, string tag)
    {
        operation.Tags ??= new HashSet<OpenApiTagReference>();
        operation.Tags.Add(new OpenApiTagReference(tag));
    }

    private static OpenApiOperation WithProtocolTag(OpenApiOperation operation)
    {
        AddTag(operation, ProtocolTag);
        return operation;
    }

    private static OpenApiOperation WithHealthTag(OpenApiOperation operation)
    {
        AddTag(operation, HealthTag);
        return operation;
    }
}
