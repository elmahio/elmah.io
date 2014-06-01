using System.Diagnostics;

namespace Elmah.Io
{
    public static class ErrorExtensions
    {
        public static StackTrace StackTraceOrNull(this Error error)
        {
            if (error == null) return null;
            if (error.Exception == null) return null;
            var stackTrace = new StackTrace(error.Exception, true);
            if (stackTrace.FrameCount < 1) return null;
            var firstFrame = stackTrace.GetFrame(0);
            if (firstFrame == null) return null;
            var method = firstFrame.GetMethod();
            if (method == null) return null;

            return stackTrace;
        }

        public static bool HasServerVariable(this Error error, string name)
        {
            if (error == null) return false;
            if (error.ServerVariables == null) return false;
            return !string.IsNullOrWhiteSpace(error.ServerVariables[name]);
        }
    }
}
