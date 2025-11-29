namespace Worker.Dependencies;

public class FastchessDependency : IResolvableDependency
{
    private const string FASTCHESS_BINARY_PATH = "/tmp/bencher-worker";
    // TODO maybe create own fork just to make sure.
    private const string FASTCHESS_GIT_URL = "https://github.com/Disservin/fastchess.git";
    private const string FASTCHESS_VERSION = "v1.7.0-alpha";
    public  const string FASTCHESS_BINARY_NAME = "fastchess";
    
    private string _errorMessage = string.Empty;
    
    public bool Validate()
    {
        var result = Directory.Exists(FASTCHESS_BINARY_PATH);
        // TODO maybe check how old binary is and remove it.
        return result;
    }
    
    public bool TryResolve(Compilers compilers)
    {
        var buildCommand = compilers == Compilers.Clang ? "make -j CXX=clang++" : "make -j";
        var commands = (string[])
        [
            $"mkdir -p {FASTCHESS_BINARY_PATH}; cd {FASTCHESS_BINARY_PATH}", 
            $"git clone --branch {FASTCHESS_VERSION} --single-branch {FASTCHESS_GIT_URL} . ;", 
            $"{buildCommand}"
        ];

        var (_, error) = Helper.RunProcess(Helper.CreateProcessStartInfo(commands[0]));
        if (!string.IsNullOrEmpty(error))
        {
            _errorMessage = error;
            return false; 
        }
        
        (_, error) = Helper.RunProcess(Helper.CreateProcessStartInfo(commands[1]));
        
        var match = Regexes.GitErrorRegex.Match(error ?? string.Empty);
        if (match.Success)
        {
            _errorMessage = error!;
            return false;
        }
        
        (_, error) = Helper.RunProcess(Helper.CreateProcessStartInfo(commands[2]));
        return !string.IsNullOrEmpty(error);
    }
    
    public string ErrorMessage() => _errorMessage;
}