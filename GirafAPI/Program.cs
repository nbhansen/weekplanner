using GirafAPI.Data;
using GirafAPI.Endpoints;
using GirafAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureJwt(builder.Configuration, builder.Environment)
    .ConfigureAuthorizationPolicies()
    .ConfigureCoreClient(builder.Configuration)
    .ConfigureSwagger();

builder.Services.AddAntiforgery(options =>
    {
      options.Cookie.Expiration = TimeSpan.Zero;
    });

var app = builder.Build();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Map endpoints
app.MapActivityEndpoints();

// Apply migrations
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAsync();
}

if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5171");
}
else
{
    app.Run();
}
