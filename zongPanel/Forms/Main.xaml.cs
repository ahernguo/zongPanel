using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using zongPanel.Forms;
using zongPanel.Library;

namespace zongPanel {

	/// <summary>主視窗</summary>
	public partial class MainWindow : Window {

		#region Fields
		/// <summary>控制核心</summary>
		private PanelCore mCore;
		/// <summary>選項視窗</summary>
		private Option mOptWind;
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<System.Windows.Controls.Image, ImageSource> mResxImgSrc = new Dictionary<System.Windows.Controls.Image, ImageSource>();
		/// <summary>儲存 <see cref="Image"/> 與其對應的 <see cref="Rect"/></summary>
		/// <remarks>其中，<see cref="Rect"/> 為 X(Top)、Y(Left)、Width、Height</remarks>
		private Dictionary<System.Windows.Controls.Image, Rect> mImgRect = new Dictionary<System.Windows.Controls.Image, Rect>();
		/// <summary>日期格式</summary>
		private Format mDateFormat;
		/// <summary>星期格式</summary>
		private Format mWeekFormat;
		/// <summary>時間格式</summary>
		private Format mTimeFormat;
		/// <summary>指出是否已經顯示過</summary>
		private bool mShown = false;
		/// <summary>UI 控制項集合</summary>
		private Dictionary<PanelComponent, Control> mCtrlDict;
		/// <summary>開始拖曳的座標</summary>
		private System.Windows.Point mDragPoint;
		/// <summary>控制項原始位置</summary>
		private Thickness mOriginMargin;
		/// <summary>時間顯示之計時器</summary>
		private Timer mTimeTmr;
		/// <summary>指出時間顯示之計時器是否活著</summary>
		private ManualResetEventSlim mTimeTmrAlive;
		/// <summary>指出時間顯示是否強制刷新，於 Format 更改後調用</summary>
		private ManualResetEventSlim mTimeRefresh;
		/// <summary>顯示時間時的原始比較時間點</summary>
		private DateTime mCompareTime;
		#endregion

		#region Constructors
		public MainWindow() {
			/* 初始化控制項 */
			InitializeComponent();
			mCtrlDict = new Dictionary<PanelComponent, Control> {
				{ PanelComponent.Background, this },
				{ PanelComponent.Date, lbDate },
				{ PanelComponent.Time, lbTime },
				{ PanelComponent.Week, lbWeek }
			};

			/* 初始化核心 */
			mCore = new PanelCore();
			mCore.ConfigChanged += ConfigChanged;

			/* 初始化計時器 */
			mTimeTmr = new Timer(TimeChange);
			mTimeTmrAlive = new ManualResetEventSlim();
			mTimeRefresh = new ManualResetEventSlim();
		}
		#endregion

		#region Time Timer Handlers
		/// <summary>顯示時間用之計時器時間到之處發</summary>
		/// <param name="state"></param>
		private void TimeChange(object state) {
			lbTime.TryAsyncInvoke(() => lbTime.Content = mTimeFormat.ToString(DateTime.Now));

			if (DateTime.Now.Date != mCompareTime.Date || mTimeRefresh.IsSet) {
				lbDate.TryInvoke(() => lbDate.Content = mDateFormat.ToString(DateTime.Now));
				lbWeek.TryInvoke(() => lbWeek.Content = mWeekFormat.ToString(DateTime.Now));
				mCompareTime = DateTime.Now;
				mTimeRefresh.Reset();
			}
		}
		#endregion

