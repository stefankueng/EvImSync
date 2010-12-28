using System;
using System.Text;
using System.IO;
using System.Threading;

namespace IMAPShell.Helpers {

	public class LineEditor
    {

        #region Delegates
        public delegate string [] AutoCompleteHandler (string text, int pos);
        delegate void KeyHandler ();
        #endregion

        #region Private Fields
        private StringBuilder _text;
        private StringBuilder _renderedText;
        private string _prompt;
        private string _shownPrompt;
        private int _shownPromptLength;
        private int _cursor;
        private int _homeRow;
        private int _maxRendered;
        private bool _done = false;
        private Thread _editThread;
        private History _history;
        private string _killBuffer = "";
        private string _search;
        private string _lastSearch;
        private int _searching;
        private int _matchAt;
        private KeyHandler _lastHandler;
        private static Handler [] _handlers;
        #endregion        			
		
		
        #region Handler Struct
        struct Handler {
			public ConsoleKeyInfo CKI;
			public KeyHandler KeyHandler;

			public Handler (ConsoleKey key, KeyHandler h)
			{
				CKI = new ConsoleKeyInfo ((char) 0, key, false, false, false);
				KeyHandler = h;
			}

			public Handler (char c, KeyHandler h)
			{
				KeyHandler = h;
				// Use the "Zoom" as a flag that we only have a character.
				CKI = new ConsoleKeyInfo (c, ConsoleKey.Zoom, false, false, false);
			}

			public Handler (ConsoleKeyInfo cki, KeyHandler h)
			{
				CKI = cki;
				KeyHandler = h;
			}
			
			public static Handler Control (char c, KeyHandler h)
			{
				return new Handler ((char) (c - 'A' + 1), h);
			}

			public static Handler Alt (char c, ConsoleKey k, KeyHandler h)
			{
				ConsoleKeyInfo cki = new ConsoleKeyInfo ((char) c, k, false, true, false);
				return new Handler (cki, h);
			}
		}
        #endregion

        #region Public Events
        /// <summary>
		///   Invoked when the user requests auto-completion using the tab character
		/// </summary>
		/// <remarks>
		///    The result is null for no values found, an array with a single
		///    string, in that case the string should be the text to be inserted
		///    for example if the word at pos is "T", the result for a completion
		///    of "ToString" should be "oString", not "ToString".
		///
		///    When there are multiple results, the result should be the full
		///    text
		/// </remarks>
		public AutoCompleteHandler AutoCompleteEvent;
        #endregion

        #region CTOR
        public LineEditor (string name) : this (name, 10) { }
		
		public LineEditor (string name, int histsize)
		{
			_handlers = new Handler [] {
				new Handler (ConsoleKey.Home,       CmdHome),
				new Handler (ConsoleKey.End,        CmdEnd),
				new Handler (ConsoleKey.LeftArrow,  CmdLeft),
				new Handler (ConsoleKey.RightArrow, CmdRight),
				new Handler (ConsoleKey.UpArrow,    CmdHistoryPrev),
				new Handler (ConsoleKey.DownArrow,  CmdHistoryNext),
				new Handler (ConsoleKey.Enter,      CmdDone),
				new Handler (ConsoleKey.Backspace,  CmdBackspace),
				new Handler (ConsoleKey.Delete,     CmdDeleteChar),
				new Handler (ConsoleKey.Tab,        CmdTabOrComplete),
				
				// Emacs keys
				Handler.Control ('A', CmdHome),
				Handler.Control ('E', CmdEnd),
				Handler.Control ('B', CmdLeft),
				Handler.Control ('F', CmdRight),
				Handler.Control ('P', CmdHistoryPrev),
				Handler.Control ('N', CmdHistoryNext),
				Handler.Control ('K', CmdKillToEOF),
				Handler.Control ('Y', CmdYank),
				Handler.Control ('D', CmdDeleteChar),
				Handler.Control ('L', CmdRefresh),
				Handler.Control ('R', CmdReverseSearch),
				Handler.Control ('G', delegate {} ),
				Handler.Alt ('B', ConsoleKey.B, CmdBackwardWord),
				Handler.Alt ('F', ConsoleKey.F, CmdForwardWord),
				
				Handler.Alt ('D', ConsoleKey.D, CmdDeleteWord),
				Handler.Alt ((char) 8, ConsoleKey.Backspace, CmdDeleteBackword),
				
				// DEBUG
				Handler.Control ('T', CmdDebug),

				// quote
				Handler.Control ('Q', delegate { HandleChar (Console.ReadKey (true).KeyChar); })
			};

			_renderedText = new StringBuilder ();
			_text = new StringBuilder ();

			_history = new History (name, histsize);						
		}
        #endregion

