using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Enable.Extensions.FileSystem
{
    internal class ProcessExecutor
    {
        private readonly IList<string> _error = new List<string>();
        private readonly IList<string> _output = new List<string>();
        private readonly TaskCompletionSource<string> _errorResult = new TaskCompletionSource<string>();
        private readonly TaskCompletionSource<string> _outputResult = new TaskCompletionSource<string>();
        private readonly TaskCompletionSource<ProcessResult> _processResult = new TaskCompletionSource<ProcessResult>();
        private readonly ProcessStartInfo _startInfo;

        private Process _process;

        private ProcessExecutor(ProcessStartInfo startInfo)
        {
            _startInfo = startInfo;
        }

        public static Task<ProcessResult> Execute(
            ProcessStartInfo startInfo,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var processExecutor = new ProcessExecutor(startInfo);
            return processExecutor.DoExecute(cancellationToken);
        }

        private Task<ProcessResult> DoExecute(CancellationToken cancellationToken)
        {
            _startInfo.CreateNoWindow = false;
            _startInfo.RedirectStandardOutput = true;
            _startInfo.RedirectStandardError = true;
            _startInfo.UseShellExecute = false;
            _startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            _process = new Process
            {
                StartInfo = _startInfo,
                EnableRaisingEvents = true
            };

            _process.OutputDataReceived += CaptureOutput;
            _process.ErrorDataReceived += CaptureError;
            _process.Exited += CaptureExit;

            using (cancellationToken.Register(Cancelled))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_process.Start())
                {
                    _processResult.TrySetException(new InvalidOperationException($"Failed to start process '{_startInfo.FileName}'."));
                }

                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                return _processResult.Task;
            }
        }

        private void CaptureOutput(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                _output.Add(args.Data);
            }
            else
            {
                _outputResult.SetResult(string.Join(Environment.NewLine, _output));
            }
        }

        private void CaptureError(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                _error.Add(args.Data);
            }
            else
            {
                _errorResult.SetResult(string.Join(Environment.NewLine, _error));
            }
        }

        private void CaptureExit(object sender, EventArgs args)
        {
            var result = new ProcessResult(
                _process.ExitCode,
                _startInfo.FileName,
                _outputResult.Task.Result,
                _errorResult.Task.Result);

            _processResult.TrySetResult(result);
        }

        private void Cancelled()
        {
            _processResult.TrySetCanceled();

            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill();
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
