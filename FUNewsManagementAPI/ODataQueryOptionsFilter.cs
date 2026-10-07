using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FUNewsManagementAPI;

public class ODataQueryOptionsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasEnableQuery = context.MethodInfo.GetCustomAttributes(typeof(EnableQueryAttribute), true).Any()
            || (context.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(EnableQueryAttribute), true).Any() ?? false);

        if (!hasEnableQuery)
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        var odataParams = new (string Name, string Description, string Type)[]
        {
            ("$filter", "Filters the results using OData expressions (e.g. NewsStatus eq true and CategoryId eq 1)", "string"),
            ("$select", "Selects specific properties to return (e.g. NewsArticleId,Headline)", "string"),
            ("$orderby", "Sorts the results (e.g. CreatedDate desc)", "string"),
            ("$top", "Specifies the maximum number of items to return", "integer"),
            ("$skip", "Specifies the number of items to skip for pagination", "integer"),
            ("$expand", "Expands related entities (e.g. Category,Tags)", "string"),
            ("$count", "Returns the total number of items when set to true", "boolean")
        };

        foreach (var (name, description, type) in odataParams)
        {
            if (!operation.Parameters.Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Query,
                    Description = description,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = type
                    }
                });
            }
        }
    }
}
