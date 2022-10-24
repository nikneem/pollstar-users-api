
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PollStar.Core.Configuration;
using PollStar.Core.Exceptions;
using PollStar.Core.ExtensionMethods;
using PollStar.Core.HealthChecks;
using PollStar.Users;

const string defaultCorsPolicyName = "default_cors";

var builder = WebApplication.CreateBuilder(args);

var azureCredential = new DefaultAzureCredential();
try
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(builder.Configuration.GetRequiredValue("AzureAppConfiguration")), azureCredential)
            .ConfigureKeyVault(kv => kv.SetCredential(azureCredential))
            .UseFeatureFlags();
    });
}
catch (Exception ex)
{
    throw new Exception("Configuration failed", ex);
}


builder.Services.AddPollStarCore(builder.Configuration);
builder.Services.AddPollStarUsers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: defaultCorsPolicyName,
        bldr =>
        {
            bldr.WithOrigins("http://localhost:4200",
                    "https://pollstar-dev.hexmaster.nl",
                    "https://pollstar-test.hexmaster.nl",
                    "https://pollstar.hexmaster.nl")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers(options => { options.Filters.Add(typeof(PollStarExceptionsFilter)); });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(defaultCorsPolicyName);

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(ep =>
{
    ep.MapHealthChecks("/health", new HealthCheckOptions
    {
         ResponseWriter = HealthCheckExtensions.WriteResponse
    });
    ep.MapControllers();
});

app.Run();
