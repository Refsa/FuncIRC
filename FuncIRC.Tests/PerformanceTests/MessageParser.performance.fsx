#r "../../FuncIRC/bin/Debug/netstandard2.0/FuncIRC.dll"

#load "../../.paket/load/netcoreapp2.0/NBench.fsx"
#load "../TestMessages.fsx"

namespace FuncIRC.Tests.Performance

open NBench
open NBench.Util
open FuncIRC.MessageParser
open FuncIRC.Tests.TestMessages

type MessageParserPerformanceTests() =

    [<PerfSetup>]
    member this.Setup (context: BenchmarkContext) =
        ()

    [<PerfBenchmark (Description="Test parseMessage performance", NumberOfIterations=1000, RunMode=RunMode.Iterations, RunTimeMilliseconds=10000, TestMode=TestMode.Measurement)>]
    [<MemoryMeasurement (MemoryMetric.TotalBytesAllocated)>]
    [<GcMeasurement (GcMetric.TotalCollections, GcGeneration.AllGc)>]
    member this.TestParseMessagePerformance() =
        testMessages
        |> List.iter 
            (fun tm -> (parseMessageString tm.Input) |> ignore )