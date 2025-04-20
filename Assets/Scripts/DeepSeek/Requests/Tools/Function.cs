using System.Collections.Generic;

namespace Xiyu.DeepSeek.Requests.Tools
{
    public class Function
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Parameters { get; set; }

        public IEnumerable<string> Required { get; set; }
    }
}