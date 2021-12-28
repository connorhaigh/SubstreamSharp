![Icon](https://raw.githubusercontent.com/connorhaigh/SubstreamSharp/master/Icon.png)

# SubstreamSharp

SubstreamSharp is a C# library that provides the ability to create substreams of fixed regions from any given stream.

## Overview

A substream is effectively a region of the underlying stream with a fixed position and offset that behaves as an independent stream. A substream supports all the operations that the underlying stream does, however only within the context of a specific region.

The main usage is the ability to provide callers with a portion of a stream that might be backed by a larger stream, for instance a large uncompressed binary file stream that may have individual files within its contents exposed as substreams.

## Examples

Creating a substream from a file stream starting from an offset of 128 bytes for the following 1024 bytes:

```csharp
using SubstreamSharp;

using (var fileStream = new FileStream("file", FileMode.Open))
{
	var substream = new Substream(fileStream, 128L, 1024L);
}
```

```csharp
using SubstreamSharp;

using (var fileStream = new FileStream("file", FileMode.Open))
{
	var substream = fileStream.Substream(128L, 1024L);
}
```

## Notes

Substreams work by explicitly seeking the underlying stream to the correct position before any read or write operation takes place. This does mean that the underlying stream will have an undefined position if it is used by itself again after a substream has been created from it. Therefore, it is important to `Seek` on the underlying stream if it is being used in parallel to any substreams.

Any substream will not permit operations that would potentially modify the data outside of its region. For instance, attempting to seek backwards before the start of the substream, or attempting to seek forwards after the end of the substream. A substream can also not have its length modified.

Since a substream is designed to be merely a chunk of an underlying stream, closing a substream will not close the underlying stream. Likewise, closing the underlying stream will inadvertendly close any substreams. Therefore, there is no need to manually call the `Close` or `Dispose` methods on a substream.
