# ProjScan

*Note: This repo is no longer being maintained.*

The ProjScan tool is now being maintained at: https://github.com/datafac/projscan

ProjScan is a very opinionated .NET project scanning tool, which checks published projects for:
- missing or malformed copyright messages;
- missing product name;
- incorrect company name;
- missing description;
- missing authors;
- missing symbol package;
- missing project and package URLs;
- missing signing details;
- missing readme file;
- nullable ref types are enabled;
- warnings as errors enabled;
- unsupported .NET frameworks.

Any warning can be disabled by adding the exemption code into the exemptions list in the projscan.json file.
