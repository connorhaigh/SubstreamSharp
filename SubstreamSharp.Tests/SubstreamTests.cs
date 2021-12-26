using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SubstreamSharp.Tests
{
	[TestClass]
	public sealed class SubstreamTests
	{
		[TestMethod]
		public void TestRead()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var buffer = new byte[4];

				var substream = new Substream(memoryStream, 2L, 4L);
				substream.Read(buffer);

				Assert.IsTrue(buffer.SequenceEqual(new byte[] { 3, 4, 5, 6 }));
				Assert.AreEqual(4L, substream.Position);
			}
		}

		[TestMethod]
		public void TestWrite()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 2L, 4L);
				substream.Write(new byte[] { 9, 10, 11, 12 });

				Assert.IsTrue(memoryStream.ToArray().SequenceEqual(new byte[] { 1, 2, 9, 10, 11, 12, 7, 8 }));
				Assert.AreEqual(4L, substream.Position);
			}
		}

		[TestMethod]
		public void TestSeekBegin()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 8L);
				substream.Seek(2L, SeekOrigin.Begin);

				Assert.AreEqual(2L, substream.Position);
			}
		}

		[TestMethod]
		public void TestSeekEnd()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 8L);
				substream.Seek(-2L, SeekOrigin.End);

				Assert.AreEqual(6L, substream.Position);
			}
		}

		[TestMethod]
		public void TestSeekCurrent()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 8L);
				substream.Read(new byte[2]);
				substream.Seek(2L, SeekOrigin.Current);

				Assert.AreEqual(4L, substream.Position);
			}
		}

		[TestMethod]
		public void TestReadBeyondBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var buffer = new byte[8];

				var substream = new Substream(memoryStream, 2L, 4L);
				var count = substream.Read(buffer);

				Assert.IsTrue(buffer.SequenceEqual(new byte[] { 3, 4, 5, 6, 0, 0, 0, 0 }));
				Assert.AreEqual(4, count);
			}
		}

		[TestMethod]
		public void TestWriteBeyondBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var buffer = new byte[8] { 9, 10, 11, 12, 13, 14, 15, 16 };

				var substream = new Substream(memoryStream, 2L, 4L);
				substream.Write(buffer);

				Assert.IsTrue(memoryStream.ToArray().SequenceEqual(new byte[] { 1, 2, 9, 10, 11, 12, 7, 8 }));
				Assert.AreEqual(4L, substream.Position);
			}
		}

		[TestMethod]
		public void TestReadOffset()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var buffer = new byte[8];

				var substream = new Substream(memoryStream, 0L, 8L);
				var count = substream.Read(buffer, 4, 4);

				Assert.IsTrue(buffer.SequenceEqual(new byte[] { 0, 0, 0, 0, 1, 2, 3, 4 }));
				Assert.AreEqual(4, count);
			}
		}

		[TestMethod]
		public void TestWriteOffset()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var buffer = new byte[8] { 9, 10, 11, 12, 13, 14, 15, 16 };

				var substream = new Substream(memoryStream, 0L, 8L);
				substream.Write(buffer, 4, 4);

				Assert.IsTrue(memoryStream.ToArray().SequenceEqual(new byte[] { 13, 14, 15, 16, 5, 6, 7, 8 }));
				Assert.AreEqual(4L, substream.Position);
			}
		}

		[TestMethod]
		public void TestSeekBeginOutOfBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 4L);
				var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => substream.Seek(8L, SeekOrigin.Begin));

				Assert.AreEqual("Offset cannot be greater than the length of the substream. (Parameter 'offset')", exception.Message);
			}
		}

		[TestMethod]
		public void TestSeekEndOutOfBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 4L);
				var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() => substream.Seek(-8L, SeekOrigin.End));

				Assert.AreEqual("Offset cannot be less than the length of the substream. (Parameter 'offset')", exception.Message);
			}
		}

		[TestMethod]
		public void TestSeekCurrentForwardOutOfBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 4L);
				var exception = Assert.ThrowsException<NotSupportedException>(() => substream.Seek(8L, SeekOrigin.Current));

				Assert.AreEqual("Attempted to seek beyond the end of the substream.", exception.Message);

			}
		}

		[TestMethod]
		public void TestSeekCurrentBackwardOutOfBounds()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
			{
				var substream = new Substream(memoryStream, 0L, 4L);
				var exception = Assert.ThrowsException<NotSupportedException>(() => substream.Seek(-8L, SeekOrigin.Current));

				Assert.AreEqual("Attempted to seek before the start of the substream.", exception.Message);
			}
		}

		[TestMethod]
		public void TestSetLength()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4 }))
			{
				var substream = new Substream(memoryStream, 0L, 2L);
				var exception = Assert.ThrowsException<NotSupportedException>(() => substream.SetLength(4L));

				Assert.AreEqual("Cannot set the length of a fixed substream.", exception.Message);
			}
		}

		[TestMethod]
		public void TestSetReadTimeout()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4 }))
			{
				var substream = new Substream(memoryStream, 0L, 2L);
				var exception = Assert.ThrowsException<NotSupportedException>(() => substream.ReadTimeout = 60);

				Assert.AreEqual("Cannot set the read timeout of a substream.", exception.Message);
			}
		}

		[TestMethod]
		public void TestSetWriteTimeout()
		{
			using (var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4 }))
			{
				var substream = new Substream(memoryStream, 0L, 2L);
				var exception = Assert.ThrowsException<NotSupportedException>(() => substream.WriteTimeout = 60);

				Assert.AreEqual("Cannot set the write timeout of a substream.", exception.Message);
			}
		}
	}
}