        #region Commands
        void CmdDebug ()
		{
			_history.Dump ();
			Console.WriteLine ();
			Render ();
		}
        #endregion

		void Render ()
		{
			ColorConsole.Write (_shownPrompt);
			ColorConsole.Write (_renderedText.ToString());

			int max = System.Math.Max (_renderedText.Length + _shownPromptLength, _maxRendered);
			
			for (int i = _renderedText.Length + _shownPromptLength; i < _maxRendered; i++)
				ColorConsole.Write (" ");
			_maxRendered = _shownPromptLength + _renderedText.Length;

			// Write one more to ensure that we always wrap around properly if we are at the
			// end of a line.
			ColorConsole.Write (" ");

			UpdateHomeRow (max);
		}

		void UpdateHomeRow (int screenpos)
		{
			int lines = 1 + (screenpos / Console.WindowWidth);

			_homeRow = Console.CursorTop - (lines - 1);
			if (_homeRow < 0)
				_homeRow = 0;
		}
		

		void RenderFrom (int pos)
		{
			int rpos = TextToRenderPos (pos);
			int i;
			
			for (i = rpos; i < _renderedText.Length; i++)
				ColorConsole.Write (_renderedText [i].ToString());

			if ((_shownPromptLength + _renderedText.Length) > _maxRendered)
				_maxRendered = _shownPromptLength + _renderedText.Length;
			else {
				int max_extra = _maxRendered - _shownPromptLength;
				for (; i < max_extra; i++)
					ColorConsole.Write (" ");
			}
		}

		void ComputeRendered ()
		{
			_renderedText.Length = 0;

			for (int i = 0; i < _text.Length; i++){
				int c = (int) _text [i];
				if (c < 26){
					if (c == '\t')
						_renderedText.Append ("    ");
					else {
						_renderedText.Append ('^');
						_renderedText.Append ((char) (c + (int) 'A' - 1));
					}
				} else
					_renderedText.Append ((char)c);
			}
		}

		int TextToRenderPos (int pos)
		{
			int p = 0;

			for (int i = 0; i < pos; i++){
				int c;

				c = (int) _text [i];
				
				if (c < 26){
					if (c == 9)
						p += 4;
					else
						p += 2;
				} else
					p++;
			}

			return p;
		}

		int TextToScreenPos (int pos)
		{
			return _shownPromptLength + TextToRenderPos (pos);
		}
		
		string Prompt {
			get { return _prompt; }
			set { _prompt = value; }
		}

		int LineCount {
			get {
				return (_shownPromptLength + _renderedText.Length)/Console.WindowWidth;
			}
		}
		
		void ForceCursor (int newpos)
		{
			_cursor = newpos;

			int actual_pos = _shownPromptLength + TextToRenderPos (_cursor);
			int row = _homeRow + (actual_pos/Console.WindowWidth);
			int col = actual_pos % Console.WindowWidth;

			if (row >= Console.BufferHeight)
				row = Console.BufferHeight-1;
			Console.SetCursorPosition (col, row);
			
			//log.WriteLine ("Going to cursor={0} row={1} col={2} actual={3} prompt={4} ttr={5} old={6}", newpos, row, col, actual_pos, prompt.Length, TextToRenderPos (cursor), cursor);
			//log.Flush ();
		}

		void UpdateCursor (int newpos)
		{
			if (_cursor == newpos)
				return;

			ForceCursor (newpos);
		}

		void InsertChar (char c)
		{
			int prev_lines = LineCount;
			_text = _text.Insert (_cursor, c);
			ComputeRendered ();
			if (prev_lines != LineCount){

				Console.SetCursorPosition (0, _homeRow);
				Render ();
				ForceCursor (++_cursor);
			} else {
				RenderFrom (_cursor);
				ForceCursor (++_cursor);
				UpdateHomeRow (TextToScreenPos (_cursor));
			}
		}

		//
		// Commands
		//
		void CmdDone ()
		{
			_done = true;
		}

		void CmdTabOrComplete ()
		{
			bool complete = false;

			if (AutoCompleteEvent != null){
				for (int i = 0; i < _cursor; i++){
					if (!Char.IsWhiteSpace (_text [i])){
						complete = true;
						break;
					}
				}
				if (complete){
					string [] completions = AutoCompleteEvent (_text.ToString (), _cursor);
					if (completions == null || completions.Length == 0)
						return;
					
					if (completions.Length == 1){
						InsertTextAtCursor (completions [0]);
					} else {
						Console.WriteLine ();
						foreach (string s in completions){
							Console.Write (s);
							Console.Write (' ');
						}
						Console.WriteLine ();
						Render ();
						ForceCursor (_cursor);
					}
				} else
					HandleChar ('\t');
			} else
				HandleChar ('t');
		}
		
