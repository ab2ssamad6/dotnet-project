// The integration tests share an in-memory SQLite connection per factory. SQLite's single
// connection is not safe for concurrent access, so run the integration tests serially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
