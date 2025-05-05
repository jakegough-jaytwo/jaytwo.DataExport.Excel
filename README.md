# jaytwo.StreamingExcelExport

[![NuGet Version](https://img.shields.io/nuget/v/jaytwo.StreamingExcelExport.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/jaytwo.StreamingExcelExport)
[![NuGet Downloads](https://img.shields.io/nuget/dt/jaytwo.StreamingExcelExport.svg?style=flat)](https://www.nuget.org/packages/jaytwo.StreamingExcelExport)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Life would be easier if users were ok with a CSV export.  Somteimtes users insist on a _real_ excel export, and they can't be fooled by renaming a CSV/TSV to `.xls`.

There are already packages out there that can do that.  It's not a problem until the data gets _big_.  Since `xslx` is a zip package full of xml files, most of the time this means you have to build up that XML structure in memory before writing it to a stream/disk.  When you have a million of rows and gigabytes of raw data to export, you start crashing the webserver.

I wanted an efficient export that prioritizes efficient memory use.  I assume there's plenty of disk space, and I assume speed is less important.

This uses `XmlWriter` to write out the xml to disk, then you zip those files to another zip file on disk.  I've exported 900k rows with a memory footprint <20mb.

## Installation

Add the NuGet package:

```powershell
PM> Install-Package jaytwo.StreamingExcelExport
```

## Usage

TODO

---

Made with &hearts; by Jake
