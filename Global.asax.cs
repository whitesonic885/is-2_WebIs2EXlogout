using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace is2EXlogout 
{
	/// <summary>
	/// Global の概要の説明です。
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		private string sLogPath = "D:\\IS2EX\\ServiceLog\\";
		private static Encoding enc = Encoding.GetEncoding("shift-jis");

		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{
			LogOut("Application_Start");

			System.Type type = System.Type.GetType("System.String");
			System.Configuration.AppSettingsReader config = new System.Configuration.AppSettingsReader();
			//ログ出力パスの取得
			sLogPath = config.GetValue("path", type).ToString();
			Context.Application.Add("sLogPath", sLogPath);
		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_Error(Object sender, EventArgs e)
		{
// MOD 2007.04.27 東都）高木 ログ出力の変更 START
//			LogOut("Application_Error:" + e.ToString());
//
//			// 最後に発生したエラー原因情報をExceptionオブジェクトとして取得 
//			Exception ex = Server.GetLastError().InnerException; 
//			LogOut(ex.ToString());
// MOD 2007.05.29 東都）高木 ログ出力の変更 START
//			EvtLogOut("Application_Error:" + e.ToString());
//
//			// 最後に発生したエラー原因情報をExceptionオブジェクトとして取得 
//			Exception ex = Server.GetLastError().InnerException; 
//			EvtLogOut(ex.ToString());

			// 最後に発生したエラー原因情報をExceptionオブジェクトとして取得 
			Exception ex = Server.GetLastError().InnerException; 
			if(ex == null){
				EvtLogOut("Application_Error:" + e.ToString());
			}else if(ex.GetType().ToString().StartsWith("System.IO.FileNotFoundException")
				&& ex.Message.ToString().EndsWith("get_aspx_ver.aspx")){
				//	[_is2_is2logout]System.IO.FileNotFoundException: D:\IS2\webapp\is2logout\get_aspx_ver.aspx
				//	   at System.Web.UI.TemplateParser.GetParserCacheItem()
				//	   at System.Web.UI.TemplateControlParser.CompileAndGetParserCacheItem(String virtualPath, String inputFile, HttpContext context)
				//	   at System.Web.UI.TemplateControlParser.GetCompiledInstance(String virtualPath, String inputFile, HttpContext context)
				//	   at System.Web.UI.PageParser.GetCompiledPageInstanceInternal(String virtualPath, String inputFile, HttpContext context)
				//	   at System.Web.UI.PageHandlerFactory.GetHandler(HttpContext context, String requestType, String url, String path)
				//	   at System.Web.HttpApplication.MapHttpHandler(HttpContext context, String requestType, String path, String pathTranslated, Boolean useAppConfig).
				;
			}else{
				EvtLogOut("Application_Error:" + e.ToString() + "\n"
							+ ex.Message + "\n"
							+ ex.ToString());
			}
// MOD 2007.05.29 東都）高木 ログ出力の変更 END
// MOD 2007.04.27 東都）高木 ログ出力の変更 END
		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{
			LogOut("Application_End");
		}
			
		#region Web フォーム デザイナで生成されたコード 
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion

// ADD 2007.04.27 東都）高木 ログ出力の変更 START
		/*********************************************************************
		 * イベントログ出力
		 * 引数：ログ出力文字列
		 * 戻値：無し-
		 *
		 *********************************************************************/
		private static bool   gbLogInit = false;
		private static string gsEvtApp = "is2web";
		private static string gsAppSrc = System.Web.HttpRuntime.AppDomainAppVirtualPath.Replace('/','_');
		private void EvtLogOut(string sLog)
		{
			try
			{
				//初期設定
				if(!gbLogInit)
				{
					try
					{
						if(!System.Diagnostics.EventLog.SourceExists(gsEvtApp))
						{
							System.Diagnostics.EventLog.CreateEventSource(gsEvtApp, "Application");
						}
					}
					catch(System.Security.SecurityException)
					{
//						gsEvtApp = "W3SVC";
						gsEvtApp = "W3SVC-WP";
					}
					gbLogInit = true;
				}

				if(System.Diagnostics.EventLog.SourceExists(gsEvtApp))
				{
					//イベントログへの書込
					System.Diagnostics.EventLog.WriteEntry(gsEvtApp, "\n[" + gsAppSrc + "]" + sLog);
				}
				else
				{
					//ログファイルへの書込
//					LogOut(sLog);
				}
			}
			catch(Exception ex)
			{
				LogOut(ex.ToString());
				LogOut(sLog);
			}
		}
// ADD 2007.04.27 東都）高木 ログ出力の変更 END
		/*********************************************************************
		 * ログ出力
		 * 引数：ログ出力文字列
		 * 戻値：無し-
		 *
		 *********************************************************************/
		private void LogOut(string sLog)
		{
			System.IO.FileStream   cfs = null;
			System.IO.StreamWriter csw = null;
			try
			{
				string fileName = sLogPath 
								+ System.DateTime.Now.ToString("MMdd") 
// MOD 2007.04.27 東都）高木 ログ出力の変更 START
//								+ "_Global.log";
								+ "_Global"
								+ gsAppSrc
								+ ".log"
								;
// MOD 2007.04.27 東都）高木 ログ出力の変更 END

				cfs = new System.IO.FileStream(fileName, 
												System.IO.FileMode.Append, 
												System.IO.FileAccess.Write, 
												System.IO.FileShare.Write);
				csw = new System.IO.StreamWriter(cfs, enc);
				csw.Write("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");
				csw.Write("["+ System.Web.HttpRuntime.AppDomainAppVirtualPath +"]");
				csw.WriteLine(sLog);
				csw.Flush();
			}
			catch(Exception ex)
			{
				if(csw != null)
				{
					csw.WriteLine("["+ System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") +"]");
					csw.WriteLine(ex.ToString());
					csw.WriteLine(ex.Message);
					csw.WriteLine(ex.StackTrace);
					csw.Flush();
				}
			}
			finally
			{
				if(csw != null) csw.Close();
				if(cfs != null) cfs.Close();
			}
		}
	}
}

