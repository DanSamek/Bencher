namespace Worker.Dependencies;

/// <summary>
/// Required compilers for another dependencies.
/// </summary>
[Flags]
public enum Compilers
{
    None = 0,
    Gcc = 0x1, 
    Clang = 0x2 
}