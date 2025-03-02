using System;
using System.Linq;

namespace GTFO.API.Utilities
{
    /// <summary>
    /// Utility Class for Safely Invoke Delegates one-by-one with try-catching
    /// </summary>
    public static class SafeInvoke
    {
        /// <summary>
        /// Safely Invoke Action one-by-one with Try-Catch
        /// </summary>
        /// <param name="actionToInvoke">Action to Invoke</param>
        public static void Invoke(Action actionToInvoke)
        {
            if (actionToInvoke == null)
                return;

            foreach (var handler in actionToInvoke.GetInvocationList().Cast<Action>())
            {
                try
                {
                    handler();
                }
                catch (Exception e)
                {
                    APILogger.Error("GTFO-API", $"Exception occured while invoking events!\n{e}");
                }
            }
        }

        /// <summary>
        /// Safely Invoke Action one-by-one with Try-Catch
        /// </summary>
        /// <param name="actionToInvoke">Action to Invoke</param>
        /// <param name="arg0">Argument to pass (Index 0)</param>
        public static void Invoke<T>(Action<T> actionToInvoke, T arg0)
        {
            if (actionToInvoke == null)
                return;

            foreach (var handler in actionToInvoke.GetInvocationList().Cast<Action<T>>())
            {
                try
                {
                    handler(arg0);
                }
                catch (Exception e)
                {
                    APILogger.Error("GTFO-API", $"Exception occured while invoking events!\n{e}");
                }
            }
        }
    }
}
