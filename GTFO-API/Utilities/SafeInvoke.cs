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

        /// <summary>
        /// Safely Invoke Action one-by-one with Try-Catch
        /// </summary>
        /// <param name="actionToInvoke">Action to Invoke</param>
        /// <param name="arg0">Argument to pass (Index 0)</param>
        /// <param name="arg1">Argument to pass (Index 1)</param>
        public static void Invoke<T0, T1>(Action<T0, T1> actionToInvoke, T0 arg0, T1 arg1)
        {
            if (actionToInvoke == null)
                return;

            foreach (var handler in actionToInvoke.GetInvocationList().Cast<Action<T0, T1>>())
            {
                try
                {
                    handler(arg0, arg1);
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
        /// <param name="arg1">Argument to pass (Index 1)</param>
        /// <param name="arg2">Argument to pass (Index 2)</param>
        public static void Invoke<T0, T1, T2>(Action<T0, T1, T2> actionToInvoke, T0 arg0, T1 arg1, T2 arg2)
        {
            if (actionToInvoke == null)
                return;

            foreach (var handler in actionToInvoke.GetInvocationList().Cast<Action<T0, T1, T2>>())
            {
                try
                {
                    handler(arg0, arg1, arg2);
                }
                catch (Exception e)
                {
                    APILogger.Error("GTFO-API", $"Exception occured while invoking events!\n{e}");
                }
            }
        }

        /// <summary>
        /// Safely Invoke Delegate one-by-one with Try-Catch
        /// </summary>
        /// <typeparam name="D">Type of Delegate</typeparam>
        /// <param name="delegateToInvoke">Delegate to Invoke</param>
        /// <param name="invoke">Direction for how this delegate should be invoked</param>
        public static void InvokeDelegate<D>(Delegate delegateToInvoke, Action<D> invoke) where D : Delegate
        {
            if (delegateToInvoke == null)
                return;

            foreach (var handler in delegateToInvoke.GetInvocationList().Cast<D>())
            {
                try
                {
                    invoke(handler);
                }
                catch (Exception e)
                {
                    APILogger.Error("GTFO-API", $"Exception occured while invoking events!\n{e}");
                }
            }
        }
    }
}
