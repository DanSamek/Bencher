namespace Worker;

public interface IValidatableDependency
{
    public bool Validate();

    public string ErrorMessage();
}