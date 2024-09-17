namespace HwoodiwissSyncer;

public interface IMapper<in TSource, TDestination>
{
    Result<TDestination> Map(TSource source);
}
