using System.Collections.Generic;

namespace Scheme.Services.Interfaces
{
    public interface IStorage
    {
        void SaveValue(decimal value);
        ICollection<decimal> GetValues();
        void SetPath(string path);
    }
}
