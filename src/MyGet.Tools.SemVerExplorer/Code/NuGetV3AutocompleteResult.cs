using System.Collections.Generic;

namespace MyGet.Tools.SemVerExplorer.Code
{
    public class NuGetV3AutocompleteResult
    {
        public long TotalHits { get; set; }
        public List<string> Data { get; set; }
    }
}