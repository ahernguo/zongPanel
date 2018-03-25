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
		private Dictionary<string, string> mPath = new Dictionary<string, string>();
		/// <summary>面板設定資訊</summary>
		private PanelConfig mConfig;
		#endregion

		#region Properties
		/// <summary>取得主要資料夾路徑</summary>
		public string MainDirectory { get { return mPath["Main"]; } }
		#endregion

		#region Event Declare
		/// <summary>捷徑選項改變事件</summary>
		public event EventHandler<ShortcutEventArgs> OnShortcutChanged;
		/// <summary>顯示秒數改變事件</summary>
		public event EventHandler<BoolEventArgs> OnShowSecondChanged;
		/// <summary>數值改變事件</summary>
		public event EventHandler<FormatEventArgs> OnFormatChanged;
		/// <summary>字型改變事件</summary>
		public event EventHandler<FontEventArgs> OnFontChanging;
		/// <summary>顏色改變事件</summary>
		public event EventHandler<ColorEventArgs> OnColorChanging;
		/// <summary>效能面板停靠改變事件</summary>
		public event EventHandler<DockEventArgs> OnDockChanged;
		/// <summary>元件座標改變事件</summary>
		public event EventHandler<PointEventArgs> OnPositionChanged;
		#endregion

		#region Constructors
		public PanelCore() {

			/* 取得相關資料夾與檔案路徑 */
			var path = Environment.GetEnvironmentVariable("userprofile") + @"\Documents\zongPanel\";
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
			var dailyListener = new DailyXmlListener(mPath["Log"]) {
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
			var config = PanelConfig.LoadFromFile(mPath["Config"]);
			/* 如果檔案不存在，建立新檔 */
			if (config is null) {
				config = new PanelConfig();
				config.SaveToFile(mPath["Config"]);
			}
			return config;
		}

		/// <summary>發布設定檔當前的設定</summary>
		private void RaiseConfig() {
			/* Colors & Fonts */
			foreach (PanelComponent item in Enum.GetValues(typeof(PanelComponent))) {
				if (mConfig.GetColor(item, out var color)) {
					OnColorChanging?.Invoke(
						this,
						new ColorEventArgs(item, color)
					);
				}
				if (mConfig.GetFont(item, out var font)) {
					OnFontChanging?.Invoke(
						this,
						new FontEventArgs(item, font)
					);
				}
				if (mConfig.GetPosition(item, out var point)) {
					OnPositionChanged?.Invoke(
						this,
						new PointEventArgs(item, point)
					);
				}
				if (mConfig.GetFormat(item, out var format)) {
					OnFormatChanged?.Invoke(
						this,
						new FormatEventArgs(item, format)
					);
				}
			}
			/* Shortcut */
			OnShortcutChanged?.Invoke(
				this,
				new ShortcutEventArgs(mConfig.Shortcuts)
			);
			/* Seconds */
			OnShowSecondChanged?.Invoke(
				this,
				new BoolEventArgs(PanelComponent.Time, mConfig.ShowSecond)
			);
			/* Dock */
			OnDockChanged?.Invoke(
				this,
				new DockEventArgs(mConfig.UsageDocking)
			);
		}
		#endregion

		#region Show Forms
		/// <summary>顯示選項視窗</summary>
		public void ShowOptionForm() {
			/* 產生介面 */
			var opFrm = new Option();
			/* 添加事件處理 */
			opFrm.OnAlphaChanged += AlphaChanged;
			opFrm.OnColorChanging += ColorChanging;
			opFrm.OnDockChanged += DockChanged;
			opFrm.OnFontChanging += FontChanging;
			opFrm.OnFormatChanged += FormatChanged;
			opFrm.OnSaveTriggered += SaveConfigTriggered;
			opFrm.OnShortcutChanged += ShortcutChanged;
			opFrm.OnShowSecondChanged += ShowSecondChanged;
			/* 顯示介面並等待關閉 */
			var dlgRet = opFrm.ShowDialog();
			/* 重新載入設定檔 */
			mConfig.Dispose();
			mConfig = InitializeConfiguration();
			/* 重新顯示 */
			RaiseConfig();
		}

		/// <summary>透明程度更改事件處理，發布完整顏色至介面</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void AlphaChanged(object sender, AlphaEventArgs e) {
			var color = mConfig.ChangeAlpha(e.Component, e.Value);
			OnColorChanging?.Invoke(this, new ColorEventArgs(e.Component, color));
		}

		/// <summary>背景或前景顏色更改事件處理，發布完整顏色至介面</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void ColorChanging(object sender, ColorEventArgs e) {
			mConfig.ChangeColor(e.Component, e.Value);
			OnColorChanging?.Invoke(this, e);
		}

		/// <summary>停駐位置更改事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void DockChanged(object sender, DockEventArgs e) {
			mConfig.ChangeUsageDock(e.Dock);
			OnDockChanged?.Invoke(this, e);
		}

		/// <summary>字型更改事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void FontChanging(object sender, FontEventArgs e) {
			mConfig.ChangeFont(e.Component, e.Value);
			OnFontChanging?.Invoke(this, e);
		}

		/// <summary>格式內容更改事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void FormatChanged(object sender, FormatNumberEventArgs e) {
			Format format = null;
			if (e.Component == PanelComponent.Date) {
				format = mConfig.ChangeDateFormat(e.Value);
			} else {
				format = mConfig.ChangeWeekFormat(e.Value);
			}
			OnFormatChanged?.Invoke(this, new FormatEventArgs(e.Component, format));
		}

		/// <summary>使用者按下儲存設定檔事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void SaveConfigTriggered(object sender, BoolEventArgs e) {
			if (e.Value) {
				mConfig.SaveToFile(mPath["Config"]);
			}
		}

		/// <summary>捷徑選項更改事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void ShortcutChanged(object sender, ShortcutVisibleEventArgs e) {
			var shortcut = mConfig.ChangeShortcut(e.Shortcut, e.Visibility);
			OnShortcutChanged?.Invoke(this, new ShortcutEventArgs(shortcut));
		}

		/// <summary>顯示秒數選項更改事件處理</summary>
		/// <param name="sender"><see cref="Option"/> 視窗</param>
		/// <param name="e">事件參數</param>
		private void ShowSecondChanged(object sender, BoolEventArgs e) {
			mConfig.ChangeShowSecond(e.Value);
			OnShowSecondChanged?.Invoke(this, e);
		}
		#endregion
	}
}
