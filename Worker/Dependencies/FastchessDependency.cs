using Worker.ProcessOperations;

namespace Worker.Dependencies;

public class FastchessDependency : IResolvableDependency
{
    private readonly IProcessRunner _runner;
    private readonly ProcessStartInfoCreator _processInfoCreator;
    
    /// <summary>
    /// .Ctor
    /// </summary>
    public FastchessDependency(IProcessRunner runner, ProcessStartInfoCreator processInfoCreator)
    {
        _runner = runner;
        _processInfoCreator = processInfoCreator;
    }
    
    private const string FASTCHESS_BINARY_PATH = "/tmp/bencher-worker";
    private const string FASTCHESS_BINARY_NAME = "fastchess";
    public const string  FASTCHESS_BINARY_FILE_PATH = $"{FASTCHESS_BINARY_PATH}/{FASTCHESS_BINARY_NAME}";
    private const string FASTCHESS_GIT_URL = "https://github.com/DanSamek/fastchess.git";
    private const string FASTCHESS_VERSION = "v1.7.0-alpha";
    
    private string _errorMessage = string.Empty;
    
    public bool Validate()
    {
        var result = File.Exists(FASTCHESS_BINARY_FILE_PATH);
        return result;
    }
    
    public bool TryResolve(Compilers compilers)
    {
        if (compilers == Compilers.None) return false;
        if (Directory.Exists(FASTCHESS_BINARY_PATH)) Directory.Delete(FASTCHESS_BINARY_PATH, true);
        
        var buildCommand = compilers == Compilers.Clang ? "make -j CXX=clang++" : "make -j";
        var commands = (string[])
        [
            $"mkdir -p {FASTCHESS_BINARY_PATH};", 
            $"cd {FASTCHESS_BINARY_PATH}; git clone --branch {FASTCHESS_VERSION} --single-branch {FASTCHESS_GIT_URL} . ;", 
            $"cd {FASTCHESS_BINARY_PATH}; {buildCommand}"
        ];

        var (_, error) = _runner.RunProcess(_processInfoCreator.Create(commands[0]));
        if (!string.IsNullOrEmpty(error))
        {
            _errorMessage = error;
            return false; 
        }
        
        (_, error) = _runner.RunProcess(_processInfoCreator.Create(commands[1]));
        
        var match = Regexes.GitErrorRegex.Match(error ?? string.Empty);
        if (match.Success)
        {
            _errorMessage = error!;
            return false;
        }
        
        (_, error) = _runner.RunProcess(_processInfoCreator.Create(commands[2]));
        return !string.IsNullOrEmpty(error);
    }
    
    public string ErrorMessage() => _errorMessage;
}