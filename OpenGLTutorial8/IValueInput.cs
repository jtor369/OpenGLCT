namespace MagnatekControl.SystemModules.Interfaces
{
    public interface IValueInput<T>
    {
        T read();
    }
    public interface IValueInput<T,U> : IValueInput<T>
    {
        T read(U value);
    }
}
