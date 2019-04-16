Imports System
Imports System.Collections.Generic
Imports System.Management
Imports System.Text

Public Class GetWMI_Info
    Public Shared Function GetUUID()
        GetUUID = String.Empty
        Try
            Dim ComputerName As String = "localhost"
            Dim Scope As ManagementScope
            Scope = New ManagementScope(String.Format("\\{0}\root\CIMV2", ComputerName), Nothing)
            Scope.Connect()
            Dim Query As ObjectQuery = New ObjectQuery("SELECT UUID FROM Win32_ComputerSystemProduct")
            Dim Searcher As ManagementObjectSearcher = New ManagementObjectSearcher(Scope, Query)

            For Each WmiObject As ManagementObject In Searcher.[Get]()
                ' Console.WriteLine("{0,-35} {1,-40}", "UUID", WmiObject("UUID"))
                GetUUID = WmiObject("UUID")
            Next

        Catch e As Exception
            GetUUID = String.Empty
        End Try

    End Function
End Class

