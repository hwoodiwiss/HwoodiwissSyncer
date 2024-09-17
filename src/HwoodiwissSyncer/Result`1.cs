using Dunet;

namespace HwoodiwissSyncer;

[Union]
public partial record Result<T>
{
    public partial record Success(T Value);

    public partial record Failure(Problem Problem);
}
