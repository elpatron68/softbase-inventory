Imports System.IO
Imports System.Threading
Imports MahApps.Metro.Controls
Imports MahApps.Metro.Controls.Dialogs

Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private Shared ThisDevice As Device
    Private Shared Snapshots As List(Of Tuple(Of String, Integer)) = New List(Of Tuple(Of String, Integer))
    Private Shared DbDevices As List(Of Device) = New List(Of Device)
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Async Function InitWindowAsync() As Task
        If DBExists() = False Then
            Await ShowMessageAsync("No database file defined", "Please select a new or existing database file in the next dialog!")
            Dim SettingsWindow = New Settings
            SettingsWindow.ShowDialog()
        End If

        Database.CreateTables()

        ThisDevice = New Device With {
            .Uuid = GetWMI_Info.GetUUID(),
            .Hostname = System.Environment.MachineName
        }

        ThisDevice.DbID = Database.GetDeviceIdFromUuid(ThisDevice.Uuid)

        ' Debug stuff

        lbDeviceUUID.Content = ThisDevice.Uuid
        lbDeviceName.Content = ThisDevice.Hostname

        CbDevices.IsEnabled = False
        CbDevices.Items.Add("No devices in DB")
        CbDevices.SelectedIndex = 0

        CbSnapshots.IsEnabled = False
        CbSnapshots.Items.Add("Not avaiable")
        CbSnapshots.SelectedIndex = 0

        BtnExportPDF.IsEnabled = False
        BtnExportXls.IsEnabled = False

        DbDevices = Database.GetAllDevices()

        LblDatabase.Content = $"{My.Settings.databasefile}"
        LblStatus.Content = "Ready."

        If ThisDevice.DbID <> -1 Then
            LoadDevicesToCombobox()
            LoadSnapshotsForDevice()
        Else
            LblStatus.Content = "No data found for this device in our database. Click 'READ INSTALLED SOFTWARE'."
        End If
    End Function

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
        ' Save list to database
        Database.SaveSoftwareList(Softlist, ThisDevice, SnapshotID)
        ' Read from database to be sure everything worked (and to have it sorted by name)
        ReadSoftwarelistFromDb(ThisDevice, SnapshotID)
        ' Update listbox
        UpdateList(Softlist)
        ' Reread devices from database
        DbDevices = Database.GetAllDevices()
        ' Load devies to combobox
        LoadDevicesToCombobox()
        ' Load snapshots
        LoadSnapshotsForDevice()

        EnableControls(True)
        Mouse.OverrideCursor = Nothing
        LblStatus.Content = "Loaded and saved list of installed programs."
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

#Region "File exports"
    Private Sub BtnExportPDF_Click(sender As Object, e As RoutedEventArgs)
        Dim FileDialog = New SelectExportFile("pdf")
        Dim result = FileDialog.ShowDialog()
        Dim filename = SelectExportFile.FN
        Dim ts = CbSnapshots.SelectedItem
        If filename <> Nothing Then
            PdfExport.CreatePdf(filename, Softlist, ThisDevice, ts)
            LblStatus.Content = $"Software list exported to {filename}"
        Else
            LblStatus.Content = "Export cancelled"
        End If

    End Sub

    Private Sub BtnSaveXls_Click(sender As Object, e As RoutedEventArgs)
        Dim FileDialog = New SelectExportFile("xlsx")
        Dim result = FileDialog.ShowDialog()
        Dim filename = SelectExportFile.FN
        Dim ts = CbSnapshots.SelectedItem
        If filename <> Nothing Then
            ExcelExport.CreateXSLT(filename, Softlist, ThisDevice, ts)
            LblStatus.Content = $"Software list exported to {filename}"
        Else
            LblStatus.Content = "Export cancelled"
        End If
    End Sub

#End Region
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
                EnableControls(True)
                LblStatus.Content = $"Data loaded from database. Last updated: {lastupdate}"
            End If
        End If
    End Sub

    Private Sub EnableControls(ByVal enable As Boolean)
        BtnRetrieve.IsEnabled = enable
        BtnExportXls.IsEnabled = enable
        BtnExportPDF.IsEnabled = enable
        CbDevices.IsEnabled = enable
        CbSnapshots.IsEnabled = enable
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
        If ThisDevice.Snapshots.Count > 0 Then
            For Each s In ThisDevice.Snapshots
                CbSnapshots.Items.Add(s.Item1)
            Next
            CbSnapshots.IsEnabled = True
            CbSnapshots.SelectedIndex = 0
        Else
            LblStatus.Content = "No snapshots found for this device, select another device."
        End If
    End Sub

    Private Function DBExists() As Boolean
        If Not File.Exists(My.Settings.databasefile) Or My.Settings.databasefile = "" Then
            Return False
        Else
            Return True
        End If
    End Function


