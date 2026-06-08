using FindMyHobbyApi;
using FindMyHobbyApi.Domain.Clients;
using FindMyHobbyApi.Domain.Handlers.Commands;
using FindMyHobbyApi.Domain.Handlers.Queries;
using FindMyHobbyApi.Domain.Models;
using FindMyHobbyApi.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IGetHobbyQueryHandler, GetHobbyQueryHandler>();
builder.Services.AddSingleton<ISearchCoursesCommandHandler, SearchCoursesCommandHandler>();
builder.Services.AddSingleton<IFindMyHobbyApiService, FindMyHobbyApiService>();
builder.Services.AddSingleton<FindMyHobbyApiController>();
builder.Services.AddHttpClient<ICourseSearchClient, OpenAiCourseSearchClient>(client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var useHttpsRedirection = builder.Configuration.GetValue("UseHttpsRedirection", false);
if (useHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapGet("/hobby", async (FindMyHobbyApiController controller, CancellationToken cancellationToken) =>
        await controller.GetHobbyAsync(cancellationToken))
    .WithName("GetHobby");

app.MapPost("/courses/search", async (
        CourseSearchRequest request,
        FindMyHobbyApiController controller,
        CancellationToken cancellationToken) =>
    await controller.SearchCoursesAsync(request, cancellationToken))
    .WithName("SearchCourses");

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
    .WithName("Health");

app.Run();
