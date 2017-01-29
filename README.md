[![Build status](https://ci.appveyor.com/api/projects/status/4mtheyq9rl07ynsl/branch/master?svg=true)](https://ci.appveyor.com/project/Danielku15/aspnet-url-versioning/branch/master)


# ASP.net API Versioning - URL segment extensions

This library is an extension to the [ASP.NET API versioning](https://github.com/Microsoft/aspnet-api-versioning) library of Microsoft addressing 
missing features on URL segment based versioning. 

It adds support for the following features. 

## Default API version in URL segments

Currently *ASP.net API versioning* cannot handle default version numbers for routes that have the API version in the URL segment. If you define a route
like `/api/v{version}/Controller/Action` the `AssumeDefaultVersionWhenUnspecified` option will not ensure that `/api/Controller/Action` will point to the 
action implementing the default version. 

```csharp
configuration.AddApiVersioningWithUrlSupport(o =>
{
    o.VersioningOptions.DefaultApiVersion = new ApiVersion(3, 0);
    o.VersioningOptions.AssumeDefaultVersionWhenUnspecified = true;
});
```
This extension is related to https://github.com/Microsoft/aspnet-api-versioning/issues/73

## ApiExplorer implementation for SwashBuckle compatibility. 

The default ApiExplorer implementation that comes with ASP.net Web API struggles generating the API descriptions for all versions. 
By providing a custom ApiExplorer implementation this library ensures that components using the ApiExplorer can access the individual versions. 
Note that also this extension focuses on URL segments and will not generate individual entries for Query String or Header based versioning. 

```csharp
configuration.AddApiVersioningAwareApiExplorer(o => {
    // in combination with the "Default API version in URL segments" extension you might want to prevent the default version routes
    // to be excluded from the API schema. 
    o.IncludeDefaultVersion = false; 
    // ASP.net API versioning supports /v3/ and /v3.0/ if you set this option to true
    // the API schema will try to shorten the versions by omitting the minor version part. 
    o.PreferShortHandVersion = true;
});
```

This extension focuses on the compatibility with [SwashBuckle](https://github.com/domaindrivendev/Swashbuckle) and is related to https://github.com/Microsoft/aspnet-api-versioning/issues/60
