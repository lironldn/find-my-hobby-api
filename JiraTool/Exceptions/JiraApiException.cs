namespace JiraTool.Exceptions;

public sealed class JiraApiException(string message) : Exception(message);