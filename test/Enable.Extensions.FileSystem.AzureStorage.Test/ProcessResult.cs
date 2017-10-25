namespace Enable.Extensions.FileSystem
{
    public class ProcessResult
    {
        public ProcessResult(
            int exitCode,
            string fileName,
            string standardOutput,
            string standardError)
        {
            ExitCode = exitCode;
            FileName = fileName;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        public int ExitCode { get; private set; }

        public string FileName { get; private set; }

        public string StandardOutput { get; private set; }

        public string StandardError { get; private set; }
    }
}
