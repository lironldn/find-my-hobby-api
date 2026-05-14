var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

var hobbies = new[]
{
    "Hiking", "Bouldering", "Karaoke", "Pottery", "Cooking", "Drawing", "Salsa dancing", "Art Galleries", "Cinema", "Theatre"
};

app.MapGet("/hobby", () =>
    {
        var hobby = Enumerable.Range(1, 5).Select(index =>
                new Hobby
                (
                    hobbies[Random.Shared.Next(hobbies.Length)]
                ))
            .ToArray();
        return hobby;
    })
    .WithName("GetHobby");

app.Run();

record Hobby(string Name)
{
    //public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}