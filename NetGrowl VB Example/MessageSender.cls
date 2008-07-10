VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "MessageSender"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
Option Explicit

Private GrowlSender As Vortex_Growl_Framework.NetGrowl
Attribute GrowlSender.VB_VarHelpID = -1
Public WarningNotification As Vortex_Growl_Framework.NotificationType
Public InformationNotification As Vortex_Growl_Framework.NotificationType

'internal class events
Private Sub Class_Initialize()
    'On Error Resume Next

    'configure the GrowlSender
    Set GrowlSender = New Vortex_Growl_Framework.NetGrowl
    GrowlSender.ApplicationName = "Test Application"
    GrowlSender.IPAddress = "127.0.0.1"
    GrowlSender.Port = 9887
    
    'set up the notification types
    Set WarningNotification = New Vortex_Growl_Framework.NotificationType
    WarningNotification.Name = "Warning"
    Set InformationNotification = New Vortex_Growl_Framework.NotificationType
    InformationNotification.Name = "Information"
End Sub

Private Sub Class_Terminate()
  On Error Resume Next
  
  Set GrowlSender = Nothing
End Sub

Public Sub Register()
    Dim notificationTypes(1) As Vortex_Growl_Framework.NotificationType
    Set notificationTypes(0) = WarningNotification
    Set notificationTypes(1) = InformationNotification
    
    Call GrowlSender.Register(notificationTypes)
End Sub

Public Sub Notify(ByRef nt As Vortex_Growl_Framework.NotificationType, ByRef title As String, ByRef description As String, ByRef priority As Vortex_Growl_Framework.priority, ByRef sticky As Boolean)
    Call GrowlSender.Notify(nt, title, description, priority, sticky)
End Sub

