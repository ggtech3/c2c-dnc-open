using Microsoft.VisualBasic.ApplicationServices;

namespace WindowsDnc;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var application = new C2CSingleInstanceApplication();
        application.Run(args);
    }

    private sealed class C2CSingleInstanceApplication : WindowsFormsApplicationBase
    {
        public C2CSingleInstanceApplication()
        {
            IsSingleInstance = true;
            ShutdownStyle = ShutdownMode.AfterMainFormCloses;
        }

        protected override void OnCreateMainForm()
        {
            var form = new MainForm();
            MainForm = form;
            string[] startupArguments = CommandLineArgs.ToArray();
            form.Shown += (_, _) => form.OpenPostedPrograms(startupArguments);
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            if (MainForm is MainForm form)
            {
                form.OpenPostedPrograms(eventArgs.CommandLine);
            }

            base.OnStartupNextInstance(eventArgs);
        }
    }
}
