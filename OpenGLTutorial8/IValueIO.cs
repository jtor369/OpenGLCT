using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagnatekControl.SystemModules.Interfaces
{
    public interface IValueIO<T> : IValueInput<T>, IValueOutput<T>
    {
    }
    public interface IValueIO<T,U> : IValueInput<T,U>, IValueOutput<T,U>
    {
    }
}
