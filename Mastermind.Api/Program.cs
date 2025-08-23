using Mastermind.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

if (!builder.Environment.IsProduction())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

// Central app registrations
builder.Services.AddMastermindServices(builder.Configuration, builder.Environment);
var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers();

app.Run();
