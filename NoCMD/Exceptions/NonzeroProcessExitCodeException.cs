using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCMD.Exceptions
{
    [Serializable]
    public sealed class NonzeroProcessExitCodeException: ApplicationException
    {
        public readonly int ExitCode;

        public NonzeroProcessExitCodeException(int exitCode)
        {
            ExitCode = exitCode;
        }

        public override string Message => $"Process finished with code {ExitCode}.";
    }
}
