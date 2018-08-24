using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Win32Draw = System.Drawing;

namespace zongPanel.Library {
	public static class Extensions {

		#region UIElement Invokes
		/// <summary>調用 <see cref="Dispatcher"/> 物件以執行特定方法</summary>
		/// <param name="obj">欲調用之 <see cref="Dispatcher"/></param>
		/// <param name="action">欲執行的方法</param>
		public static void TryInvoke(this DispatcherObject obj, Action action) {
			if (!obj.Dispatcher.CheckAccess()) {
				obj.Dispatcher.Invoke(action);
			} else {
				action();
			}
		}

		/// <summary>調用 <see cref="Dispatcher"/> 物件以取得特定數值</summary>
		/// <typeparam name="TObj">可由控制項或其運算屬性之型別</typeparam>
		/// <param name="obj">欲調用之 <see cref="Dispatcher"/></param>
		/// <param name="action">欲執行的方法</param>
		public static TObj TryInvoke<TObj>(this DispatcherObject obj, Func<TObj> action) {
			if (!obj.Dispatcher.CheckAccess()) {
				return obj.Dispatcher.Invoke(action);
			} else {
				return action();
			}
		}

		/// <summary>調用 <see cref="Dispatcher"/> 物件以執行特定方法，使用非同步</summary>
		/// <param name="obj">欲調用之 <see cref="Dispatcher"/></param>
		/// <param name="action">欲執行的方法</param>
		/// <returns>非同步執行結果</returns>
		public static DispatcherOperation TryAsyncInvoke(this DispatcherObject obj, Action action) {
			return obj.Dispatcher.InvokeAsync(action);
		}
		#endregion

		#region Image Extensions
		/// <summary>取得 <see cref="Image"/> 對應的 <see cref="ImageSource"/>，可供 <see cref="Image.Source"/> 使用</summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static BitmapImage GetImageSource(this Win32Draw.Image img) {
			BitmapImage imgSrc = new BitmapImage();
			using (MemoryStream ms = new MemoryStream()) {
				img.Save(ms, img.RawFormat);
				imgSrc.BeginInit();
				imgSrc.CacheOption = BitmapCacheOption.OnLoad;
				imgSrc.StreamSource = ms;
				imgSrc.EndInit();
			}
			return imgSrc;
		}
		#endregion

