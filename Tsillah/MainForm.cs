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
			this.RemoveTextSelectionChangedEventHandler();
			this.AddTextSelectionChangedEventHandler();

			this.RemoveTextChangedEventHandler();
			this.AddTextChangedEventHandler();
		}

		private void AddTextSelectionChangedEventHandler()
		{
			_currentElement = AutomationElement.FocusedElement;
			if (_currentElement != null)
			{
				// Limit to VS WPF editors
				if (_currentElement.Current.AutomationId != "WpfTextView")
					return;

				_textSelectionChangedEventHandler = new AutomationEventHandler(OnTextSelectionChanged);
				Automation.AddAutomationEventHandler(
					TextPattern.TextSelectionChangedEvent,
					_currentElement,
					TreeScope.Element,
					_textSelectionChangedEventHandler);
			}
			else
				txtOutput.Text = "(no element)";
		}

		private void AddTextChangedEventHandler()
		{
			_currentElement = AutomationElement.FocusedElement;
			if (_currentElement != null)
			{
				// Limit to VS WPF editors
				if (_currentElement.Current.AutomationId != "WpfTextView")
					return;

				_textChangedEventHandler = new AutomationEventHandler(OnTextChanged);
				Automation.AddAutomationEventHandler(
					TextPattern.TextChangedEvent,
					_currentElement,
					TreeScope.Element,
					_textChangedEventHandler);
			}
			else
				txtOutput.Text = "(no element)";
		}

		private void OnTextChanged(object sender, AutomationEventArgs e)
		{
			// Make sure the element still exists. Elements such as tooltips
			// can disappear before the event is processed.
			AutomationElement element;
			try
			{
				element = sender as AutomationElement;
			}
			catch (ElementNotAvailableException)
			{
				return;
			}

			try
			{
				var pattern = (TextPattern)element.GetCurrentPattern(TextPatternIdentifiers.Pattern);
				var ranges = pattern.GetSelection();
				if (ranges.Length >= 1)
				{
					var range = ranges[0];
					range.ExpandToEnclosingUnit(TextUnit.Character);
					var rectangles = range.GetBoundingRectangles();
					if (rectangles.Length >= 1)
					{
						var rect = rectangles[0];
						this.MoveMousePointer((int)rect.X, (int)rect.Y + 10);
					}
				}
			}
			catch (Exception ex)
			{
				txtOutput.Text = ex.Message;
			}
		}

		private void RemoveTextSelectionChangedEventHandler()
		{
			if (_currentElement != null && _textSelectionChangedEventHandler != null)
			{
				Automation.RemoveAutomationEventHandler(
					TextPattern.TextSelectionChangedEvent,
					_currentElement,
					_textSelectionChangedEventHandler);
			}
		}

		private void RemoveTextChangedEventHandler()
		{
			if (_currentElement != null && _textChangedEventHandler != null)
			{
				Automation.RemoveAutomationEventHandler(
					TextPattern.TextChangedEvent,
					_currentElement,
					_textChangedEventHandler);
			}
		}

		private void OnTextSelectionChanged(object sender, AutomationEventArgs e)
		{
			//if (e.EventId == InvokePattern.InvokedEvent.)

			// Make sure the element still exists. Elements such as tooltips
			// can disappear before the event is processed.
			AutomationElement element;
			try
			{
				element = sender as AutomationElement;
			}
			catch (ElementNotAvailableException)
			{
				return;
			}

			try
			{
				var pattern = (TextPattern)element.GetCurrentPattern(TextPatternIdentifiers.Pattern);
				var ranges = pattern.GetSelection();
				if (ranges.Length >= 1)
				{
					var range = ranges[0];
					range.ExpandToEnclosingUnit(TextUnit.Character);
					var rectangles = range.GetBoundingRectangles();
					if (rectangles.Length >= 1)
					{
						var rect = rectangles[0];
						this.MoveMousePointer((int)rect.X, (int)rect.Y + 10);
					}
				}
			}
			catch (Exception ex)
			{
				txtOutput.Text = ex.Message;
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
			int x,
			int y)
		{
			//this.Cursor = new Cursor(Cursor.Current.Handle);
			Cursor.Position = new Point(x, y);
			// For some reason, Hide() does not hide the mouse cursor.
			//Cursor.Hide();
		}

	}
}