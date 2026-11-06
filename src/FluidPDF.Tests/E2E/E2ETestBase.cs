namespace FluidPDF.Tests.E2E
{
    public abstract class E2ETestBase : IAsyncLifetime
    {
        protected string OutputPath { get; } = Path.Combine(AppContext.BaseDirectory, "E2E");

        public ValueTask InitializeAsync()
        {
            Directory.CreateDirectory(OutputPath);
            return default;
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return default;
        }
    }
}
