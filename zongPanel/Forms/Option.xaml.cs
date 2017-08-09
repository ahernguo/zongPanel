using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using zongPanel.Library;

namespace zongPanel.Forms {
	/// <summary>
	/// Option.xaml 的互動邏輯
	/// </summary>
	public partial class Option : Window {

		#region Definitions
		/// <summary>顯示字體切換按鈕之 <see cref="Control.FontSize"/></summary>
		private static readonly float BTN_FONT_SIZE = 14;
		#endregion

		#region Fields
		/// <summary>由 <see cref="Window.Owner"/> 所帶入的面板設定資訊</summary>
		private PanelConfig rConfig;
		/// <summary>由 <see cref="Window.Owner"/> 所帶入的面板設定資訊複製品</summary>
		private PanelConfig mCopiedConfig;
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<string, ImageSource> mResxImgSrc = new Dictionary<string, ImageSource>();
		/// <summary>是否有按下儲存，需覆蓋原始設定資訊</summary>
		private bool mSaved = false;
		#endregion

		#region Constructors
		/// <summary>建立選項視窗，並帶入當前的面板設定資訊</summary>
		/// <param name="config">目前的面板設定資訊，請帶入原始 instance，會於內部進行複製與修改</param>
		public Option(PanelConfig config) {
			InitializeComponent();

			/* 暫存至全域變數 */
			rConfig = config;
			mCopiedConfig = rConfig.Clone();

			/* 讀取資源檔圖片並轉換為影像來源 */
			InitializeImageSources();
		}
		#endregion

		#region Methods
		/// <summary>載入資源檔圖片並轉換為 <see cref="ImageSource"/></summary>
		private void InitializeImageSources() {
			/* 建立 Resource 管理器，指向整個專案的 Resource */
			var rm = new ResourceManager("zongPanel.Properties.Resources", Assembly.GetExecutingAssembly());

			var pic = rm.GetObject("save_org") as System.Drawing.Bitmap;
			mResxImgSrc.Add("SaveOrg", pic.GetImageSource());
			pic = rm.GetObject("save") as System.Drawing.Bitmap;
			mResxImgSrc.Add("SaveHover", pic.GetImageSource());
			pic = rm.GetObject("close_org") as System.Drawing.Bitmap;
			mResxImgSrc.Add("CloseOrg", pic.GetImageSource());
			pic = rm.GetObject("close") as System.Drawing.Bitmap;
			mResxImgSrc.Add("CloseHover", pic.GetImageSource());

			imgExit.Source = mResxImgSrc["CloseOrg"];
			imgSave.Source = mResxImgSrc["SaveOrg"];
		}

		/// <summary>將 <see cref="System.Drawing.Color.A"/> 轉換為 10 級距的 Index，供 <see cref="ComboBox.SelectedIndex"/> 使用</summary>
		/// <param name="color">欲判斷的顏色</param>
		/// <returns>10 級距的 Index</returns>
		private int CalculateAlphaToSelectedIndex(System.Drawing.Color color) {
			var alpha = (color.A / 255) * 100;
			var quotient = alpha / 10;
			if ((alpha % 10) > 5) quotient++;

			return quotient - 1;
		}

