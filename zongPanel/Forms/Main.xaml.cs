using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using zongPanel.Library;

namespace zongPanel {

	/// <summary>主視窗</summary>
	public partial class MainWindow : Window {

		#region Fields
		/// <summary>控制核心</summary>
		private PanelCore mCore;
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
			if (e.Component == PanelComponent.Date) {
				mDateFormat = e.Value;
			} else {
				mWeekFormat = e.Value;
			}
		}

		private void ShortcutChanged(object sender, ShortcutEventArgs e) {
			ChangeShortcut(e.Shortcut);
		}

		private void ShowSecondChanged(object sender, BoolEventArgs e) {
			if (e.Value) {
				mTimeFormat = new Format("HH:mm:ss", "en-US");
			} else {
				mTimeFormat = new Format("HH:mm", "en-US");
			}
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
				var ctrl = sender as Control;
				ctrl.MouseMove -= Dragging;
			}
		}

		private void Dragging(object sender, MouseEventArgs e) {
			var ctrl = sender as Control;
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

		private void ReadyToDrag(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				mDragPoint = e.GetPosition(this);
				var ctrl = sender as Control;
				mOriginMargin = ctrl.Margin;
				ctrl.MouseMove += Dragging;
			}
		}

		private void OptionClosing(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
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
			var opFrm = mCore.CreateOptionWindow();
			/* 添加事件處理 */
			opFrm.OnColorChanging += ColorChanging;
			opFrm.OnDockChanged += DockChanged;
			opFrm.OnFontChanging += FontChanging;
			opFrm.OnFormatChanged += FormatChanged;
			opFrm.OnShortcutChanged += ShortcutChanged;
			opFrm.OnShowSecondChanged += ShowSecondChanged;
			opFrm.OnLockChanged += LockChanged;
			opFrm.OnWindowClosing += OptionClosing;
			/* 顯示介面 */
			opFrm.Show();
			/* 隱藏 Option 按鈕 */
			btnOption.TryInvoke(() => btnOption.Visibility = Visibility.Collapsed);
		}

		protected override void OnContentRendered(EventArgs e) {
			base.OnContentRendered(e);

			if (!mShown) {
				mCore.ThrowConfig();
				mShown = true;
			}
		}
		#endregion

		private void imgOption_Click(object sender, RoutedEventArgs e) {
		}
	}
}
