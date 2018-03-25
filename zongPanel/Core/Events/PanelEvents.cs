using System;
using System.Drawing;

namespace zongPanel {

	/// <summary>面板事件參數</summary>
	/// <typeparam name="T">數值類型</typeparam>
	public abstract class PanelEventArgs<T> : EventArgs {

		#region Properties
		/// <summary>取得觸發事件之元件</summary>
		public PanelComponent Component { get; }
		/// <summary>取得改變後的新數值</summary>
		public T Value { get; }
		#endregion

		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public PanelEventArgs(PanelComponent comp, T value) {
			Component = comp;
			Value = value;
		}
		#endregion
	}

	/// <summary>數值格式改變事件參數</summary>
	public class FormatEventArgs : PanelEventArgs<Format> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public FormatEventArgs(PanelComponent component, Format value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>數值格式改變事件參數</summary>
	public class FormatNumberEventArgs : PanelEventArgs<int> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值，以代號為主，實際內容由 <see cref="PanelConfig"/> 決定</param>
		public FormatNumberEventArgs(PanelComponent component, int value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>顏色改變事件參數</summary>
	public class ColorEventArgs : PanelEventArgs<Color> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public ColorEventArgs(PanelComponent component, Color value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>字型改變事件參數</summary>
	public class FontEventArgs : PanelEventArgs<Font> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public FontEventArgs(PanelComponent component, Font value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>透明度改變事件參數</summary>
	public class AlphaEventArgs : PanelEventArgs<int> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值，以數值 0 ~ 9 對應 10% ~ 100%</param>
		public AlphaEventArgs(PanelComponent component, int value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>布林數值改變事件參數</summary>
	public class BoolEventArgs : PanelEventArgs<bool> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public BoolEventArgs(PanelComponent component, bool value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>座標位置改變事件參數</summary>
	public class PointEventArgs : PanelEventArgs<PointF> {
		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="comp">觸發事件之元件</param>
		/// <param name="value">改變後的新數值</param>
		public PointEventArgs(PanelComponent component, PointF value)
			: base(component, value) {

		}
		#endregion
	}

	/// <summary>設定檔事件參數</summary>
	public class ConfigEventArgs : EventArgs {

		#region Properties
		/// <summary>取得設定檔</summary>
		public PanelConfig Config { get; }
		#endregion

		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="config">改變後的新設定檔</param>
		public ConfigEventArgs(PanelConfig config) {
			Config = config;
		}
		#endregion
	}

	/// <summary>效能監視面板停靠位置改變事件參數</summary>
	public class DockEventArgs : EventArgs {

		#region Properties
		/// <summary>取得停駐位置</summary>
		public UsageDock Dock { get; }
		#endregion

		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="value">改變後的新數值</param>
		public DockEventArgs(UsageDock value) {
			Dock = value;
		}
		#endregion
	}

	/// <summary>主面板捷徑改變事件參數</summary>
	public class ShortcutEventArgs : EventArgs {

		#region Properties
		/// <summary>取得捷徑選項</summary>
		public Shortcut Shortcut { get; }
		#endregion

		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="sc">欲更改的捷徑</param>
		/// <param name="vis">顯示或隱藏</param>
		public ShortcutEventArgs(Shortcut sc) {
			Shortcut = sc;
		}
		#endregion
	}

	/// <summary>主面板捷徑改變事件參數</summary>
	public class ShortcutVisibleEventArgs : EventArgs {

		#region Properties
		/// <summary>取得捷徑選項</summary>
		public Shortcut Shortcut { get; }
		/// <summary>取得是否顯示此捷徑</summary>
		public bool Visibility { get; }
		#endregion

		#region Constructors
		/// <summary>建構事件參數</summary>
		/// <param name="sc">欲更改的捷徑</param>
		/// <param name="vis">顯示或隱藏</param>
		public ShortcutVisibleEventArgs(Shortcut sc, bool vis) {
			Shortcut = sc;
			Visibility = vis;
		}
		#endregion
	}
}
