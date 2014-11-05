# JpegMetadata

Read and write basic JPEG (Exif) metadata.

[![Build status](http://img.shields.io/appveyor/ci/mwijnands/jpegmetadata.svg?style=flat)](https://ci.appveyor.com/project/mwijnands/jpegmetadata) [![NuGet version](http://img.shields.io/nuget/v/XperiCode.JpegMetadata.svg?style=flat)](https://www.nuget.org/packages/XperiCode.JpegMetadata)

## Installation

The `JpegMetadata` package is available at [NuGet](https://www.nuget.org/packages/XperiCode.JpegMetadata). To install `JpegMetadata`, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

> ### Install-Package XperiCode.JpegMetadata

## Usage

	var adapter = new JpegMetadataAdapter(@"d:\test.jpg");
	
	adapter.Metadata.Title = "Beach";
	adapter.Metadata.Subject = "Summer holiday 2014";
	adapter.Metadata.Rating = 4;
	adapter.Metadata.Keywords.Add("beach");
	adapter.Metadata.Comments = "This is a comment.";
	
	bool saved = adapter.Save();
