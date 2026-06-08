namespace FindMyHobbyApi.OpenAi.Models;

record OpenAiRequest(
    string Model,
    OpenAiTool[] Tools,
    string Input);