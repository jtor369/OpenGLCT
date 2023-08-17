namespace MagnatekControl.SystemModules.Interfaces
{
    public interface IValueOutput<T>
    {
        void write(T value);
    }
    public interface IValueOutput<T,U> : IValueOutput<T>
    {
        void write(T value, U value2);
    }
}