		#region PanelCore Event Handlers
		/// <summary>設定檔變更處理，已採非同步觸發此事件，且設定檔為複製體</summary>
		/// <param name="sender"><see cref="PanelConfig"/></param>
		/// <param name="e">設定檔參數</param>
		/// <remarks>
		/// 有嘗試過使用 TryAsyncInvoke
		/// 但因為 e.Config 在離開此括號後就刪了
		/// 導致後續的非同步跑到的時候已經變成 null 而跳 Exception，所以這邊全採同步啦!
		/// </remarks>
		private void ConfigChanged(object sender, ConfigEventArgs e) {
			/* 面板 */
			this.TryInvoke(
				() => {
					if (e.Config.GetBrush(PanelComponent.Background, out var bg)) {
						this.Background = bg;
					}
					this.SetRectangle(e.Config.WindowRectangle);
				}
			);
			/* 日期、星期、時間 */
			foreach (var kvp in mCtrlDict) {
				kvp.Value.TryInvoke(
					() => {
						if (e.Config.GetBrush(kvp.Key, out var fg)) {
							kvp.Value.Foreground = fg;
						}
						if (e.Config.GetFont(kvp.Key, out var font)) {
							kvp.Value.SetFont(font);
						}
						if (e.Config.GetMargin(kvp.Key, out var margin)) {
							kvp.Value.Margin = margin;
						}
					}
				);
			}
			/* 捷徑 */
			ChangeShortcut(e.Config.Shortcuts);
			/* 變數 */
			e.Config.GetFormat(PanelComponent.Date, out mDateFormat);
			e.Config.GetFormat(PanelComponent.Time, out mTimeFormat);
			e.Config.GetFormat(PanelComponent.Week, out mWeekFormat);
			/* 顯示當前時間 */
			mCompareTime = DateTime.Now;
			lbDate.TryInvoke(() => lbDate.Content = mDateFormat.ToString(mCompareTime));
			lbWeek.TryInvoke(() => lbWeek.Content = mWeekFormat.ToString(mCompareTime));
			lbTime.TryInvoke(() => lbTime.Content = mTimeFormat.ToString(mCompareTime));
			/* 開啟計時器 */
			if (!mTimeTmrAlive.IsSet) {
				mTimeTmr.Change(0, 1000);
				mTimeTmrAlive.Set();
			}
		}
		#endregion

		#region Option Window Event Handlers
		private void ColorChanging(object sender, ColorEventArgs e) {
			ChangeColor(e.Component, e.Value);
		}

		private void DockChanged(object sender, DockEventArgs e) {

		}

		private void FontChanging(object sender, FontEventArgs e) {
			ChangeFont(e.Component, e.Value);
		}

		private void FormatChanged(object sender, FormatEventArgs e) {
			switch (e.Component) {
				case PanelComponent.Time:
					mTimeFormat = e.Value;
					break;
				case PanelComponent.Date:
					mDateFormat = e.Value;
					break;
				case PanelComponent.Week:
					mWeekFormat = e.Value;
					break;
				default:
					break;
			}
			mTimeRefresh.Set();
		}

		private void ShortcutChanged(object sender, ShortcutEventArgs e) {
			ChangeShortcut(e.Shortcut);
		}

		private void LockChanged(object sender, BoolEventArgs e) {
			if (e.Value) {
				foreach (var kvp in mCtrlDict) {
					if (kvp.Key != PanelComponent.Background) {
						kvp.Value.MouseDown -= ReadyToDrag;
						kvp.Value.MouseUp -= DragRelease;
					}
				}
			} else {
				foreach (var kvp in mCtrlDict) {
					if (kvp.Key != PanelComponent.Background) {
						kvp.Value.MouseDown += ReadyToDrag;
						kvp.Value.MouseUp += DragRelease;
					}
				}
			}
		}

		private void DragRelease(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Released) {
				if (sender is Control ctrl) {
					ctrl.MouseMove -= Dragging;
					if (mOptWind != null) {
						var component = (PanelComponent) ctrl.Tag;
						mOptWind.MarginChanged(component, ctrl.Margin);
					}
				}
			}
		}

		private void Dragging(object sender, MouseEventArgs e) {
			if (sender is Control ctrl) {
				var curPt = e.GetPosition(this);
				ctrl.TryInvoke(
					() => {
						var margin = new Thickness(
							mOriginMargin.Left + (curPt.X - mDragPoint.X),
							mOriginMargin.Top + (curPt.Y - mDragPoint.Y),
							0, 0
						);
						ctrl.Margin = margin;
					}
				);
			}
		}

