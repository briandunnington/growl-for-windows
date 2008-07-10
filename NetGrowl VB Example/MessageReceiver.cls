VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "MessageReceiver"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Private WithEvents GrowlReceiver As Vortex_Growl_Framework.MessageReceiver
Attribute GrowlReceiver.VB_VarHelpID = -1
Public Event RegistrationReceived(ByVal rp As Vortex_Growl_Framework.RegistrationPacket, ByVal receivedFrom As String)
Public Event NotificationReceived(ByVal np As Vortex_Growl_Framework.NotificationPacket, ByVal receivedFrom As String)

'internal class events
Private Sub Class_Initialize()
    'On Error Resume Next

    'configure the GrowlReceiver
    Set GrowlReceiver = New Vortex_Growl_Framework.MessageReceiver
    GrowlReceiver.Port = 9887
    GrowlReceiver.AllowRemoteRegistration = True
    
End Sub

Private Sub Class_Terminate()
  On Error Resume Next
  
  Set GrowlReceiver = Nothing
End Sub

Public Sub StartListening()
    GrowlReceiver.Start
End Sub

Public Sub StopListening()
    GrowlReceiver.Stop
End Sub

Private Sub GrowlReceiver_RegistrationReceived(ByVal rp As Vortex_Growl_Framework.RegistrationPacket, ByVal receivedFrom As String)
    Debug.Print (rp.ApplicationName)
    RaiseEvent RegistrationReceived(rp, receivedFrom)
End Sub

Private Sub GrowlReceiver_NotificationReceived(ByVal np As Vortex_Growl_Framework.NotificationPacket, ByVal receivedFrom As String)
    Debug.Print (np.ApplicationName)
    RaiseEvent NotificationReceived(np, receivedFrom)
End Sub

