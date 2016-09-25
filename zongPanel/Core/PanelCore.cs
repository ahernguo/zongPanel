using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using zongPanel.Library;

namespace zongPanel {
	public class PanelCore {

		#region Fields
		/// <summary>上次更新日期之時間，供檢查是否需要更新日期</summary>
		private DateTime mTime = DateTime.Now;
		/// <summary>儲存相關路徑</summary>
		private Dictionary<string, string> mPath = new Dictionary<string, string>();
		/// <summary>面板設定資訊</summary>
		private PanelConfig mConfig;
		#endregion

		#region Properties
		/// <summary>取得主要資料夾路徑</summary>
		public string MainDirectory { get { return mPath["Main"]; } }
		#endregion

		#region Event Declare
		/// <summary>時間變更事件，發報已套用格式之文字</summary>
		public event TimeEventArgs TimeChanged;
		/// <summary>日期變更事件，發報已套用格式之日期與星期</summary>
		public event DateEventArgs DateChanged;
		#endregion

		#region Event Raiser
		protected virtual void RaiseTimeChanged(DateTime time) {
			if (TimeChanged != null) {
				string timeStr = time.ToString("HH:mm");
				TimeChanged.BeginInvoke(timeStr, null, null);
			}
		}

		protected virtual void RaiseDateChanged(DateTime date) {
			if (DateChanged != null) {
				string dateStr = date.ToString("dd/MM/yyyy");

				DateChanged.BeginInvoke(dateStr, "", null, null);
			}
		}
		#endregion

		#region Constructors
		public PanelCore() {

			/* 取得相關資料夾與檔案路徑 */
			string path = Environment.GetEnvironmentVariable("userprofile") + @"\Documents\zongPanel\";
			mPath.Add("Main", path);
			path = mPath["Main"] + "Config.xml";
			mPath.Add("Config", path);
			path = mPath["Main"] + @"Notes\";
			mPath.Add("Note", path);
			path = mPath["Main"] + @"Logs\";
			mPath.Add("Log", path);

			/* 檢查並建立資料夾 */
			EnsureDirectory();

			/* 設定監聽器 */
			InitializeTraceListener();

			/* 載入設定檔 */
			InitializeConfiguration();
		}
		#endregion

		#region Private Utilities
		/// <summary>檢查並建立相關資料夾</summary>
		private void EnsureDirectory() {
			foreach (var kvp in mPath) {
				string dir = Path.GetDirectoryName(kvp.Value);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			}
		}

		/// <summary>設定 <see cref="Trace.Listeners"/>。以 <see cref="DailyXmlListener"/> 為準</summary>
		private void InitializeTraceListener() {
			/* 清除預設的監聽器 */
			Trace.Listeners.Clear();
			/* 建立 XML 紀錄器 */
			DailyXmlListener dailyListener = new DailyXmlListener(mPath["Log"]) {
				Filter = new EventTypeFilter(SourceLevels.All),
				TraceOutputOptions = TraceOptions.Callstack | TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
			};
			/* 加入監聽器 */
			Trace.Listeners.Add(dailyListener);
			/* 設定為自動輸出 */
			Trace.AutoFlush = true;
		}

		/// <summary>載入設定檔，如未建立設定檔則新建之</summary>
		private void InitializeConfiguration() {
			if (File.Exists(mPath["Config"])) {
				using (XmlReader xr = XmlReader.Create(mPath["Config"])) {
					DataContractSerializer contSer = new DataContractSerializer(typeof(PanelConfig));
					mConfig = contSer.ReadObject(xr) as PanelConfig;
				}

			} else {
				mConfig = new PanelConfig();
				SaveConfig();
			}
		}

		/// <summary>以 SOAP 序列化的方式儲存 <see cref="PanelConfig"/></summary>
		private void SaveConfig() {
			XmlWriterSettings setting = new XmlWriterSettings() { Indent = true, IndentChars = "\t" };
			using (XmlWriter xw = XmlWriter.Create(mPath["Config"], setting)) {
				DataContractSerializer contSer = new DataContractSerializer(typeof(PanelConfig));
				contSer.WriteObject(xw, mConfig);
			}
		}
		#endregion
	}
}
