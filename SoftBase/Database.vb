Imports System.IO
Imports System.Data.SQLite

Public Class Database
    Private Shared dbfile As String = My.Settings.databasefile
    Private Shared sqlite_conn As SQLiteConnection
    Public Shared Sub SaveList(ByVal Softlist As List(Of software), ByVal device As Device)
        Dim DeviceId As Integer = GetIdFromUuid(device.Uuid)
        DeleteOldEntries(DeviceId)
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SOFTWARE] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [MACHINEID] INTEGER NOT NULL,
                                  [NAME] NVARCHAR(2048) NOT NULL,
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
                sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
                sqlite_conn.Open()
                Dim sqlite_cmd = sqlite_conn.CreateCommand()
                sqlite_cmd.CommandText = $"SELECT MACHINEID, NAME, VERSION FROM SOFTWARE WHERE MACHINEID = {DeviceId} ORDER BY NAME"
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
        Dim timestamp As String = DateTime.Today.ToShortDateString
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [DEVICES] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [MACHINEID] NVARCHAR(2048) NOT NULL,
                                  [NAME] NVARCHAR(2048) NOT NULL,
                                  [LASTUPDATE] NVARCHAR(2048) NULL)"
        sqlite_cmd.ExecuteNonQuery()
        sqlite_cmd.CommandText = $"INSERT INTO DEVICES (MACHINEID, NAME, LASTUPDATE) VALUES ('{device.Uuid}', '{device.Hostname}', '{timestamp}');"
        sqlite_cmd.ExecuteNonQuery()
    End Sub

    Public Shared Function AddSnapshot(ByVal device As Device) As Integer
        Dim timestamp As String = DateTime.Now.ToLocalTime.ToString
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SNAPSHOTS] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [DEVICEID] INTEGER NOT NULL,
                                  [TIMESTAMP] NVARCHAR(2048) NOT NULL)"
        sqlite_cmd.ExecuteNonQuery()
        sqlite_cmd.CommandText = $"INSERT INTO DEVICES (MACHINEID, NAME, LASTUPDATE) VALUES ('{device.Uuid}', '{device.Hostname}', '{timestamp}');"
        sqlite_cmd.ExecuteNonQuery()
    End Function


    Public Shared Function GetIdFromUuid(ByVal uuid As String) As Integer
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

    Public Shared Function GetDevices() As List(Of Device)
        Dim devices As List(Of Device) = New List(Of Device)
        Dim id As Integer = 0
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"SELECT Id, NAME FROM DEVICES"
        Try
            Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
            If r.HasRows Then
                While r.Read
                    Dim d = New Device
                    d.Hostname = r("NAME")
                    d.DbID = r("Id")
                    devices.Add(d)
                End While
            End If
        Catch ex As Exception

        End Try

        Return devices
    End Function
    Private Shared Function GetTimeStamp(ByVal MachineID As Integer) As String
        Dim ts As String = String.Empty
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"SELECT Id, LASTUPDATE FROM DEVICES WHERE Id = {MachineID}"
        Try
            Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
            While r.Read
                ts = r("LASTUPDATE")
            End While
        Catch ex As Exception

        End Try
        Return ts
    End Function

    Private Shared Sub DeleteTable(ByVal TableName As String)
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"DELETE FROM {TableName}"
        Try
            sqlite_cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
    End Sub

    Private Shared Sub DeleteOldEntries(ByVal MachineID As Integer)
        sqlite_conn = New SQLiteConnection($"Data Source={dbfile};Version=3;")
        sqlite_conn.Open()
        Dim sqlite_cmd = sqlite_conn.CreateCommand()
        sqlite_cmd.CommandText = $"DELETE FROM SOFTWARE WHERE MACHINEID = {MachineID}"
        Try
            sqlite_cmd.ExecuteNonQuery()
        Catch ex As Exception

        End Try
    End Sub
End Class
