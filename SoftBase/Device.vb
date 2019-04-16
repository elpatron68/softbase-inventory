Public Class Device
    Public Property Uuid As String
    Public Property Hostname As String
    Public Property Description As String

    Public Sub New(ByVal Uuid As String)
        Me.Uuid = Uuid
        Me.Hostname = System.Environment.MachineName
    End Sub
End Class
