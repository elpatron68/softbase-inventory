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
                                  [MACHINEID] INTEGER NOT NULL,
                                  [NAME] NVARCHAR(2048) NULL,
                                  [VERSION] NVARCHAR(2048) NULL)"
        sqlite_cmd.ExecuteNonQuery()

        For Each soft In Softlist
            sqlite_cmd.CommandText = $"INSERT INTO SOFTWARE (MACHINEID, NAME, VERSION) VALUES ('{DeviceId}', '{soft.Name}', '{soft.Version}');"
            sqlite_cmd.ExecuteNonQuery()
        Next
    End Sub

    Public Shared Function LoadSoftwareListForDevice(ByVal device As Device) As (List(Of software), Timestamp As String)
        Dim Softlist As List(Of software) = New List(Of software)
        Dim DeviceId As Integer = GetIdFromUuid(device.Uuid)
        Dim ts As String = GetTimeStamp(DeviceId)

        If DeviceId <> -1 Then
            Try
                Dim sqlite_conn As SQLiteConnection
                sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
                sqlite_conn.Open()
                Dim sqlite_cmd = sqlite_conn.CreateCommand()
                sqlite_cmd.CommandText = $"SELECT MACHINEID, NAME, VERSION FROM SOFTWARE WHERE MACHINEID = {DeviceId}"
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                If r.HasRows Then
                    While r.Read
                        Dim soft As New software
                        soft.Name = r("NAME")
                        soft.Version = r("VERSION")
                        Softlist.Add(soft)
                    End While
                End If
            Catch ex As Exception
                ts = "-1"
            End Try
        End If
        Return (Softlist, ts)
    End Function

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
        sqlite_cmd.CommandText = $"INSERT INTO DEVICES (MACHINEID, NAME, LASTUPDATE) VALUES ('{device.Uuid}', '{device.Hostname}', '{timestamp}');"
        sqlite_cmd.ExecuteNonQuery()
    End Sub

    Public Shared Function GetIdFromUuid(ByVal uuid As String) As Integer
        Dim sqlite_conn As SQLiteConnection
        Dim id As Integer = 0
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"SELECT Id, MACHINEID FROM DEVICES WHERE MACHINEID LIKE '{uuid}'"
        Try
            Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
            If r.HasRows Then
                While r.Read
                    id = r("Id")
                End While
            Else
                id = -1
            End If
        Catch ex As Exception
            id = -1
        End Try
        Return id
    End Function

    Private Shared Function GetTimeStamp(ByVal MachineID) As String
        Dim sqlite_conn As SQLiteConnection
        Dim ts As String = String.Empty
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"SELECT MACHINEID, LASTUPDATE FROM DEVICES WHERE MACHINEID = {MachineID}"
        Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
        While r.Read
            ts = r("LASTUPDATE")
        End While
        Return ts
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
