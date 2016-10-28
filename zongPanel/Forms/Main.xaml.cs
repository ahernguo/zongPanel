using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace zongPanel {

	/// <summary>主視窗</summary>
	public partial class MainWindow : Window {

		#region Fields
		/// <summary>控制核心</summary>
		private PanelCore mCore;
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<Image, ImageSource> mResxImgSrc = new Dictionary<Image, ImageSource>();
		/// <summary>儲存 <see cref="Image"/> 與其對應的 <see cref="Rect"/></summary>
		/// <remarks>其中，<see cref="Rect"/> 為 X(Top)、Y(Left)、Width、Height</remarks>
		private Dictionary<Image, Rect> mImgRect = new Dictionary<Image, Rect>();
		#endregion

		#region Constructors
		public MainWindow() {
			/* 初始化控制項 */
			InitializeComponent();

			/* 初始化核心 */
			mCore = new PanelCore();

			/* 讀取資源檔圖片並轉換為影像來源 */
			InitializeImageSources();

			/* 抓取每個 Image 的 Bound */
			InitializeImageRectangle();
			/* 添加滑鼠移動事件，移到 Image Bound 上時顯示之 */
			this.MouseMove += MainWindow_MouseMove;
		}
		#endregion

		#region Methods
		/// <summary>載入資源檔圖片並轉換為 <see cref="ImageSource"/></summary>
		private void InitializeImageSources() {
			/* 建立 Resource 管理器，指向整個專案的 Resource */
			ResourceManager rm = new ResourceManager("zongPanel.Properties.Resources", Assembly.GetExecutingAssembly());

			/* 抓取 Images */
			var imgColl = grid.Children.Cast<UIElement>().Where(ui => ui is Image).Cast<Image>();
			/* 利用 Image.Tag 去抓取對應的 Resource 圖片並加到 Dictionary 裡 */
			foreach (var img in imgColl) {
				var res = img.Tag.ToString();                           //Tag，應為 Resource 名稱
				var pic = rm.GetObject(res) as System.Drawing.Bitmap;   //取得其 Resource(Bitmap)
				if (pic != null) {
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
				Thickness margin = kvp.Key.Margin;

				/* 取得 Top、Left 座標，暫時找不到對應的方法... */
				x = (float)((margin.Left > margin.Right) ? margin.Left : (this.Width - margin.Right - kvp.Key.Width));
				y = (float)((margin.Top > margin.Bottom) ? margin.Top : (this.Height - margin.Bottom - kvp.Key.Height));

				/* 建立 Rect */
				Rect rect = new Rect(x, y, kvp.Key.Width, kvp.Key.Height);

				/* 加到集合 */
				mImgRect.Add(kvp.Key, rect);
			}
		}
		#endregion

		#region UI Events
		/// <summary>滑鼠移動事件，判斷當前滑鼠位置是否在 <see cref="Image"/> 上，是則顯示之</summary>
		private void MainWindow_MouseMove(object sender, MouseEventArgs e) {
			/* 取得滑鼠座標，以 Window 為準 (Gird 應該也行) */
			var pos = e.GetPosition(this);
			/* 查看每個 Image，檢查滑鼠座標是否在其對應的 Rect 裡，是則顯示，反之隱藏 */
			foreach (var kvp in mImgRect) {
				/* 縮寫 */
				Image img = kvp.Key;
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
		private void imgExit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			this.TryAsyncInvoke(() => this.Close());
		}

		/// <summary>開啟計算機</summary>
		private void imgCalc_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			try {
				/* 直接開啟，不需等待結束 */
				Process.Start("calc");
			} catch (Exception ex) {
				Trace.Write(ex);
			}
		}
		#endregion

		private void imgOption_MouseDown(object sender, MouseButtonEventArgs e) {
			mCore.ShowOptionForm();
		}
	}
}
