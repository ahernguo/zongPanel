using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using zongPanel.Forms;
using zongPanel.Library;

namespace zongPanel {
	public class PanelCore {

		#region Fields
		/// <summary>上次更新日期之時間，供檢查是否需要更新日期</summary>
		private DateTime mTime = DateTime.Now;
		/// <summary>儲存相關路徑</summary>
		private Dictionary<Paths, string> mPath = new Dictionary<Paths, string>();
		/// <summary>面板設定資訊</summary>
		private PanelConfig mConfig;
		#endregion

		#region Events
		/// <summary>設定檔變更事件</summary>
		public event EventHandler<ConfigEventArgs> ConfigChanged;

		/// <summary>發布設定檔變更事件，直接抓取全域變數發布</summary>
		private void RaiseConfChg() {
			if (ConfigChanged != null) {
				var config = mConfig.Clone();
				ConfigChanged?.BeginInvoke(
					this,
					new ConfigEventArgs(config),
					obj => {
						config.Dispose();
						config = null;
					},
					null
				);
			}
		}
		#endregion

		#region Constructors
		public PanelCore() {

			/* 取得相關資料夾與檔案路徑 */
			var path = Environment.GetEnvironmentVariable("userprofile") + @"\Documents\zongPanel\";
			mPath.Add(Paths.Main, path);
			path = mPath[Paths.Main] + "Config.xml";
			mPath.Add(Paths.Config, path);
			path = mPath[Paths.Main] + @"Notes\";
			mPath.Add(Paths.Note, path);
			path = mPath[Paths.Main] + @"Logs\";
			mPath.Add(Paths.Log, path);

			/* 檢查並建立資料夾 */
			EnsureDirectory();

			/* 設定監聽器 */
			InitializeTraceListener();

			/* 載入設定檔 */
			mConfig = InitializeConfiguration();
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
			var dailyListener = new DailyXmlListener(mPath[Paths.Log]) {
				Filter = new EventTypeFilter(SourceLevels.All),
				TraceOutputOptions = TraceOptions.Callstack | TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
			};
			/* 加入監聽器 */
			Trace.Listeners.Add(dailyListener);
			/* 設定為自動輸出 */
			Trace.AutoFlush = true;
		}

		/// <summary>載入設定檔，如未建立設定檔則新建之</summary>
		/// <returns>載入或新建的設定檔</returns>
		private PanelConfig InitializeConfiguration() {
			/* 載入檔案 */
			var config = PanelConfig.LoadFromFile(mPath[Paths.Config]);
			/* 如果檔案不存在，建立新檔 */
			if (config is null) {
				config = new PanelConfig();
				config.SaveToFile(mPath[Paths.Config]);
			}
			return config;
		}

		#endregion

		#region Windows
		/// <summary>取得 <see cref="Option"/> 視窗</summary>
		/// <returns>新的視窗，並具有可更改的設定檔</returns>
		public Option CreateOptionWindow() {
			var config = mConfig.Clone();
			return new Option(config);
		}
		#endregion

		#region Public Operations
		/// <summary>重新載入設定檔</summary>
		public void ReloadConfig() {
			/* 先把當前的清除 */
			if (mConfig != null) {
				mConfig.Dispose();
			}
			/* 重新載入 */
			mConfig = InitializeConfiguration();
			/* 重新發布設定檔 */
			RaiseConfChg();
		}

		/// <summary>發布當前載入的設定檔</summary>
		public void ThrowConfig() {
			RaiseConfChg();
		}
		#endregion
	}
}
