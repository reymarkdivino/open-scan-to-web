Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports NTwain
Imports NTwain.Data
Imports System.Drawing
Imports System.IO
Imports System.Net.WebSockets
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports Image = System.Drawing.Image

Public Class Form1
    Private scannedImages As New List(Of System.Drawing.Image) ' Store multiple scanned images
    Private currentImageIndex As Integer = -1 ' Track current image in PictureBox
    Private WithEvents btnSend As New Button ' Button for saving to PDF and sending to WebSocket
    'Private WithEvents btnScan As New Button ' Button for scanning
    Private WithEvents pictureBox As New PictureBox ' PictureBox for image preview
    Private WithEvents btnZoomIn As New Button ' Button for zooming in
    Private WithEvents btnZoomOut As New Button ' Button for zooming out
    Private WithEvents btnNext As New Button ' Button for next image
    Private WithEvents btnPrev As New Button ' Button for previous image
    Private WithEvents progressBar As New ProgressBar ' Loading indicator
    Private WithEvents txtSavePath As New TextBox ' TextBox for custom save path
    Private WithEvents btnSetPath As New Button ' Button to apply custom path
    Private zoomLevel As Single = 1.0F ' Current zoom level for PictureBox
    Private savePath As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) ' Default to Downloads

    ' Helper function to resize icons
    Private Function ResizeImage(sourceImage As Image, targetSize As Size) As Image
        Dim resizedImage As New Bitmap(targetSize.Width, targetSize.Height)
        Using g As Graphics = Graphics.FromImage(resizedImage)
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            g.DrawImage(sourceImage, 0, 0, targetSize.Width, targetSize.Height)
        End Using
        Return resizedImage
    End Function

    Private Async Function SendToWebSocket(base64Image As String) As Task
        Try
            Dim wsUri As New Uri("ws://localhost:8080") ' Placeholder WebSocket URL
            Using ws As New ClientWebSocket()
                Await ws.ConnectAsync(wsUri, CancellationToken.None)
                Dim buffer As Byte() = Encoding.UTF8.GetBytes(base64Image)
                Await ws.SendAsync(New ArraySegment(Of Byte)(buffer), WebSocketMessageType.Text, True, CancellationToken.None)
                Await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None)
            End Using
            Me.Invoke(Sub() MessageBox.Show("Websocket IN!"))
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error sa WebSocket: " & ex.Message))
        End Try
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set form size
        Me.Size = New Size(800, 600)
        Me.MinimumSize = New Size(600, 400) ' Ensure form is resizable but not too small

        ' Setup PictureBox (responsive to form size)
        pictureBox.Size = New Size(CInt(Me.ClientSize.Width * 0.7), CInt(Me.ClientSize.Height * 0.7))
        pictureBox.Location = New Point(10, 10)
        pictureBox.SizeMode = PictureBoxSizeMode.Zoom
        pictureBox.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(pictureBox)

        ' Setup Scan button
        btnScan.Text = "Scan"
        btnScan.Size = New Size(100, 30)
        btnScan.Location = New Point(10, Me.ClientSize.Height - 120)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "scan.png"))
            btnScan.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnScan.ImageAlign = ContentAlignment.MiddleLeft
            btnScan.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading scan icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnScan)

        ' Setup Send button
        btnSend.Text = "Save as PDF and Send"
        btnSend.Size = New Size(150, 30)
        btnSend.Location = New Point(120, Me.ClientSize.Height - 120)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "save.png"))
            btnSend.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnSend.ImageAlign = ContentAlignment.MiddleLeft
            btnSend.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading save icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnSend)

        ' Setup Zoom In button
        btnZoomIn.Text = "Zoom In"
        btnZoomIn.Size = New Size(100, 30)
        btnZoomIn.Location = New Point(280, Me.ClientSize.Height - 120)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "zoom_in.png"))
            btnZoomIn.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnZoomIn.ImageAlign = ContentAlignment.MiddleLeft
            btnZoomIn.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading zoom in icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnZoomIn)

        ' Setup Zoom Out button
        btnZoomOut.Text = "Zoom Out"
        btnZoomOut.Size = New Size(100, 30)
        btnZoomOut.Location = New Point(390, Me.ClientSize.Height - 120)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "zoom_out.png"))
            btnZoomOut.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnZoomOut.ImageAlign = ContentAlignment.MiddleLeft
            btnZoomOut.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading zoom out icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnZoomOut)

        ' Setup Previous button
        btnPrev.Text = "Previous"
        btnPrev.Size = New Size(100, 30)
        btnPrev.Location = New Point(10, Me.ClientSize.Height - 80)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "previous.png"))
            btnPrev.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnPrev.ImageAlign = ContentAlignment.MiddleLeft
            btnPrev.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading previous icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnPrev)

        ' Setup Next button
        btnNext.Text = "Next"
        btnNext.Size = New Size(100, 30)
        btnNext.Location = New Point(120, Me.ClientSize.Height - 80)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "next.png"))
            btnNext.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnNext.ImageAlign = ContentAlignment.MiddleLeft
            btnNext.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading next icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnNext)

        ' Setup Save Path TextBox
        txtSavePath.Size = New Size(200, 30)
        txtSavePath.Location = New Point(230, Me.ClientSize.Height - 80)
        txtSavePath.Text = savePath ' Default to Downloads
        Me.Controls.Add(txtSavePath)

        ' Setup Set Path button
        btnSetPath.Text = "Set Path"
        btnSetPath.Size = New Size(100, 30)
        btnSetPath.Location = New Point(440, Me.ClientSize.Height - 80)
        Try
            Dim icon As Image = Image.FromFile(Path.Combine(Application.StartupPath, "Icons", "folder.png"))
            btnSetPath.Image = ResizeImage(icon, New Size(24, 24))
            icon.Dispose()
            btnSetPath.ImageAlign = ContentAlignment.MiddleLeft
            btnSetPath.TextAlign = ContentAlignment.MiddleRight
        Catch ex As Exception
            Me.Invoke(Sub() MessageBox.Show("Error loading folder icon: " & ex.Message))
        End Try
        Me.Controls.Add(btnSetPath)

        ' Setup ProgressBar
        progressBar.Size = New Size(200, 30)
        progressBar.Location = New Point(230, Me.ClientSize.Height - 40)
        progressBar.Style = ProgressBarStyle.Marquee
        progressBar.Visible = False ' Hidden by default
        Me.Controls.Add(progressBar)

        ' Handle form resize to make PictureBox and controls responsive
        AddHandler Me.Resize, Sub()
                                  pictureBox.Size = New Size(CInt(Me.ClientSize.Width * 0.7), CInt(Me.ClientSize.Height * 0.7))
                                  btnScan.Location = New Point(10, Me.ClientSize.Height - 120)
                                  btnSend.Location = New Point(120, Me.ClientSize.Height - 120)
                                  btnZoomIn.Location = New Point(280, Me.ClientSize.Height - 120)
                                  btnZoomOut.Location = New Point(390, Me.ClientSize.Height - 120)
                                  btnPrev.Location = New Point(10, Me.ClientSize.Height - 80)
                                  btnNext.Location = New Point(120, Me.ClientSize.Height - 80)
                                  txtSavePath.Location = New Point(230, Me.ClientSize.Height - 80)
                                  btnSetPath.Location = New Point(440, Me.ClientSize.Height - 80)
                                  progressBar.Location = New Point(230, Me.ClientSize.Height - 40)
                              End Sub
    End Sub

    Private Sub btnSetPath_Click(sender As Object, e As EventArgs) Handles btnSetPath.Click
        Try
            Dim newPath As String = txtSavePath.Text.Trim()
            If String.IsNullOrEmpty(newPath) Then
                savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                MessageBox.Show("Save path reset to Downloads folder.")
            ElseIf Directory.Exists(newPath) Then
                savePath = newPath
                MessageBox.Show("Save path set to: " & savePath)
            Else
                MessageBox.Show("Invalid path. Please enter a valid directory.")
            End If
            txtSavePath.Text = savePath
        Catch ex As Exception
            MessageBox.Show("Error setting save path: " & ex.Message)
        End Try
    End Sub

    Private Async Sub btnScan_Click(sender As Object, e As EventArgs) Handles btnScan.Click
        Try
            ' Show loading indicator
            Me.Invoke(Sub()
                          progressBar.Visible = True
                          btnScan.Enabled = False ' Disable scan button during scanning
                      End Sub)

            ' Initialize TWAIN session
            Dim appId = TWIdentity.CreateFromAssembly(DataGroups.Image, GetType(Form1).Assembly)
            Dim twainApp As New TwainSession(appId)
            twainApp.SynchronizationContext = Nothing
            twainApp.Open()

            ' List available scanners for debugging
            Dim sources = twainApp.GetSources().ToList()
            If Not sources.Any() Then
                Me.Invoke(Sub() MessageBox.Show("No TWAIN-compatible scanners detected. Please ensure your scanner is connected and drivers are installed."))
                twainApp.Close()
                Me.Invoke(Sub()
                              progressBar.Visible = False
                              btnScan.Enabled = True
                          End Sub)
                Return
            End If
            Me.Invoke(Sub() MessageBox.Show("Detected scanners: " & String.Join(", ", sources.Select(Function(s) s.Name))))

            ' Use ShowSourceSelector as per original logic
            Dim dataSource = twainApp.ShowSourceSelector()
            If dataSource Is Nothing Then
                Me.Invoke(Sub() MessageBox.Show("Walang napiling scanner o kinansela ang pagpili."))
                twainApp.Close()
                Me.Invoke(Sub()
                              progressBar.Visible = False
                              btnScan.Enabled = True
                          End Sub)
                Return
            End If

            ' Open the selected data source
            dataSource.Open()

            ' Set scanning parameters
            If dataSource.Capabilities.CapXferCount.CanSet Then
                dataSource.Capabilities.CapXferCount.SetValue(1)
            End If
            If dataSource.Capabilities.ICapPixelType.CanSet AndAlso
               dataSource.Capabilities.ICapPixelType.GetValues().Contains(PixelType.RGB) Then
                dataSource.Capabilities.ICapPixelType.SetValue(PixelType.RGB)
            End If
            If dataSource.Capabilities.ICapUnits.CanSet AndAlso
               dataSource.Capabilities.ICapUnits.GetValues().Contains(Unit.Inches) Then
                dataSource.Capabilities.ICapUnits.SetValue(Unit.Inches)
            End If
            If dataSource.Capabilities.ICapXResolution.CanSet Then
                dataSource.Capabilities.ICapXResolution.SetValue(New TWFix32 With {.Whole = 300, .Fraction = 0})
            End If
            If dataSource.Capabilities.ICapYResolution.CanSet Then
                dataSource.Capabilities.ICapYResolution.SetValue(New TWFix32 With {.Whole = 300, .Fraction = 0})
            End If

            ' Handle DataTransferred event
            Dim scanCompleted As Boolean = False
            AddHandler twainApp.DataTransferred, Sub(s, ea)
                                                     Using stream As Stream = ea.GetNativeImageStream()
                                                         If stream IsNot Nothing AndAlso stream.Length > 0 Then
                                                             Try
                                                                 ' Create a new Bitmap to avoid GDI+ issues
                                                                 Dim tempImage As New Bitmap(stream)
                                                                 Dim newImage As New Bitmap(tempImage) ' Clone to prevent disposal issues
                                                                 scannedImages.Add(newImage) ' Add to list
                                                                 tempImage.Dispose()
                                                                 currentImageIndex = scannedImages.Count - 1 ' Show latest image
                                                                 If newImage IsNot Nothing Then
                                                                     Me.Invoke(Sub()
                                                                                   pictureBox.Image = scannedImages(currentImageIndex) ' Display in PictureBox
                                                                                   MessageBox.Show("Image successfully captured and displayed.")
                                                                               End Sub)
                                                                 Else
                                                                     Me.Invoke(Sub() MessageBox.Show("Failed to create image from stream."))
                                                                 End If
                                                             Catch ex As Exception
                                                                 Me.Invoke(Sub() MessageBox.Show("Error processing image stream: " & ex.Message))
                                                             End Try
                                                         Else
                                                             Me.Invoke(Sub() MessageBox.Show("Walang na-scan na larawan o walang laman ang stream."))
                                                         End If
                                                     End Using
                                                 End Sub

            ' Handle SourceDisabled event
            AddHandler twainApp.SourceDisabled, Sub(s, ea)
                                                    scanCompleted = True
                                                    Me.Invoke(Sub()
                                                                  progressBar.Visible = False ' Hide loading indicator
                                                                  btnScan.Enabled = True ' Re-enable scan button
                                                              End Sub)
                                                End Sub

            ' Enable scanner
            dataSource.Enable(SourceEnableMode.ShowUI, True, Me.Handle)

            ' Wait for scan to complete
            While Not scanCompleted
                Thread.Sleep(100)
                Application.DoEvents()
            End While

            dataSource.Close()
            twainApp.Close()
        Catch ex As Exception
            Me.Invoke(Sub()
                          MessageBox.Show("Error sa pag-scan: " & ex.Message)
                          progressBar.Visible = False ' Hide loading indicator on error
                          btnScan.Enabled = True
                      End Sub)
        End Try
    End Sub

    Private Async Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        If scannedImages.Count > 0 Then
            Try
                ' Show loading indicator
                Me.Invoke(Sub()
                              progressBar.Visible = True
                              btnSend.Enabled = False ' Disable send button during processing
                          End Sub)

                ' Save each image as a separate PDF
                Dim savedFiles As New List(Of String)()
                For i As Integer = 0 To scannedImages.Count - 1
                    Dim img = scannedImages(i)
                    If img.Width > 0 AndAlso img.Height > 0 Then
                        Dim filePath As String = Path.Combine(savePath, $"scan_pdf{i + 1}.pdf")
                        Using document As New Document(PageSize.A4, 10, 10, 10, 10)
                            Using stream As New FileStream(filePath, FileMode.Create)
                                PdfWriter.GetInstance(document, stream)
                                document.Open()
                                Dim pdfImg As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(img, System.Drawing.Imaging.ImageFormat.Png)
                                pdfImg.ScaleToFit(document.PageSize.Width - 20, document.PageSize.Height - 20)
                                document.Add(pdfImg)
                                document.Close()
                            End Using
                        End Using
                        ' Verify PDF file exists and has content
                        If File.Exists(filePath) AndAlso New FileInfo(filePath).Length > 0 Then
                            savedFiles.Add(filePath)
                            ' Convert PDF to base64 and send to WebSocket
                            Dim pdfBytes As Byte() = File.ReadAllBytes(filePath)
                            Dim base64Pdf As String = Convert.ToBase64String(pdfBytes)
                            Await SendToWebSocket(base64Pdf)
                        Else
                            Me.Invoke(Sub() MessageBox.Show($"PDF {filePath} is empty or was not created correctly."))
                        End If
                    Else
                        Me.Invoke(Sub() MessageBox.Show($"Skipping invalid image {i + 1} with zero dimensions."))
                    End If
                Next

                ' Hide loading indicator and show result
                Me.Invoke(Sub()
                              progressBar.Visible = False
                              btnSend.Enabled = True
                              If savedFiles.Count > 0 Then
                                  MessageBox.Show("PDFs successfully saved at: " & String.Join(", ", savedFiles))
                              Else
                                  MessageBox.Show("No valid PDFs were saved.")
                              End If
                          End Sub)
            Catch ex As Exception
                Me.Invoke(Sub()
                              MessageBox.Show("Error sa pag-save o pag-send: " & ex.Message)
                              progressBar.Visible = False
                              btnSend.Enabled = True
                          End Sub)
            End Try
        Else
            Me.Invoke(Sub()
                          MessageBox.Show("Walang larawan na na-scan para i-save.")
                          progressBar.Visible = False
                          btnSend.Enabled = True
                      End Sub)
        End If
    End Sub

    Private Sub btnZoomIn_Click(sender As Object, e As EventArgs) Handles btnZoomIn.Click
        If pictureBox.Image IsNot Nothing Then
            zoomLevel += 0.1F
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage
            pictureBox.Width = CInt(pictureBox.Width * 1.1)
            pictureBox.Height = CInt(pictureBox.Height * 1.1)
        End If
    End Sub

    Private Sub btnZoomOut_Click(sender As Object, e As EventArgs) Handles btnZoomOut.Click
        If pictureBox.Image IsNot Nothing AndAlso zoomLevel > 0.2F Then
            zoomLevel -= 0.1F
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage
            pictureBox.Width = CInt(pictureBox.Width / 1.1)
            pictureBox.Height = CInt(pictureBox.Height / 1.1)
        End If
    End Sub

    Private Sub btnPrev_Click(sender As Object, e As EventArgs) Handles btnPrev.Click
        If scannedImages.Count > 0 AndAlso currentImageIndex > 0 Then
            currentImageIndex -= 1
            pictureBox.Image = scannedImages(currentImageIndex)
            zoomLevel = 1.0F
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom
            pictureBox.Width = CInt(Me.ClientSize.Width * 0.7)
            pictureBox.Height = CInt(Me.ClientSize.Height * 0.7)
        End If
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        If scannedImages.Count > 0 AndAlso currentImageIndex < scannedImages.Count - 1 Then
            currentImageIndex += 1
            pictureBox.Image = scannedImages(currentImageIndex)
            zoomLevel = 1.0F
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom
            pictureBox.Width = CInt(Me.ClientSize.Width * 0.7)
            pictureBox.Height = CInt(Me.ClientSize.Height * 0.7)
        End If
    End Sub
End Class