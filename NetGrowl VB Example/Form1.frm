VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   5070
   ClientLeft      =   60
   ClientTop       =   450
   ClientWidth     =   6465
   LinkTopic       =   "Form1"
   ScaleHeight     =   5070
   ScaleWidth      =   6465
   StartUpPosition =   3  'Windows Default
   Begin VB.Frame Frame2 
      Caption         =   "Receive"
      Height          =   2655
      Left            =   240
      TabIndex        =   1
      Top             =   2280
      Width           =   6015
      Begin VB.CommandButton ClearButton 
         Caption         =   "Clear"
         Height          =   375
         Left            =   4680
         TabIndex        =   14
         Top             =   360
         Width           =   975
      End
      Begin VB.CommandButton StopReceiveButton 
         Caption         =   "StopListening"
         Height          =   375
         Left            =   1680
         TabIndex        =   4
         Top             =   360
         Width           =   1215
      End
      Begin VB.TextBox ReceivedTextBox 
         Height          =   1335
         Left            =   240
         MultiLine       =   -1  'True
         TabIndex        =   3
         Top             =   1080
         Width           =   5535
      End
      Begin VB.CommandButton ReceiveButton 
         Caption         =   "Start Listening"
         Height          =   375
         Left            =   360
         TabIndex        =   2
         Top             =   360
         Width           =   1215
      End
   End
   Begin VB.Frame Frame1 
      Caption         =   "Send"
      Height          =   1815
      Left            =   240
      TabIndex        =   0
      Top             =   240
      Width           =   6015
      Begin VB.CommandButton SendButton 
         Caption         =   "Send"
         Height          =   375
         Left            =   3720
         TabIndex        =   13
         Top             =   1320
         Width           =   1095
      End
      Begin VB.TextBox DescriptionTextBox 
         Height          =   285
         Left            =   1200
         TabIndex        =   12
         Text            =   "description goes here..."
         Top             =   1320
         Width           =   2175
      End
      Begin VB.TextBox TitleTextBox 
         Height          =   285
         Left            =   1200
         TabIndex        =   11
         Text            =   "Your Notification Title"
         Top             =   960
         Width           =   2175
      End
      Begin VB.TextBox PortTextBox 
         Height          =   285
         Left            =   1200
         TabIndex        =   10
         Text            =   "9887"
         Top             =   600
         Width           =   2175
      End
      Begin VB.TextBox IPAddressTextBox 
         Height          =   285
         Left            =   1200
         TabIndex        =   9
         Text            =   "127.0.0.1"
         Top             =   240
         Width           =   2175
      End
      Begin VB.Label Label4 
         Caption         =   "Description"
         Height          =   255
         Left            =   240
         TabIndex        =   8
         Top             =   1320
         Width           =   1815
      End
      Begin VB.Label Label3 
         Caption         =   "Title"
         Height          =   255
         Left            =   240
         TabIndex        =   7
         Top             =   960
         Width           =   1935
      End
      Begin VB.Label Label2 
         Caption         =   "Port"
         Height          =   255
         Left            =   240
         TabIndex        =   6
         Top             =   600
         Width           =   1335
      End
      Begin VB.Label Label1 
         Caption         =   "IP Address:"
         Height          =   255
         Left            =   240
         TabIndex        =   5
         Top             =   240
         Width           =   1335
      End
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private sender As MessageSender
Private WithEvents receiver As MessageReceiver
Attribute receiver.VB_VarHelpID = -1

Private Sub Form_Load()
    Set receiver = New MessageReceiver
    Set sender = New MessageSender
    
    Me.StopReceiveButton.Enabled = False
End Sub

Private Sub Form_Unload(Cancel As Integer)
    Call receiver.StopListening
End Sub

Private Sub ReceiveButton_Click()
    Call receiver.StartListening
    Me.ReceiveButton.Enabled = False
    Me.StopReceiveButton.Enabled = True
End Sub

Private Sub ClearButton_Click()
    Me.ReceivedTextBox.Text = ""
End Sub

Private Sub SendButton_Click()
    Call sender.Register
    Call sender.Notify(sender.InformationNotification, Me.TitleTextBox.Text, Me.DescriptionTextBox.Text, Vortex_Growl_Framework.priority.Priority_Normal, False)
End Sub

Private Sub StopReceiveButton_Click()
    Call receiver.StopListening
    Me.StopReceiveButton.Enabled = False
    Me.ReceiveButton.Enabled = True
End Sub

Private Sub receiver_RegistrationReceived(ByVal rp As Vortex_Growl_Framework.RegistrationPacket, ByVal receivedFrom As String)
    Me.ReceivedTextBox.Text = Me.ReceivedTextBox.Text & "REGISTERED: " & rp.ApplicationName & vbNewLine
    Dim nt As Variant
    For Each nt In rp.notificationTypes
        Debug.Print (nt.Name)
    Next
End Sub

Private Sub receiver_NotificationReceived(ByVal np As Vortex_Growl_Framework.NotificationPacket, ByVal receivedFrom As String)
    Me.ReceivedTextBox.Text = Me.ReceivedTextBox.Text & "NOTIFICATION: " & np.title & vbCrLf
End Sub
