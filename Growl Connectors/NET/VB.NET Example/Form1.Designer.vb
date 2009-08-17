<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.button2 = New System.Windows.Forms.Button
        Me.label3 = New System.Windows.Forms.Label
        Me.label2 = New System.Windows.Forms.Label
        Me.textBox3 = New System.Windows.Forms.TextBox
        Me.textBox2 = New System.Windows.Forms.TextBox
        Me.button1 = New System.Windows.Forms.Button
        Me.label1 = New System.Windows.Forms.Label
        Me.textBox1 = New System.Windows.Forms.TextBox
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(196, 113)
        Me.button2.Name = "button2"
        Me.button2.Size = New System.Drawing.Size(75, 23)
        Me.button2.TabIndex = 15
        Me.button2.Text = "Send"
        Me.button2.UseVisualStyleBackColor = True
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(11, 67)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(84, 13)
        Me.label3.TabIndex = 14
        Me.label3.Text = "Notification Text"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(8, 13)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(83, 13)
        Me.label2.TabIndex = 13
        Me.label2.Text = "Notification Title"
        '
        'textBox3
        '
        Me.textBox3.Location = New System.Drawing.Point(8, 86)
        Me.textBox3.Name = "textBox3"
        Me.textBox3.Size = New System.Drawing.Size(264, 20)
        Me.textBox3.TabIndex = 12
        Me.textBox3.Text = "This notification came from Sample App."
        '
        'textBox2
        '
        Me.textBox2.Location = New System.Drawing.Point(8, 32)
        Me.textBox2.Name = "textBox2"
        Me.textBox2.Size = New System.Drawing.Size(264, 20)
        Me.textBox2.TabIndex = 11
        Me.textBox2.Text = "You received a notification"
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(204, 72)
        Me.button1.Name = "button1"
        Me.button1.Size = New System.Drawing.Size(75, 23)
        Me.button1.TabIndex = 10
        Me.button1.Text = "Register"
        Me.button1.UseVisualStyleBackColor = True
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(12, 26)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(90, 13)
        Me.label1.TabIndex = 9
        Me.label1.Text = "Application Name"
        '
        'textBox1
        '
        Me.textBox1.Location = New System.Drawing.Point(12, 45)
        Me.textBox1.Name = "textBox1"
        Me.textBox1.Size = New System.Drawing.Size(268, 20)
        Me.textBox1.TabIndex = 8
        Me.textBox1.Text = "Sample VB.NET App"
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.label2)
        Me.Panel1.Controls.Add(Me.button2)
        Me.Panel1.Controls.Add(Me.textBox2)
        Me.Panel1.Controls.Add(Me.label3)
        Me.Panel1.Controls.Add(Me.textBox3)
        Me.Panel1.Location = New System.Drawing.Point(4, 101)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(286, 160)
        Me.Panel1.TabIndex = 16
        Me.Panel1.Visible = False
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.button1)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.textBox1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents textBox3 As System.Windows.Forms.TextBox
    Private WithEvents textBox2 As System.Windows.Forms.TextBox
    Private WithEvents button1 As System.Windows.Forms.Button
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents textBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel

End Class
