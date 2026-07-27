namespace NavNames;

/// <summary>
/// Canonical outbound URLs for the app. Kept in one place so the repo address isn't
/// re-typed per feature (the update feed in <see cref="Services.UpdateService"/> is the
/// other consumer of the same repo).
/// </summary>
internal static class AppLinks
{
    /// <summary>The project's GitHub repository.</summary>
    public const string RepositoryUrl = "https://github.com/LewisIsWorking/NavNames";

    /// <summary>Worked examples of command shortcuts worth stealing.</summary>
    public const string RecipesUrl = RepositoryUrl + "/blob/main/docs/recipes.md";
}
