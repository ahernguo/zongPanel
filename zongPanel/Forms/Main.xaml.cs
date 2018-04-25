using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using zongPanel.Library;
using zongPanel.Forms;
using System.Windows.Threading;

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

			/* 讀取資源檔圖片並轉換為影像來源 */
			InitializeImageSources();

			/* 抓取每個 Image 的 Bound */
			InitializeImageRectangle();
			/* 添加滑鼠移動事件，移到 Image Bound 上時顯示之 */
			this.MouseMove += MainWindow_MouseMove;
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
			imgOption.TryInvoke(() => imgOption.Visibility = Visibility.Visible);
		}
		#endregion

		#region Methods
		/// <summary>載入資源檔圖片並轉換為 <see cref="ImageSource"/></summary>
		private void InitializeImageSources() {
			/* 建立 Resource 管理器，指向整個專案的 Resource */
			var rm = new ResourceManager("zongPanel.Properties.Resources", Assembly.GetExecutingAssembly());

			/* 抓取 Images */
			var imgColl = grid.Children.Cast<UIElement>().Where(ui => ui is System.Windows.Controls.Image).Cast<System.Windows.Controls.Image>();
			/* 利用 Image.Tag 去抓取對應的 Resource 圖片並加到 Dictionary 裡 */
			foreach (var img in imgColl) {
				var res = img.Tag.ToString();                           //Tag，應為 Resource 名稱
				if (rm.GetObject(res) is System.Drawing.Bitmap pic) {   //取得其 Resource(Bitmap)
					var imgSrc = pic.GetImageSource();      //使用 Stream 方式建立 ImageSource
					img.Source = imgSrc;                    //設定圖片
					img.Visibility = Visibility.Hidden;     //隱藏
					mResxImgSrc.Add(img, imgSrc);           //加到 Dictionary 做備份
				}
			}
		}

		/// <summary>建立每個 <see cref="Image"/> 對應的 <see cref="Rect"/></summary>
		private void InitializeImageRectangle() {
			float x = 0, y = 0;
			foreach (var kvp in mResxImgSrc) {
				/* 縮寫 */
				var margin = kvp.Key.Margin;

				/* 取得 Top、Left 座標，暫時找不到對應的方法... */
				x = (float)((margin.Left > margin.Right) ? margin.Left : (this.Width - margin.Right - kvp.Key.Width));
				y = (float)((margin.Top > margin.Bottom) ? margin.Top : (this.Height - margin.Bottom - kvp.Key.Height));

				/* 建立 Rect */
				var rect = new Rect(x, y, kvp.Key.Width, kvp.Key.Height);

				/* 加到集合 */
				mImgRect.Add(kvp.Key, rect);
			}
		}

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
			var left = 10.0;
			if (shortcut.HasFlag(Shortcut.Note)) {
				imgNote.TryInvoke(
					() => {
						var margin = new Thickness(left, 10, 0, 0);
						imgNote.Margin = margin;
						left += (imgNote.Width + 5);
					}
				);
			}
			if (shortcut.HasFlag(Shortcut.Radio)) {
				imgRadio.TryInvoke(
					() => {
						var margin = new Thickness(left, 10, 0, 0);
						imgRadio.Margin = margin;
						left += (imgRadio.Width + 5);
					}
				);
			}
			if (shortcut.HasFlag(Shortcut.Calculator)) {
				imgCalc.TryInvoke(
					() => {
						var margin = new Thickness(left, 10, 0, 0);
						imgCalc.Margin = margin;
					}
				);
			}
		}
		#endregion

		#endregion

		#region UI Events
		/// <summary>滑鼠移動事件，判斷當前滑鼠位置是否在 <see cref="Image"/> 上，是則顯示之</summary>
		private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
			/* 取得滑鼠座標，以 Window 為準 (Gird 應該也行) */
			var pos = e.GetPosition(this);
			/* 查看每個 Image，檢查滑鼠座標是否在其對應的 Rect 裡，是則顯示，反之隱藏 */
			foreach (var kvp in mImgRect) {
				/* 縮寫 */
				var img = kvp.Key;
				/* 判斷是否滑鼠座標在範圍內 */
				if (kvp.Value.Contains(pos)) {  //滑鼠座標在其 Rect 裡
					img.TryInvoke(
						() => {
							if (img.Visibility != Visibility.Visible)
								img.Visibility = Visibility.Visible;    //顯示
						}
					);
				} else {                        //滑鼠座標不在其 Rect 裡
					img.TryInvoke(
						() => {
							if (img.Visibility != Visibility.Hidden)
								img.Visibility = Visibility.Hidden;     //隱藏
						}
					);
				}
			}
		}

		/// <summary>關閉應用程式</summary>
		private void ExitClicked(object sender, MouseButtonEventArgs e) {
			this.TryAsyncInvoke(() => this.Close());
		}

		/// <summary>開啟計算機</summary>
		private void CalcClicked(object sender, MouseButtonEventArgs e) {
			try {
				/* 直接開啟，不需等待結束 */
				Process.Start("calc");
			} catch (Exception ex) {
				Trace.Write(ex);
			}
		}

		/// <summary>開啟選項視窗</summary>
		private void OptionClicked(object sender, MouseButtonEventArgs e) {
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
			imgOption.TryInvoke(() => imgOption.Visibility = Visibility.Collapsed);
		}

		protected override void OnContentRendered(EventArgs e) {
			base.OnContentRendered(e);

			if (!mShown) {
				mCore.ThrowConfig();
				mShown = true;
			}
		}
		#endregion
	}
}
