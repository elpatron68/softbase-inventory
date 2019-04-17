Public Class Device
    Public Property Uuid As String
    Public Property Hostname As String
    Public Property Description As String
    Public Property DbID As Int32

    Public Sub New()
        Me.Uuid = GetWMI_Info.GetUUID()
        Me.Hostname = System.Environment.MachineName
    End Sub
End Class
