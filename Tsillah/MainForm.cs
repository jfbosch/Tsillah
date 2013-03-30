/*
	Thanks to: http://social.msdn.microsoft.com/Forums/en-US/windowsaccessibilityandautomation/thread/c0618787-aea4-4cce-9714-fd61dd5677b6
*/

using System;
using System.Drawing;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Forms;

namespace Tsillah
{
	public partial class MainForm : Form
	{
		private AutomationFocusChangedEventHandler _focusChangedHandler = null;
		private AutomationEventHandler _textSelectionChangedEventHandler = null;
		private AutomationEventHandler _textChangedEventHandler = null;
		private AutomationElement _currentElement = null;

		public MainForm()
		{
			InitializeComponent();
		}

		private void ShutdownUia()
		{
			Automation.RemoveAllEventHandlers();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			SubscribeToFocusChange();
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			UnsubscribeFocusChange();
			ShutdownUia();
		}

		public void SubscribeToFocusChange()
		{
			_focusChangedHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
			Automation.AddAutomationFocusChangedEventHandler(_focusChangedHandler);
		}

		private void OnFocusChange(object sender, AutomationFocusChangedEventArgs e)
		{
			_currentElement = AutomationElement.FocusedElement;
			if (_currentElement == null)
				return;

			// Limit to VS WPF editors
			if (_currentElement.Current.AutomationId != "WpfTextView")
				return;

			this.UiFeedback(
				string.Format("hooking into control {0}, {2} in ProcessId {1}", _currentElement.Current.AutomationId, _currentElement.Current.ProcessId, _currentElement.Current.ControlType.ProgrammaticName));

			this.RemoveTextSelectionChangedEventHandlerIfSet();
			this.AddTextSelectionChangedEventHandler();

			this.RemoveTextChangedEventHandlerIfSet();
			this.AddTextChangedEventHandler();
		}

		private void AddTextSelectionChangedEventHandler()
		{
			_textSelectionChangedEventHandler = new AutomationEventHandler(TextEventHandler);
			this.AddEventHandler(TextPattern.TextSelectionChangedEvent, _textSelectionChangedEventHandler);
		}

		private void AddTextChangedEventHandler()
		{
			_textChangedEventHandler = new AutomationEventHandler(TextEventHandler);
			this.AddEventHandler(TextPattern.TextChangedEvent, _textChangedEventHandler);
		}

		private void AddEventHandler(
			AutomationEvent automationEvent,
			AutomationEventHandler eventHandler)
		{
			_currentElement = AutomationElement.FocusedElement;
			if (_currentElement != null)
			{
				Automation.AddAutomationEventHandler(
					automationEvent,
					_currentElement,
					TreeScope.Element,
					eventHandler);
			}
			else
				this.UiFeedback("(no element)");
		}

		private void RemoveTextSelectionChangedEventHandlerIfSet()
		{
			if (_currentElement != null && _textSelectionChangedEventHandler != null)
			{
				Automation.RemoveAutomationEventHandler(
					TextPattern.TextSelectionChangedEvent,
					_currentElement,
					_textSelectionChangedEventHandler);
			}
		}

		private void RemoveTextChangedEventHandlerIfSet()
		{
			if (_currentElement != null && _textChangedEventHandler != null)
			{
				Automation.RemoveAutomationEventHandler(
					TextPattern.TextChangedEvent,
					_currentElement,
					_textChangedEventHandler);
			}
		}

		private void TextEventHandler(object sender, AutomationEventArgs e)
		{
			//if (e.EventId == InvokePattern.InvokedEvent.)

			// Make sure the element still exists. Elements such as tooltips
			// can disappear before the event is processed.
			AutomationElement element;
			try
			{
				element = sender as AutomationElement;

				var pattern = (TextPattern)element.GetCurrentPattern(TextPatternIdentifiers.Pattern);
				var ranges = pattern.GetSelection();
				if (ranges.Length == 1)
				{
					var range = ranges[0].Clone();
					range.ExpandToEnclosingUnit(TextUnit.Character);
					var rectangles = range.GetBoundingRectangles();
					if (rectangles.Length == 1)
					{
						var rect = rectangles[0];
						this.MoveMousePointer(rect);
					}
				}
			}
			catch (ElementNotAvailableException)
			{
				return;
			}
			catch (Exception ex)
			{
				this.UiFeedback(ex.Message);
			}
		}

		public void UnsubscribeFocusChange()
		{
			if (_focusChangedHandler != null)
			{
				Automation.RemoveAutomationFocusChangedEventHandler(_focusChangedHandler);
				_focusChangedHandler = null;
			}
		}

		private void MoveMousePointer(
			System.Windows.Rect rect)
		{
			//this.Cursor = new Cursor(Cursor.Current.Handle);

			var halfCarotHeight = rect.Height / 2;
			var halfCursorHeight = Cursor.Size.Height / 2;
			var offset = (int)halfCarotHeight;
			Cursor.Position = new Point((int)rect.X, (int)rect.Y + offset);
			
			// For some reason, Hide() does not hide the mouse cursor.
			//Cursor.Hide();
		}

		private void UiFeedback(
			params string[] info)
		{
			MethodInvoker updateUI = delegate()
			{
				foreach (string line in info)
				{
					txtOutput.Text += line;
					txtOutput.Text += Environment.NewLine;
					txtOutput.SelectionStart = txtOutput.TextLength;
				}
			};
			if (this.InvokeRequired)
			{
				this.Invoke(updateUI);
			}
			else
			{
				updateUI();
			}
		}

	}
}