#Region "Events from ComboBoxes"
    Private Sub CbSnapshots_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbSnapshots.SelectionChanged
        If CbSnapshots.SelectedIndex = 0 Or CbSnapshots.SelectedIndex = -1 Then Exit Sub
        Dim index = ThisDevice.Snapshots(CbSnapshots.SelectedIndex - 1).Item2
        ReadSoftwarelistFromDb(ThisDevice, index)
    End Sub

    Private Sub CbDevices_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CbDevices.SelectionChanged
        If CbDevices.SelectedIndex = 0 Or CbDevices.SelectedIndex = -1 Then Exit Sub
        Dim index = CbDevices.SelectedIndex - 1
        Dim uuid = DbDevices(index).Uuid
        ThisDevice = Database.GetDeviceFromUuid(uuid)
        LoadSnapshotsForDevice()
    End Sub
#End Region

#Region "Menu click events"
    Private Sub MnExit_Click(sender As Object, e As RoutedEventArgs) Handles MnExit.Click
        Close()
    End Sub

    Private Sub MnSettings_Click(sender As Object, e As RoutedEventArgs) Handles MnSettings.Click
        Dim SettingsWindow = New Settings
        SettingsWindow.ShowDialog()
        LblDatabase.Content = $"{My.Settings.databasefile}"
    End Sub

    Private Sub MnDonate_Click(sender As Object, e As RoutedEventArgs) Handles MnDonate.Click
        Dim DonateWindow = New Donate
        DonateWindow.ShowDialog()
    End Sub

    Private Sub Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2K5Z6QV5GREA4&source=url")
    End Sub

#Region "License menu items"
    Private Sub SoftBase_Click(sender As Object, e As RoutedEventArgs) Handles SoftBase.Click
        Dim unused = Process.Start("http://www.gnu.de/documents/gpl-3.0.en.html")
    End Sub

    Private Sub CommonLogging_Click(sender As Object, e As RoutedEventArgs) Handles CommonLogging.Click
        Dim unused = Process.Start("https://raw.githubusercontent.com/net-commons/common-logging/master/license.txt")
    End Sub

    Private Sub ControlzEx_Click(sender As Object, e As RoutedEventArgs) Handles ControlzEx.Click
        Dim unused = Process.Start("https://raw.githubusercontent.com/ControlzEx/ControlzEx/master/LICENSE")
    End Sub

    Private Sub EntityFramework_Click(sender As Object, e As RoutedEventArgs) Handles EntityFramework.Click
        Process.Start("https://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm")
    End Sub

    Private Sub EPPlus_Click(sender As Object, e As RoutedEventArgs) Handles EPPlus.Click
        Process.Start("https://licenses.nuget.org/LGPL-3.0-or-later")
    End Sub


    Private Sub Itext7_Click(sender As Object, e As RoutedEventArgs) Handles itext7.Click
        Process.Start("https://www.gnu.org/licenses/agpl.html")
    End Sub

    Private Sub MahAppsMetro_Click(sender As Object, e As RoutedEventArgs) Handles MahAppsMetro.Click
        Process.Start("https://raw.githubusercontent.com/MahApps/MahApps.Metro/master/LICENSE")
    End Sub

    Private Sub MicrosoftCSharp_Click(sender As Object, e As RoutedEventArgs) Handles MicrosoftCSharp.Click
        Process.Start("https://raw.githubusercontent.com/dotnet/corefx/master/LICENSE.TXT")
    End Sub

    Private Sub PortableBouncyCastle_Click(sender As Object, e As RoutedEventArgs) Handles PortableBouncyCastle.Click
        Process.Start("https://www.bouncycastle.org/csharp/licence.html")
    End Sub

    Private Sub SystemDataSQLite_Click(sender As Object, e As RoutedEventArgs) Handles SystemDataSQLite.Click
        Process.Start("https://www.sqlite.org/copyright.html")
    End Sub

    Private Sub Mw_Loaded(sender As Object, e As RoutedEventArgs) Handles mw.Loaded
        InitWindowAsync()
    End Sub


#End Region

#End Region

End Class
