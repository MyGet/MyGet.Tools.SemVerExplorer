using System.Collections.Generic;
using System.ComponentModel;
using NuGet.Versioning;

namespace MyGet.Tools.SemVerExplorer.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Versions = new List<NuGetVersion>();
        }

        public string FeedUrl { get; set; }
        public string PackageId { get; set; }
        public string VersionRange { get; set; }
        public VersionRange NuGetVersionRange { get; set; }
        public List<NuGetVersion> Versions { get; set; }
    }
}