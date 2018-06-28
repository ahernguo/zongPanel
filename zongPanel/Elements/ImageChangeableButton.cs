using System.Windows.Controls;
using System.Windows.Media;

namespace zongPanel {

	/// <summary>可由滑鼠事件更改內容圖片的 <see cref="Button"/></summary>
	public class ImageChageableButton : Button {

		#region Properties
		/// <summary>取得或設定當滑鼠停留在控制項上時的圖片</summary>
		public ImageSource HoverImage { get; set; }
		/// <summary>取得或設定當滑鼠未在控制項上時的圖片</summary>
		public ImageSource IdleImage { get; set; }
		#endregion

		#region Constructors
		/// <summary>建構可更改圖片的 <see cref="Button"/></summary>
		public ImageChageableButton() {

		}
		#endregion
	}
}
