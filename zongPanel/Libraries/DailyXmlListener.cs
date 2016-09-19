using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace zongPanel.Library {

	/// <summary>適用於 <see cref="Trace"/> 之監聽器，採以每日一檔的方式進行記錄</summary>
	internal class DailyXmlListener : TraceListener {

		#region Fields
		/// <summary>處理 XML 記錄檔</summary>
		private XDocument mDoc;
		/// <summary>紀錄上一筆資料，如跨日則需清除內容</summary>
		private DateTime mLastDate = DateTime.Now;
		/// <summary>記錄檔目錄，如 @"D:\zongPanel\Logs"</summary>
		private string mLogDir = string.Empty;
		#endregion

		#region Constructors
		/// <summary>建立並初始化 <see cref="DailyXmlListener"/></summary>
		/// <param name="directory">欲存放記錄檔之目錄，如 @"D:\zongPanel\Logs"</param>
		public DailyXmlListener(string directory) {
			/* 擷取路徑 */
			string dir = Path.GetDirectoryName(directory);
			if (!dir.EndsWith(@"\")) dir += @"\";
			mLogDir = dir;

			/* 查看是否現有檔案需要載入 */
			string file = EnsureFile();
			if (File.Exists(file)) {    //有檔案，載入之
				mDoc = XDocument.Load(file);
			} else {                    //沒檔案，建立之
				XDeclaration declare = new XDeclaration("1.0", "UTF-8", string.Empty);
				XElement root = new XElement("Logs");
				mDoc = new XDocument(declare, root);
			}
		}
		#endregion

		#region Private Utilities
		/// <summary>確保並回傳正確的檔案名稱(含路徑)</summary>
		/// <returns>檔案路徑</returns>
		private string EnsureFile() {
			string path = mLogDir;
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			path = $"{mLogDir}{DateTime.Now.ToString("yyyyMMdd")}.xml";
			return path;
		}

		/// <summary>檢查是否跨日。如跨日則需要將 <see cref="XDocument"/> 內容清空</summary>
		private void CheckContext() {
			if (DateTime.Now.Subtract(mLastDate).TotalDays >= 1) {
				mDoc.Root.RemoveAll();
			}
		}

		/// <summary>檢查是否跨日(需要清空 XML)並添加至 Root</summary>
		/// <param name="elmt">欲添加的 XML 資訊</param>
		private void AddXElement(XElement elmt) {
			CheckContext();
			mDoc.Root.Add(elmt);
		}

		/// <summary>檢查此訊息是否需要寫入記錄檔，依照 <see cref="TraceListener.Filter"/> 設定為準</summary>
		/// <param name="source"><see cref="SourceFilter.Source"/></param>
		/// <param name="eventType"><see cref="EventTypeFilter.EventType"/></param>
		/// <returns>(True)需要寫入記錄檔 (False)不須寫入</returns>
		private bool IsNecessary(string source, TraceEventType eventType) {
			bool necessary = false;
			/* 判斷 Filter 是否為 EventTypeFilter */
			EventTypeFilter evnFilter = Filter as EventTypeFilter;
			if (evnFilter != null) {
				/* 因 SourceLevels 與 TraceEventType 兩個 Enum 數值不同，故用單一比較 */
				SourceLevels srcLv = evnFilter.EventType;
				necessary = srcLv.HasFlag(SourceLevels.All)
							|| (srcLv.HasFlag(SourceLevels.Error) && eventType.HasFlag(TraceEventType.Error))
							|| (srcLv.HasFlag(SourceLevels.Information) && eventType.HasFlag(TraceEventType.Information))
							|| (srcLv.HasFlag(SourceLevels.Warning) && eventType.HasFlag(TraceEventType.Warning))
							|| (srcLv.HasFlag(SourceLevels.Critical) && eventType.HasFlag(TraceEventType.Critical))
							|| (srcLv.HasFlag(SourceLevels.Verbose) && eventType.HasFlag(TraceEventType.Verbose));
			} else {
				/* 檢查是否為 SourceFilter */
				SourceFilter srcFilter = Filter as SourceFilter;
				if (srcFilter != null) necessary = srcFilter.Source == source;
			}
			return necessary;
		}

		/// <summary>將執行緒呼叫堆疊(<see cref="TraceEventCache.Callstack"/>)轉換為 <see cref="XElement"/></summary>
		/// <param name="stack">執行緒呼叫堆疊</param>
		/// <returns>對應的 XML 節點資訊</returns>
		private XElement GetCallstack(string stack) {
			/* 因 Callstack 裡面 "\r\n" 不一定與空白成雙成對，但開頭都是 "於 "！為了排版美觀，故重新 .Split */
			IEnumerable<string> split = stack
										.Split(new char[] { '於' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(str => str.Trim())
										.Where(str => !string.IsNullOrEmpty(str));

			/* 將每段落整合並補上 Tab，其中因為 "於" 已經被分割，所以要重新補上 */
			string combine = string.Join("\r\n\t\t\t\t於 ", split);
			/* 組成完整一段 String */
			string content = $"\r\n\t\t\t\t於 {combine}\r\n\t\t\t";
			/* 回傳 */
			return new XElement("Callstack", content);
		}

		/// <summary>將 <see cref="TraceEventCache.LogicalOperationStack"/> 轉換為 <see cref="XElement"/></summary>
		/// <param name="stack">相互關連堆疊</param>
		/// <returns>對應的 XML 節點資訊</returns>
		private XElement GetOperationStack(Stack stack) {
			IEnumerable<string> split = stack.ToArray().Select(obj => obj.ToString().Trim());
			string combine = string.Join("\r\n\t\t\t", split);
			string content = $"\r\n\t\t\t{combine}\r\n\t\t";
			return new XElement("OperationStack", content);
		}

		/// <summary>將呼叫堆疊(<see cref="Exception.StackTrace"/>)轉換為 <see cref="XElement"/></summary>
		/// <param name="stack">呼叫堆疊</param>
		/// <returns>對應的 XML 節點資訊</returns>
		private XElement GetStackTrace(Exception ex) {
			StackTrace st = new StackTrace(ex, true);
			if (st.FrameCount > 0) {
				/* 取得有行號的部分即可 */
				IEnumerable<string> split = st
											.GetFrames()
											.Where(sf => sf.GetFileLineNumber() > 0)
											.Select(sf => $"{sf.GetMethod().Name} ({sf.GetFileName()}:{sf.GetFileLineNumber().ToString()})");

				/* 將每段落整合並補上 Tab，其中因為 "於" 已經被分割，所以要重新補上 */
				string combine = string.Join("\r\n\t\t\t於 ", split);
				/* 組成完整一段 String */
				string content = $"\r\n\t\t\t於 {combine}\r\n\t\t";
				/* 回傳 */
				return new XElement("StackTrace", content); 
			} else {
				/* 沒東西則直接回傳 */
				return new XElement("StackTrace");
			}
		}

		/// <summary>將追蹤事件資料(<see cref="TraceEventCache"/>)轉換為對應的 <see cref="XElement"/> 集合</summary>
		/// <param name="cache">追蹤事件資料</param>
		/// <returns>對應的 XML 節點資訊集合</returns>
		private List<XElement> GetEventCache(TraceEventCache cache) {
			List<XElement> elmt = new List<XElement>();
			if (this.TraceOutputOptions.HasFlag(TraceOptions.Timestamp))
				elmt.Add(new XElement("Timestamp", cache.Timestamp));
			if (this.TraceOutputOptions.HasFlag(TraceOptions.ProcessId))
				elmt.Add(new XElement("ProcessId", cache.ProcessId));
			if (this.TraceOutputOptions.HasFlag(TraceOptions.ThreadId))
				elmt.Add(new XElement("ThreadId", cache.ThreadId));
			if (this.TraceOutputOptions.HasFlag(TraceOptions.Callstack) && !string.IsNullOrEmpty(cache.Callstack))
				elmt.Add(GetCallstack(cache.Callstack));
			if (this.TraceOutputOptions.HasFlag(TraceOptions.LogicalOperationStack))
				elmt.Add(GetOperationStack(cache.LogicalOperationStack));
			return elmt;
		}

		/// <summary>將 <see cref="Exception"/> 轉換為對應的 XML 資訊</summary>
		/// <param name="ex">欲轉換的例外狀況</param>
		/// <returns>對應的 XML 資訊</returns>
		private List<XElement> GetException(Exception ex) {
			List<XElement> elmt = new List<XElement>();
			elmt.Add(new XElement("Message", ex.Message));
			if (ex.InnerException != null) elmt.Add(new XElement("InnerException", GetException(ex.InnerException)));
			elmt.Add(GetStackTrace(ex));
			return elmt;
		}
		#endregion

		#region Overrides
		/// <summary>錯誤訊息</summary>
		/// <param name="message">訊息內容</param>
		public override void Fail(string message) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"Fail",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XElement("Message", message)
			);
			AddXElement(elmt);
		}

		/// <summary>錯誤訊息(含詳細資料)</summary>
		/// <param name="message">訊息內容</param>
		/// <param name="detailMessage">詳細資料</param>
		public override void Fail(string message, string detailMessage) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"Fail",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XElement("Message", message),
				new XElement("DetailMessage", detailMessage)
			);
			AddXElement(elmt);
		}

		/// <summary>排清輸出緩衝區，即寫入目前所有已紀錄之記錄檔資訊</summary>
		public override void Flush() {
			/* 確保檔案路徑可存取 */
			string file = EnsureFile();
			/* 採用 Tab 縮排 */
			XmlWriterSettings xWrSet = new XmlWriterSettings() {
				Indent = true, IndentChars = "\t"
			};
			/* 寫入至 XML */
			using (XmlWriter xWriter = XmlWriter.Create(file, xWrSet)) {
				mDoc.Save(xWriter);
			}
		}

		/// <summary>取得此監聽器之文字描述</summary>
		/// <returns>文字描述</returns>
		public override string ToString() {
			string file = EnsureFile();
			string count = mDoc.Root.Elements().Count().ToString();
			return $"Total {count} logs, TargetLogFile : {file}";
		}

		/// <summary>寫入追蹤資訊、資料物件與事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="eventType">指定引發追蹤的事件類型</param>
		/// <param name="id">事件的數值識別項</param>
		/// <param name="data">要發出的追蹤資料</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data) {
			if (IsNecessary(source, eventType)) {
				var traceElmt = new XElement("Trace");
				traceElmt.Add(new XElement("Id", id));
				traceElmt.Add(new XElement("Data", data.ToString()));
				var caches = GetEventCache(eventCache);
				if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
				if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
					traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
				AddXElement(traceElmt);
			}
		}

		/// <summary>寫入追蹤資訊、資料物件的陣列與事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="eventType">指定引發追蹤的事件類型</param>
		/// <param name="id">事件的數值識別項</param>
		/// <param name="data">要發出做為資料的物件陣列</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data) {
			if (IsNecessary(source, eventType)) {
				var traceElmt = new XElement("Trace");
				traceElmt.Add(new XElement("Id", id));
				traceElmt.Add(new XElement("DataCollection", data.Select(obj => new XElement("Data", obj.ToString()))));
				var caches = GetEventCache(eventCache);
				if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
				if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
					traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
				AddXElement(traceElmt);
			}
		}

		/// <summary>寫入追蹤和事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="eventType">指定引發追蹤的事件類型</param>
		/// <param name="id">事件的數值識別項</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id) {
			if (IsNecessary(source, eventType)) {
				var traceElmt = new XElement("Trace");
				traceElmt.Add(new XElement("Id", id));
				var caches = GetEventCache(eventCache);
				if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
				if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
					traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
				AddXElement(traceElmt);
			}
		}

		/// <summary>寫入追蹤資訊、格式化的物件陣列與事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="eventType">指定引發追蹤的事件類型</param>
		/// <param name="id">事件的數值識別項</param>
		/// <param name="format">包含零或多個格式項目的格式字串，它與 args 陣列中的物件相對應</param>
		/// <param name="args">object 陣列，含有零或多個要格式化的物件</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args) {
			if (IsNecessary(source, eventType)) {
				var traceElmt = new XElement("Trace");
				traceElmt.Add(new XElement("Id", id));
				traceElmt.Add(new XElement("Data", string.Format(format, args)));
				var caches = GetEventCache(eventCache);
				if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
				if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
					traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
				AddXElement(traceElmt);
			}
		}

		/// <summary>寫入追蹤資訊、訊息與事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="eventType">指定引發追蹤的事件類型</param>
		/// <param name="id">事件的數值識別項</param>
		/// <param name="message">要寫入的訊息</param>
		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message) {
			if (IsNecessary(source, eventType)) {
				var traceElmt = new XElement("Trace");
				traceElmt.Add(new XElement("Id", id));
				traceElmt.Add(new XElement("Data", message));
				var caches = GetEventCache(eventCache);
				if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
				if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
					traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
				AddXElement(traceElmt);
			}
		}

		/// <summary>寫入追蹤資訊、訊息、相關活動身分識別與事件資訊</summary>
		/// <param name="eventCache"><see cref="TraceEventCache"/> 物件，包含目前處理程序識別碼、執行緒識別碼與堆疊追蹤資訊</param>
		/// <param name="source">用來識別輸出的名稱，通常是產生追蹤事件的應用程式名稱</param>
		/// <param name="id">事件的數值識別項</param>
		/// <param name="message">要寫入的訊息</param>
		/// <param name="relatedActivityId"><see cref="Guid"/> 物件，辨識相關活動</param>
		public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId) {
			SourceFilter srcFilter = Filter as SourceFilter;
			if (srcFilter != null && !srcFilter.Source.Equals(source)) return;

			var traceElmt = new XElement("Transfer");
			traceElmt.Add(new XElement("GUID", relatedActivityId.ToString()));
			traceElmt.Add(new XElement("Id", id));
			traceElmt.Add(new XElement("Data", message));
			var caches = GetEventCache(eventCache);
			if (caches.Count > 0) traceElmt.Add(new XElement("EventCache", caches));
			if (this.TraceOutputOptions.HasFlag(TraceOptions.DateTime))
				traceElmt.Add(new XAttribute("Time", eventCache.DateTime.ToLocalTime().ToString("HH:mm:ss.fff")));
			AddXElement(traceElmt);
		}

		/// <summary>將物件的 <see cref="object.ToString"/> 方法的值寫入記錄檔</summary>
		/// <param name="o">要寫入其完整分類名稱的 <see cref="object"/></param>
		public override void Write(object o) {
			DateTime time = DateTime.Now;
			XElement elmt = null;
			Exception ex = o as Exception;
			if (ex != null) {
				elmt = new XElement(
					"Exception",
					new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
					GetException(ex)
				);
			} else {
				elmt = new XElement(
					"Write",
					new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
					new XElement("Data", o.ToString())
				);
			}
			AddXElement(elmt);
		}

		/// <summary>將類別名稱和物件的 <see cref="object.ToString"/> 方法的值寫入記錄檔</summary>
		/// <param name="o">要寫入其完整分類名稱的 <see cref="object"/></param>
		/// <param name="category">用來組織輸出的類別名稱</param>
		public override void Write(object o, string category) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"Write",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XAttribute("Category", category),
				new XElement("Data", o.ToString())
			);
			AddXElement(elmt);
		}

		/// <summary>將訊息寫入記錄檔</summary>
		/// <param name="message">要寫入的訊息</param>
		public override void Write(string message) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"Write",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XElement("Data", message)
			);
			AddXElement(elmt);
		}

		/// <summary>將分類名稱和訊息寫入記錄檔</summary>
		/// <param name="message">要寫入的訊息</param>
		/// <param name="category">用來組織輸出的類別名稱</param>
		public override void Write(string message, string category) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"Write",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XAttribute("Category", category),
				new XElement("Data", message)
			);
			AddXElement(elmt);
		}

		/// <summary>將物件的 <see cref="object.ToString"/> 方法的值寫入記錄檔</summary>
		/// <param name="o">要寫入其完整分類名稱的 <see cref="object"/></param>
		public override void WriteLine(object o) {
			DateTime time = DateTime.Now;
			XElement elmt = null;
			Exception ex = o as Exception;
			if (ex != null) {
				elmt = new XElement(
					"Exception",
					new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
					GetException(ex)
				);
			} else {
				elmt = new XElement(
					"WriteLine",
					new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
					new XElement("Data", o.ToString())
				);
			}
			AddXElement(elmt);
		}

		/// <summary>將分類名稱和物件的 <see cref="object.ToString"/> 方法的值寫入記錄檔</summary>
		/// <param name="o">要寫入其完整分類名稱的 <see cref="object"/></param>
		/// <param name="category">用來組織輸出的類別名稱</param>
		public override void WriteLine(object o, string category) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"WriteLine",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XAttribute("Category", category),
				new XElement("Data", o.ToString())
			);
			AddXElement(elmt);
		}

		/// <summary>將訊息寫入記錄檔</summary>
		/// <param name="message">要寫入的訊息</param>
		public override void WriteLine(string message) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"WriteLine",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XElement("Data", message)
			);
			AddXElement(elmt);
		}

		/// <summary>將分類名稱和物件的 <see cref="object.ToString"/> 方法的值寫入記錄檔</summary>
		/// <param name="message">要寫入的訊息</param>
		/// <param name="category">用來組織輸出的類別名稱</param>
		public override void WriteLine(string message, string category) {
			DateTime time = DateTime.Now;
			XElement elmt = new XElement(
				"WriteLine",
				new XAttribute("Time", time.ToString("HH:mm:ss.fff")),
				new XAttribute("Category", category),
				new XElement("Data", message)
			);
			AddXElement(elmt);
		}
		#endregion
	}
}
