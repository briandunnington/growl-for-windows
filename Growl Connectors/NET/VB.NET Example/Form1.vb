Imports Growl.Connector

Public Class Form1

    Dim WithEvents growl As GrowlConnector
    Dim nt As NotificationType
    Dim app As Growl.Connector.Application
    Dim sampleNotificationType As String = "SAMPLE_NOTIFICATION"


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.nt = New NotificationType(Me.sampleNotificationType, "Sample Notification")

        Me.growl = New GrowlConnector()
        'Me.growl = New GrowlConnector("password")    ' use this if you need to set a password - you can also pass null or an empty string to this constructor to use no password
        'Me.growl = New GrowlConnector("password", "hostname", GrowlConnector.TCP_PORT)   ' use this if you want to connect to a remote Growl instance on another machine

        ' set this so messages are sent in plain text (easier for debugging)
        Me.growl.EncryptionAlgorithm = Cryptography.SymmetricAlgorithmType.PlainText
    End Sub


    Private Sub button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button1.Click
        Me.app = New Growl.Connector.Application(Me.textBox1.Text)
        Dim types() As NotificationType = New NotificationType() {Me.nt}

        Me.growl.Register(Me.app, types)

        Me.Panel1.Visible = True
    End Sub

    Private Sub button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button2.Click
        Dim cc As CallbackContext = New CallbackContext("some fake information", "fake data")

        Dim n As New Notification(Me.app.Name, Me.nt.Name, DateTime.Now.Ticks.ToString(), Me.textBox2.Text, Me.textBox3.Text)
        Me.growl.Notify(n, cc)
    End Sub

    Private Sub growl_NotificationCallback(ByVal response As Response, ByVal callbackData As CallbackData, ByVal state As Object) Handles growl.NotificationCallback
        Dim text As String = String.Format("Response Type: {0} - Notification ID: {1} - Callback Data: {2} - Callback Data Type: {3}", callbackData.Result, callbackData.NotificationID, callbackData.Data, callbackData.Type)
        MessageBox.Show(text, "Callback received", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
    End Sub

End Class
