Imports System.Data.SQLite
Imports System.Management
Imports System.Threading
Imports MahApps.Metro.Controls
Imports NLog
Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private Shared Device As Device = New Device
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
        lbDeviceUUID.Content = Device.Uuid
        lbDeviceName.Content = Device.Hostname
        Dim dbid = Database.GetIdFromUuid(Device.Uuid)
        If dbid = -1 Then
            Database.AddDevice(Device)
        Else
            MsgBox(dbid.ToString)
        End If
    End Sub

    Private Sub BtnRetrieve_Click(sender As Object, e As RoutedEventArgs)
        LblStatus.Content = "Loading list of installed programs, this may take a while, please be patient."
        LoadSoftwareList()
    End Sub

    Private Async Sub LoadSoftwareList()
        BtnRetrieve.IsEnabled = False
        BtnSaveDb.IsEnabled = False
        lbSoftware.Items.Clear()

        Try
            Dim cancellationTokenSource = New CancellationTokenSource()
            Me._cancelWork = Function()
                                 BtnRetrieve.IsEnabled = True
                                 BtnSaveDb.IsEnabled = True
                                 cancellationTokenSource.Cancel()
                                 Return Nothing
                             End Function

            Dim limit = 10
            Dim token = cancellationTokenSource.Token
            Await Task.Run(Function() DoWork(token), token)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        For Each soft In Softlist
            lbSoftware.Items.Add(soft.Name + " - Version " + soft.Version)
        Next
        BtnRetrieve.IsEnabled = True
        BtnSaveDb.IsEnabled = True
        LblStatus.Content = "List of installed programs updated."
    End Sub

    Private Function DoWork(ByVal token As CancellationToken) As Integer
        Dim moReturn As Management.ManagementObjectCollection
        Dim moSearch As Management.ManagementObjectSearcher
        Dim mo As Management.ManagementObject

        moSearch = New Management.ManagementObjectSearcher("Select Name, Version from Win32_Product")
        moReturn = moSearch.Get

        For Each mo In moReturn
            Dim soft As software = New software
            soft.Name = mo("Name").ToString
            Softlist.Add(soft)
            soft.Version = mo("Version").ToString
        Next
        Return Nothing
    End Function

    Private Sub BtnSaveDb_Click(sender As Object, e As RoutedEventArgs)
        Database.SaveList(Softlist, Device)
        LblStatus.Content = "Database saved."
    End Sub

    Private Sub BtnExportPDF_Click(sender As Object, e As RoutedEventArgs)
        PdfExport.CreatePdf("test.pdf", Softlist)
        LblStatus.Content = "Software list exported to 'test.pdf'"
    End Sub

End Class
