using System.Text.Json.Serialization;
using AlgoritmosDeTransporteAPI.Services;
using AlgoritmosDeTransporteAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaLocal", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

builder.Services.AddScoped<ITransporteService, TransporteService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IResolverProblemaRequestValidator, ResolverProblemaRequestValidator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("SpaLocal");
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/html/index.html"));

app.Run();
