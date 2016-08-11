using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyGet.Tools.SemVerExplorer.Code;
using MyGet.Tools.SemVerExplorer.Models;
using NuGet.Versioning;

namespace MyGet.Tools.SemVerExplorer.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost, ActionName("Index")]
        public IActionResult Index_Post(HomeViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PackageId))
            {
                // Redirect to friendly URL
                return RedirectToRoute("friendly-nuget-search", new
                {
                    PackageId = model.PackageId,
                    VersionRange = model.VersionRange,
                    FeedUrl = model.FeedUrl
                });
            }

            return RedirectToAction("Index");
        }

        [HttpGet, ActionName("Index")]
        public async Task<IActionResult> Index_Get(HomeViewModel model)
        {
            // Ensure we have a model
            //model.PackageId = !string.IsNullOrEmpty(model.PackageId)
            //    ? model.PackageId
            //    : "EntityFramework";

            //model.VersionRange = !string.IsNullOrEmpty(model.VersionRange)
            //    ? model.VersionRange
            //    : "6.0.*";

            if (!string.IsNullOrEmpty(model.PackageId))
            {
                // Perform search and fetch all versions
                VersionRange versionRange;
                if (VersionRange.TryParse(model.VersionRange, out versionRange))
                {
                    model.NuGetVersionRange = versionRange;
                }

                try
                {
                    var feedUrl = !string.IsNullOrEmpty(model.FeedUrl)
                        ? model.FeedUrl
                        : NuGetV3SearchService.DefaultServiceIndexUrl;

                    var searchService = new NuGetV3SearchService(feedUrl);
                    var versions = await searchService.AutocompleteAsync(
                        id: model.PackageId.ToLowerInvariant(), prerelease: true);

                    foreach (var version in versions.Data)
                    {
                        model.Versions.Add(NuGetVersion.Parse(version));
                    }
                }
                catch (InvalidOperationException)
                {
                    // intentional
                }
                catch (HttpRequestException)
                {
                    // intentional
                }
            }
            
            return View(model);
        }

        [HttpGet, ActionName("Autocomplete")]
        public async Task<IActionResult> Autocomplete_Get(string id, string feedUrl)
        {
            try
            {
                feedUrl = !string.IsNullOrEmpty(feedUrl)
                    ? feedUrl
                    : NuGetV3SearchService.DefaultServiceIndexUrl;

                var searchService = new NuGetV3SearchService(feedUrl);
                var result = await searchService.AutocompleteAsync(
                    query: id, take: 20, prerelease: true);

                return Json(result.Data);
            }
            catch (InvalidOperationException)
            {
                // intentional
                return Json(new string[] { });
            }
            catch (HttpRequestException)
            {
                // intentional
                return Json(new string[] { });
            }
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
