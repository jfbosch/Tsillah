using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Forms;
/*
	http://social.msdn.microsoft.com/Forums/en-US/windowsaccessibilityandautomation/thread/c0618787-aea4-4cce-9714-fd61dd5677b6
 
	1. Use the UI Automation eventing system to watch for TextSelectionChanged events.  (You can try this with AccEvent.)  You'll get one whenever the cursor moves.
	I meant that event specifically.  In the COM client for UIA, you register for an AutomationEvent and pass UIA_Text_TextSelectionChangedEventId as the event
	you are requesting.  In the managed client, you would call AddAutomationEventHandler and request the TextSelectionChangedEvent as the event ID.

	2. Query the source of the event for its Text Pattern.  It will should have one.  (Some text sources don't have Text Pattern, but they don't fire that event, either.)

	3. Ask the Text Pattern for its selection by calling GetSelection.  The selection, if its just the cursor, will be empty.

	4. Call the selection range's ExpandToEnclosingUnit() method with TextUnit_Character.  Now you have the character to the right of where the cursor is.

	5. Call the expanded range's GetBoundingRectangles() method to find out where that character is.  The cursor will be on the left edge of the character.
*/

namespace UIATracer
{
	public partial class MainForm : Form
	{
		private AutomationFocusChangedEventHandler focusHandler = null;
		private AutomationEventHandler UIAeventHandler;
		private AutomationElement currentElement;

		public MainForm()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			SubscribeToFocusChange();
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			UnsubscribeFocusChange();
		}

		/// <summary>
		/// Create an event handler and register it.
		/// </summary>
		public void SubscribeToFocusChange()
		{
			focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
			Automation.AddAutomationFocusChangedEventHandler(focusHandler);
		}

		/// <summary>
		/// Handle the event.
		/// </summary>
		/// <param name="src">Object that raised the event.</param>
		/// <param name="e">Event arguments.</param>
		private void OnFocusChange(object src, AutomationFocusChangedEventArgs e)
		{
			//BookMark??
			//if (UIAeventHandler != null)
			//{
			//   Automation.RemoveAutomationEventHandler (InvokePattern.InvokedEvent,
			//       currentElement, UIAeventHandler);
			//}

			currentElement = AutomationElement.FocusedElement;
			if (currentElement != null)
			{
				// Limit to VS WPF editors
				if (currentElement.Current.AutomationId != "WpfTextView")
					return;

				SystemSounds.Beep.Play();
				//txtOutput.Text += currentElement.Current.Name;
				//txtOutput.Text += Environment.NewLine;


				Automation.AddAutomationEventHandler(
					TextPattern.TextSelectionChangedEvent,
					currentElement,
					TreeScope.Element,
					new AutomationEventHandler(OnTextSelectionChangedEvent));

				//Automation.AddAutomationEventHandler(
				//	InvokePattern.InvokedEvent,
				//	currentElement, TreeScope.Element,
				//	UIAeventHandler = new AutomationEventHandler(OnUIAutomationEvent));

			}
			else
				txtOutput.Text = "(no element)";
		}

		private void OnTextSelectionChangedEvent(object sender, AutomationEventArgs e)
		{
			var element = (AutomationElement)sender;
			var patterns = element.GetSupportedPatterns();
			try
			{
				var pattern = (TextPattern)element.GetCurrentPattern(TextPatternIdentifiers.Pattern);
				var range = pattern.GetSelection();
				if (range.Length >= 1)
				{
					range[0].ExpandToEnclosingUnit(TextUnit.Character);
					var rect = range[0].GetBoundingRectangles();
					if (rect.Length >= 1)
					{
						this.MoveMousePointer((int)rect[0].X, (int)rect[0].Y);
						Debug.Print("rect: " + rect[0].X + ", " + rect[0].Y);
					}
				}
			}
			catch (Exception ex)
			{
				txtOutput.Text = ex.Message;
			}


			//10014; TextPatternIdentifiers.Pattern
			//foreach (var p in patterns)
			//{
			//	txtOutput.Text += p.Id + "; " + p.ProgrammaticName;
			//	txtOutput.Text += Environment.NewLine;
			//}

			//txtOutput.Text += element.GetCurrentPattern
			//txtOutput.Text += sender.ToString();
			//txtOutput.Text += Environment.NewLine;
		}

		/// <summary>
		/// Cancel subscription to the event.
		/// </summary>
		public void UnsubscribeFocusChange()
		{
			if (focusHandler != null)
				Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
		}


		//------------------------------------------------


		private void ShutdownUIA()
		{
			Automation.RemoveAllEventHandlers();
		}

		private void OnUIAutomationEvent(object src, AutomationEventArgs e)
		{
			SystemSounds.Beep.Play();

			// Make sure the element still exists. Elements such as tooltips
			// can disappear before the event is processed.
			AutomationElement sourceElement;
			try
			{
				sourceElement = src as AutomationElement;
			}
			catch (ElementNotAvailableException)
			{
				return;
			}

			if (e.EventId == InvokePattern.InvokedEvent)
			{
				SystemSounds.Beep.Play();
			}
			else
			{
				// TODO Handle any other events that have been subscribed to.
			}
		}

		private void MoveMousePointer(
			int x,
			int y)
		{
			this.Cursor = new Cursor(Cursor.Current.Handle);
			Cursor.Position = new Point(x, y);
			//Cursor.Clip = new Rectangle(this.Location, this.Size);
		}

	}
}