Imports System.Data.SQLite
Imports System.Management
Imports System.Threading
Imports MahApps.Metro.Controls
Imports NLog
Class MainWindow
    Private Shared Softlist As List(Of software) = New List(Of software)
    Private _cancelWork As Action

    Public Sub New()
        InitializeComponent()
        Dim uuid = GetWMI_Info.GetUUID()
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
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

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim sqlite_conn As SQLiteConnection

        ' create a new database connection:
        sqlite_conn = New SQLiteConnection("Data Source=database.sqlite;Version=3;")

        ' open the connection:
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SOFTWARE] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [NAME] NVARCHAR(2048) NULL,
                                  [VERSION] NVARCHAR(2048) NULL)"
        sqlite_cmd.ExecuteNonQuery()
        'sqlite_cmd.CommandText = "DELETE FROM SOFTWARE"
        'sqlite_cmd.ExecuteNonQuery()

        For Each soft In Softlist
            sqlite_cmd.CommandText = $"INSERT INTO SOFTWARE (NAME, VERSION) VALUES ('{soft.Name}', '{soft.Version}');"
            sqlite_cmd.ExecuteNonQuery()
        Next
        LblStatus.Content = "Database saved."
        PdfExport.CreatePdf("test.pdf", Softlist)
    End Sub

    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        PdfExport.CreatePdf("test.pdf", Softlist)
        LblStatus.Content = "Software list exported to 'test.pdf'"
    End Sub

End Class
