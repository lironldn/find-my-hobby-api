using FindMyHobbyApi.Domain.Models;

namespace FindMyHobbyApi.Domain.Handlers.Commands;

public sealed record SearchCoursesCommand(CourseSearchRequest Request);
