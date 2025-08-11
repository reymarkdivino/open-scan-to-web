<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        btnScan = New Button()
        SuspendLayout()
        ' 
        ' btnScan
        ' 
        btnScan.Location = New Point(221, 57)
        btnScan.Name = "btnScan"
        btnScan.Size = New Size(156, 76)
        btnScan.TabIndex = 0
        btnScan.Text = "Button1"
        btnScan.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(660, 353)
        Controls.Add(btnScan)
        Name = "Form1"
        Text = "Open Scan To Web Gateway"
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnScan As Button

End Class
