Tsillah
=======

This is a small standalone .Net .exe that locks the mouse pointer to the text cursor so that the mouse pointer shadows the text cursor while moving in Visual Studio's WPF editors. In other words, the mouse pointer is reposition to where ever the text cursor is if the text cursor is moved, either by using the arrow keys on the keyboard, or by entering new text on the keyboard.

Usage couldn't be simpler. Just run Tsillah.exe. It will hook into any VS 2012 code editor that gets focus.

Why
===

In short, Accessibility.
I have been a Visual Studio user since VS 2003. While most developers welcomed the switch to WPF code editors in Visual Studio 2010, I was not one of them. While I am not blind, I cannot see much, and require magnification software and screen readers to work on a computer. I use a package called Zoomtext by AI Squared.  http://www.aisquared.com/zoomtext
The problem is that Zoomtext does not have the ability to track the WPF text cursor so that the magnified portion of the screen follows the cursor. That is quite a problem as you might well imagine.

My workaround for the last 3 years has been to use the built in zooming capability of the code editor in Visual Studio. While this is a workable solution, it has several drawbacks:
1. The text in the code editor is huge while the menus and all other tool windows, solution explorer, tool tips, IntelliSense, etc. remains unzoomed.
2. We have a distributed team, and screen sharing during pair programming does not work, as the massively zoomed text is shared, and not very easy to follow for a person who can see just fine.

By using Tsillah, I can run the VS code editor at the normal WPF zoom size, and zoom with Zoomtext instead. Since Zoomtext can track the mouse pointer just fine, the result is that now Zoomtext can track the WPF text cursor also, by proxy.

How
===

I used .Net UI Automation: http://msdn.microsoft.com/en-us/library/windows/desktop/ee684009.aspx
When the program starts up, it registers handlers for window focus changed events across the entire desktop. When it detects that a Visual Studio 2012 WPF text editor has received focus, it registers handlers for text changed and text selection changed events. This allows it to extract the coordinates of the text cursor when it moves as a result of keyboard input, and to reposition the mouse pointer.

The name?
=========

Tsillah: Hebrew name meaning "shade, shadow." In the bible, this is the name of Lamech's second wife.
