using Dunet;

namespace HwoodiwissSyncer;

[Union]
public partial record Option<T>
{
    public partial record Some(T Value);

    public partial record None;
}
