using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;

namespace is2EXlogout
{
	/// <summary>
	/// [is2EXlogout]
	/// </summary>
	//--------------------------------------------------------------------------
	// 修正履歴
	//--------------------------------------------------------------------------
	// MOD 2007.01.09 東都）高木 ログ出力の強化 
	// MOD 2007.04.20 東都）高木 ログ出力の強化 
	//--------------------------------------------------------------------------
	// MOD 2010.10.07 東都）高木 ログバッファの拡張 
	//--------------------------------------------------------------------------
	//==========================================================================
	// 2010.11.17 KCL）小倉 エコー金属殿向けとして作成＆コメント削除 
	//==========================================================================
	[System.Web.Services.WebService(
		 Namespace="http://Walkthrough/XmlWebServices/",
		 Description="is2EXlogout")]

	public class Service1 : System.Web.Services.WebService
	{
		private static string sLogPath = "";

		private static Thread trLogOut = null;
		private static uint uiLogOut = 0;
		private static uint uiBuff   = 0;
		private static bool bSetLog = false;
		private static bool bGetLog = false;
		private static string[] sBuff = new string[16 * 1024];
		private static Encoding enc = Encoding.GetEncoding("shift-jis");

		public Service1()
		{
			//CODEGEN: この呼び出しは、ASP.NET Web サービス デザイナで必要です。
			InitializeComponent();

			if(sLogPath == null || sLogPath.Length == 0)
			{
				// ログ出力パスの取得				
				object obj = null;
				obj = Context.Application.Get("sLogPath");
				if(obj != null) sLogPath = (string)obj;
			}

			if(trLogOut == null || trLogOut.IsAlive == false)
			{
				// ログ出力スレッドを開始
				trLogOut = new Thread(new ThreadStart(ThreadLogOut));
				trLogOut.IsBackground = true;
				trLogOut.Start();
			}
		}

		#region コンポーネント デザイナで生成されたコード 
		
		//Web サービス デザイナで必要です。
		private IContainer components = null;
				
		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// 使用されているリソースに後処理を実行します。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/*********************************************************************
		 * ログ出力用バッファへの書込み
		 * 引数：ログ出力文字列
		 * 戻値：無し
		 *
		 *********************************************************************/
		[WebMethod]
		public int LogOut(string sLog)
		{
			if(sLog == null || sLog.Length == 0) return 0;
			if(SetLogState()) return -1;
			int iRet = LogOutSub(sLog);

			ClearLogState();
			return iRet;
		}

		/*********************************************************************
		 * ログ出力用バッファへの書込み
		 * 引数：ログ出力文字列
		 * 戻値：無し
		 *
		 *********************************************************************/
		private static int LogOutSub(string sLog)
		{

			try
			{
				//（１周回っている場合）
				if(sBuff[uiBuff] != null) return -2;
				sBuff[uiBuff++] = sLog;
				if(uiBuff >= sBuff.Length) uiBuff = 0;
			}
			catch
			{
			}
			finally
			{
				ClearLogState();
			}

			return 0;
		}

		/*********************************************************************
		 * ログ出力ステータス設定
		 * 引数：なし
		 * 戻値：なし
		 *
		 *********************************************************************/
		private static bool SetLogState()
		{
			if(bSetLog) return true;
			bSetLog = true;
			return false;
		}
		private static void ClearLogState()
		{
			bSetLog = false;
		}
		/*********************************************************************
		 * ログ出力用バッファからの取出し
		 * 引数：無し
		 * 戻値：ログ出力文字列
		 *
		 *********************************************************************/
		private static string GetLog()
		{
			if(bGetLog)
			{
				bGetLog = false;
				return "[GetLog]エラー：他のプロセスで使用中";
			}

			bGetLog = true;
			string sRet;
			try
			{
				sRet = sBuff[uiLogOut];
				if(sRet != null)
				{
					sBuff[uiLogOut] = null;
					uiLogOut++;
					if(uiLogOut >= sBuff.Length) uiLogOut = 0;
				}
			}
			finally
			{
				bGetLog = false;
			}
			return sRet;
		}

		/*********************************************************************
		 * ログ出力スレッド
		 * 引数：無し
		 * 戻値：無し
		 *
		 *********************************************************************/
		private static void ThreadLogOut()
		{
			string sLog;
			while(true)
			{
				// ログバッファよりデータを取得する
				sLog = GetLog();
				if(sLog == null)
				{
					Thread.Sleep(500); // 0.5秒
					continue;
				}

				System.IO.FileStream   cfs = null;
				System.IO.StreamWriter csw = null;
				string sFileName = sLogPath
								 + System.DateTime.Now.ToString("MMdd")
								 + "_is2EXLogOut.log";

				try
				{
					// ファイルオープン
					cfs = new System.IO.FileStream(sFileName, 
													System.IO.FileMode.Append, 
													System.IO.FileAccess.Write, 
													System.IO.FileShare.Write);
					csw = new System.IO.StreamWriter(cfs, enc);

					// ログバッファにたまっているものを全て出力する
					uint uiCnt = 0;
					while(sLog != null){
						uiCnt++;
						//無限ループを防ぐ
						if(uiCnt > sBuff.Length){
							break;
						}
						csw.WriteLine(sLog);
						csw.Flush();
						sLog = GetLog();
					}
				}
				catch
				{
				}
				finally
				{
					// ファイルクローズ
					if(csw != null) csw.Close();
					csw = null;
					if(cfs != null) cfs.Close();
					cfs = null;
				}
			}
		}
	}
}