		private void ReadyToDrag(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				mDragPoint = e.GetPosition(this);
				if (sender is Control ctrl) {
					mOriginMargin = ctrl.Margin;
					ctrl.MouseMove += Dragging;
				}
			}
		}

		private void OptionClosing(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			/* 若還沒鎖定控制項，鎖定之 */
			if (!mOptWind.IsLocked) {
				LockChanged(null, new BoolEventArgs(PanelComponent.Background, true));
			}
			/* 重新載入設定檔 */
			mCore.ReloadConfig();
			/* 還原按鈕 */
			btnOption.TryInvoke(() => btnOption.Visibility = Visibility.Visible);
		}
		#endregion

		#region UI Changes
		private void ChangeColor(PanelComponent component, System.Drawing.Color color) {
			switch (component) {
				case PanelComponent.Background:
				case PanelComponent.Time:
				case PanelComponent.Date:
				case PanelComponent.Week:
					mCtrlDict[component].TryInvoke(
						() => {
							var brush = color.GetBrush();
							mCtrlDict[component].Foreground = brush;
						}
					);
					break;
				case PanelComponent.StatusBar:
					break;
				default:
					break;
			}
		}

		private void ChangeFont(PanelComponent component, Font font) {
			switch (component) {
				case PanelComponent.Time:
				case PanelComponent.Date:
				case PanelComponent.Week:
					mCtrlDict[component].SetFont(font);
					break;
				case PanelComponent.StatusBar:
					break;
				default:
					break;
			}
		}

		private void ChangePosition(PanelComponent component, PointF point) {
			switch (component) {
				case PanelComponent.Background:
				case PanelComponent.Time:
				case PanelComponent.Date:
				case PanelComponent.Week:
					mCtrlDict[component].TryInvoke(
						() => {
							var margin = new Thickness(point.X, point.Y, 0, 0);
							mCtrlDict[component].Margin = margin;
						}
					);
					break;
				default:
					break;
			}
		}

		private void ChangeShortcut(Shortcut shortcut) {
			spnShortcut.TryInvoke(
				() => {
					/* 先移除 StackPanel 內容 */
					spnShortcut.Children.Clear();
					/* 依照捷徑放回去 */
					if (shortcut.HasFlag(Shortcut.Note)) {
						spnShortcut.Children.Add(btnNote);
					}
					if (shortcut.HasFlag(Shortcut.Calculator)) {
						spnShortcut.Children.Add(btnCalc);
					}
				}
			);
		}
		#endregion

		#region UI Events
		/// <summary>關閉應用程式</summary>
		private void ExitClicked(object sender, RoutedEventArgs e) {
			this.TryAsyncInvoke(() => this.Close());
		}

		/// <summary>開啟計算機</summary>
		private void CalcClicked(object sender, RoutedEventArgs e) {
			try {
				/* 直接開啟，不需等待結束 */
				Process.Start("calc");
			} catch (Exception ex) {
				Trace.Write(ex);
			}
		}

		/// <summary>開啟選項視窗</summary>
		private void OptionClicked(object sender, RoutedEventArgs e) {
			/* 產生介面 */
			mOptWind = mCore.CreateOptionWindow();
			/* 添加事件處理 */
			mOptWind.OnColorChanging += ColorChanging;
			mOptWind.OnDockChanged += DockChanged;
			mOptWind.OnFontChanging += FontChanging;
			mOptWind.OnFormatChanged += FormatChanged;
			mOptWind.OnShortcutChanged += ShortcutChanged;
			mOptWind.OnLockChanged += LockChanged;
			mOptWind.OnWindowClosing += OptionClosing;
			/* 顯示介面 */
			mOptWind.Show();
			/* 隱藏 Option 按鈕 */
			btnOption.TryInvoke(() => btnOption.Visibility = Visibility.Collapsed);
		}

		protected override void OnContentRendered(EventArgs e) {
			base.OnContentRendered(e);

			if (!mShown) {
				/* 丟出設定檔，依照設定檔進行相關位置與樣式切換 */
				mCore.ThrowConfig();
				/* 切換旗標 */
				mShown = true;
			}
		}
		#endregion
	}
}
