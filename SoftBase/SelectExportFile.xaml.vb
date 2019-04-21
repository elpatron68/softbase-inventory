Imports Microsoft.Win32

Public Class SelectExportFile
    Public Shared FN As String
    Public Shared FT As String

    Public Sub New(ByVal filetype)
        InitializeComponent()
        FT = filetype
    End Sub

    Private Sub BtnBrowse_Click(sender As Object, e As RoutedEventArgs) Handles BtnBrowse.Click
        Dim Ofd As New OpenFileDialog()
        With Ofd
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            .CheckFileExists = False
            .CheckPathExists = True
            If FT = "pdf" Then
                .Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
                .FileName = "SoftBase-Inventory-Export.pdf"
            ElseIf FT = "xslt" Then
                .Filter = "Excel files (*.xslt)|*.xslt|All files (*.*)|*.*"
                .FileName = "SoftBase-Inventory-Export.xslt"
            End If
        End With
        If Ofd.ShowDialog() = True Then
            TxExportFile.Text = Ofd.FileName
        End If
    End Sub

    Private Sub Export_Click(sender As Object, e As RoutedEventArgs) Handles Export.Click
        FN = TxExportFile.Text
        Me.Close()
    End Sub
End Class
