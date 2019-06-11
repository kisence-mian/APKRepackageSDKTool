using System;
using System.Drawing;

public interface MessageBoxInterface
{
	string MessageBoxCaption{get;set;}
	string MessageBoxText{get;set;}
	System.Drawing.Color MessageBoxGradientBegin{get;set;}
	System.Drawing.Color MessageBoxGradientEnd{get;set;}
}

