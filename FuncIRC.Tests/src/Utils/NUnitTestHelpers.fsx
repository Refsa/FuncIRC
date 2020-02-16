#load "../.paket/load/netstandard2.0/NUnit.fsx"

namespace FuncIRC.Tests

open NUnit
open NUnit.Framework

module NUnitTestHelpers =
    /// Functional wrapper for Assert.True
    let AssertTrue (statement: bool) (message: string) =
        Assert.True (statement, message)

    /// Functional wrapper for Assert.False
    let AssertFalse (statement: bool) (message: string) =
        Assert.False (statement, message)

    /// Functional wrapper for Assert.AreEqual
    let AssertAreEqual (object1: obj) (object2: obj) =
        Assert.AreEqual (object1, object2)

    /// Functional wrapper for Assert.AreEqual
    let AssertAreEqualM (object1: obj) (object2: obj) (message: string) =
        Assert.AreEqual (object1, object2, message)

    // Constructs a string of given length
    let createString maxLength =
        let rec buildLongMessage (currentMessage: string) =
            if currentMessage.Length = maxLength then currentMessage
            else buildLongMessage (currentMessage + "A")
        buildLongMessage ""