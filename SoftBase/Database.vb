Imports System.Data.SQLite
Imports SoftBase

Public Class Database
    Private Shared dbfile As String = My.Settings.databasefile
    Public Shared Sub CreateTables()
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SNAPSHOTS] (
                                  [SNAPSHOTID] INTEGER PRIMARY KEY,
                                  [DEVICEID] INTEGER NOT NULL,
                                  [TIMESTAMP] NVARCHAR(2048) NOT NULL)"

            sqlite_cmd.ExecuteNonQuery()

            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [SOFTWARE] (
                                  [Id] INTEGER PRIMARY KEY,
                                  [DEVICEID] INTEGER NOT NULL,
                                  [NAME] NVARCHAR(2048) NOT NULL,
                                  [VERSION] NVARCHAR(2048) NULL,
                                  [SNAPSHOTID] INTEGER NOT NULL)"
            sqlite_cmd.ExecuteNonQuery()

            sqlite_cmd.CommandText = "CREATE TABLE IF NOT EXISTS [DEVICES] (
                                  [DEVICEID] INTEGER PRIMARY KEY,
                                  [UUID] NVARCHAR(2048) NOT NULL,
                                  [NAME] NVARCHAR(2048) NOT NULL,
                                  [LASTUPDATE] NVARCHAR(2048) NULL)"
            sqlite_cmd.ExecuteNonQuery()
            sqlite_conn.Close()
        End Using
    End Sub

    Public Shared Function GetAllDevices() As List(Of Device)
        Dim devices As List(Of Device) = New List(Of Device)
        Dim id As Integer = 0

        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT DEVICEID, UUID, NAME FROM DEVICES"
            Try
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                If r.HasRows Then
                    While r.Read
                        Dim d = New Device
                        d.Hostname = r("NAME")
                        d.Uuid = r("UUID")
                        d.DbID = r("DEVICEID")
                        devices.Add(d)
                    End While
                End If
            Catch ex As Exception
            End Try
            sqlite_conn.Close()
        End Using
        Return devices
    End Function

    Public Shared Function GetDeviceIdFromUuid(ByVal uuid As String) As Integer
        Dim id As Integer = 0
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT DEVICEID, UUID FROM DEVICES WHERE UUID = '{uuid}'"
            Try
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                If r.HasRows Then
                    r.Read()
                    id = r("DEVICEID")
                Else
                    id = -1
                End If
            Catch ex As Exception
                id = -1
            End Try
            sqlite_conn.Close()
        End Using
        Return id
    End Function

    Friend Shared Function GetDeviceFromUuid(uuid As String) As Device
        Dim d As Device = New Device
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT * FROM DEVICES WHERE UUID = '{uuid}'"
            Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
            If r.HasRows Then
                r.Read()
                d.Hostname = r("NAME")
                d.Uuid = r("UUID")
                d.DbID = r("DEVICEID")
            End If
        End Using
        Return d
    End Function


    Public Shared Function AddNewSnapshot(ByVal device As Device) As Long
        Dim timestamp As String = DateTime.Now.ToLocalTime.ToString
        Dim Id As Long = -1
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()

            Using sqlite_cmd As SQLiteCommand = sqlite_conn.CreateCommand()
                sqlite_cmd.CommandText = $"INSERT INTO SNAPSHOTS (DEVICEID, TIMESTAMP) VALUES ({device.DbID}, '{timestamp}');"
                sqlite_cmd.ExecuteNonQuery()
                sqlite_cmd.CommandText = $"SELECT last_insert_rowid()"
                Id = sqlite_cmd.ExecuteScalar()
            End Using
            sqlite_conn.Close()
        End Using
        Return Id
    End Function

    Public Shared Function GetLatestSnapshotIdForDevice(ByVal DeviceId As Integer) As Integer
        Dim Id As Integer = 0
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT * FROM SNAPSHOTS WHERE DEVICEID = {DeviceId}"
            Try
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                If r.HasRows Then
                    While r.Read
                        Id = r("SNAPSHOTID")
                    End While
                End If
            Catch ex As Exception
                Id = -1
            End Try
            sqlite_conn.Close()
        End Using
        Return Id
    End Function


    Public Shared Function GetAllSnapshotIdForDevice(ByVal DeviceId As Integer) As List(Of Tuple(Of String, Long))
        Dim Snaps As List(Of Tuple(Of String, Long)) = New List(Of Tuple(Of String, Long))
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT * FROM SNAPSHOTS WHERE DEVICEID = {DeviceId}"
            Try
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                If r.HasRows Then
                    While r.Read
                        Dim s As Tuple(Of String, Long) = New Tuple(Of String, Long)(r("TIMESTAMP"), r("SNAPSHOTID"))
                        Snaps.Add(s)
                    End While
                End If
            Catch ex As Exception

            End Try
            sqlite_conn.Close()
        End Using
        Return Snaps
    End Function


    Public Shared Sub SaveSoftwareList(ByVal Softlist As List(Of software), ByVal device As Device, ByVal SnapshotID As Long)
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()

            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            Dim transaction As SQLiteTransaction = sqlite_conn.BeginTransaction()
            For Each soft In Softlist
                sqlite_cmd.CommandText = $"INSERT INTO SOFTWARE (DEVICEID, NAME, VERSION, SNAPSHOTID) VALUES ('{device.DbID}', '{soft.Name}', '{soft.Version}', {SnapshotID});"
                sqlite_cmd.ExecuteNonQuery()
            Next
            Try
                transaction.Commit()
            Catch ex As Exception
            End Try
            sqlite_conn.Close()
        End Using
    End Sub

    Public Shared Function AddDevice(ByVal device As Device) As Long
        Dim timestamp As String = DateTime.Today.ToShortDateString
        Dim Id As Long = -1
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()

            Using tr As SQLiteTransaction = sqlite_conn.BeginTransaction()
                Using sqlite_cmd As SQLiteCommand = sqlite_conn.CreateCommand()
                    sqlite_cmd.Transaction = tr
                    sqlite_cmd.CommandText = $"INSERT INTO DEVICES (UUID, NAME, LASTUPDATE) VALUES ('{device.Uuid}', '{device.Hostname}', '{timestamp}');"
                    sqlite_cmd.ExecuteNonQuery()
                End Using
                tr.Commit()
                Id = sqlite_conn.LastInsertRowId
            End Using
            sqlite_conn.Close()
        End Using
        Return Id
    End Function

    Public Shared Function LoadSoftwareListForDevice(ByVal Device As Device, ByVal SnapshotId As Integer) As (List(Of software), Timestamp As String)
        Dim Softlist As List(Of software) = New List(Of software)
        Dim ts As String = GetTimeStamp(Device.DbID)
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            Try
                sqlite_conn.Open()
                Dim sqlite_cmd = sqlite_conn.CreateCommand()
                sqlite_cmd.CommandText = $"SELECT DEVICEID, NAME, VERSION FROM SOFTWARE WHERE (DEVICEID = {Device.DbID} AND SNAPSHOTID = {SnapshotId}) ORDER BY NAME"
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
            sqlite_conn.Close()
        End Using
        Return (Softlist, ts)
    End Function

    Private Shared Function GetTimeStamp(ByVal DeviceID As Integer) As String
        Dim ts As String = String.Empty
        Using sqlite_conn As SQLiteConnection = New SQLiteConnection($"Data Source={dbfile};Version=3;")
            sqlite_conn.Open()
            Dim sqlite_cmd = sqlite_conn.CreateCommand()
            sqlite_cmd.CommandText = $"SELECT DEVICEID, LASTUPDATE FROM DEVICES WHERE DEVICEID = {DeviceID}"
            Try
                Dim r As SQLiteDataReader = sqlite_cmd.ExecuteReader()
                While r.Read
                    ts = r("LASTUPDATE")
                End While
            Catch ex As Exception
            End Try
            sqlite_conn.Close()
        End Using
        Return ts
    End Function

End Class
