Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.IO
Imports OfficeOpenXml
Imports System.Xml
Imports System.Drawing
Imports OfficeOpenXml.Style

Public Class ExcelExport
    Public Shared Sub CreateXSLT(ByVal dest As String, ByVal softlist As List(Of software), device As Device, SnapshotTimestamp As String)
        Using package As ExcelPackage = New ExcelPackage
            Dim worksheet As ExcelWorksheet = package.Workbook.Worksheets.Add("SoftBase Inventory")
            worksheet.Cells(1, 1).Value = "Program name"
            worksheet.Cells(1, 2).Value = "Version"
            Dim linecount As Integer = 2
            For Each s In softlist
                worksheet.Cells(linecount, 1).Value = s.Name
                worksheet.Cells(linecount, 2).Value = s.Version
                linecount += 1
            Next

            worksheet.Cells.AutoFitColumns(0)
            Dim xlFile = My.Computer.FileSystem.GetFileInfo(dest)
            package.SaveAs(xlFile)
        End Using
    End Sub
End Class
