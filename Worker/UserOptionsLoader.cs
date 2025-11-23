namespace Worker;

public static class UserOptionsLoader
{
    public record UserOptions(int numberOfThreads);
    public static UserOptions LoadParams()
    {
        return new(0);
    }
}