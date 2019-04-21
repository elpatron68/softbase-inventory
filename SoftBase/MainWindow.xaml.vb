Imports System.IO
Imports System.Threading
'Imports MahApps.Metro.Controls
Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private Shared ThisDevice As Device
    Private Shared Snapshots As List(Of Tuple(Of String, Integer)) = New List(Of Tuple(Of String, Integer))
    Private Shared DbDevices As List(Of Device) = New List(Of Device)
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
        Database.CreateTables()

        If DBExists() = False Then
            Dim SettingsWindow = New Settings
            SettingsWindow.ShowDialog()
        End If

        ThisDevice = New Device With {
            .Uuid = GetWMI_Info.GetUUID(),
            .Hostname = System.Environment.MachineName
        }

        ThisDevice.DbID = Database.GetDeviceIdFromUuid(ThisDevice.Uuid)

        ' Debug stuff
        ' Database.GetSnapshots(1)
        ' Database.AddSnapshot(Device)

        lbDeviceUUID.Content = ThisDevice.Uuid
        lbDeviceName.Content = ThisDevice.Hostname

        CbDevices.IsEnabled = False
        CbDevices.Items.Add("No devices in DB")
        CbDevices.SelectedIndex = 0

        CbSnapshots.IsEnabled = False
        CbSnapshots.Items.Add("Not avaiable")
        CbSnapshots.SelectedIndex = 0

        DbDevices = Database.GetAllDevices()

        If ThisDevice.DbID <> -1 Then
            LoadSnapshotsForDevice()
            LoadDevicesToCombobox()
            LoadSnapshotsForDevice()
        Else
            LblStatus.Content = "No data found for this device in our database. Click 'READ INSTALLED SOFTWARE'."
        End If
    End Sub

    Private Sub BtnRetrieve_Click(sender As Object, e As RoutedEventArgs)
        lbSoftware.Items.Clear()
        LblStatus.Content = "Loading list of installed programs, this may take a while, please be patient."
        LoadSoftwareListFromWMI()
    End Sub

    Private Async Sub LoadSoftwareListFromWMI()
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
        If ThisDevice.DbID = -1 Then
            ThisDevice.DbID = Database.AddDevice(ThisDevice)
        End If
        Dim SnapshotID = Database.AddNewSnapshot(ThisDevice)
        Database.SaveSoftwareList(Softlist, ThisDevice, SnapshotID)
        ReadSoftwarelistFromDb(ThisDevice, SnapshotID)

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
        'Database.SaveSoftwareList(Softlist, ThisDevice)
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

    Private Sub ReadSoftwarelistFromDb(ByVal device As Device, ByVal SnapshotId As Integer)
        Dim tmp = Database.LoadSoftwareListForDevice(device, SnapshotId)
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

    Private Sub LoadDevicesToCombobox()
        If DbDevices.Count > 0 Then
            CbDevices.Items.Clear()
            CbDevices.Items.Add("Select device from DB")
            For Each d In DbDevices
                CbDevices.Items.Add(d.Hostname)
            Next
            CbDevices.IsEnabled = True
            CbDevices.SelectedIndex = 0
        End If
    End Sub

    Private Sub LoadSnapshotsForDevice()
        CbSnapshots.Items.Clear()
        CbSnapshots.Items.Add("Select Snapshot")
        ThisDevice.Snapshots = Database.GetAllSnapshotIdForDevice(ThisDevice.DbID)
        For Each s In ThisDevice.Snapshots
            CbSnapshots.Items.Add(s.Item1)
        Next
        CbSnapshots.IsEnabled = True
        CbSnapshots.SelectedIndex = 0
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

    Private Sub CbSnapshots_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbSnapshots.SelectionChanged
        If CbSnapshots.SelectedIndex = 0 Or CbSnapshots.SelectedIndex = -1 Then Exit Sub
        'Dim Snapshotindex As Integer = 0
        'For Each i In Snapshots
        '    If i.Item1 = CbSnapshots.SelectedItem Then
        '        Snapshotindex = i.Item2
        '    End If
        'Next

        'For Each d In DbDevices
        '    If d.Hostname = CbDevices.SelectedValue Then
        '        ThisDevice = d
        '    End If
        'Next
        'ReadSoftwarelistFromDb(ThisDevice, Snapshotindex)

    End Sub

    Private Sub CbDevices_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbDevices.SelectionChanged
        If CbDevices.SelectedIndex = 0 Or CbDevices.SelectedIndex = -1 Then Exit Sub
        For Each d As Device In DbDevices
            If d.Hostname = CbDevices.Text Then
                ThisDevice = Database.GetDeviceFromUuid(d.Uuid)
                LoadSnapshotsForDevice()
            End If
        Next
    End Sub
End Class
