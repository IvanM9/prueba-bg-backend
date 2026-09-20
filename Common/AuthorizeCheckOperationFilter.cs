using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace bg_backend.Common;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var controllerMetadata = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                                     .OfType<IAuthorizeData>()
                                 ?? [];
        var actionMetadata = context.MethodInfo.GetCustomAttributes(true)
            .OfType<IAuthorizeData>();

        var requiresAuthorization = endpointMetadata.OfType<IAuthorizeData>().Any()
                                    || controllerMetadata.Any()
                                    || actionMetadata.Any();

        var allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any()
                              || context.MethodInfo.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any()
                              || context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<IAllowAnonymous>().Any() == true;

        if (!requiresAuthorization || allowsAnonymous)
            return;

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
    }
}