		/// <summary>將 <see cref="PanelConfig"/> 之設定套用到當前的控制項</summary>
		/// <param name="config">欲套用的設定檔</param>
		private void LoadConfig(PanelConfig config) {
			chkShowSec.IsChecked = config.ShowSecond;

			chkCalc.IsChecked = config.Shortcuts.HasFlag(Shortcut.CALCULATOR);
			chkNote.IsChecked = config.Shortcuts.HasFlag(Shortcut.NOTE);
			chkRadio.IsChecked = config.Shortcuts.HasFlag(Shortcut.RADIO);

			cbDateAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.DateForeground);
			cbNoteBgAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.NoteBackground);
			cbNoteCntAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.NoteContentForeground);
			cbNoteTitAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.NoteTitleForeground);
			cbPnBgAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.PanelBackground);
			cbTimeAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.TimeForeground);
			cbUsgFontAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.UsageForeground);
			cbWeekAlpha.SelectedIndex = CalculateAlphaToSelectedIndex(config.WeekForeground);

			btnDateFont.SetFont(config.DateFont, BTN_FONT_SIZE);
			btnNoteCntFont.SetFont(config.NoteContentFont, BTN_FONT_SIZE);
			btnNoteTitFont.SetFont(config.NoteTitleFont, BTN_FONT_SIZE);
			btnTimeFont.SetFont(config.TimeFont, BTN_FONT_SIZE);
			btnUsgFont.SetFont(config.UsageFont, BTN_FONT_SIZE);
			btnWeekFont.SetFont(config.WeekFont, BTN_FONT_SIZE);
		}
		#endregion

		#region UI Events
		private void ImageMouseEnter(object sender, MouseEventArgs e) {
			var img = sender as Image;
			var resName = $"{img.Tag.ToString()}Hover";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ImageMouseLeave(object sender, MouseEventArgs e) {
			var img = sender as Image;
			var resName = $"{img.Tag.ToString()}Org";
			img.TryInvoke(() => img.Source = mResxImgSrc[resName]);
		}

		private void ShortcutChanged(object sender, RoutedEventArgs e) {
			var chkBox = sender as CheckBox;
			var tag = string.Empty;
			var chk = false;
			chkBox.TryInvoke(
				() => {
					tag = chkBox.Tag.ToString();
					chk = chkBox.IsChecked.Value;
				}
			);
			mCopiedConfig.ChangeShortcut(tag, chk);
		}

		private void ShowSecondChanged(object sender, RoutedEventArgs e) {
			var chkBox = sender as CheckBox;
			var chk = chkBox.TryInvoke(() => chkBox.IsChecked.Value);
			mCopiedConfig.ChangeShowSecond(chk);
		}

		private void DateWeekFormatChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var tag = string.Empty;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = comboBox.Tag.ToString();
					idx = comboBox.SelectedIndex;
				}
			);
			if ("Week".Equals(tag)) mCopiedConfig.ChangeWeekFormat(idx);
			else if ("Date".Equals(tag)) mCopiedConfig.ChangeDateFormat(idx);
		}

		private void FontClicked(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			var tag = btn.TryInvoke(() => btn.Tag.ToString());
			var newFont = mCopiedConfig.ChangeFont(tag);
			if (newFont != null) btn.TryInvoke(() => btn.SetFont(newFont, BTN_FONT_SIZE));
		}

		private void ColorChanged(object sender, MouseButtonEventArgs e) {
			var rect = sender as Rectangle;
			rect.TryInvoke(
				() => {
					string tag = rect.Tag.ToString();
					Brush br = mCopiedConfig.ChangeColor(tag, rect.Fill.GetColor()).GetBrush();
					rect.Fill = br;
				}
			);
		}

		private void AlphaChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var tag = string.Empty;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = comboBox.Tag.ToString();
					idx = comboBox.SelectedIndex;
				}
			);
			mCopiedConfig.ChangeAlpha(tag, idx);
		}

		private void UsageDockChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var idx = comboBox.TryInvoke(() => comboBox.SelectedIndex);
			mCopiedConfig.ChangeUsageDock(idx);
		}

		private void SaveClicked(object sender, MouseButtonEventArgs e) {
			mSaved = true;
			rConfig = mCopiedConfig.Clone();
		}

		private void ExitClicked(object sender, MouseButtonEventArgs e) {
			DialogResult = mSaved;
			this.TryInvoke(() => this.Close());
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			chkNote.Checked += ShortcutChanged;
			chkRadio.Checked += ShortcutChanged;
			chkCalc.Checked += ShortcutChanged;

			cbUsgDock.SelectionChanged += UsageDockChanged;

			cbDateAlpha.SelectionChanged += AlphaChanged;
			cbNoteBgAlpha.SelectionChanged += AlphaChanged;
			cbNoteCntAlpha.SelectionChanged += AlphaChanged;
			cbNoteTitAlpha.SelectionChanged += AlphaChanged;
			cbPnBgAlpha.SelectionChanged += AlphaChanged;
			cbTimeAlpha.SelectionChanged += AlphaChanged;
			cbUsgFontAlpha.SelectionChanged += AlphaChanged;
			cbWeekAlpha.SelectionChanged += AlphaChanged;

			rectDateColor.MouseLeftButtonUp += ColorChanged;
			rectNoteBg.MouseLeftButtonUp += ColorChanged;
			rectNoteCntColor.MouseLeftButtonUp += ColorChanged;
			rectNoteTitColor.MouseLeftButtonUp += ColorChanged;
			rectPnBg.MouseLeftButtonUp += ColorChanged;
			rectTimeColor.MouseLeftButtonUp += ColorChanged;
			rectUsgFont.MouseLeftButtonUp += ColorChanged;
			rectWeekColor.MouseLeftButtonUp += ColorChanged;

			btnDateFont.Click += FontClicked;
			btnNoteCntFont.Click += FontClicked;
			btnNoteTitFont.Click += FontClicked;
			btnTimeFont.Click += FontClicked;
			btnUsgFont.Click += FontClicked;
			btnWeekFont.Click += FontClicked;

			cbDateFmt.SelectionChanged += DateWeekFormatChanged;
			cbWeekFmt.SelectionChanged += DateWeekFormatChanged;

			chkShowSec.Checked += ShowSecondChanged;
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			var pos = e.GetPosition(this);
			var posAllow = (pos.X > 0) && (pos.Y > 0) && (pos.X < imgSave.Margin.Left) && (pos.Y < tabControl.Margin.Top);
			if ((e.ChangedButton == MouseButton.Left) && posAllow)
				this.DragMove();
		}

		#endregion
	}
}
