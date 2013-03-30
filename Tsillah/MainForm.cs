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
		private AutomationFocusChangedEventHandler _focusHandler = null;
		private AutomationElement _currentElement;
		private AutomationEventHandler _textSelectionChangedEventHandler = null;

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
			_focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
			Automation.AddAutomationFocusChangedEventHandler(_focusHandler);
		}

		private void OnFocusChange(object sender, AutomationFocusChangedEventArgs e)
		{
			RemoveTextSelectionChangedEventHandler();
			AddTextSelectionChangedEventHandler();
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
			if (_focusHandler != null)
			{
				Automation.RemoveAutomationFocusChangedEventHandler(_focusHandler);
				_focusHandler = null;
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