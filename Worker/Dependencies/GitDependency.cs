namespace Worker.Dependencies;

public class GitDependency : IValidatableDependency
{
    public bool Validate()
    {
        var processInfo = Helper.CreateProcessStartInfo("git");
        var (_, error) = Helper.RunProcess(processInfo);
        return string.IsNullOrEmpty(error);
    }
    
    string IValidatableDependency.ErrorMessage() => "Unable to resolve git dependency";
}