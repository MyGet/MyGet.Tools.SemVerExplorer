# MyGet SemVer Explorer

Tool for exploring Semantic Version on existing NuGet packages. [Try it now!](http://semver.myget.org)

## What is it?

When authoring [NuGet](https://www.nuget.org) packages, you can declare package dependency versions using [Semantic Versioning](https://www.semver.org). NuGet allows specifying dependencies as floating ranges, using interval notation or using fixed version numbers, as [explained in the NuGet docs](http://docs.nuget.org/Create/Versioning).

[MyGet SemVer Explorer](http://semver.myget.org) allows you to specify a SemVer dependency range, and will check the target package repository for the package versions that match.

![https://raw.githubusercontent.com/MyGet/MyGet.Tools.SemVerExplorer/master/assets/homepage.png](https://raw.githubusercontent.com/MyGet/MyGet.Tools.SemVerExplorer/master/assets/homepage.png)

## Can I target MyGet feeds?

Definitely! By default, the tool is configured to query the v3 NuGet.org repository at [https://api.nuget.org/v3/index.json](https://api.nuget.org/v3/index.json).
You can simply change the target feed URL to the v3 NuGet feed of a MyGet repository you have access to, and we'll query that one instead.

## Can I target private MyGet feeds?

If you have an access token that grants you read-access to the MyGet repository, you can leverage MyGet's support for pre-authenticated feed URLs. Make sure you target the pre-authenticated v3 NuGet endpoint. See our MyGet documentation for [further guidance](http://docs.myget.org/docs/reference/feed-endpoints).