		void CmdHome ()
		{
			UpdateCursor (0);
		}

		void CmdEnd ()
		{
			UpdateCursor (_text.Length);
		}
		
		void CmdLeft ()
		{
			if (_cursor == 0)
				return;

			UpdateCursor (_cursor-1);
		}

		void CmdBackwardWord ()
		{
			int p = WordBackward (_cursor);
			if (p == -1)
				return;
			UpdateCursor (p);
		}

		void CmdForwardWord ()
		{
			int p = WordForward (_cursor);
			if (p == -1)
				return;
			UpdateCursor (p);
		}

		void CmdRight ()
		{
			if (_cursor == _text.Length)
				return;

			UpdateCursor (_cursor+1);
		}

		void RenderAfter (int p)
		{
			ForceCursor (p);
			RenderFrom (p);
			ForceCursor (_cursor);
		}
		
		void CmdBackspace ()
		{
			if (_cursor == 0)
				return;

			_text.Remove (--_cursor, 1);
			ComputeRendered ();
			RenderAfter (_cursor);
		}

		void CmdDeleteChar ()
		{
			// If there is no input, this behaves like EOF
			if (_text.Length == 0){
				_done = true;
				_text = null;
				Console.WriteLine ();
				return;
			}
			
			if (_cursor == _text.Length)
				return;
			_text.Remove (_cursor, 1);
			ComputeRendered ();
			RenderAfter (_cursor);
		}

		int WordForward (int p)
		{
			if (p >= _text.Length)
				return -1;

			int i = p;
			if (Char.IsPunctuation (_text [p]) || Char.IsWhiteSpace (_text[p])){
				for (; i < _text.Length; i++){
					if (Char.IsLetterOrDigit (_text [i]))
					    break;
				}
				for (; i < _text.Length; i++){
					if (!Char.IsLetterOrDigit (_text [i]))
					    break;
				}
			} else {
				for (; i < _text.Length; i++){
					if (!Char.IsLetterOrDigit (_text [i]))
					    break;
				}
			}
			if (i != p)
				return i;
			return -1;
		}

		int WordBackward (int p)
		{
			if (p == 0)
				return -1;

			int i = p-1;
			if (i == 0)
				return 0;
			
			if (Char.IsPunctuation (_text [i]) || Char.IsSymbol (_text [i]) || Char.IsWhiteSpace (_text[i])){
				for (; i >= 0; i--){
					if (Char.IsLetterOrDigit (_text [i]))
						break;
				}
				for (; i >= 0; i--){
					if (!Char.IsLetterOrDigit (_text[i]))
						break;
				}
			} else {
				for (; i >= 0; i--){
					if (!Char.IsLetterOrDigit (_text [i]))
						break;
				}
			}
			i++;
			
			if (i != p)
				return i;

			return -1;
		}
		
		void CmdDeleteWord ()
		{
			int pos = WordForward (_cursor);

			if (pos == -1)
				return;

			string k = _text.ToString (_cursor, pos-_cursor);
			
			if (_lastHandler == CmdDeleteWord)
				_killBuffer = _killBuffer + k;
			else
				_killBuffer = k;
			
			_text.Remove (_cursor, pos-_cursor);
			ComputeRendered ();
			RenderAfter (_cursor);
		}
		
		void CmdDeleteBackword ()
		{
			int pos = WordBackward (_cursor);
			if (pos == -1)
				return;

			string k = _text.ToString (pos, _cursor-pos);
			
			if (_lastHandler == CmdDeleteBackword)
				_killBuffer = k + _killBuffer;
			else
				_killBuffer = k;
			
			_text.Remove (pos, _cursor-pos);
			ComputeRendered ();
			RenderAfter (pos);
		}
		
		//
		// Adds the current line to the history if needed
		//
		void HistoryUpdateLine ()
		{
			_history.Update (_text.ToString ());
		}
		
		void CmdHistoryPrev ()
		{
			if (!_history.PreviousAvailable ())
				return;

			HistoryUpdateLine ();
			
			SetText (_history.Previous ());
		}

		void CmdHistoryNext ()
		{
			if (!_history.NextAvailable())
				return;

			_history.Update (_text.ToString ());
			SetText (_history.Next ());
			
		}

		void CmdKillToEOF ()
		{
			_killBuffer = _text.ToString (_cursor, _text.Length-_cursor);
			_text.Length = _cursor;
			ComputeRendered ();
			RenderAfter (_cursor);
		}

