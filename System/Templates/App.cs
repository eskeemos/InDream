using System.Collections.Generic;

namespace System.Templates
{
    public class App
    {
        public string Name { get; set; }
        public IEnumerable<Exchange> Exchanges { get; set; }
        public Main Main { get; set; }
    }
}
