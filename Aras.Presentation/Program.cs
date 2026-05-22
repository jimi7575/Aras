using Aras.Application;
using Aras.Filters;
using Aras.Infrastructure;
using Aras.Services;
using Hangfire;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.MigrateDatabaseAsync();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire");

app.UseAuthorization();

app.MapControllers();
RecurringJob.AddOrUpdate<IWalletJob>(
    "apply-pending-wallet-orders",
    job => job.ApplyPendingOrdersAsync(CancellationToken.None),
    Cron.Minutely);

app.Run();
