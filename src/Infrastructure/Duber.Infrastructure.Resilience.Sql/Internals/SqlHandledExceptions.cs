namespace Duber.Infrastructure.Resilience.Sql.Internals
{
    internal enum SqlHandledExceptions
    {
        DatabaseNotCurrentlyAvailable = 40613,
        ErrorProcessingRequest = 40197,
        ServiceCurrentlyBusy = 40501,
        NotEnoughResources = 49918,
        SessionTerminatedLongTransaction = 40549,
        SessionTerminatedToManyLocks = 40550
    }
}
