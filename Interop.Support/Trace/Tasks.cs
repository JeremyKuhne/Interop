// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Interop.Support.Trace
{
    public enum Tasks : uint
    {
        GarbageCollection = 1,
        WorkerThreadCreation = 2,
        IOThreadCreation = 3,
        WorkerThreadRetirement = 4,
        IOThreadRetirement = 5,
        ThreadpoolSuspension = 6,
        Exception = 7,
        Contention = 8,
        ClrMethod = 9,
        ClrLoader = 10,
        ClrStack = 11,
        ClrStrongNameVerification = 12,
        ClrAuthenticodeVerification = 13,
        AppDomainResourceManagement = 14,
        ClrIlStub = 15,
        ThreadPoolWorkerThread = 16,
        ThreadPoolWorkerThreadRetirement = 17,
        ThreadPoolWorkerThreadAdjustment = 18,
        ClrRuntimeInformation = 19,
        ClrPerfTrack = 20,
        Type = 21,
        ThreadPoolWorkingThreadCount = 22,
        ThreadPool = 23,
        Thread = 24,
        DebugIpcEvent = 25,
        DebugExceptionProcessing = 26,
        ExceptionCatch = 27,
        ExceptionFinally = 28,
        ExceptionFilter = 29,
    }
}
