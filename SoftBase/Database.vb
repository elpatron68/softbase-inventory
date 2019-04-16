Imports System.Data.SQLite

Public Class Database
    Private Const dbfile As String = "database.sqlite"
    Public Shared Sub SaveList(ByVal Softlist As List(Of software), ByVal device As Device)
        Dim sqlite_conn As SQLiteConnection
        Dim DeviceId As Integer = GetIdFromUuid(device.Uuid)
        ' create a new database connection:
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")

        ' open the connection:
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SOFTWARE] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [MACHINEID] NVARCHAR(2048) NULL,
                                  [NAME] NVARCHAR(2048) NULL,
                                  [VERSION] NVARCHAR(2048) NULL)"
        sqlite_cmd.ExecuteNonQuery()

        For Each soft In Softlist
            sqlite_cmd.CommandText = $"INSERT INTO SOFTWARE (MACHINEID, NAME, VERSION) VALUES ('{DeviceId}', '{soft.Name}', '{soft.Version}');"
            sqlite_cmd.ExecuteNonQuery()
        Next
    End Sub

    Public Shared Sub LoadListForDevice(ByVal device As Device)
        Dim DeviceId As Integer = GetIdFromUuid(device.Uuid)

    End Sub

    Public Shared Sub AddDevice(ByVal device As Device)
        Dim sqlite_conn As SQLiteConnection
        Dim timestamp As String = DateTime.Today.ToShortDateString

        ' create a new database connection:
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")

        ' open the connection:
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [DEVICES] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [MACHINEID] NVARCHAR(2048) NULL,
                                  [NAME] NVARCHAR(2048) NULL,
                                  [LASTUPDATE] NVARCHAR(2048) NULL)"
        sqlite_cmd.ExecuteNonQuery()
        sqlite_cmd.CommandText = $"INSERT INTO SOFTWARE (MACHINEID, NAME, LASTUPDATE) VALUES ('{device.Uuid}', '{device.Hostname}', '{timestamp}');"

    End Sub

    Private Shared Function GetIdFromUuid(ByVal uuid As String) As Integer

    End Function

    Private Shared Sub DeleteTable(ByVal TableName As String)
        Dim sqlite_conn As SQLiteConnection
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()

        sqlite_cmd.CommandText = $"DELETE FROM {TableName}"
        sqlite_cmd.ExecuteNonQuery()
        sqlite_cmd.ExecuteNonQuery()
    End Sub
End Class
