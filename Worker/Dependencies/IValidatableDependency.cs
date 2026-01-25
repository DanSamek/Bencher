namespace Worker;

/// <summary>
/// Interface for the dependency, that can be only validated (for installation is required root)
/// </summary>
public interface IValidatableDependency
{
    public bool Validate();

    public string ErrorMessage();
}