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
		/// <summary>儲存資源檔圖片與其對應的 <see cref="ImageSource"/></summary>
		private Dictionary<string, ImageSource> mResxImgSrc = new Dictionary<string, ImageSource>();
		/// <summary>是否有按下儲存，需覆蓋原始設定資訊</summary>
		private bool mSaved = false;
		#endregion

		#region Event Declarations
		/// <summary>捷徑選項改變事件</summary>
		public event EventHandler<ShortcutVisibleEventArgs> OnShortcutChanged;
		/// <summary>顯示秒數改變事件</summary>
		public event EventHandler<BoolEventArgs> OnShowSecondChanged;
		/// <summary>數值改變事件</summary>
		public event EventHandler<FormatNumberEventArgs> OnFormatChanged;
		/// <summary>字型改變事件</summary>
		public event EventHandler<FontEventArgs> OnFontChanging;
		/// <summary>顏色改變事件</summary>
		public event EventHandler<ColorEventArgs> OnColorChanging;
		/// <summary>透明度改變事件</summary>
		public event EventHandler<AlphaEventArgs> OnAlphaChanged;
		/// <summary>效能面板停靠改變事件</summary>
		public event EventHandler<DockEventArgs> OnDockChanged;
		/// <summary>儲存設定事件</summary>
		public event EventHandler<BoolEventArgs> OnSaveTriggered;
		#endregion

		#region Constructors
		/// <summary>建立選項視窗，並帶入當前的面板設定資訊</summary>
		/// <param name="config">目前的面板設定資訊，請帶入複製或可更改的 instance</param>
		public Option() {
			InitializeComponent();

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

			chkCalc.IsChecked = config.Shortcuts.HasFlag(Shortcut.Calculator);
			chkNote.IsChecked = config.Shortcuts.HasFlag(Shortcut.Note);
			chkRadio.IsChecked = config.Shortcuts.HasFlag(Shortcut.Radio);

			config.GetAlpha(PanelComponent.Date, out var alpha);
			cbDateAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Note, out alpha);
			cbNoteBgAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.NoteContent, out alpha);
			cbNoteCntAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.NoteTitle, out alpha);
			cbNoteTitAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Background, out alpha);
			cbPnBgAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Time, out alpha);
			cbTimeAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.StatusBar, out alpha);
			cbUsgFontAlpha.SelectedIndex = alpha;
			config.GetAlpha(PanelComponent.Week, out alpha);
			cbWeekAlpha.SelectedIndex = alpha;

			config.GetBrush(PanelComponent.Date, out var brush);
			rectDateColor.Fill = brush;
			config.GetBrush(PanelComponent.Note, out brush);
			rectNoteBg.Fill = brush;
			config.GetBrush(PanelComponent.NoteContent, out brush);
			rectNoteCntColor.Fill = brush;
			config.GetBrush(PanelComponent.NoteTitle, out brush);
			rectNoteTitColor.Fill = brush;
			config.GetBrush(PanelComponent.Background, out brush);
			rectPnBg.Fill = brush;
			config.GetBrush(PanelComponent.Time, out brush);
			rectTimeColor.Fill = brush;
			config.GetBrush(PanelComponent.StatusBar, out brush);
			rectUsgFont.Fill = brush;
			config.GetBrush(PanelComponent.Week, out brush);
			rectWeekColor.Fill = brush;

			config.GetFont(PanelComponent.Date, out var font);
			btnDateFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.NoteContent, out font);
			btnNoteCntFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.NoteTitle, out font);
			btnNoteTitFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.Time, out font);
			btnTimeFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.StatusBar, out font);
			btnUsgFont.SetFont(font, BTN_FONT_SIZE);
			config.GetFont(PanelComponent.Week, out font);
			btnWeekFont.SetFont(font, BTN_FONT_SIZE);
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
			var tag = Shortcut.Calculator;
			var chk = false;
			chkBox.TryInvoke(
				() => {
					tag = (Shortcut)chkBox.Tag;
					chk = chkBox.IsChecked.Value;
				}
			);
			OnShortcutChanged?.BeginInvoke(
				this,
				new ShortcutVisibleEventArgs(tag, chk),
				null, null
			);
		}

		private void ShowSecondChanged(object sender, RoutedEventArgs e) {
			var chkBox = sender as CheckBox;
			var chk = chkBox.TryInvoke(() => chkBox.IsChecked.Value);
			OnShowSecondChanged?.BeginInvoke(
				this,
				new BoolEventArgs(PanelComponent.Time, chk),
				null, null
			);
		}

		private void DateWeekFormatChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var tag = PanelComponent.Week;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = (PanelComponent)comboBox.Tag;
					idx = comboBox.SelectedIndex;
				}
			);
			if (tag == PanelComponent.Week) {
				OnFormatChanged?.BeginInvoke(
					this,
					new FormatNumberEventArgs(tag, idx),
					null, null
				);
			}
		}

		private void FontClicked(object sender, RoutedEventArgs e) {
			var btn = sender as Button;
			var tag = btn.TryInvoke(() => (PanelComponent)btn.Tag);

			using (var diag = new System.Windows.Forms.FontDialog()) {
				diag.ShowColor = false;
				diag.ShowEffects = true;
				diag.Font = btn.GetFont();
				diag.FixedPitchOnly = false;
				if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					OnFontChanging?.Invoke( //跑同步，避免 Font 被刪掉
						this,
						new FontEventArgs(tag, diag.Font)
					);
					btn.SetFont(diag.Font, BTN_FONT_SIZE);
				}
			}
		}

		private void ColorChanged(object sender, MouseButtonEventArgs e) {
			var rect = sender as Rectangle;
			var tag = rect.TryInvoke(() => (PanelComponent)rect.Tag);

			using (var diag = new System.Windows.Forms.ColorDialog()) {
				diag.AllowFullOpen = true;
				diag.AnyColor = true;
				diag.FullOpen = true;
				diag.SolidColorOnly = true;
				if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					OnColorChanging?.Invoke(    //跑同步，避免 Font 被刪掉
						this,
						new ColorEventArgs(tag, diag.Color)
					);
				}
				rect.TryInvoke(() => rect.Fill = diag.Color.GetBrush());
			}
		}

		private void AlphaChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var tag = PanelComponent.Background;
			var idx = 0;
			comboBox.TryInvoke(
				() => {
					tag = (PanelComponent)comboBox.Tag;
					idx = comboBox.SelectedIndex;
				}
			);
			OnAlphaChanged?.BeginInvoke(
				this,
				new AlphaEventArgs(tag, idx),
				null, null
			);
		}

		private void UsageDockChanged(object sender, SelectionChangedEventArgs e) {
			var comboBox = sender as ComboBox;
			var idx = comboBox.TryInvoke(() => comboBox.SelectedIndex);
			OnDockChanged?.BeginInvoke(
				this,
				new DockEventArgs((UsageDock)idx),
				null, null
			);
		}

		private void SaveClicked(object sender, MouseButtonEventArgs e) {
			mSaved = true;
			OnSaveTriggered?.BeginInvoke(
				this,
				new BoolEventArgs(PanelComponent.Background, true),
				null, null
			);
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
