using System.Runtime.InteropServices;

//https://stackoverflow.com/questions/1119841/net-console-application-exit-event
internal class ClosingHandler
{
    #region Trap application termination

    [DllImport("Kernel32")]
    private static extern bool SetConsoleCtrlHandler(OnClosing handler, bool add);

    private delegate void OnClosing(CtrlType sig);
    private static OnClosing? _handler;

    enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }

    #endregion

    private static Action? ClosingEvent;

    public static void Create(Action onClosing, Action onClosed)
    {
        ClosingEvent = onClosing;

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            onClosed();
        };

        _handler += new OnClosing(Handler);
        SetConsoleCtrlHandler(_handler, true);
    }

    private static void Handler(CtrlType sig)
    {
        if (ClosingEvent != null)
            ClosingEvent();
        Environment.Exit(-1);
    }
}
