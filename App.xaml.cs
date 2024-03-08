using System;
using System.Threading;
using System.Windows;

namespace Re_Hippocamp
{
    public partial class App : Application
    {

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (e.Args.Length == 1)
				if (e.Args[0] == "wait") { Console.WriteLine("WAITING."); Thread.Sleep(500); }
			
			MainWindow wnd = new MainWindow();
			wnd.Show();
		}

	}
}