		void CmdYank ()
		{
			InsertTextAtCursor (_killBuffer);
		}

		void InsertTextAtCursor (string str)
		{
			int prev_lines = LineCount;
			_text.Insert (_cursor, str);
			ComputeRendered ();
			if (prev_lines != LineCount){
				Console.SetCursorPosition (0, _homeRow);
				Render ();
				_cursor += str.Length;
				ForceCursor (_cursor);
			} else {
				RenderFrom (_cursor);
				_cursor += str.Length;
				ForceCursor (_cursor);
				UpdateHomeRow (TextToScreenPos (_cursor));
			}
		}
		
		void SetSearchPrompt (string s)
		{
			SetPrompt ("(reverse-i-search)`" + s + "': ");
		}

		void ReverseSearch ()
		{
			int p;

			if (_cursor == _text.Length){
				// The cursor is at the end of the string
				
				p = _text.ToString ().LastIndexOf (_search);
				if (p != -1){
					_matchAt = p;
					_cursor = p;
					ForceCursor (_cursor);
					return;
				}
			} else {
				// The cursor is somewhere in the middle of the string
				int start = (_cursor == _matchAt) ? _cursor - 1 : _cursor;
				if (start != -1){
					p = _text.ToString ().LastIndexOf (_search, start);
					if (p != -1){
						_matchAt = p;
						_cursor = p;
						ForceCursor (_cursor);
						return;
					}
				}
			}

			// Need to search backwards in history
			HistoryUpdateLine ();
			string s = _history.SearchBackward (_search);
			if (s != null){
				_matchAt = -1;
				SetText (s);
				ReverseSearch ();
			}
		}
		
		void CmdReverseSearch ()
		{
			if (_searching == 0){
				_matchAt = -1;
				_lastSearch = _search;
				_searching = -1;
				_search = "";
				SetSearchPrompt ("");
			} else {
				if (_search == ""){
					if (_lastSearch != "" && _lastSearch != null){
						_search = _lastSearch;
						SetSearchPrompt (_search);

						ReverseSearch ();
					}
					return;
				}
				ReverseSearch ();
			} 
		}

		void SearchAppend (char c)
		{
			_search = _search + c;
			SetSearchPrompt (_search);

			//
			// If the new typed data still matches the current text, stay here
			//
			if (_cursor < _text.Length){
				string r = _text.ToString (_cursor, _text.Length - _cursor);
				if (r.StartsWith (_search))
					return;
			}

			ReverseSearch ();
		}
		
		void CmdRefresh ()
		{
			Console.Clear ();
			_maxRendered = 0;
			Render ();
			ForceCursor (_cursor);
		}

		void InterruptEdit (object sender, ConsoleCancelEventArgs a)
		{
			// Do not abort our program:
			a.Cancel = true;

			// Interrupt the editor
			_editThread.Abort();
		}

		void HandleChar (char c)
		{
			if (_searching != 0)
				SearchAppend (c);
			else
				InsertChar (c);
		}

		void EditLoop ()
		{
			ConsoleKeyInfo cki;

			while (!_done){
				cki = Console.ReadKey (true);

				bool handled = false;
				foreach (Handler handler in _handlers){
					ConsoleKeyInfo t = handler.CKI;

					if (t.Key == cki.Key && t.Modifiers == cki.Modifiers){
						handled = true;
						handler.KeyHandler ();
						_lastHandler = handler.KeyHandler;
						break;
					} else if (t.KeyChar == cki.KeyChar && t.Key == ConsoleKey.Zoom){
						handled = true;
						handler.KeyHandler ();
						_lastHandler = handler.KeyHandler;
						break;
					}
				}
				if (handled){
					if (_searching != 0){
						if (_lastHandler != CmdReverseSearch){
							_searching = 0;
							SetPrompt (_prompt);
						}
					}
					continue;
				}

				if (cki.KeyChar != (char) 0)
					HandleChar (cki.KeyChar);
			} 
		}

		void InitText (string initial)
		{
			_text = new StringBuilder (initial);
			ComputeRendered ();
			_cursor = _text.Length;
			Render ();
			ForceCursor (_cursor);
		}

		void SetText (string newtext)
		{
			Console.SetCursorPosition (0, _homeRow);
			InitText (newtext);
		}

		void SetPrompt (string newprompt)
		{
			_shownPrompt = newprompt;
			Console.SetCursorPosition (0, _homeRow);
			Render ();
			ForceCursor (_cursor);
		}
		
