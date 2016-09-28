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
	// �C������
	//--------------------------------------------------------------------------
	// MOD 2007.01.09 ���s�j���� ���O�o�͂̋��� 
	// MOD 2007.04.20 ���s�j���� ���O�o�͂̋��� 
	//--------------------------------------------------------------------------
	// MOD 2010.10.07 ���s�j���� ���O�o�b�t�@�̊g�� 
	//--------------------------------------------------------------------------
	//==========================================================================
	// 2010.11.17 KCL�j���q �G�R�[�����a�����Ƃ��č쐬���R�����g�폜 
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
			//CODEGEN: ���̌Ăяo���́AASP.NET Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
			InitializeComponent();

			if(sLogPath == null || sLogPath.Length == 0)
			{
				// ���O�o�̓p�X�̎擾				
				object obj = null;
				obj = Context.Application.Get("sLogPath");
				if(obj != null) sLogPath = (string)obj;
			}

			if(trLogOut == null || trLogOut.IsAlive == false)
			{
				// ���O�o�̓X���b�h���J�n
				trLogOut = new Thread(new ThreadStart(ThreadLogOut));
				trLogOut.IsBackground = true;
				trLogOut.Start();
			}
		}

		#region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
		
		//Web �T�[�r�X �f�U�C�i�ŕK�v�ł��B
		private IContainer components = null;
				
		/// <summary>
		/// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
		/// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
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
		 * ���O�o�͗p�o�b�t�@�ւ̏�����
		 * �����F���O�o�͕�����
		 * �ߒl�F����
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
		 * ���O�o�͗p�o�b�t�@�ւ̏�����
		 * �����F���O�o�͕�����
		 * �ߒl�F����
		 *
		 *********************************************************************/
		private static int LogOutSub(string sLog)
		{

			try
			{
				//�i�P������Ă���ꍇ�j
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
		 * ���O�o�̓X�e�[�^�X�ݒ�
		 * �����F�Ȃ�
		 * �ߒl�F�Ȃ�
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
		 * ���O�o�͗p�o�b�t�@����̎�o��
		 * �����F����
		 * �ߒl�F���O�o�͕�����
		 *
		 *********************************************************************/
		private static string GetLog()
		{
			if(bGetLog)
			{
				bGetLog = false;
				return "[GetLog]�G���[�F���̃v���Z�X�Ŏg�p��";
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
		 * ���O�o�̓X���b�h
		 * �����F����
		 * �ߒl�F����
		 *
		 *********************************************************************/
		private static void ThreadLogOut()
		{
			string sLog;
			while(true)
			{
				// ���O�o�b�t�@���f�[�^���擾����
				sLog = GetLog();
				if(sLog == null)
				{
					Thread.Sleep(500); // 0.5�b
					continue;
				}

				System.IO.FileStream   cfs = null;
				System.IO.StreamWriter csw = null;
				string sFileName = sLogPath
								 + System.DateTime.Now.ToString("MMdd")
								 + "_is2EXLogOut.log";

				try
				{
					// �t�@�C���I�[�v��
					cfs = new System.IO.FileStream(sFileName, 
													System.IO.FileMode.Append, 
													System.IO.FileAccess.Write, 
													System.IO.FileShare.Write);
					csw = new System.IO.StreamWriter(cfs, enc);

					// ���O�o�b�t�@�ɂ��܂��Ă�����̂�S�ďo�͂���
					uint uiCnt = 0;
					while(sLog != null){
						uiCnt++;
						//�������[�v��h��
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
					// �t�@�C���N���[�Y
					if(csw != null) csw.Close();
					csw = null;
					if(cfs != null) cfs.Close();
					cfs = null;
				}
			}
		}
	}
}
