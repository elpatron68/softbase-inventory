Public Class Device
    Public Property Uuid As String
    Public Property Hostname As String
    Public Property Description As String
    Public Property DbID As Int32
    Public Property Snapshots As List(Of Tuple(Of String, Long))

    'Public Sub New()
    'End Sub
End Class
