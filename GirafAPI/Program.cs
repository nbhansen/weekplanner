using GirafAPI.Data;
using GirafAPI.Endpoints;
using GirafAPI.Extensions;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment)
    .ConfigureIdentity()
    .ConfigureJwt(builder.Configuration, builder.Environment)
    .ConfigureAuthorizationPolicies()
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
app.UseStaticFiles();
app.UseAntiforgery();

// Map endpoints
app.MapCitizensEndpoints();
app.MapUsersEndpoints();
app.MapLoginEndpoint();
app.MapActivityEndpoints();
app.MapOrganizationEndpoints();
app.MapInvitationEndpoints();
app.MapGradeEndpoints();
app.MapPictogramEndpoints();

// Apply migrations and optionally seed data
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAsync();

    var seedDataEnabled = app.Environment.IsDevelopment() ||
                           app.Configuration.GetValue<bool>("SeedData:Enabled");
    if (seedDataEnabled)
    {
        if (app.Configuration.GetValue<bool>("SeedData:CreateDefaultUser"))
        {
            await app.SeedDataAsync();
        }

        if (app.Configuration.GetValue<bool>("SeedData:AddDefaultPictograms"))
        {
            await app.AddDefaultPictograms();
        }
    }
}



if (app.Environment.IsDevelopment())
{
    app.Run("http://0.0.0.0:5171");
}
else
{
    app.Run();
}

