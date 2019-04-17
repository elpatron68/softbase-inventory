Imports System.Data.SQLite
Imports System.Management
Imports System.Threading
Imports MahApps.Metro.Controls
Imports NLog
Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private Shared Device As Device = New Device
    Private Shared DbDevices As List(Of Device) = New List(Of Device)
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
        DbDevices = Database.GetDevices()
        If DbDevices.Count > 0 Then
            For Each d In DbDevices
                CbDevices.Items.Add(d.Hostname)
            Next
        End If
        lbDeviceUUID.Content = Device.Uuid
        lbDeviceName.Content = Device.Hostname

        ReadSoftwarelistFromDb()
    End Sub

    Private Sub BtnRetrieve_Click(sender As Object, e As RoutedEventArgs)
        lbSoftware.Items.Clear()
        LblStatus.Content = "Loading list of installed programs, this may take a while, please be patient."
        LoadSoftwareList()
    End Sub

    Private Async Sub LoadSoftwareList()
        BtnRetrieve.IsEnabled = False
        BtnExportXls.IsEnabled = False
        BtnExportPDF.IsEnabled = False
        CbDevices.IsEnabled = False

        Try
            Dim cancellationTokenSource = New CancellationTokenSource()
            Me._cancelWork = Function()
                                 BtnRetrieve.IsEnabled = True
                                 BtnExportXls.IsEnabled = True
                                 BtnExportPDF.IsEnabled = True
                                 CbDevices.IsEnabled = True
                                 cancellationTokenSource.Cancel()
                                 Return Nothing
                             End Function

            Dim limit = 10
            Dim token = cancellationTokenSource.Token
            Await Task.Run(Function() DoWork(token), token)

        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
        Database.SaveList(Softlist, Device)
        ReadSoftwarelistFromDb()

        UpdateList(Softlist)
        BtnRetrieve.IsEnabled = True
        BtnExportXls.IsEnabled = True
        BtnExportPDF.IsEnabled = True
        CbDevices.IsEnabled = True
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

    Private Sub BtnSaveXls_Click(sender As Object, e As RoutedEventArgs)

    End Sub
    Private Sub UpdateList(ByVal softlist As List(Of software))
        lbSoftware.Items.Clear()
        For Each soft In softlist
            lbSoftware.Items.Add(soft.Name + " - Version " + soft.Version)
        Next
    End Sub
    Private Sub ReadSoftwarelistFromDb()
        Dim tmp = Database.LoadSoftwareListForDevice(Device)
        Dim lastupdate = tmp.Timestamp
        If lastupdate <> "-1" Then
            Softlist = tmp.Item1
            If Softlist.Count > 0 Then
                UpdateList(Softlist)
                LblStatus.Content = $"Data read from database. Last updated: {lastupdate}"
            End If
        End If
    End Sub
End Class
