using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public static class LanguageMap
    {
        public static readonly Dictionary<ProgrammingLanguage, string> ToolLanguageKeys = new()
        {
            { ProgrammingLanguage.Csharp, "csharp" },
            { ProgrammingLanguage.Java, "java" },
            { ProgrammingLanguage.JavaScript, "javascript" },
            { ProgrammingLanguage.TypeScript, "typescript" },
            { ProgrammingLanguage.Python, "python" },
            { ProgrammingLanguage.PHP, "php" },
            { ProgrammingLanguage.Go, "go" },
            { ProgrammingLanguage.Ruby, "ruby" },
            { ProgrammingLanguage.CPlusPlus, "cpp" },
            { ProgrammingLanguage.Swift, "swift" },
            { ProgrammingLanguage.Kotlin, "kotlin" }
        };

        public static string? GetToolKey(ProgrammingLanguage language)
        {
            return ToolLanguageKeys.ContainsKey(language) ? ToolLanguageKeys[language] : null;
        }
    }
}
