// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.Sbom.Contracts.Enums;
using Microsoft.Sbom.Exceptions;
using Microsoft.Sbom.Parser.Strings;
using Microsoft.Sbom.Parsers.Spdx22SbomParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Sbom.Parser;

[TestClass]
public class SbomExternalDocumentReferenceParserTests
{
    [TestMethod]
    public void ParseSbomExternalDocumentReferenceTest()
    {
        var bytes = Encoding.UTF8.GetBytes(ExternalDocumentReferenceStrings.GoodJsonWith2ExtDocumentRefsString);
        using var stream = new MemoryStream(bytes);
        var count = 0;

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        foreach (var extReference in parser.GetReferences())
        {
            count++;
            Assert.IsNotNull(extReference);
        }

        Assert.AreEqual(2, count);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void NullStreamThrows()
    {
        new SbomExternalDocumentReferenceParser(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ObjectDisposedException))]
    public void StreamClosedTestReturnsNull()
    {
        var bytes = Encoding.UTF8.GetBytes(ExternalDocumentReferenceStrings.GoodJsonWith2ExtDocumentRefsString);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);
        stream.Close();

        parser.GetReferences().GetEnumerator().MoveNext();
    }

    [TestMethod]
    [ExpectedException(typeof(EndOfStreamException))]
    public void StreamEmptyTestReturnsNull()
    {
        using var stream = new MemoryStream();
        stream.Read(new byte[Constants.ReadBufferSize]);
        var buffer = new byte[Constants.ReadBufferSize];

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        parser.GetReferences().GetEnumerator().MoveNext();
    }

    [DataTestMethod]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringMissingChecksum)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringMissingDocumentId)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringMissingDocument)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringMissingSHA1Checksum)]
    [TestMethod]
    [ExpectedException(typeof(ParserException))]
    public void MissingPropertiesTest_Throws(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        parser.GetReferences().GetEnumerator().MoveNext();
    }

    [DataTestMethod]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringAdditionalObject)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringAdditionalString)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringAdditionalArray)]
    [DataRow(ExternalDocumentReferenceStrings.JsonExtDocumentRefsStringAdditionalArrayNoKey)]
    [TestMethod]
    public void IgnoresAdditionalPropertiesTest(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        foreach (var reference in parser.GetReferences())
        {
            Assert.IsNotNull(reference);
        }
    }

    [DataTestMethod]
    [DataRow(ExternalDocumentReferenceStrings.MalformedJson)]
    [TestMethod]
    [ExpectedException(typeof(ParserException))]
    public void MalformedJsonTest_Throws(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        parser.GetReferences().GetEnumerator().MoveNext();
    }

    [TestMethod]
    public void EmptyArray_ValidJson()
    {
        var bytes = Encoding.UTF8.GetBytes(ExternalDocumentReferenceStrings.EmptyArray);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        parser.GetReferences().GetEnumerator().MoveNext();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NullOrEmptyBuffer_Throws()
    {
        var bytes = Encoding.UTF8.GetBytes(SbomFileJsonStrings.MalformedJson);
        using var stream = new MemoryStream(bytes);

        SPDXParser parser = new(stream, Array.Empty<ParserState>(), 0, ignoreValidation: true);

        var state = parser.Next();
        Assert.AreEqual(ParserState.REFERENCES, state);

        parser.GetReferences().GetEnumerator().MoveNext();
    }
}
