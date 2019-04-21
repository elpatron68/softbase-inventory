Imports Microsoft.Win32

Public Class Settings
    Private Sub MetroWindow_Loaded(sender As Object, e As RoutedEventArgs)
        TxDatabasepath.Text = My.Settings.databasefile
    End Sub

    Private Sub MetroWindow_Closed(sender As Object, e As EventArgs)
        My.Settings.databasefile = TxDatabasepath.Text
        My.Settings.Save()
    End Sub

    Private Sub BtnBrowse_Click(sender As Object, e As RoutedEventArgs) Handles BtnBrowse.Click
        Dim Ofd As New OpenFileDialog()
        With Ofd
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            .FileName = "SoftBase-Inventory.sqlite"
            .CheckFileExists = False
            .CheckPathExists = True
            .Filter = "Sqlite files (*.sqlite)|*.sqlite|All files (*.*)|*.*"
        End With
        If Ofd.ShowDialog() = True Then
            TxDatabasepath.Text = Ofd.FileName
        End If
    End Sub
End Class
