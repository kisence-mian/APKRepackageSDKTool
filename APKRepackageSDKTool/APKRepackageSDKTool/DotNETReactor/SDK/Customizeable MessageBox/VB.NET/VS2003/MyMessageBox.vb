Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms

Public Class MyMessageBox
    Inherits System.Windows.Forms.Form
    Implements MessageBoxInterface
    Private panel2 As System.Windows.Forms.Panel
    Private cationlabel As System.Windows.Forms.Label
    Private textlabel As System.Windows.Forms.Label
    Private button1 As System.Windows.Forms.Button
    Private colorpanel1 As System.Windows.Forms.Panel
    Private colorpanel2 As System.Windows.Forms.Panel
    Private components As System.ComponentModel.Container = Nothing

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Property MessageBoxCaption() As String Implements MessageBoxInterface.MessageBoxCaption
        Get
            Return Me.cationlabel.Text
        End Get
        Set(ByVal Value As String)
            Me.cationlabel.Text = Value
        End Set
    End Property

    Public Property MessageBoxText() As String Implements MessageBoxInterface.MessageBoxText
        Get
            Return Me.textlabel.Text
        End Get
        Set(ByVal Value As String)
            Me.textlabel.Text = Value
        End Set
    End Property

    Public Property MessageBoxGradientBegin() As Color Implements MessageBoxInterface.MessageBoxGradientBegin
        Get
            Return Me.colorpanel1.BackColor
        End Get
        Set(ByVal Value As Color)
            Me.colorpanel1.BackColor = Value
        End Set
    End Property

    Public Property MessageBoxGradientEnd() As Color Implements MessageBoxInterface.MessageBoxGradientEnd
        Get
            Return Me.colorpanel2.BackColor
        End Get
        Set(ByVal Value As Color)
            Me.colorpanel2.BackColor = Value
        End Set
    End Property

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private Sub InitializeComponent()
        Me.panel2 = New System.Windows.Forms.Panel
        Me.cationlabel = New System.Windows.Forms.Label
        Me.textlabel = New System.Windows.Forms.Label
        Me.button1 = New System.Windows.Forms.Button
        Me.colorpanel1 = New System.Windows.Forms.Panel
        Me.colorpanel2 = New System.Windows.Forms.Panel
        Me.panel2.SuspendLayout()
        Me.SuspendLayout()
        Me.panel2.BackColor = System.Drawing.Color.FromArgb(CType((192), System.Byte), CType((255), System.Byte), CType((255), System.Byte))
        Me.panel2.Controls.Add(Me.cationlabel)
        Me.panel2.Location = New System.Drawing.Point(8, 8)
        Me.panel2.Name = "panel2"
        Me.panel2.Size = New System.Drawing.Size(232, 32)
        Me.panel2.TabIndex = 1
        Me.cationlabel.BackColor = System.Drawing.Color.FromArgb(CType((192), System.Byte), CType((255), System.Byte), CType((255), System.Byte))
        Me.cationlabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 9)
        Me.cationlabel.Location = New System.Drawing.Point(8, 8)
        Me.cationlabel.Name = "cationlabel"
        Me.cationlabel.Size = New System.Drawing.Size(216, 16)
        Me.cationlabel.TabIndex = 0
        Me.cationlabel.Text = "Caption"
        Me.cationlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.textlabel.BackColor = System.Drawing.Color.White
        Me.textlabel.Location = New System.Drawing.Point(8, 48)
        Me.textlabel.Name = "textlabel"
        Me.textlabel.Size = New System.Drawing.Size(232, 104)
        Me.textlabel.TabIndex = 1
        Me.textlabel.Text = "Text"
        Me.button1.BackColor = System.Drawing.SystemColors.Control
        Me.button1.Location = New System.Drawing.Point(160, 160)
        Me.button1.Name = "button1"
        Me.button1.Size = New System.Drawing.Size(80, 23)
        Me.button1.TabIndex = 2
        Me.button1.Text = "OK"
        AddHandler Me.button1.Click, AddressOf Me.button1_Click
        Me.colorpanel1.Location = New System.Drawing.Point(8, 160)
        Me.colorpanel1.Name = "colorpanel1"
        Me.colorpanel1.Size = New System.Drawing.Size(24, 24)
        Me.colorpanel1.TabIndex = 3
        Me.colorpanel2.Location = New System.Drawing.Point(40, 160)
        Me.colorpanel2.Name = "colorpanel2"
        Me.colorpanel2.Size = New System.Drawing.Size(24, 24)
        Me.colorpanel2.TabIndex = 4
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(246, 188)
        Me.Controls.Add(Me.colorpanel2)
        Me.Controls.Add(Me.colorpanel1)
        Me.Controls.Add(Me.panel2)
        Me.Controls.Add(Me.button1)
        Me.Controls.Add(Me.textlabel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Name = "MyMessageBox"
        Me.Text = "MyOwnMessageBox"
        Me.panel2.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Private Sub button1_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Me.Close()
    End Sub
End Class