		public string Edit (string prompt, string initial)
		{
			_editThread = Thread.CurrentThread;
			_searching = 0;
			Console.CancelKeyPress += InterruptEdit;
			
			_done = false;
			_history.CursorToEnd();
			_maxRendered = 0;
			
			Prompt = prompt;
			_shownPrompt = prompt;
		    _shownPromptLength = ColorConsole.Calculate(prompt);
			InitText (initial);
			_history.Append (initial);

			do {
				try {
					EditLoop();
				} catch (ThreadAbortException){
					_searching = 0;
					Thread.ResetAbort();
					Console.WriteLine();
					SetPrompt(prompt);
					SetText("");
				}
			} while (!_done);
			Console.WriteLine();
			
			Console.CancelKeyPress -= InterruptEdit;

			if (_text == null){
				_history.Close();
				return null;
			}

			string result = _text.ToString();
			if (result != "")
				_history.Accept (result);
			else
				_history.RemoveLast();

			return result;
		}

		//
		// Emulates the bash-like behavior, where edits done to the
		// history are recorded
		//
		class History {
			string [] history;
			int head, tail;
			int cursor, count;
			string histfile;
			
			public History (string app, int size)
			{
				if (size < 1)
					throw new ArgumentException("size");

				if (app != null){
					string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
					//Console.WriteLine (dir);
					if (!Directory.Exists(dir)){
						try {
							Directory.CreateDirectory(dir);
						} catch {
							app = null;
						}
					}
					if (app != null)
						histfile = Path.Combine (dir, app) + ".history";
				}
				
				history = new string [size];
				head = tail = cursor = 0;

				if (File.Exists (histfile)){
					using (StreamReader sr = File.OpenText (histfile)){
						string line;
						
						while ((line = sr.ReadLine ()) != null){
							if (line != "")
								Append (line);
						}
					}
				}
			}

			public void Close ()
			{
				if (histfile == null)
					return;

				try {
					using (StreamWriter sw = File.CreateText (histfile)){
						int start = (count == history.Length) ? head : tail;
						for (int i = start; i < start+count; i++){
							int p = i % history.Length;
							sw.WriteLine (history [p]);
						}
					}
				} catch {
					// ignore
				}
			}
			
			//
			// Appends a value to the history
			//
			public void Append (string s)
			{
				//Console.WriteLine ("APPENDING {0} {1}", s, Environment.StackTrace);
				history [head] = s;
				head = (head+1) % history.Length;
				if (head == tail)
					tail = (tail+1 % history.Length);
				if (count != history.Length)
					count++;
			}

			//
			// Updates the current cursor location with the string,
			// to support editing of history items.   For the current
			// line to participate, an Append must be done before.
			//
			public void Update (string s)
			{
				history [cursor] = s;
			}

			public void RemoveLast ()
			{
				head = head-1;
				if (head < 0)
					head = history.Length-1;
			}
			
			public void Accept (string s)
			{
				int t = head-1;
				if (t < 0)
					t = history.Length-1;
				
				history [t] = s;
			}
			
			public bool PreviousAvailable ()
			{
				//Console.WriteLine ("h={0} t={1} cursor={2}", head, tail, cursor);
				if (count == 0 || cursor == tail)
					return false;

				return true;
			}

			public bool NextAvailable ()
			{
				int next = (cursor + 1) % history.Length;
				if (count == 0 || next > head)
					return false;

				return true;
			}
			
			
			//
			// Returns: a string with the previous line contents, or
			// nul if there is no data in the history to move to.
			//
			public string Previous ()
			{
				if (!PreviousAvailable ())
					return null;

				cursor--;
				if (cursor < 0)
					cursor = history.Length - 1;

				return history [cursor];
			}

			public string Next ()
			{
				if (!NextAvailable ())
					return null;

				cursor = (cursor + 1) % history.Length;
				return history [cursor];
			}

			public void CursorToEnd ()
			{
				if (head == tail)
					return;

				cursor = head;
			}

			public void Dump ()
			{
				Console.WriteLine ("Head={0} Tail={1} Cursor={2}", head, tail, cursor);
				for (int i = 0; i < history.Length;i++){
					Console.WriteLine (" {0} {1}: {2}", i == cursor ? "==>" : "   ", i, history[i]);
				}
				//log.Flush ();
			}

			public string SearchBackward (string term)
			{
				for (int i = 1; i < count; i++){
					int slot = cursor-i;
					if (slot < 0)
						slot = history.Length-1;
					if (history [slot] != null && history [slot].IndexOf (term) != -1){
						cursor = slot;
						return history [slot];
					}

					// Will the next hit tail?
					slot--;
					if (slot < 0)
						slot = history.Length-1;
					if (slot == tail)
						break;
				}

				return null;
			}
			
		}
	}


}

