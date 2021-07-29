using ElectronicObserver.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using ElectronicObserver.Utility;

namespace ElectronicObserver
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			Application.ThreadException += Application_ThreadException;

			bool allowMultiInstance = args.Contains("-m") || args.Contains("--multi-instance");


			using (var mutex = new Mutex(false, Application.ExecutablePath.Replace('\\', '/'), out var created))
			{

				/*
				bool hasHandle = false;

				try
				{
					hasHandle = mutex.WaitOne(0, false);
				}
				catch (AbandonedMutexException)
				{
					hasHandle = true;
				}
				*/

				if (!created && !allowMultiInstance)
				{
					// 多重起動禁止
					MessageBox.Show("已经启动了。不能多重启动。\r\n错误检测的情况下，请从命令行添加-m选项启动。", "七四式電子観測儀", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FormMain());

			}
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.ToString(), "ElectronicObserverExtended", MessageBoxButtons.OK, MessageBoxIcon.Error);
			ErrorReporter.SendErrorReport(e.Exception, "Error in thread: " + e.Exception.Message);
		}

		public static System.Drawing.Font Window_Font;
	}
}
