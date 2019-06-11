Imports System
Imports System.Drawing

Public Interface MessageBoxInterface

    Property MessageBoxCaption() As String

    Property MessageBoxText() As String

    Property MessageBoxGradientBegin() As System.Drawing.Color

    Property MessageBoxGradientEnd() As System.Drawing.Color
End Interface