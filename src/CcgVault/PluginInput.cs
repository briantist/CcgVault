using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CcgVault
{
    class PluginInput
    {
        private string[] _parsed;
        private ArraySegment<string> _arguments;
        
        public PluginInput(string input)
        {
            _parsed = input.Split('|');
            Path = _parsed[0];
            _arguments = new ArraySegment<string>(_parsed, 1, _parsed.Length - 1);
        }

        public string Path { get; }
        public ArraySegment<string> Arguments => _arguments;
    }
}
