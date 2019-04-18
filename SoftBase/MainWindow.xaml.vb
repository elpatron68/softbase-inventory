Imports System.IO
Imports System.Threading
Imports MahApps.Metro.Controls
Imports NLog
Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private Shared ReadOnly Device As Device = New Device
    Private Shared DbDevices As List(Of Device) = New List(Of Device)
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
        If DBExists() = False Then
            Dim SettingsWindow = New Settings
            SettingsWindow.ShowDialog()
        End If

        ' Debug stuff
        ' Database.GetSnapshots(1)
        ' Database.AddSnapshot(Device)

        lbDeviceUUID.Content = Device.Uuid
        lbDeviceName.Content = Device.Hostname
        ReadDevices()
        ReadSoftwarelistFromDb()
    End Sub

    Private Sub BtnRetrieve_Click(sender As Object, e As RoutedEventArgs)
        lbSoftware.Items.Clear()
        LblStatus.Content = "Loading list of installed programs, this may take a while, please be patient."
        LoadSoftwareList()
    End Sub

    Private Async Sub LoadSoftwareList()
        EnableControls(False)
        Mouse.OverrideCursor = Cursors.Wait

        Try
            Dim cancellationTokenSource = New CancellationTokenSource()
            Me._cancelWork = Function()
                                 EnableControls(True)
                                 Mouse.OverrideCursor = Nothing
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
        EnableControls(True)
        Mouse.OverrideCursor = Nothing
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
        Dim tmp = Database.LoadSoftwareListForDevice(Device, 1)
        Dim lastupdate = tmp.Timestamp
        If lastupdate <> "-1" Then
            Softlist = tmp.Item1
            If Softlist.Count > 0 Then
                UpdateList(Softlist)
                LblStatus.Content = $"Data loaded from database. Last updated: {lastupdate}"
            End If
        End If
    End Sub

    Private Sub EnableControls(ByVal enable As Boolean)
        BtnRetrieve.IsEnabled = enable
        BtnExportXls.IsEnabled = enable
        BtnExportPDF.IsEnabled = enable
        CbDevices.IsEnabled = enable
    End Sub

    Private Sub ReadDevices()
        CbSnapshots.IsEnabled = False
        CbSnapshots.Items.Add("Select snapshot")
        CbSnapshots.SelectedIndex = 0
        CbDevices.Items.Add("Select device from DB")
        CbDevices.SelectedIndex = 0
        DbDevices = Database.GetDevices()
        If DbDevices.Count > 0 Then
            For Each d In DbDevices
                CbDevices.Items.Add(d.Hostname)
            Next
        Else
            CbDevices.Items(0) = "No devices in DB"
            CbSnapshots.Items(0) = "Not avaiable"
            CbDevices.IsEnabled = False
            CbSnapshots.IsEnabled = False
        End If
    End Sub

    Private Function DBExists() As Boolean
        If Not File.Exists(My.Settings.databasefile) Or My.Settings.databasefile = "" Then
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub MnExit_Click(sender As Object, e As RoutedEventArgs) Handles MnExit.Click
        Close()
    End Sub

    Private Sub MnSettings_Click(sender As Object, e As RoutedEventArgs) Handles MnSettings.Click
        Dim SettingsWindow = New Settings
        SettingsWindow.ShowDialog()
    End Sub

    Private Sub MnDonate_Click(sender As Object, e As RoutedEventArgs) Handles MnDonate.Click
        Dim DonateWindow = New Donate
        DonateWindow.ShowDialog()
    End Sub

    Private Sub Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2K5Z6QV5GREA4&source=url")
    End Sub
End Class
