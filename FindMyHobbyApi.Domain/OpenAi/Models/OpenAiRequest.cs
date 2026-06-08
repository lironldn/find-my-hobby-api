namespace FindMyHobbyApi.Domain.OpenAi.Models;

record OpenAiRequest(
    string Model,
    OpenAiTool[] Tools,
    string Input);