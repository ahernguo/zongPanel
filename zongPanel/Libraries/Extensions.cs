using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace zongPanel.Library {
	public static class Extensions {

		#region UIElement Invokes
		/// <summary>嘗試調用 <see cref="Control"/></summary>
		/// <param name="ctrl">欲調用之控制項</param>
		/// <param name="action">欲執行的方法</param>
		public static void TryInvoke(this UIElement ctrl, Action action) {
			if (ctrl.Dispatcher.CheckAccess()) ctrl.Dispatcher.Invoke(action);
			else action();
		}

		/// <summary>嘗試調用 <see cref="Control"/>，並取得其方法回傳值</summary>
		/// <typeparam name="TObj">可由控制項或其運算屬性之型別</typeparam>
		/// <param name="ctrl">欲調用之控制項</param>
		/// <param name="action">欲執行的方法</param>
		public static TObj TryInvoke<TObj>(this UIElement ctrl, Func<TObj> action) {
			if (ctrl.Dispatcher.CheckAccess()) return ctrl.Dispatcher.Invoke(action);
			else return action();
		}

		/// <summary>嘗試調用 <see cref="Control"/></summary>
		/// <param name="ctrl">欲調用之控制項</param>
		/// <param name="action">欲執行的方法</param>
		public static void TryAsyncInvoke(this UIElement ctrl, Action action) {
			if (ctrl.Dispatcher.CheckAccess()) ctrl.Dispatcher.InvokeAsync(action);
			else action();
		}

		/// <summary>嘗試調用 <see cref="Control"/>，並取得其方法回傳值</summary>
		/// <typeparam name="TObj">可由控制項或其運算屬性之型別</typeparam>
		/// <param name="ctrl">欲調用之控制項</param>
		/// <param name="action">欲執行的方法</param>
		public static TObj TryAsyncInvoke<TObj>(this UIElement ctrl, Func<TObj> action) {
			if (ctrl.Dispatcher.CheckAccess()) return ctrl.Dispatcher.InvokeAsync(action).Result;
			else return action();
		}
		#endregion

		#region Image Extensions
		/// <summary>取得 <see cref="Image"/> 對應的 <see cref="ImageSource"/>，可供 <see cref="Image.Source"/> 使用</summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static BitmapImage GetImageSource(this System.Drawing.Image img) {
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
		public static System.Drawing.Color ToColor(this Color wpfColor) {
			return System.Drawing.Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);
		}

		public static System.Drawing.Brush ToBrush(this Brush wpfBrush) {
			System.Drawing.Brush convertBrush = null;
			SolidColorBrush wpfSolid = wpfBrush as SolidColorBrush;
			if (wpfSolid != null) {
				convertBrush = new System.Drawing.SolidBrush(wpfSolid.Color.ToColor());
			}
			return convertBrush;
		}

		public static System.Drawing.Color GetColor(this Brush wpfBrush) {
			System.Drawing.Color convertColor = System.Drawing.Color.Empty;
			SolidColorBrush wpfSolid = wpfBrush as SolidColorBrush;
			if (wpfSolid != null) {
				convertColor = wpfSolid.Color.ToColor();
			}
			return convertColor;
		}

		public static System.Drawing.Font GetFont(this Control ctrl) {
			string fontFamily = ctrl.FontFamily.Source;
			float emSize = (float)ctrl.FontSize;
			System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Regular;
			if (ctrl.FontStyle == FontStyles.Italic) fontStyle = System.Drawing.FontStyle.Italic;
			if (ctrl.FontWeight == FontWeights.Bold) fontStyle |= System.Drawing.FontStyle.Bold;

			return new System.Drawing.Font(fontFamily, emSize, fontStyle);
		}
		#endregion
	}
}
