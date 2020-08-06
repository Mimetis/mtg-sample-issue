using System;
using System.Collections.Generic;
using System.Text;

namespace Mtg.Sample.Models
{
    public class GraphOptions
    {
        public string BaseAddress { get; set; }
        public string Scopes { get; set; }

        public IEnumerable<string> GetScopes()
        {
            if (string.IsNullOrEmpty(Scopes))
                return null;

            return Scopes.Split(new char[] { ' ' });
        }
    }
}
