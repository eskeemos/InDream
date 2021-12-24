using Scheme.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Scheme.Services.Implementations
{
    public class Storage : IStorage
    {
        #region Variables

        private string filePath;

        #endregion

        #region Implemented functions

        public ICollection<decimal> GetValues()
        {
            var list = new List<decimal>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    list.Add(Convert.ToDecimal(line));
                }
            }

            return list;
        }

        public void SaveValue(decimal value)
        {
            using StreamWriter writer = new StreamWriter(filePath, true);
            writer.WriteLine(value);
        }

        public void SetPath(string _filePath)
        {
            filePath = _filePath;
        }

        #endregion
    }
}