		#region Windows <-> Forms
		/// <summary>將 Windows.Media.Color 轉換為 Drawing.Color</summary>
		/// <param name="wpfColor">欲轉換的 <see cref="Color"/></param>
		/// <returns><see cref="Win32Draw.Color"/></returns>
		public static Win32Draw.Color ToColor(this Color wpfColor) {
			return Win32Draw.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);
		}

		/// <summary>將 Drawing.Color 轉換為 Windows.Media.Color</summary>
		/// <param name="winColor">欲轉換的 <see cref="Win32Draw.Color"/></param>
		/// <returns><see cref="Color"/></returns>
		public static Color ToColor(this Win32Draw.Color winColor) {
			return Color.FromArgb(winColor.A, winColor.R, winColor.G, winColor.B);
		}

		/// <summary>將 Windows.Media.Brush 轉換為 Drawing.Brush</summary>
		/// <param name="wpfBrush">欲轉換的 <see cref="Brush"/></param>
		/// <returns><see cref="Win32Draw.Brush"/></returns>
		public static Win32Draw.Brush ToBrush(this Brush wpfBrush) {
			Win32Draw.Brush convertBrush = null;
			if (wpfBrush is SolidColorBrush wpfSolid) {
				convertBrush = new Win32Draw.SolidBrush(wpfSolid.Color.ToColor());
			}
			return convertBrush;
		}

		/// <summary>取得 Windows.Media.Brush 之對應的 Drawing.Color</summary>
		/// <param name="wpfBrush">欲轉換的 <see cref="Brush"/></param>
		/// <returns><see cref="Win32Draw.Color"/></returns>
		public static Win32Draw.Color GetColor(this Brush wpfBrush) {
			Win32Draw.Color convertColor = Win32Draw.Color.Empty;
			if (wpfBrush is SolidColorBrush wpfSolid) {
				convertColor = wpfSolid.Color.ToColor();
			}
			return convertColor;
		}

		/// <summary>取得 Drawing.Color 之對應的 Windows.Media.Brush</summary>
		/// <param name="wpfBrush">欲轉換的 <see cref="Win32Draw.Color"/></param>
		/// <returns><see cref="Brush"/></returns>
		public static Brush GetBrush(this Win32Draw.Color winColor) {
			return new SolidColorBrush(Color.FromArgb(winColor.A, winColor.R, winColor.G, winColor.B));
		}

		/// <summary>取得 Windows.Controls.Control 之對應的 Drawing.Font</summary>
		/// <param name="ctrl">欲轉換的 <see cref="Control"/></param>
		/// <returns><see cref="Win32Draw.Font"/></returns>
		public static Win32Draw.Font GetFont(this Control ctrl) {
			return ctrl.TryInvoke(
				() => {
					string fontFamily = ctrl.FontFamily.Source;
					float emSize = (float)ctrl.FontSize;
					Win32Draw.FontStyle fontStyle = Win32Draw.FontStyle.Regular;
					if (ctrl.FontStyle == FontStyles.Italic) fontStyle = Win32Draw.FontStyle.Italic;
					if (ctrl.FontWeight == FontWeights.Bold) fontStyle |= Win32Draw.FontStyle.Bold;

					return new Win32Draw.Font(fontFamily, emSize, fontStyle);
				}
			);
		}

		/// <summary>設定 Windows.Controls.Control 之對應的 Drawing.Font</summary>
		/// <param name="ctrl">欲設定的 <see cref="Control"/></param>
		/// <param name="font">欲更換的 <see cref="Win32Draw.Font"/></param>
		public static void SetFont(this Control ctrl, Win32Draw.Font font) {
			ctrl.TryInvoke(
				() => {
					ctrl.FontFamily = new FontFamily(font.FontFamily.Name);
					ctrl.FontSize = font.Size;
					if (font.Style.HasFlag(Win32Draw.FontStyle.Bold)) ctrl.FontWeight = FontWeights.Bold;
					if (font.Style.HasFlag(Win32Draw.FontStyle.Italic)) ctrl.FontStyle = FontStyles.Italic;
				}
			);
		}

		/// <summary>設定 Windows.Controls.Control 之對應的 Drawing.Font</summary>
		/// <param name="ctrl">欲設定的 <see cref="Control"/></param>
		/// <param name="font">欲更換的 <see cref="Win32Draw.Font"/></param>
		/// <param name="size">指定要更換的字體大小</param>
		public static void SetFont(this Control ctrl, Win32Draw.Font font, float size) {
			ctrl.TryInvoke(
				() => {
					ctrl.FontFamily = new FontFamily(font.FontFamily.Name);
					ctrl.FontSize = size;
					if (font.Style.HasFlag(Win32Draw.FontStyle.Bold)) ctrl.FontWeight = FontWeights.Bold;
					if (font.Style.HasFlag(Win32Draw.FontStyle.Italic)) ctrl.FontStyle = FontStyles.Italic;
				}
			);
		}

		/// <summary>取得 System.Windows.Window 對應的 System.Drawing.RectangleF</summary>
		/// <param name="window">欲取得的 <see cref="Window"/></param>
		/// <returns>框體資訊</returns>
		public static Win32Draw.RectangleF GetRectangle(this Window window) {
			return window.TryInvoke(
				() => new Win32Draw.RectangleF(
					(float)window.Left,
					(float)window.Top,
					(float)window.Width,
					(float)window.Height
				)
			);
		}

		/// <summary>將 System.Drawing.RectangleF 設定至對應的 System.Windows.Window</summary>
		/// <param name="window">欲寫入的 <see cref="Window"/></param>
		/// <param name="rect">欲設定的 <see cref="Win32Draw.RectangleF"/></param>
		public static void SetRectangle(this Window window, Win32Draw.RectangleF rect) {
			window.TryInvoke(
				() => {
					window.Left = rect.Left;
					window.Top = rect.Top;
					window.Width = rect.Width;
					window.Height = rect.Height;
				}
			);
		}
		#endregion

		#region Copies
		/// <summary>取得 <see cref="Win32Draw.Color"/> 之複製品</summary>
		/// <param name="color">欲複製的 <see cref="Win32Draw.Color"/></param>
		/// <returns><see cref="Win32Draw.Color"/></returns>
		public static Win32Draw.Color Clone(this Win32Draw.Color color) {
			return Win32Draw.Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		/// <summary>取得 <see cref="Win32Draw.RectangleF"/> 之複製品</summary>
		/// <param name="color">欲複製的 <see cref="Win32Draw.RectangleF"/></param>
		/// <returns><see cref="Win32Draw.RectangleF"/></returns>
		public static Win32Draw.RectangleF Clone(this Win32Draw.RectangleF rect) {
			return new Win32Draw.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
		}

		/// <summary>取得 <see cref="Win32Draw.SizeF"/> 之複製品</summary>
		/// <param name="color">欲複製的 <see cref="Win32Draw.SizeF"/></param>
		/// <returns><see cref="Win32Draw.SizeF"/></returns>
		public static Win32Draw.SizeF Clone(this Win32Draw.SizeF size) {
			return new Win32Draw.SizeF(size);
		}

		/// <summary>取得 <see cref="Win32Draw.Font"/> 之複製品</summary>
		/// <param name="color">欲複製的 <see cref="Win32Draw.Font"/></param>
		/// <returns><see cref="Win32Draw.Font"/></returns>
		public static Win32Draw.Font Copy(this Win32Draw.Font font) {
			return new Win32Draw.Font(font.FontFamily, font.Size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
		}

		/// <summary>取得 <see cref="Win32Draw.PointF"/> 之複製品</summary>
		/// <param name="color">欲複製的 <see cref="Win32Draw.PointF"/></param>
		/// <returns><see cref="Win32Draw.PointF"/></returns>
		public static Win32Draw.PointF Clone(this Win32Draw.PointF point) {
			return new Win32Draw.PointF(point.X, point.Y);
		}
		#endregion
	}
}
